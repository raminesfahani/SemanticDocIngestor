using SemanticDocIngestor.Domain.DTOs;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using System.Threading;

namespace SemanticDocIngestor.Domain.Abstractions.Services
{
    public interface IDocumentIngestorService
    {
        event EventHandler<IngestionProgress>? OnProgress;
        event EventHandler<IngestionProgress>? OnCompleted;

        Task FlushAsync(CancellationToken cancellationToken);
        Task<IngestionProgress?> GetProgressAsync(CancellationToken cancellationToken = default);
        Task IngestDocumentsAsync(IEnumerable<string> documentPaths, int maxChunkSize = 500, CancellationToken cancellationToken = default);
        Task IngestFolderAsync(string folderPath, CancellationToken cancellationToken = default);
        Task<List<DocumentChunkDto>> SearchDocumentsAsync(
            string query,
            ulong limit = 10,
            CancellationToken cancellationToken = default);
    }
}