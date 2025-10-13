using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Domain.Abstractions.Persistence
{
    public interface IElasticStore
    {
        Task EnsureIndexExistsAsync();
        Task<bool> UpsertAsync(DocumentChunk chunk, CancellationToken ct = default);
        Task<bool> UpsertAsync(List<DocumentChunk> chunks, CancellationToken ct = default);
        Task<IEnumerable<DocumentChunk>> SearchAsync(string query, int size = 10, CancellationToken ct = default);
        Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default);
        Task DeleteExistingChunks(List<DocumentChunk> chunks, CancellationToken ct);
        Task DeleteExistingChunks(string filePath, CancellationToken ct);
    }
}
