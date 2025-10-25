using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SemanticDocIngestor.Core.Hubs;
using SemanticDocIngestor.Domain.Abstractions.Factories;
using SemanticDocIngestor.Domain.Abstractions.Hubs;
using SemanticDocIngestor.Domain.Abstractions.Persistence;
using SemanticDocIngestor.Domain.Abstractions.Services;
using SemanticDocIngestor.Domain.DTOs;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using SemanticDocIngestor.Infrastructure.Factories.Ollama;
using SemanticDocIngestor.Infrastructure.Persistence.Cache;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SemanticDocIngestor.Core.Services
{
    /// <summary>
    /// Primary implementation of document ingestion service providing hybrid search and RAG capabilities.
    /// Orchestrates document processing, chunking, embedding generation, and storage in both vector and keyword stores.
    /// Supports multi-source ingestion from local files, OneDrive, and Google Drive with real-time progress tracking.
    /// </summary>
    public class DocumentIngestorService : IDocumentIngestorService, IDisposable
    {
        private readonly IDocumentProcessor _documentProcessor;
        private readonly IMapper _mapper;
        private readonly IElasticStore _elasticStore;
        private readonly IVectorStore _vectorStore;
        private readonly HybridCache _cache;
        private readonly IEnumerable<ICloudFileResolver> _resolvers;
        private readonly ILogger<DocumentIngestorService> _logger;
        private readonly IHubContext<IngestionHub, IIngestionHubClient>? _hubContext;
        private readonly IRagService _ragService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentIngestorService"/> class.
        /// </summary>
        /// <param name="documentProcessor">Service for processing and chunking documents.</param>
        /// <param name="mapper">AutoMapper instance for DTO/entity mapping.</param>
        /// <param name="elasticStore">Elasticsearch store for keyword search.</param>
        /// <param name="vectorStore">Qdrant vector store for semantic search.</param>
        /// <param name="cache">Hybrid cache for storing progress and temporary data.</param>
        /// <param name="ragService">RAG service for generating AI-powered answers.</param>
        /// <param name="logger">Logger instance for diagnostic logging.</param>
        /// <param name="cloudResolvers">Optional collection of cloud file resolvers for OneDrive, Google Drive, etc.</param>
        /// <param name="hubContext">Optional SignalR hub context for real-time progress notifications.</param>
        public DocumentIngestorService(IDocumentProcessor documentProcessor,
                                             IMapper mapper,
                                             IElasticStore elasticStore,
                                             IVectorStore vectorStore,
                                             HybridCache cache,
                                             IRagService ragService,
                                             ILogger<DocumentIngestorService> logger,
                                             IEnumerable<ICloudFileResolver>? cloudResolvers = null,
                                             IHubContext<IngestionHub, IIngestionHubClient>? hubContext = null)
        {
            _documentProcessor = documentProcessor;
            _mapper = mapper;
            _logger = logger;
            _elasticStore = elasticStore;
            _vectorStore = vectorStore;
            _cache = cache;
            _ragService = ragService;
            _resolvers = cloudResolvers ?? [];
            _hubContext = hubContext;

            OnProgress += async (s, e) => await UpdateProgressCacheAsync(e);
            OnCompleted += async (s, e) => await UpdateProgressCacheAsync(e);
        }

        public event EventHandler<IngestionProgress>? OnProgress;
        public event EventHandler<IngestionProgress>? OnCompleted;

        /// <summary>
        /// Updates the ingestion progress in cache and notifies SignalR clients.
        /// </summary>
        private async Task UpdateProgressCacheAsync(IngestionProgress progress, CancellationToken cancellationToken = default)
        {
            await _cache.SetAsync(CacheKeyHelper.IngestionProgressKey, progress, new HybridCacheEntryOptions() { Expiration = TimeSpan.FromHours(2) }, cancellationToken: cancellationToken);
            _logger.LogInformation("Ingestion Progress - {Completed}/{Total} - {FilePath}", progress.Completed, progress.Total, progress.FilePath);
            
            // Notify SignalR clients if hub context is available
            if (_hubContext != null)
            {
                await _hubContext.Clients.All.ReceiveProgress(progress);
            }
        }

        /// <summary>
        /// Gets the current ingestion progress.
        /// </summary>
        public async Task<IngestionProgress?> GetProgressAsync(CancellationToken cancellationToken = default)
        {
            return await _cache.GetOrCreateAsync(CacheKeyHelper.IngestionProgressKey, async (ct) =>
            {
                return await Task.FromResult(new IngestionProgress());
            }, new HybridCacheEntryOptions()
            {
                Expiration = TimeSpan.FromHours(2)
            }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Ingests all documents from the specified folder path.
        /// </summary>
        public async Task IngestFolderAsync(string folderPath, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"The folder path '{folderPath}' does not exist.");

            // Ensure backing stores exist before any operations to avoid index/collection missing errors
            await _vectorStore.EnsureCollectionExistsAsync();
            await _elasticStore.EnsureSemanticDocIndexExistsAsync();

            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                         .Where(f => _documentProcessor.SupportedFileExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                         .ToList();

            await IngestDocumentsAsync(files, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Flushes (deletes) all data in the vector and elastic stores.
        /// </summary>
        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            await _vectorStore.DeleteCollectionAsync(cancellationToken);
            await _elasticStore.DeleteCollectionAsync(cancellationToken);
        }

        /// <summary>
        /// Ingests multiple documents specified by their paths.
        /// </summary>
        public async Task IngestDocumentsAsync(IEnumerable<string> documentPaths, int maxChunkSize = 500, CancellationToken cancellationToken = default)
        {
            // Ensure backing stores exist before any operations to prevent 404 on delete-by-query/index
            await _vectorStore.EnsureCollectionExistsAsync();
            await _elasticStore.EnsureSemanticDocIndexExistsAsync();

            // Resolve each input into a processing plan (local path or cloud temp download with stable identity)
            var plans = new List<(string LocalPath, string IdentityPath, IngestionSource Source, IAsyncDisposable? Lease)>();

            try
            {
                foreach (var input in documentPaths)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (File.Exists(input))
                    {
                        plans.Add((input, input, IngestionSource.Local, null));
                        continue;
                    }

                    var resolver = _resolvers.FirstOrDefault(r => r.CanResolve(input)) ?? throw new InvalidOperationException($"No resolver registered for input: {input}");
                    var resolved = await resolver.ResolveAsync(input, cancellationToken);
                    plans.Add((resolved.LocalPath, resolved.IdentityPath, resolved.Source, resolved));
                }

                int total = plans.Count;
                int completed = 0;
                var initialProgress = new IngestionProgress
                {
                    FilePath = string.Empty,
                    Completed = completed,
                    Total = total
                };
                
                OnProgress?.Invoke(this, initialProgress);

                // Pre-delete by identity to avoid duplicates across re-ingests
                foreach (var p in plans)
                {
                    await _vectorStore.DeleteExistingChunksAsync(p.IdentityPath, cancellationToken);
                    await _elasticStore.DeleteExistingChunks(p.IdentityPath, cancellationToken);
                }

                var processedDocs = await Task.WhenAll(plans.Select(p => _documentProcessor.ProcessDocument(p.LocalPath, maxChunkSize, cancellationToken)));

                for (int i = 0; i < processedDocs.Length; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var p = plans[i];
                    var docChunks = processedDocs[i];

                    // Stamp stable identity
                    foreach (var c in docChunks)
                    {
                        c.Metadata.Source = p.Source;
                        c.Metadata.FilePath = p.IdentityPath;
                    }

                    await _vectorStore.UpsertAsync(docChunks, cancellationToken);
                    await _elasticStore.UpsertAsync(docChunks, cancellationToken);

                    completed++;
                    var progressUpdate = new IngestionProgress
                    {
                        FilePath = p.IdentityPath,
                        Completed = completed,
                        Total = total
                    };
                    
                    OnProgress?.Invoke(this, progressUpdate);
                }

                var completedProgress = new IngestionProgress
                {
                    Completed = plans.Count,
                    Total = plans.Count
                };
                
                OnCompleted?.Invoke(this, completedProgress);
                
                // Notify SignalR clients about completion
                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.ReceiveCompleted(completedProgress);
                }
            }
            finally
            {
                // Dispose any temporary cloud downloads
                foreach (var p in plans)
                {
                    if (p.Lease is not null)
                    {
                        try { await p.Lease.DisposeAsync(); } catch { /* best effort */ }
                    }
                }
            }
        }

        /// <summary>
        /// Searches for documents matching the specified query.
        /// </summary>
        public async Task<List<DocumentChunkDto>> SearchDocumentsAsync(
            string query,
            ulong limit = 10,
            CancellationToken cancellationToken = default)
        {
            if (limit == 0)
            {
                return [];
            }

            var vectorTask = _vectorStore.SearchAsync(query, topK: limit, cancellationToken: cancellationToken);
            var keywordTask = _elasticStore.SearchAsync(query, size: (int)limit, ct: cancellationToken);

            await Task.WhenAll(vectorTask, keywordTask).ConfigureAwait(false);

            var vectorResults = vectorTask.Result ?? [];
            var keywordResults = keywordTask.Result ?? [];

            List<DocumentChunk> results = [];

            vectorResults.Concat(keywordResults)
                         .GroupBy(c => new { c.Content, c.Metadata.Source, c.Metadata.FileName, c.Metadata.FilePath, c.Metadata.PageNumber }) // Deduplicate
                         .Select(g => g.First())
                         .Take((int)limit)
                         .ToList()
                         .ForEach(results.Add);

            return _mapper.Map<List<DocumentChunkDto>>(results);
        }

        /// <summary>
        /// Searches for documents and generates a RAG response.
        /// </summary>
        public async Task<SearchAndGetRagResponseDto> SearchAndGetRagResponseAsync(string search = "", ulong limit = 5, CancellationToken cancellationToken = default)
        {
            var contextChunks = await SearchDocumentsAsync(search, limit: limit, cancellationToken: cancellationToken);
            var ragResponse = await _ragService.GetAnswerAsync(search, contextChunks, cancellationToken: cancellationToken);

            return new SearchAndGetRagResponseDto
            {
                Answer = ragResponse,
                ReferencesPath = contextChunks?.GroupBy(c => c.Metadata.FilePath)
                                               .ToDictionary(g => g.Key ?? "", g => g.ToList()) ?? []
            };
        }

        /// <summary>
        /// Searches for documents and generates a streaming RAG response.
        /// </summary>
        public async Task<SearchAndGetRagStreamingResponseDto> SearchAndGetRagStreamResponseAsync(string search = "", ulong limit = 5, CancellationToken cancellationToken = default)
        {
            var contextChunks = await SearchDocumentsAsync(search, limit: limit, cancellationToken: cancellationToken);
            var ragResponse = _ragService.GetStreamingAnswer(search, contextChunks, cancellationToken: cancellationToken);

            return new SearchAndGetRagStreamingResponseDto
            {
                Answer = ragResponse,
                ReferencesPath = contextChunks?.GroupBy(c => c.Metadata.FilePath)
                                               .ToDictionary(g => g.Key ?? "", g => g.ToList()) ?? []
            };
        }

        /// <summary>
        /// Lists all ingested documents.
        /// </summary>
        public async Task<List<DocumentRepoItemDto>> ListIngestedDocumentsAsync(CancellationToken cancellationToken = default)
        {
            var ingestedDocs = await _elasticStore.GetIngestedDocumentsAsync(cancellationToken);
            return _mapper.Map<List<DocumentRepoItemDto>>(ingestedDocs);
        }

        /// <summary>
        /// Disposes of event handlers to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            OnProgress = null;
            OnCompleted = null;
            GC.SuppressFinalize(this);
        }
    }
}
