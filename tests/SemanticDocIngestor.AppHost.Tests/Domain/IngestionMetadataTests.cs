using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Tests.Domain;

/// <summary>
/// Unit tests for IngestionMetadata entity.
/// </summary>
public class IngestionMetadataTests
{
    [Fact]
    public void IngestionMetadata_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var metadata = new IngestionMetadata();

        // Assert
        Assert.Equal(string.Empty, metadata.FileName);
        Assert.Equal(string.Empty, metadata.FileType);
        Assert.Equal(string.Empty, metadata.FilePath); // FilePath defaults to empty string, not null
        Assert.Null(metadata.PageNumber);
        Assert.Null(metadata.SheetName);
        Assert.Null(metadata.RowIndex);
        Assert.Equal(IngestionSource.Local, metadata.Source);
    }

    [Fact]
    public void IngestionMetadata_CanSetAllProperties()
    {
        // Arrange & Act
        var metadata = new IngestionMetadata
        {
            FileName = "report.pdf",
            FileType = ".pdf",
            FilePath = "documents/reports/report.pdf",
            PageNumber = "5",
            Source = IngestionSource.OneDrive
        };

        // Assert
        Assert.Equal("report.pdf", metadata.FileName);
        Assert.Equal(".pdf", metadata.FileType);
        Assert.Equal("documents/reports/report.pdf", metadata.FilePath);
        Assert.Equal("5", metadata.PageNumber);
        Assert.Equal(IngestionSource.OneDrive, metadata.Source);
    }

    [Fact]
    public void IngestionMetadata_ForExcelDocument_ContainsSheetInfo()
    {
        // Arrange & Act
        var metadata = new IngestionMetadata
        {
            FileName = "data.xlsx",
            FileType = ".xlsx",
            FilePath = "spreadsheets/data.xlsx",
            SheetName = "Sales Report",
            RowIndex = 42,
            Source = IngestionSource.Local
        };

        // Assert
        Assert.Equal("data.xlsx", metadata.FileName);
        Assert.Equal(".xlsx", metadata.FileType);
        Assert.Equal("Sales Report", metadata.SheetName);
        Assert.Equal(42, metadata.RowIndex);
    }

    [Theory]
    [InlineData(".pdf", "1")]
    [InlineData(".docx", "3")]
    [InlineData(".txt", null)]
    public void IngestionMetadata_PageNumber_WorksCorrectly(string fileType, string? pageNumber)
    {
        // Arrange & Act
        var metadata = new IngestionMetadata
        {
            FileType = fileType,
            PageNumber = pageNumber
        };

        // Assert
        Assert.Equal(fileType, metadata.FileType);
        Assert.Equal(pageNumber, metadata.PageNumber);
    }

    [Fact]
    public void IngestionMetadata_ForCloudSource_ContainsCorrectSource()
    {
        // Arrange & Act
        var oneDriveMetadata = new IngestionMetadata
        {
            FileName = "cloud-doc.docx",
            FilePath = "onedrive://drive123/item456",
            Source = IngestionSource.OneDrive
        };

        var googleDriveMetadata = new IngestionMetadata
        {
            FileName = "gdrive-doc.pdf",
            FilePath = "gdrive://file789",
            Source = IngestionSource.GoogleDrive
        };

        // Assert
        Assert.Equal(IngestionSource.OneDrive, oneDriveMetadata.Source);
        Assert.Equal(IngestionSource.GoogleDrive, googleDriveMetadata.Source);
    }

    [Fact]
    public void IngestionMetadata_NullableProperties_CanBeNull()
    {
        // Arrange & Act
        var metadata = new IngestionMetadata
        {
            FileName = "test.txt",
            FileType = ".txt",
            FilePath = null,
            PageNumber = null,
            SheetName = null,
            RowIndex = null
        };

        // Assert
        Assert.Null(metadata.FilePath);
        Assert.Null(metadata.PageNumber);
        Assert.Null(metadata.SheetName);
        Assert.Null(metadata.RowIndex);
    }
}
