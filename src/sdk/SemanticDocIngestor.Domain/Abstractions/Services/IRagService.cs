using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Domain.Abstractions.Services
{
    public interface IRagService
    {
        Task<string> GetAnswerAsync(string question, List<DocumentChunk> contextChunks, CancellationToken cancellationToken);
        Task<List<string>> GetDocumentChunksAsync(string content, CancellationToken cancellationToken);
    }
}