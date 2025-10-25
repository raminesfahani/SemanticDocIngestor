using SemanticDocIngestor.Domain.Entities.Ingestion;
using SemanticDocIngestor.Domain.Abstractions.Factories;

namespace SemanticDocIngestor.Tests.Domain;

/// <summary>
/// Unit tests for ResolvedCloudFile record.
/// </summary>
public class ResolvedCloudFileTests
{
    [Fact]
    public void ResolvedCloudFile_CanBeCreated()
    {
        // Arrange & Act
        var resolved = new ResolvedCloudFile(
                   "C:\\Temp\\file.pdf",
         "onedrive://drive123/item456",
             IngestionSource.OneDrive
             );

        // Assert
        Assert.Equal("C:\\Temp\\file.pdf", resolved.LocalPath);
        Assert.Equal("onedrive://drive123/item456", resolved.IdentityPath);
        Assert.Equal(IngestionSource.OneDrive, resolved.Source);
    }

    [Fact]
    public void ResolvedCloudFile_ForGoogleDrive_IsValid()
    {
        // Arrange & Act
        var resolved = new ResolvedCloudFile(
         "/tmp/gdrive_file.docx",
   "gdrive://file789",
    IngestionSource.GoogleDrive
    );

        // Assert
        Assert.Equal("/tmp/gdrive_file.docx", resolved.LocalPath);
        Assert.Equal("gdrive://file789", resolved.IdentityPath);
        Assert.Equal(IngestionSource.GoogleDrive, resolved.Source);
    }

    [Fact]
    public void ResolvedCloudFile_ForLocalFile_IsValid()
    {
        // Arrange & Act
        var resolved = new ResolvedCloudFile(
        "documents/local.pdf",
    "documents/local.pdf",
  IngestionSource.Local
        );

        // Assert
        Assert.Equal("documents/local.pdf", resolved.LocalPath);
        Assert.Equal("documents/local.pdf", resolved.IdentityPath);
        Assert.Equal(IngestionSource.Local, resolved.Source);
    }

    [Fact]
    public async Task ResolvedCloudFile_DisposeAsync_CompletesSuccessfully()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.txt");
        File.WriteAllText(tempFile, "test content");

        var resolved = new ResolvedCloudFile(
           tempFile,
       "test://file",
           IngestionSource.Dropbox
              );

        // Act
        await resolved.DisposeAsync();

        // Assert - File should be deleted after disposal
        Assert.False(File.Exists(tempFile), "Temporary file should be cleaned up after disposal");
    }

    [Theory]
    [InlineData(IngestionSource.Local)]
    [InlineData(IngestionSource.OneDrive)]
    [InlineData(IngestionSource.GoogleDrive)]
    [InlineData(IngestionSource.SharePoint)]
    [InlineData(IngestionSource.Dropbox)]
    public void ResolvedCloudFile_SupportsAllSources(IngestionSource source)
    {
        // Arrange & Act
        var resolved = new ResolvedCloudFile(
      "path/to/file",
          "identity/path",
     source
      );

        // Assert
        Assert.Equal(source, resolved.Source);
    }

    [Fact]
    public void ResolvedCloudFile_LocalPathAndIdentityPath_CanBeDifferent()
    {
        // Arrange & Act
        var resolved = new ResolvedCloudFile(
               "C:\\Temp\\downloaded_file.pdf",
       "onedrive://myDrive/myFolder/original_file.pdf",
               IngestionSource.OneDrive
           );

        // Assert
        Assert.NotEqual(resolved.LocalPath, resolved.IdentityPath);
        Assert.Contains("Temp", resolved.LocalPath);
        Assert.Contains("onedrive://", resolved.IdentityPath);
    }

    [Fact]
    public void ResolvedCloudFile_IsRecord_SupportsValueEquality()
    {
        // Arrange
        var resolved1 = new ResolvedCloudFile("path1", "identity1", IngestionSource.Local);
        var resolved2 = new ResolvedCloudFile("path1", "identity1", IngestionSource.Local);
        var resolved3 = new ResolvedCloudFile("path2", "identity2", IngestionSource.Local);

        // Act & Assert
        Assert.Equal(resolved1, resolved2); // Value equality
        Assert.NotEqual(resolved1, resolved3);
    }
}
