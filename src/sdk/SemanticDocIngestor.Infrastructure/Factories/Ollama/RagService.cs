using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ollama;
using SemanticDocIngestor.Domain.Abstractions.Factories;
using SemanticDocIngestor.Domain.Abstractions.Services;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using SemanticDocIngestor.Infrastructure.Configurations;

namespace SemanticDocIngestor.Infrastructure.Factories.Ollama
{
    public class RagService(IOptions<AppSettings> options,
                            IOllamaServiceFactory ollamaServiceFactory) : IRagService
    {
        private readonly AppSettings _settings = options.Value;
        private readonly IOllamaServiceFactory _ollamaServiceFactory = ollamaServiceFactory;

        public async Task<string> GetAnswerAsync(string question, List<DocumentChunk> contextChunks, CancellationToken cancellationToken)
        {
            var request = new GenerateChatCompletionRequest()
            {
                Model = _settings.Ollama.ChatModel,
                Messages =
                [
                    new ()
                    {
                        Role = MessageRole.System,
                        Content = RagPromptBuilder.Build(contextChunks, question)
                    }
                ],
            };

            return await _ollamaServiceFactory.GetChatCompletionAsync(request, ct: cancellationToken);
        }

        public async Task<List<string>> GetDocumentChunksAsync(string content, CancellationToken cancellationToken)
        {
            var request = new GenerateChatCompletionRequest()
            {
                Model = _settings.Ollama.ChatModel,
                Messages =
                [
                    new ()
                    {
                        Role = MessageRole.System,
                        Content = RagPromptBuilder.BuildChunkingPrompt(content)
                    }
                ],
            };

            var response = await _ollamaServiceFactory.GetChatCompletionAsync(request, ct: cancellationToken);
            if (string.IsNullOrWhiteSpace(response))
            {
                return [];
            }

            response = CleanContent(response);

            // Parse the response into a list of document chunks
            var chunks = JsonConvert.DeserializeObject<List<string>>(response);
            if (chunks == null || chunks.Count == 0)
            {
                return [];
            }

            return chunks;
        }

        private static string CleanContent(string response)
        {
            // Remove any code fences or unwanted characters
            response = response.StartsWith("```json") ? response["```json".Length..].Trim() : response;
            response = response.EndsWith("```") ? response[..^3].Trim() : response;

            return response;
        }
    }
}
