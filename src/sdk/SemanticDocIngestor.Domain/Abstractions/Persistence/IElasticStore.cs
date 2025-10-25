using SemanticDocIngestor.Domain.DTOs;
using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Domain.Abstractions.Persistence
{
    /// <summary>
    /// Interface for Elasticsearch operations providing keyword-based search capabilities.
    /// Handles BM25 full-text search and document metadata storage.
    /// </summary>
    public interface IElasticStore
    {
        /// <summary>
        /// Ensures the semantic document index exists in Elasticsearch, creating it if necessary.
        /// Should be called before any operations that require the index to exist.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task EnsureSemanticDocIndexExistsAsync();

        /// <summary>
        /// Inserts or updates a single document chunk in the Elasticsearch index.
        /// </summary>
        /// <param name="chunk">The document chunk to upsert with its content and metadata.</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>True if the upsert was successful; otherwise, false.</returns>
        Task<bool> UpsertAsync(DocumentChunk chunk, CancellationToken ct = default);

        /// <summary>
        /// Inserts or updates multiple document chunks in the Elasticsearch index using bulk operations.
        /// More efficient than multiple individual upserts for batch processing.
        /// </summary>
        /// <param name="chunks">The list of document chunks to upsert.</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>True if all upserts were successful; otherwise, false.</returns>
        Task<bool> UpsertAsync(List<DocumentChunk> chunks, CancellationToken ct = default);

        /// <summary>
        /// Performs keyword-based full-text search using Elasticsearch's BM25 algorithm.
        /// Searches across document content and metadata fields.
        /// </summary>
        /// <param name="query">The search query text.</param>
        /// <param name="size">Maximum number of results to return (default: 10).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>List of matching document chunks ordered by relevance score.</returns>
        Task<IEnumerable<DocumentChunk>> SearchAsync(string query, int size = 10, CancellationToken ct = default);

        /// <summary>
        /// Deletes the entire document collection from Elasticsearch, removing all indexed data.
        /// This operation is irreversible.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>True if the collection was successfully deleted; otherwise, false.</returns>
        Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes specific document chunks from the Elasticsearch index.
        /// Used to remove outdated chunks before re-ingesting updated documents.
        /// </summary>
        /// <param name="chunks">The list of document chunks to delete.</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        Task DeleteExistingChunks(List<DocumentChunk> chunks, CancellationToken ct);

        /// <summary>
        /// Deletes all document chunks associated with a specific file path from the index.
        /// Used to remove all chunks of a document before re-ingestion.
        /// </summary>
        /// <param name="filePath">The file path or identity path of the document whose chunks should be deleted.</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        Task DeleteExistingChunks(string filePath, CancellationToken ct);

        /// <summary>
        /// Retrieves metadata for all documents that have been ingested into the system.
        /// Includes information about file names, sources, timestamps, and other metadata.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>List of document repository items with metadata for all ingested documents.</returns>
        Task<List<DocumentRepoItemDto>> GetIngestedDocumentsAsync(CancellationToken cancellationToken = default);
    }
}
