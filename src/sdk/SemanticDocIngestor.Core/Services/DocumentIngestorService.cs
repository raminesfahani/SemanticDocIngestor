using SemanticDocIngestor.Domain.Abstractions.Factories;
using SemanticDocIngestor.Domain.Abstractions.Persistence;
using SemanticDocIngestor.Domain.Abstractions.Services;
using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Core.Services
{
    public class DocumentIngestorService(IDocumentProcessor documentProcessor,
                                  IElasticStore elasticStore,
                                  IVectorStore vectorStore,
                                  IEnumerable<ICloudFileResolver>? cloudResolvers = null) : IDocumentIngestorService
    {
        private readonly IDocumentProcessor _documentProcessor = documentProcessor;
        private readonly IElasticStore _elasticStore = elasticStore;
        private readonly IVectorStore _vectorStore = vectorStore;
        private readonly IEnumerable<ICloudFileResolver> _resolvers = cloudResolvers ?? [];

        public event EventHandler<IngestionProgress>? OnProgress;
        public event EventHandler<IngestionProgress>? OnCompleted;

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

            int total = files.Count;
            int completed = 0;

            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    // Pre-delete by the stable identity (local path)
                    await _vectorStore.DeleteExistingChunksAsync(file, cancellationToken);
                    await _elasticStore.DeleteExistingChunks(file, cancellationToken);

                    var processedDoc = await _documentProcessor.ProcessDocument(file, default, cancellationToken);

                    // Ensure consistent identity metadata for local files
                    foreach (var c in processedDoc)
                    {
                        c.Metadata.Source = IngestionSource.Local;
                        c.Metadata.FilePath = file;
                    }

                    await _vectorStore.UpsertAsync(processedDoc, cancellationToken);
                    await _elasticStore.UpsertAsync(processedDoc, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error ingesting {file}: {ex.Message}");
                }

                completed++;
                OnProgress?.Invoke(this, new IngestionProgress
                {
                    FilePath = file,
                    Completed = completed,
                    Total = total
                });
            }

            OnCompleted?.Invoke(this, new IngestionProgress
            {
                Completed = completed,
                Total = total
            });
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
                    OnProgress?.Invoke(this, new IngestionProgress
                    {
                        FilePath = p.IdentityPath,
                        Completed = completed,
                        Total = total
                    });
                }

                OnCompleted?.Invoke(this, new IngestionProgress
                {
                    Completed = plans.Count,
                    Total = plans.Count
                });
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

        public async Task<List<DocumentChunk>> SearchDocumentsAsync(
            string query,
            ulong limit = 10,
            Func<List<DocumentChunk>, List<DocumentChunk>>? reranker = null,
            CancellationToken cancellationToken = default)
        {
            if (limit == 0)
            {
                return [];
            }

            // Run searches in parallel to minimize end-to-end latency
            var vectorTask = _vectorStore.SearchAsync(query, topK: limit, cancellationToken: cancellationToken);
            var keywordTask = _elasticStore.SearchAsync(query, size: (int)limit, ct: cancellationToken);

            await Task.WhenAll(vectorTask, keywordTask).ConfigureAwait(false);

            var vectorResults = vectorTask.Result ?? [];
            var keywordResults = keywordTask.Result ?? [];

            // Streamed de-duplication with early-exit to avoid full materialization and GroupBy allocations
            int intLimit = limit > int.MaxValue ? int.MaxValue : (int)limit;
            var results = new List<DocumentChunk>(intLimit);
            int setCapacity = intLimit > (int.MaxValue / 2) ? int.MaxValue : Math.Max(0, intLimit * 2);
            var seen = new HashSet<(string? Content, IngestionSource Source, string? FileName, string? FilePath, string? PageNumber)>(setCapacity);

            void Accumulate(IEnumerable<DocumentChunk> sequence)
            {
                foreach (var c in sequence)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var m = c.Metadata;
                    var key = (c.Content, m.Source, m.FileName, m.FilePath, m.PageNumber);

                    if (seen.Add(key))
                    {
                        results.Add(c);
                        if (results.Count >= intLimit)
                            break;
                    }
                }
            }

            // Prefer vector results; fill with keyword results as needed
            Accumulate(vectorResults);
            if (results.Count < intLimit)
            {
                Accumulate(keywordResults);
            }

            // Apply reranker if provided
            if (reranker != null)
            {
                results = reranker(results);
                if (results.Count > intLimit)
                {
                    results = [.. results.Take(intLimit)];
                }
            }

            return results;
        }
    }
}
