using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Domain.Abstractions.Hubs
{
    /// <summary>
    /// SignalR hub client interface for real-time ingestion progress notifications.
    /// Clients implementing this interface can receive live updates during document ingestion.
    /// </summary>
    public interface IIngestionHubClient
    {
        /// <summary>
        /// Receives progress updates during document ingestion.
        /// Called for each document processed to provide real-time progress information.
        /// </summary>
        /// <param name="progress">Current ingestion progress with completed and total counts.</param>
        /// <returns>A task representing the asynchronous notification operation.</returns>
        Task ReceiveProgress(IngestionProgress progress);
        
        /// <summary>
        /// Receives notification when document ingestion completes.
        /// Called once at the end of the ingestion process with final statistics.
        /// </summary>
        /// <param name="progress">Final ingestion progress with completion details.</param>
        /// <returns>A task representing the asynchronous notification operation.</returns>
        Task ReceiveCompleted(IngestionProgress progress);
        
        /// <summary>
        /// Receives general messages from the ingestion process.
        /// Can be used for status updates, warnings, or informational messages.
        /// </summary>
        /// <param name="message">The message text to display to the client.</param>
        /// <returns>A task representing the asynchronous notification operation.</returns>
        Task ReceiveMessage(string message);
    }
}