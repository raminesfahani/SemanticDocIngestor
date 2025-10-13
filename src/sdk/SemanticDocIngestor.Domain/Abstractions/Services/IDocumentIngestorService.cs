using SemanticDocIngestor.Domain.Entities.Ingestion;
using System.Threading;

namespace SemanticDocIngestor.Domain.Abstractions.Services
{
    public interface IDocumentIngestorService
    {
        event EventHandler<IngestionProgress>? OnProgress;

        Task FlushAsync(CancellationToken cancellationToken);
        Task IngestDocumentsAsync(IEnumerable<string> documentPaths, int maxChunkSize = 500, CancellationToken cancellationToken = default);
        Task IngestFolderAsync(string folderPath, CancellationToken cancellationToken = default);
        Task<List<DocumentChunk>> SearchDocumentsAsync(
            string query,
            ulong limit = 10,
            Func<List<DocumentChunk>, List<DocumentChunk>>? reranker = null,
            CancellationToken cancellationToken = default);
    }
}