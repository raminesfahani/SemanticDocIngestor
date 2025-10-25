using Ollama;
using SemanticDocIngestor.Domain.DTOs;
using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Domain.Abstractions.Services
{
    /// <summary>
    /// Service interface for retrieval-augmented generation (RAG) operations using Ollama LLM models.
    /// Provides methods for generating answers based on document context and streaming responses.
    /// </summary>
    public interface IRagService
    {
        /// <summary>
        /// Generates an AI-powered answer to a question using the provided document chunks as context.
        /// Uses Ollama chat completion with the configured model and temperature settings.
        /// </summary>
        /// <param name="question">The user's question or query.</param>
        /// <param name="contextChunks">List of relevant document chunks to use as context for answering.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>Generated answer as a string.</returns>
        Task<string> GetAnswerAsync(string question, List<DocumentChunkDto> contextChunks, CancellationToken cancellationToken);

        /// <summary>
        /// Splits a document's content into semantic chunks suitable for vector storage and retrieval.
        /// Uses intelligent chunking strategies to preserve context and meaning.
        /// </summary>
        /// <param name="content">The full document content to chunk.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>List of text chunks.</returns>
        Task<List<string>> GetDocumentChunksAsync(string content, CancellationToken cancellationToken);

        /// <summary>
        /// Generates an AI-powered answer with streaming response, allowing real-time display
        /// as the answer is being generated token by token from the LLM.
        /// </summary>
        /// <param name="question">The user's question or query.</param>
        /// <param name="contextChunks">List of relevant document chunks to use as context for answering.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>Async enumerable stream of chat completion responses.</returns>
        IAsyncEnumerable<GenerateChatCompletionResponse> GetStreamingAnswer(string question, List<DocumentChunkDto> contextChunks, CancellationToken cancellationToken);
    }
}