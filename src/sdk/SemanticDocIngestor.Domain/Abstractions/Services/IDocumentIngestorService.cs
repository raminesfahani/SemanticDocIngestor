using SemanticDocIngestor.Domain.DTOs;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using System.Threading;

namespace SemanticDocIngestor.Domain.Abstractions.Services
{
    /// <summary>
    /// Primary service interface for document ingestion, search, and retrieval-augmented generation (RAG).
    /// Provides comprehensive document processing with hybrid search capabilities combining vector and keyword search.
    /// </summary>
    public interface IDocumentIngestorService
    {
        /// <summary>
        /// Event raised during document ingestion to report progress updates.
        /// Subscribe to this event to monitor the ingestion process in real-time.
        /// </summary>
        event EventHandler<IngestionProgress>? OnProgress;

        /// <summary>
        /// Event raised when document ingestion completes successfully.
        /// Provides final statistics about the ingestion process.
        /// </summary>
        event EventHandler<IngestionProgress>? OnCompleted;

        /// <summary>
        /// Removes all ingested documents from both vector and keyword stores.
        /// This operation is irreversible and will delete all indexed data.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous flush operation.</returns>
        Task FlushAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the current ingestion progress, including the number of documents processed.
        /// Useful for displaying progress in user interfaces or monitoring ingestion status.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>Current ingestion progress or null if no ingestion is in progress.</returns>
        Task<IngestionProgress?> GetProgressAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Ingests multiple documents from various sources (local files, OneDrive, Google Drive).
        /// Documents are processed, chunked, embedded, and stored in both vector and keyword stores.
        /// Supports idempotent re-ingestion with automatic deduplication.
        /// </summary>
        /// <param name="documentPaths">
        /// Collection of document paths or URIs. Supported formats:
        /// - Local paths: "C:\docs\file.pdf"
        /// - OneDrive: "onedrive://{driveId}/{itemId}" or share links
        /// - Google Drive: "gdrive://{fileId}" or drive.google.com URLs
        /// </param>
        /// <param name="maxChunkSize">Maximum size of each text chunk in tokens (default: 500).</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous ingestion operation.</returns>
        Task IngestDocumentsAsync(IEnumerable<string> documentPaths, int maxChunkSize = 500, CancellationToken cancellationToken = default);

        /// <summary>
        /// Recursively ingests all supported documents from a specified folder.
        /// Automatically filters files by supported extensions and processes them in batch.
        /// </summary>
        /// <param name="folderPath">Path to the folder containing documents to ingest.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous folder ingestion operation.</returns>
        /// <exception cref="DirectoryNotFoundException">Thrown when the specified folder does not exist.</exception>
        Task IngestFolderAsync(string folderPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves metadata for all documents that have been ingested into the system.
        /// Includes information about file names, sources, ingestion timestamps, and more.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>List of document metadata for all ingested documents.</returns>
        Task<List<DocumentRepoItemDto>> ListIngestedDocumentsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs hybrid search combining vector similarity and keyword matching, then generates
        /// an AI-powered answer using retrieval-augmented generation (RAG) with Ollama.
        /// </summary>
        /// <param name="search">The search query or question.</param>
        /// <param name="limit">Maximum number of document chunks to use as context (default: 5).</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>RAG response containing the generated answer and source document references.</returns>
        Task<SearchAndGetRagResponseDto> SearchAndGetRagResponseAsync(string search = "", ulong limit = 5, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs hybrid search and generates an AI-powered answer with streaming response.
        /// Allows real-time display of the answer as it's being generated by the LLM.
        /// </summary>
        /// <param name="search">The search query or question.</param>
        /// <param name="limit">Maximum number of document chunks to use as context (default: 5).</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>Streaming RAG response with async enumerable answer stream and source references.</returns>
        Task<SearchAndGetRagStreamingResponseDto> SearchAndGetRagStreamResponseAsync(string search = "", ulong limit = 5, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs hybrid search across ingested documents using both vector similarity and keyword matching.
        /// Results are deduplicated and ranked by relevance.
        /// </summary>
        /// <param name="query">The search query text.</param>
        /// <param name="limit">Maximum number of document chunks to return (default: 10).</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>List of relevant document chunks with metadata.</returns>
        Task<List<DocumentChunkDto>> SearchDocumentsAsync(
            string query,
  ulong limit = 10,
            CancellationToken cancellationToken = default);
    }
}