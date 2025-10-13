using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Domain.Abstractions.Persistence
{
    public interface IVectorStore
    {
        Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default);
        Task DeleteExistingChunksAsync(string filePath, CancellationToken cancellationToken);
        Task EnsureCollectionExistsAsync();
        Task<List<DocumentChunk>> SearchAsync(string query, ulong topK = 5, CancellationToken cancellationToken = default);
        Task UpsertAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default);
    }
}
