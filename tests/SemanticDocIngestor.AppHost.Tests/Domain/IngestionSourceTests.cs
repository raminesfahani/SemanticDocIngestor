using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Tests.Domain;

/// <summary>
/// Unit tests for IngestionSource enum.
/// </summary>
public class IngestionSourceTests
{
    [Fact]
    public void IngestionSource_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)IngestionSource.Local);
        Assert.Equal(1, (int)IngestionSource.GoogleDrive);
        Assert.Equal(2, (int)IngestionSource.SharePoint);
        Assert.Equal(3, (int)IngestionSource.OneDrive);
        Assert.Equal(4, (int)IngestionSource.Dropbox);
    }

    [Theory]
    [InlineData(IngestionSource.Local, "Local")]
    [InlineData(IngestionSource.OneDrive, "OneDrive")]
    [InlineData(IngestionSource.GoogleDrive, "GoogleDrive")]
    [InlineData(IngestionSource.SharePoint, "SharePoint")]
    [InlineData(IngestionSource.Dropbox, "Dropbox")]
    public void IngestionSource_ToString_ReturnsCorrectName(IngestionSource source, string expectedName)
    {
        // Act
        var name = source.ToString();

        // Assert
        Assert.Equal(expectedName, name);
    }

    [Fact]
    public void IngestionSource_CanBeParsedFromString()
    {
        // Act
        var local = Enum.Parse<IngestionSource>("Local");
        var oneDrive = Enum.Parse<IngestionSource>("OneDrive");
        var googleDrive = Enum.Parse<IngestionSource>("GoogleDrive");
        var sharePoint = Enum.Parse<IngestionSource>("SharePoint");
        var dropbox = Enum.Parse<IngestionSource>("Dropbox");

        // Assert
        Assert.Equal(IngestionSource.Local, local);
        Assert.Equal(IngestionSource.OneDrive, oneDrive);
        Assert.Equal(IngestionSource.GoogleDrive, googleDrive);
        Assert.Equal(IngestionSource.SharePoint, sharePoint);
        Assert.Equal(IngestionSource.Dropbox, dropbox);
    }

    [Fact]
    public void IngestionSource_AllValuesAreDefined()
    {
        // Arrange
        var allValues = Enum.GetValues<IngestionSource>();

        // Assert
        Assert.Equal(5, allValues.Length);
        Assert.Contains(IngestionSource.Local, allValues);
        Assert.Contains(IngestionSource.OneDrive, allValues);
        Assert.Contains(IngestionSource.GoogleDrive, allValues);
        Assert.Contains(IngestionSource.SharePoint, allValues);
        Assert.Contains(IngestionSource.Dropbox, allValues);
    }

    [Theory]
    [InlineData(IngestionSource.Local)]
    [InlineData(IngestionSource.OneDrive)]
    [InlineData(IngestionSource.GoogleDrive)]
    [InlineData(IngestionSource.SharePoint)]
    [InlineData(IngestionSource.Dropbox)]
    public void IngestionSource_IsDefined(IngestionSource source)
    {
        // Act
        var isDefined = Enum.IsDefined(typeof(IngestionSource), source);

        // Assert
        Assert.True(isDefined);
    }

    [Fact]
    public void IngestionSource_CanBeUsedInSwitch()
    {
        // Arrange
        var sources = new[]
        {
            IngestionSource.Local,
            IngestionSource.OneDrive,
            IngestionSource.GoogleDrive,
            IngestionSource.SharePoint,
            IngestionSource.Dropbox
        };

        // Act & Assert
        foreach (var source in sources)
        {
            var description = source switch
            {
                IngestionSource.Local => "File system",
                IngestionSource.OneDrive => "Microsoft OneDrive",
                IngestionSource.GoogleDrive => "Google Drive",
                IngestionSource.SharePoint => "Microsoft SharePoint",
                IngestionSource.Dropbox => "Dropbox cloud storage",
                _ => "Unknown"
            };

            Assert.NotEqual("Unknown", description);
        }
    }
}
