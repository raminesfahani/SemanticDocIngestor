using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SemanticDocIngestor.Core;
using SemanticDocIngestor.Domain.Abstractions.Services;

namespace SemanticDocIngestor.Tests.Core;

/// <summary>
/// Unit tests for ServiceCollectionExtensions ensuring proper dependency injection configuration.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSemanticDocIngestorCore_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddSemanticDocIngestorCore(configuration);

        // Assert - Verify services are registered (without building provider which requires all dependencies)
        var documentIngestorDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDocumentIngestorService));
        Assert.NotNull(documentIngestorDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, documentIngestorDescriptor.Lifetime);
    }

    [Fact]
    public void AddSemanticDocIngestorCore_RegistersDocumentIngestorServiceAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddSemanticDocIngestorCore(configuration);

        // Assert - Check service descriptor
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDocumentIngestorService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void RegisterMappingProfiles_RegistersAutoMapper()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.RegisterMappingProfiles();

        // Assert - Check AutoMapper is registered
        var mapperDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(AutoMapper.IMapper));
        Assert.NotNull(mapperDescriptor);
    }

    [Fact]
    public void AddSemanticDocIngestorCore_WithNullConfiguration_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Expecting NullReferenceException or ArgumentNullException
        Assert.ThrowsAny<Exception>(() => services.AddSemanticDocIngestorCore(null!));
    }

    [Fact]
    public void AddSemanticDocIngestorCore_RegistersAutoMapperServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddSemanticDocIngestorCore(configuration);

        // Assert
        var mapperDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(AutoMapper.IMapper));
        Assert.NotNull(mapperDescriptor);
    }

    private static IConfiguration CreateTestConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
      {"ConnectionStrings:elasticsearch", "http://localhost:9200"},
{"ConnectionStrings:qdrant", "Endpoint=http://localhost:6333;Key="},
          {"ConnectionStrings:ollama", "http://localhost:11434"},
       {"AppSettings:Ollama:ChatModel", "llama3.2"},
   {"AppSettings:Ollama:EmbeddingModel", "nomic-embed-text"},
  {"AppSettings:Ollama:Temperature", "0.7"},
            {"AppSettings:Ollama:MaxTokens", "2048"},
            {"AppSettings:Qdrant:CollectionName", "documents"},
  {"AppSettings:Qdrant:VectorSize", "768"},
            {"AppSettings:Qdrant:Distance", "Cosine"},
       {"AppSettings:Elastic:SemanticDocIndexName", "semantic_docs"},
      {"AppSettings:Elastic:DocRepoIndexName", "docs_repo"},
 {"ResiliencyMiddlewareOptions:RetryCount", "3"},
            {"ResiliencyMiddlewareOptions:TimeoutSeconds", "30"},
         {"ResiliencyMiddlewareOptions:ExceptionsAllowedBeforeCircuitBreaking", "5"},
            {"ResiliencyMiddlewareOptions:CircuitBreakingDurationSeconds", "60"},
    // Azure AD configuration for GraphServiceClient
 {"AzureAd:TenantId", "test-tenant-id"},
      {"AzureAd:ClientId", "test-client-id"},
        {"AzureAd:ClientSecret", "test-client-secret"},
// Google configuration
          {"Google:ApplicationName", "SemanticDocIngestor"}
  };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
 .Build();
    }
}
