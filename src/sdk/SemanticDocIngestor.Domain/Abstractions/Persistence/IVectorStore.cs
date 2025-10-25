using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Domain.Abstractions.Persistence
{
    /// <summary>
    /// Interface for vector database operations providing semantic search capabilities.
    /// Implementations typically use vector databases like Qdrant, Pinecone, or Weaviate.
    /// </summary>
    public interface IVectorStore
    {
        /// <summary>
        /// Deletes the entire vector collection, removing all stored document embeddings.
        /// This operation is irreversible and will delete all vector data.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>True if the collection was successfully deleted; otherwise, false.</returns>
        Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all document chunks associated with a specific file path from the vector store.
        /// Used to remove outdated chunks before re-ingesting updated documents.
        /// </summary>
        /// <param name="filePath">The file path or identity path of the document whose chunks should be deleted.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        Task DeleteExistingChunksAsync(string filePath, CancellationToken cancellationToken);

        /// <summary>
        /// Ensures the vector collection exists in the database, creating it if necessary.
        /// Should be called before any operations that require the collection to exist.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task EnsureCollectionExistsAsync();

        /// <summary>
        /// Performs semantic vector search to find the most similar document chunks to the query.
        /// Uses cosine similarity or other distance metrics to rank results.
        /// </summary>
        /// <param name="query">The search query text to convert to an embedding and search for.</param>
        /// <param name="topK">The maximum number of top results to return (default: 5).</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>List of the most semantically similar document chunks, ordered by relevance.</returns>
        Task<List<DocumentChunk>> SearchAsync(string query, ulong topK = 5, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts or updates document chunks in the vector store with their embeddings.
        /// Generates vector embeddings for each chunk's content and stores them for semantic search.
        /// </summary>
        /// <param name="chunks">The document chunks to upsert with their content and metadata.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous upsert operation.</returns>
        Task UpsertAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default);
    }
}
