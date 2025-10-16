using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Domain.Abstractions.Hubs
{
    public interface IIngestionHubClient
    {
        Task ReceiveProgress(IngestionProgress progress);
        Task ReceiveCompleted(IngestionProgress progress);
        Task ReceiveMessage(string message);
    }
}