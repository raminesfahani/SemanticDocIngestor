using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Domain.Abstractions.Factories;

/// <summary>
/// Represents a cloud file that has been resolved to a local temporary path.
/// Implements <see cref="IAsyncDisposable"/> to automatically clean up the temporary file after processing.
/// </summary>
/// <param name="LocalPath">The local file system path where the cloud file has been downloaded.</param>
/// <param name="IdentityPath">The stable identity path or URI for the cloud file (e.g., "onedrive://driveId/itemId", "gdrive://fileId").</param>
/// <param name="Source">The ingestion source indicating which cloud provider the file came from.</param>
public sealed record ResolvedCloudFile(string LocalPath, string IdentityPath, IngestionSource Source) : IAsyncDisposable
{
    /// <summary>
    /// Disposes of the temporary local file when processing is complete.
    /// Performs a best-effort deletion without throwing exceptions.
    /// </summary>
    /// <returns>A completed ValueTask.</returns>
    public ValueTask DisposeAsync()
    {
        try { if (File.Exists(LocalPath)) File.Delete(LocalPath); }
        catch { /* best effort */ }
        return ValueTask.CompletedTask;
    }
}
