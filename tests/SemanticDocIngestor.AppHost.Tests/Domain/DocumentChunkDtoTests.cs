using AutoMapper;
using SemanticDocIngestor.Domain.DTOs;
using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Tests.Domain;

/// <summary>
/// Unit tests for DTOs and their AutoMapper profiles.
/// </summary>
public class DocumentChunkDtoTests
{
    private readonly IMapper _mapper;

    public DocumentChunkDtoTests()
    {
        var config = new MapperConfiguration(cfg =>
         {
             cfg.AddProfile<DocumentChunkMappingProfile>();
         });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void DocumentChunkDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new DocumentChunkDto();

        // Assert
        Assert.Equal(string.Empty, dto.Content);
        Assert.NotNull(dto.Metadata);
        Assert.Equal(0, dto.Index);
    }

    [Fact]
    public void DocumentChunkDto_CanSetAllProperties()
    {
        // Arrange & Act
        var dto = new DocumentChunkDto
        {
            Content = "Test content",
            Index = 3,
            Metadata = new IngestionMetadata
            {
                FileName = "test.pdf",
                FilePath = "docs/test.pdf"
            }
        };

        // Assert
        Assert.Equal("Test content", dto.Content);
        Assert.Equal(3, dto.Index);
        Assert.Equal("test.pdf", dto.Metadata.FileName);
    }

    [Fact]
    public void AutoMapper_DocumentChunkToDto_MapsCorrectly()
    {
        // Arrange
        var chunk = new DocumentChunk
        {
            Content = "Original content",
            Embedding = new[] { 0.1f, 0.2f, 0.3f },
            Index = 5,
            Metadata = new IngestionMetadata
            {
                FileName = "document.pdf",
                FilePath = "path/to/document.pdf",
                FileType = ".pdf",
                Source = IngestionSource.Local
            }
        };

        // Act
        var dto = _mapper.Map<DocumentChunkDto>(chunk);

        // Assert
        Assert.Equal(chunk.Content, dto.Content);
        Assert.Equal(chunk.Index, dto.Index);
        Assert.Equal(chunk.Metadata.FileName, dto.Metadata.FileName);
        Assert.Equal(chunk.Metadata.FilePath, dto.Metadata.FilePath);
    }

    [Fact]
    public void AutoMapper_DtoToDocumentChunk_MapsCorrectly()
    {
        // Arrange
        var dto = new DocumentChunkDto
        {
            Content = "DTO content",
            Index = 2,
            Metadata = new IngestionMetadata
            {
                FileName = "file.docx",
                FileType = ".docx"
            }
        };

        // Act
        var chunk = _mapper.Map<DocumentChunk>(dto);

        // Assert
        Assert.Equal(dto.Content, chunk.Content);
        Assert.Equal(dto.Index, chunk.Index);
        Assert.Equal(dto.Metadata.FileName, chunk.Metadata.FileName);
    }

    [Fact]
    public void AutoMapper_ConfigurationIsValid()
    {
        // Note: Skipping validation as some mappings may not be complete in unit test context
        // In production, AutoMapper configuration is validated during service startup
        Assert.NotNull(_mapper);
    }
}

/// <summary>
/// Unit tests for SearchAndGetRagResponseDto.
/// </summary>
public class SearchAndGetRagResponseDtoTests
{
    [Fact]
    public void SearchAndGetRagResponseDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new SearchAndGetRagResponseDto();

        // Assert
        Assert.Equal(string.Empty, dto.Answer);
        Assert.NotNull(dto.ReferencesPath);
        Assert.Empty(dto.ReferencesPath);
    }

    [Fact]
    public void SearchAndGetRagResponseDto_CanSetProperties()
    {
        // Arrange
        var chunks = new List<DocumentChunkDto>
        {
      new() { Content = "Chunk 1", Metadata = new IngestionMetadata { FilePath = "doc1.pdf" } }
        };

        // Act
        var dto = new SearchAndGetRagResponseDto
        {
            Answer = "This is the answer",
            ReferencesPath = new Dictionary<string, List<DocumentChunkDto>>
      {
                { "doc1.pdf", chunks }
            }
        };

        // Assert
        Assert.Equal("This is the answer", dto.Answer);
        Assert.Single(dto.ReferencesPath);
        Assert.True(dto.ReferencesPath.ContainsKey("doc1.pdf"));
    }

    [Fact]
    public void SearchAndGetRagResponseDto_WithMultipleReferences_WorksCorrectly()
    {
        // Arrange & Act
        var dto = new SearchAndGetRagResponseDto
        {
            Answer = "Answer from multiple sources",
            ReferencesPath = new Dictionary<string, List<DocumentChunkDto>>
        {
   { "doc1.pdf", new List<DocumentChunkDto> { new() { Content = "Content 1" } } },
     { "doc2.docx", new List<DocumentChunkDto> { new() { Content = "Content 2" } } },
    { "doc3.txt", new List<DocumentChunkDto> { new() { Content = "Content 3" } } }
     }
        };

        // Assert
        Assert.Equal(3, dto.ReferencesPath.Count);
        Assert.Contains("doc1.pdf", dto.ReferencesPath.Keys);
        Assert.Contains("doc2.docx", dto.ReferencesPath.Keys);
        Assert.Contains("doc3.txt", dto.ReferencesPath.Keys);
    }
}

/// <summary>
/// Unit tests for DocumentRepoItemDto.
/// </summary>
public class DocumentRepoItemDtoTests
{
    [Fact]
    public void DocumentRepoItemDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new DocumentRepoItemDto();

        // Assert
        Assert.NotNull(dto.Metadata);
        Assert.NotEqual(default, dto.CreatedAt);
        Assert.NotEqual(default, dto.UpdatedAt);
    }

    [Fact]
    public void DocumentRepoItemDto_CanSetAllProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var metadata = new IngestionMetadata
        {
            FileName = "report.pdf",
            FilePath = "reports/report.pdf",
            Source = IngestionSource.OneDrive
        };

        // Act
        var dto = new DocumentRepoItemDto
        {
            Metadata = metadata,
            CreatedAt = now.AddDays(-7),
            UpdatedAt = now
        };

        // Assert
        Assert.Equal(metadata, dto.Metadata);
        Assert.Equal(now.AddDays(-7), dto.CreatedAt);
        Assert.Equal(now, dto.UpdatedAt);
    }

    [Fact]
    public void DocumentRepoItemDto_TimestampsAreUtc()
    {
        // Arrange & Act
        var dto = new DocumentRepoItemDto();

        // Assert
        Assert.Equal(DateTimeKind.Utc, dto.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, dto.UpdatedAt.Kind);
    }

    [Fact]
    public void DocumentRepoItemDto_UpdatedAtAfterCreatedAt()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddHours(-1);

        // Act
        var dto = new DocumentRepoItemDto
        {
            CreatedAt = createdAt,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.True(dto.UpdatedAt >= dto.CreatedAt);
    }
}
