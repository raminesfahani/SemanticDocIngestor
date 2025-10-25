using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Tests.Domain;

/// <summary>
/// Unit tests for IngestionProgress entity.
/// </summary>
public class IngestionProgressTests
{
    [Fact]
    public void IngestionProgress_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var progress = new IngestionProgress();

        // Assert
        Assert.Equal(string.Empty, progress.FilePath);
        Assert.Equal(0, progress.Completed);
        Assert.Equal(0, progress.Total);
    }

    [Fact]
    public void IngestionProgress_CanSetProperties()
    {
        // Arrange
        var progress = new IngestionProgress();

        // Act
        progress.FilePath = "test.pdf";
        progress.Completed = 5;
        progress.Total = 10;

        // Assert
        Assert.Equal("test.pdf", progress.FilePath);
        Assert.Equal(5, progress.Completed);
        Assert.Equal(10, progress.Total);
    }

    [Fact]
    public void IngestionProgress_CanBeInitializedWithValues()
    {
        // Arrange & Act
        var progress = new IngestionProgress
        {
            FilePath = "document.docx",
            Completed = 3,
            Total = 7
        };

        // Assert
        Assert.Equal("document.docx", progress.FilePath);
        Assert.Equal(3, progress.Completed);
        Assert.Equal(7, progress.Total);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(5, 10, 50)]
    [InlineData(10, 10, 100)]
    public void IngestionProgress_CalculatePercentage(int completed, int total, int expectedPercentage)
    {
        // Arrange
        var progress = new IngestionProgress
        {
            Completed = completed,
            Total = total
        };

        // Act
        var percentage = total > 0 ? (completed * 100) / total : 0;

        // Assert
        Assert.Equal(expectedPercentage, percentage);
    }
}
