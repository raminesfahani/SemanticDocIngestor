using AutoMapper;
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
using SemanticDocIngestor.Infrastructure.Persistence.Cache;
using System.Collections.Generic;

namespace SemanticDocIngestor.Core.Services
{
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

        public DocumentIngestorService(IDocumentProcessor documentProcessor,
                                             IMapper mapper,
                                             IElasticStore elasticStore,
                                             IVectorStore vectorStore,
                                             HybridCache cache,
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
            _resolvers = cloudResolvers ?? [];
            _hubContext = hubContext;

            OnProgress += async (s, e) => await UpdateProgressCacheAsync(e);
            OnCompleted += async (s, e) => await UpdateProgressCacheAsync(e);
        }

        public event EventHandler<IngestionProgress>? OnProgress;
        public event EventHandler<IngestionProgress>? OnCompleted;

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

        public async Task IngestFolderAsync(string folderPath, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"The folder path '{folderPath}' does not exist.");

            // Ensure backing stores exist before any operations to avoid index/collection missing errors
            await _vectorStore.EnsureCollectionExistsAsync();
            await _elasticStore.EnsureIndexExistsAsync();

            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                         .Where(f => _documentProcessor.SupportedFileExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                         .ToList();

            await IngestDocumentsAsync(files, cancellationToken: cancellationToken);
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            await _vectorStore.DeleteCollectionAsync(cancellationToken);
            await _elasticStore.DeleteCollectionAsync(cancellationToken);
        }

        public async Task IngestDocumentsAsync(IEnumerable<string> documentPaths, int maxChunkSize = 500, CancellationToken cancellationToken = default)
        {
            // Ensure backing stores exist before any operations to prevent 404 on delete-by-query/index
            await _vectorStore.EnsureCollectionExistsAsync();
            await _elasticStore.EnsureIndexExistsAsync();

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

        public void Dispose()
        {
            OnProgress = null;
            OnCompleted = null;
            GC.SuppressFinalize(this);
        }
    }
}
