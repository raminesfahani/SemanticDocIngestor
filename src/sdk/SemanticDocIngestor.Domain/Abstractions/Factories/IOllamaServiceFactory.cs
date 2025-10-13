using Ollama;

namespace SemanticDocIngestor.Domain.Abstractions.Factories
{
    /// <summary>
    /// Defines a factory interface for managing interactions with the Ollama AI client,
    /// including model handling, conversation persistence, and streaming chat completions.
    /// </summary>
    public interface IOllamaServiceFactory
    {
        Task<string> GetChatCompletionAsync(GenerateChatCompletionRequest request, CancellationToken ct = default);
        Task<List<float>> GetEmbeddingAsync(string input, CancellationToken ct = default);
        IAsyncEnumerable<GenerateChatCompletionResponse> GetStreamChatCompletionAsync(GenerateChatCompletionRequest request, CancellationToken ct = default);
    }
}
