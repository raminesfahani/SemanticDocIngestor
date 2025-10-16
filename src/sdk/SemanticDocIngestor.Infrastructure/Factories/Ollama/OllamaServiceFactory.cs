using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ollama;
using SemanticDocIngestor.Domain.Abstractions.Factories;
using SemanticDocIngestor.Infrastructure.Configurations;
using System.Runtime.CompilerServices;

namespace SemanticDocIngestor.Infrastructure.Factories.Ollama
{
    /// <inheritdoc />
    public class OllamaServiceFactory(
        IOptions<AppSettings> options,
        IOllamaApiClient client,
        ILogger<OllamaServiceFactory> logger,
        HybridCache cache
    ) : IOllamaServiceFactory
    {
        private readonly AppSettings _appSettings = options.Value;
        private readonly IOllamaApiClient _client = client;
        private readonly ILogger<OllamaServiceFactory> _logger = logger;
        private readonly HybridCache _cache = cache;

        /// <inheritdoc />
        public IOllamaApiClient Client => _client;

        /// <inheritdoc />
        public async Task<List<float>> GetEmbeddingAsync(string input, CancellationToken ct = default)
        {
            var cacheKey = $"embedding:{input.GetHashCode()}";
            return await _cache.GetOrCreateAsync(
                cacheKey,
                async cancellationToken =>
                {
                    var embedding = await _client.Embeddings.GenerateEmbeddingAsync(new GenerateEmbeddingRequest()
                    {
                        Model = _appSettings.Ollama.EmbeddingModel,
                        Prompt = input,
                    }, ct) ?? throw new InvalidOperationException("Failed to retrieve embedding from Ollama API.");

                    if (embedding.Embedding == null)
                        throw new InvalidOperationException("Received null embedding from Ollama API.");

                    var floatEmbedding = embedding.Embedding.Select(d => (float)d).ToList();
                    return floatEmbedding;
                },
                new HybridCacheEntryOptions() { Expiration = TimeSpan.FromDays(1) },
                cancellationToken: ct
            );
        }

        /// <inheritdoc />
        public async Task EnsureModelIsPulledAsync(string model, CancellationToken ct = default)
        {
            var models = await _client.Models.ListModelsAsync(ct) ?? throw new InvalidOperationException("Failed to retrieve models from Ollama API.");

            if (models.Models != null && models.Models.Any(x => !string.IsNullOrEmpty(x.Model1)
                && x.Model1.Equals(model, StringComparison.OrdinalIgnoreCase)))
                return;

            _logger.LogInformation("Pulling Ollama model '{Model}'...", model);
            await _client.Models.PullModelAsync(new PullModelRequest()
            {
                Model = model
            }, ct)
            .EnsureSuccessAsync();

            _logger.LogInformation("Model '{Model}' pulled successfully.", model);
        }

        /// <inheritdoc />
        public async Task<string> GetChatCompletionAsync(GenerateChatCompletionRequest request, CancellationToken ct = default)
        {
            var response = "";
            await foreach (var item in _client.Chat.GenerateChatCompletionAsync(request, ct).WithCancellation(ct))
            {
                response += item?.Message.Content;
            }
            return response;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<GenerateChatCompletionResponse> GetStreamChatCompletionAsync(GenerateChatCompletionRequest request, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var stream = _client.Chat.GenerateChatCompletionAsync(request, ct);

            await foreach (var item in stream.WithCancellation(ct))
            {
                if (item == null) continue;

                yield return item;
            }
        }
    }
}
