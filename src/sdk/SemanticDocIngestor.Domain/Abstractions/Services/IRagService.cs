using SemanticDocIngestor.Domain.DTOs;
using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Domain.Abstractions.Services
{
    public interface IRagService
    {
        Task<string> GetAnswerAsync(string question, List<DocumentChunkDto> contextChunks, CancellationToken cancellationToken);
        Task<List<string>> GetDocumentChunksAsync(string content, CancellationToken cancellationToken);
    }
}