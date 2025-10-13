using SemanticDocIngestor.Domain.Options;

namespace SemanticDocIngestor.Domain.Abstractions.Settings
{
    public interface IAppSettings
    {
        public OllamaOptions Ollama { get; set; }
    }
}
