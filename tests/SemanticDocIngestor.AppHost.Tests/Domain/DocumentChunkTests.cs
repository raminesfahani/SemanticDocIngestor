using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Tests.Domain;

/// <summary>
/// Unit tests for DocumentChunk entity.
/// </summary>
public class DocumentChunkTests
{
    [Fact]
    public void DocumentChunk_CanSetAllProperties()
    {
        // Arrange
        var chunk = new DocumentChunk();
        var embedding = new[] { 0.1f, 0.2f, 0.3f };
        var metadata = new IngestionMetadata
        {
            FileName = "test.pdf",
            FilePath = "docs/test.pdf",
            FileType = ".pdf"
        };

        // Act
        chunk.Content = "Test content";
        chunk.Embedding = embedding;
        chunk.Index = 5;
        chunk.Metadata = metadata;

        // Assert
        Assert.Equal("Test content", chunk.Content);
        Assert.Equal(embedding, chunk.Embedding);
        Assert.Equal(5, chunk.Index);
        Assert.Equal(metadata, chunk.Metadata);
    }

    [Fact]
    public void DocumentChunk_WithEmbedding_HasCorrectDimension()
    {
        // Arrange
        var embedding = new float[768]; // Common embedding size
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] = i * 0.001f;
        }

        var chunk = new DocumentChunk
        {
            Embedding = embedding
        };

        // Act & Assert
        Assert.Equal(768, chunk.Embedding.Length);
    }

    [Fact]
    public void DocumentChunk_Metadata_ContainsSourceInformation()
    {
        // Arrange & Act
        var chunk = new DocumentChunk
        {
            Content = "Sample text",
            Metadata = new IngestionMetadata
            {
                FileName = "sample.docx",
                FilePath = "documents/sample.docx",
                FileType = ".docx",
                Source = IngestionSource.Local,
                PageNumber = "1"
            }
        };

        // Assert
        Assert.Equal("sample.docx", chunk.Metadata.FileName);
        Assert.Equal("documents/sample.docx", chunk.Metadata.FilePath);
        Assert.Equal(".docx", chunk.Metadata.FileType);
        Assert.Equal(IngestionSource.Local, chunk.Metadata.Source);
        Assert.Equal("1", chunk.Metadata.PageNumber);
    }

    [Theory]
    [InlineData(IngestionSource.Local)]
    [InlineData(IngestionSource.OneDrive)]
    [InlineData(IngestionSource.GoogleDrive)]
    [InlineData(IngestionSource.SharePoint)]
    [InlineData(IngestionSource.Dropbox)]
    public void DocumentChunk_SupportsAllIngestionSources(IngestionSource source)
    {
        // Arrange & Act
        var chunk = new DocumentChunk
        {
            Content = "Test",
            Metadata = new IngestionMetadata { Source = source }
        };

        // Assert
        Assert.Equal(source, chunk.Metadata.Source);
    }

    [Fact]
    public void DocumentChunk_WithEmptyEmbedding_IsValid()
    {
        // Arrange & Act
        var chunk = new DocumentChunk
        {
            Content = "Content without embedding",
            Embedding = Array.Empty<float>()
        };

        // Assert
        Assert.NotNull(chunk.Embedding);
        Assert.Empty(chunk.Embedding);
    }
}
