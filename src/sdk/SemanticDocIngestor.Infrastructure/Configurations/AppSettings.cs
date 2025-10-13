using SemanticDocIngestor.Domain.Abstractions.Settings;
using SemanticDocIngestor.Domain.Options;

namespace SemanticDocIngestor.Infrastructure.Configurations
{
    public class AppSettings : IAppSettings
    {
        public OllamaOptions Ollama { get; set; } = new();
        public QdrantOptions Qdrant { get; set; } = new();
        public ElasticOptions Elastic { get; set; } = new();
    }
}
