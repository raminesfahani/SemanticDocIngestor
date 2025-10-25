namespace SemanticDocIngestor.Domain.Abstractions.Factories;

/// <summary>
/// Interface for resolving cloud file references to local file paths.
/// Implementations handle downloading and authentication for cloud storage providers like OneDrive and Google Drive.
/// </summary>
public interface ICloudFileResolver
{
    /// <summary>
    /// Determines whether this resolver can handle the given input path or URI.
    /// </summary>
    /// <param name="input">The input path or URI to check (e.g., "onedrive://...", "gdrive://...", or cloud share links).</param>
    /// <returns>True if this resolver can handle the input; otherwise, false.</returns>
    bool CanResolve(string input);

    /// <summary>
    /// Resolves a cloud file reference to a local temporary file path.
    /// Downloads the file from the cloud storage and returns information about the local copy and its stable identity.
    /// </summary>
    /// <param name="input">The cloud file URI or share link to resolve.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>Resolved cloud file with local path, identity path, and source information.</returns>
    /// <remarks>
    /// The returned <see cref="ResolvedCloudFile"/> implements <see cref="IAsyncDisposable"/> 
    /// to clean up temporary files after processing.
    /// </remarks>
    Task<ResolvedCloudFile> ResolveAsync(string input, CancellationToken ct = default);
}
