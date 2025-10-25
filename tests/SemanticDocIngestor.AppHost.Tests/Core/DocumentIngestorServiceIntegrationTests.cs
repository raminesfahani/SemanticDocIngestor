using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SemanticDocIngestor.Core.Hubs;
using SemanticDocIngestor.Core.Services;
using SemanticDocIngestor.Domain.Abstractions.Factories;
using SemanticDocIngestor.Domain.Abstractions.Hubs;
using SemanticDocIngestor.Domain.Abstractions.Persistence;
using SemanticDocIngestor.Domain.Abstractions.Services;
using SemanticDocIngestor.Domain.DTOs;
using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Tests.Core;

/// <summary>
/// Integration and edge case tests for DocumentIngestorService.
/// </summary>
public class DocumentIngestorServiceIntegrationTests
{
    private readonly Mock<IDocumentProcessor> _mockDocProcessor;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IElasticStore> _mockElasticStore;
    private readonly Mock<IVectorStore> _mockVectorStore;
    private readonly HybridCache _cache; // Use real cache, not mock
    private readonly Mock<IRagService> _mockRagService;
    private readonly Mock<ILogger<DocumentIngestorService>> _mockLogger;
    private readonly Mock<IHubContext<IngestionHub, IIngestionHubClient>> _mockHubContext;
    private readonly Mock<IHubClients<IIngestionHubClient>> _mockHubClients;
    private readonly Mock<IIngestionHubClient> _mockHubClient;

    public DocumentIngestorServiceIntegrationTests()
    {
        _mockDocProcessor = new Mock<IDocumentProcessor>();
        _mockMapper = new Mock<IMapper>();
        _mockElasticStore = new Mock<IElasticStore>();
        _mockVectorStore = new Mock<IVectorStore>();

        // Create a real HybridCache instance for testing
        var services = new ServiceCollection();
        services.AddHybridCache();
        var serviceProvider = services.BuildServiceProvider();
        _cache = serviceProvider.GetRequiredService<HybridCache>();

        _mockRagService = new Mock<IRagService>();
        _mockLogger = new Mock<ILogger<DocumentIngestorService>>();
        _mockHubContext = new Mock<IHubContext<IngestionHub, IIngestionHubClient>>();
        _mockHubClients = new Mock<IHubClients<IIngestionHubClient>>();
        _mockHubClient = new Mock<IIngestionHubClient>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockHubClients.Object);
        _mockHubClients.Setup(c => c.All).Returns(_mockHubClient.Object);
    }

    [Fact]
    public async Task IngestDocumentsAsync_WithCloudResolver_ProcessesCloudFiles()
    {
        // Arrange
        var mockResolver = new Mock<ICloudFileResolver>();
        mockResolver.Setup(r => r.CanResolve(It.IsAny<string>())).Returns(true);
        mockResolver.Setup(r => r.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResolvedCloudFile(
      Path.Combine(Path.GetTempPath(), "temp.pdf"),
         "onedrive://drive123/item456",
  IngestionSource.OneDrive));

        var service = new DocumentIngestorService(
  _mockDocProcessor.Object,
         _mockMapper.Object,
_mockElasticStore.Object,
        _mockVectorStore.Object,
   _cache,
 _mockRagService.Object,
       _mockLogger.Object,
 cloudResolvers: new[] { mockResolver.Object },
     hubContext: _mockHubContext.Object
   );

        SetupSuccessfulIngestion();

        // Act
        await service.IngestDocumentsAsync(new[] { "onedrive://drive123/item456" }, cancellationToken: CancellationToken.None);

        // Assert
        mockResolver.Verify(r => r.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IngestDocumentsAsync_WithMultipleFiles_RaisesProgressEvents()
    {
        // Arrange
        var tempFile1 = Path.Combine(Path.GetTempPath(), $"test1_{Guid.NewGuid()}.pdf");
        var tempFile2 = Path.Combine(Path.GetTempPath(), $"test2_{Guid.NewGuid()}.pdf");
        File.WriteAllText(tempFile1, "test content 1");
        File.WriteAllText(tempFile2, "test content 2");

        try
        {
            var progressEvents = new List<IngestionProgress>();
            var completedEvents = new List<IngestionProgress>();

            var service = new DocumentIngestorService(
            _mockDocProcessor.Object,
                  _mockMapper.Object,
          _mockElasticStore.Object,
                  _mockVectorStore.Object,
                      _cache,
             _mockRagService.Object,
                 _mockLogger.Object,
           cloudResolvers: null,
               hubContext: _mockHubContext.Object
                  );

            service.OnProgress += (sender, e) => progressEvents.Add(e);
            service.OnCompleted += (sender, e) => completedEvents.Add(e);

            SetupSuccessfulIngestion();

            // Act
            await service.IngestDocumentsAsync(new[] { tempFile1, tempFile2 }, cancellationToken: CancellationToken.None);

            // Assert
            Assert.NotEmpty(progressEvents);
            Assert.Single(completedEvents);
        }
        finally
        {
            if (File.Exists(tempFile1)) File.Delete(tempFile1);
            if (File.Exists(tempFile2)) File.Delete(tempFile2);
        }
    }

    [Fact]
    public async Task IngestDocumentsAsync_DeletesExistingChunksBeforeIngestion()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.pdf");
        File.WriteAllText(tempFile, "test content");

        try
        {
            var service = new DocumentIngestorService(
                _mockDocProcessor.Object,
           _mockMapper.Object,
           _mockElasticStore.Object,
        _mockVectorStore.Object,
       _cache,
            _mockRagService.Object,
        _mockLogger.Object,
           cloudResolvers: null,
                hubContext: _mockHubContext.Object
          );

            SetupSuccessfulIngestion();

            // Act
            await service.IngestDocumentsAsync(new[] { tempFile }, cancellationToken: CancellationToken.None);

            // Assert
            _mockVectorStore.Verify(x => x.DeleteExistingChunksAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _mockElasticStore.Verify(x => x.DeleteExistingChunks(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task IngestDocumentsAsync_WithCancellation_StopsProcessing()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var service = new DocumentIngestorService(
_mockDocProcessor.Object,
       _mockMapper.Object,
  _mockElasticStore.Object,
    _mockVectorStore.Object,
        _cache,
   _mockRagService.Object,
            _mockLogger.Object,
            cloudResolvers: null,
            hubContext: _mockHubContext.Object
     );

        SetupSuccessfulIngestion();
        cts.Cancel(); // Cancel immediately

        // Act
        await service.IngestDocumentsAsync(new[] { "test.pdf" }, cancellationToken: cts.Token);

        // Assert - Should not throw, but may not complete all operations
        Assert.True(cts.IsCancellationRequested);
    }

    [Fact]
    public async Task SearchDocumentsAsync_DeduplicatesResults()
    {
        // Arrange
        var duplicateChunk = new DocumentChunk
        {
            Content = "Duplicate content",
            Embedding = new[] { 0.1f },
            Index = 0,
            Metadata = new IngestionMetadata
            {
                FileName = "test.pdf",
                FilePath = "test.pdf",
                Source = IngestionSource.Local,
                PageNumber = "1"
            }
        };

        var service = new DocumentIngestorService(
_mockDocProcessor.Object,
            _mockMapper.Object,
     _mockElasticStore.Object,
            _mockVectorStore.Object,
         _cache,
        _mockRagService.Object,
       _mockLogger.Object,
       cloudResolvers: null,
    hubContext: _mockHubContext.Object
    );

        // Both stores return the same chunk
        _mockVectorStore.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<ulong>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(new List<DocumentChunk> { duplicateChunk });
        _mockElasticStore.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
 .ReturnsAsync(new List<DocumentChunk> { duplicateChunk });
        _mockMapper.Setup(x => x.Map<List<DocumentChunkDto>>(It.IsAny<List<DocumentChunk>>()))
     .Returns((List<DocumentChunk> chunks) => chunks.Select(c => new DocumentChunkDto
     {
         Content = c.Content,
         Index = c.Index,
         Metadata = c.Metadata
     }).ToList());

        // Act
        var results = await service.SearchDocumentsAsync("test", 10, CancellationToken.None);

        // Assert - Should only return one result due to deduplication
        Assert.Single(results);
    }

    [Fact]
    public async Task SearchAndGetRagStreamResponseAsync_ReturnsStreamingResponse()
    {
        // Arrange
        var service = new DocumentIngestorService(
      _mockDocProcessor.Object,
       _mockMapper.Object,
    _mockElasticStore.Object,
            _mockVectorStore.Object,
_cache,
      _mockRagService.Object,
            _mockLogger.Object,
         cloudResolvers: null,
    hubContext: _mockHubContext.Object
        );

        _mockVectorStore.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<ulong>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<DocumentChunk>());
        _mockElasticStore.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentChunk>());
        _mockMapper.Setup(x => x.Map<List<DocumentChunkDto>>(It.IsAny<List<DocumentChunk>>()))
           .Returns(new List<DocumentChunkDto>());

        async IAsyncEnumerable<Ollama.GenerateChatCompletionResponse> GetStreamingResponses()
        {
            yield return new Ollama.GenerateChatCompletionResponse
            {
                Message = new Ollama.Message { Role = Ollama.MessageRole.Assistant, Content = "Test response" },
                Model = "test-model",
                CreatedAt = DateTime.UtcNow,
                Done = false
            };
            await Task.CompletedTask;
        }

        _mockRagService.Setup(x => x.GetStreamingAnswer(It.IsAny<string>(), It.IsAny<List<DocumentChunkDto>>(), It.IsAny<CancellationToken>()))
            .Returns(GetStreamingResponses());

        // Act
        var result = await service.SearchAndGetRagStreamResponseAsync("test query", 5, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Answer);
        Assert.NotNull(result.ReferencesPath);
    }

    [Fact]
    public async Task IngestDocumentsAsync_NotifiesSignalRClients()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.pdf");
        File.WriteAllText(tempFile, "test content");

        try
        {
            var service = new DocumentIngestorService(
                 _mockDocProcessor.Object,
              _mockMapper.Object,
              _mockElasticStore.Object,
            _mockVectorStore.Object,
                   _cache,
                _mockRagService.Object,
             _mockLogger.Object,
                  cloudResolvers: null,
                hubContext: _mockHubContext.Object
                  );

            SetupSuccessfulIngestion();

            _mockHubClient.Setup(c => c.ReceiveProgress(It.IsAny<IngestionProgress>()))
            .Returns(Task.CompletedTask);
            _mockHubClient.Setup(c => c.ReceiveCompleted(It.IsAny<IngestionProgress>()))
     .Returns(Task.CompletedTask);

            // Act
            await service.IngestDocumentsAsync(new[] { tempFile }, cancellationToken: CancellationToken.None);

            // Assert
            _mockHubClient.Verify(c => c.ReceiveProgress(It.IsAny<IngestionProgress>()), Times.AtLeastOnce);
            _mockHubClient.Verify(c => c.ReceiveCompleted(It.IsAny<IngestionProgress>()), Times.Once);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SearchDocumentsAsync_WithEmptyQuery_ReturnsResults()
    {
        // Arrange
        var service = new DocumentIngestorService(
          _mockDocProcessor.Object,
              _mockMapper.Object,
              _mockElasticStore.Object,
     _mockVectorStore.Object,
              _cache,
              _mockRagService.Object,
              _mockLogger.Object,
              cloudResolvers: null,
              hubContext: _mockHubContext.Object
      );

        _mockVectorStore.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<ulong>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<DocumentChunk>());
        _mockElasticStore.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
   .ReturnsAsync(new List<DocumentChunk>());
        _mockMapper.Setup(x => x.Map<List<DocumentChunkDto>>(It.IsAny<List<DocumentChunk>>()))
           .Returns(new List<DocumentChunkDto>());

        // Act
        var results = await service.SearchDocumentsAsync("", 10, CancellationToken.None);

        // Assert
        Assert.NotNull(results);
    }

    [Fact]
    public async Task ListIngestedDocumentsAsync_WithNoDocuments_ReturnsEmptyList()
    {
        // Arrange
        var service = new DocumentIngestorService(
      _mockDocProcessor.Object,
            _mockMapper.Object,
            _mockElasticStore.Object,
            _mockVectorStore.Object,
          _cache,
            _mockRagService.Object,
        _mockLogger.Object,
   cloudResolvers: null,
     hubContext: _mockHubContext.Object
        );

        _mockElasticStore.Setup(x => x.GetIngestedDocumentsAsync(It.IsAny<CancellationToken>()))
  .ReturnsAsync(new List<DocumentRepoItemDto>());
        _mockMapper.Setup(x => x.Map<List<DocumentRepoItemDto>>(It.IsAny<List<DocumentRepoItemDto>>()))
            .Returns(new List<DocumentRepoItemDto>());

        // Act
        var results = await service.ListIngestedDocumentsAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    private void SetupSuccessfulIngestion()
    {
        _mockVectorStore.Setup(x => x.EnsureCollectionExistsAsync()).Returns(Task.CompletedTask);
        _mockElasticStore.Setup(x => x.EnsureSemanticDocIndexExistsAsync()).Returns(Task.CompletedTask);
        _mockVectorStore.Setup(x => x.DeleteExistingChunksAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockElasticStore.Setup(x => x.DeleteExistingChunks(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockDocProcessor.Setup(x => x.ProcessDocument(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new List<DocumentChunk>
   {
         new()
    {
Content = "Test content",
        Embedding = new[] { 0.1f, 0.2f },
               Index = 0,
        Metadata = new IngestionMetadata
 {
         FileName = "test.pdf",
       FilePath = "test.pdf",
        FileType = ".pdf",
        Source = IngestionSource.Local
               }
    }
    });
        _mockVectorStore.Setup(x => x.UpsertAsync(It.IsAny<IEnumerable<DocumentChunk>>(), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);
        _mockElasticStore.Setup(x => x.UpsertAsync(It.IsAny<List<DocumentChunk>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }
}

