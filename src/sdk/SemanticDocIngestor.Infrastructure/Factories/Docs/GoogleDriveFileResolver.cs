using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using SemanticDocIngestor.Domain.Abstractions.Factories;
using SemanticDocIngestor.Domain.Abstractions.Services;
using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Infrastructure.Factories.Docs;

public sealed class GoogleDriveFileResolver(DriveService drive) : ICloudFileResolver
{
    private readonly DriveService _drive = drive;

    public bool CanResolve(string input) =>
        input.StartsWith("gdrive://", StringComparison.OrdinalIgnoreCase) ||
        input.Contains("drive.google.com", StringComparison.OrdinalIgnoreCase);

    public async Task<ResolvedCloudFile> ResolveAsync(string input, CancellationToken ct = default)
    {
        var fileId = ExtractFileId(input);
        if (string.IsNullOrWhiteSpace(fileId))
            throw new ArgumentException("Could not extract Google Drive file id.");

        var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bin");
        await using (var fs = File.Create(temp))
        {
            var request = _drive.Files.Get(fileId);
            var stream = new MemoryStream();
            var progress = await request.DownloadAsync(stream, ct);
            if (progress.Status != DownloadStatus.Completed)
                throw new InvalidOperationException($"Download failed: {progress.Status}");

            stream.Position = 0;
            await stream.CopyToAsync(fs, ct);
        }

        var identity = $"gdrive://{fileId}";
        return new ResolvedCloudFile(temp, identity, IngestionSource.GoogleDrive);
    }

    private static string? ExtractFileId(string input)
    {
        if (input.StartsWith("gdrive://", StringComparison.OrdinalIgnoreCase))
            return input["gdrive://".Length..];

        const string marker = "/file/d/";
        var idx = input.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var start = idx + marker.Length;
            var end = input.IndexOf('/', start);
            return end > start ? input[start..end] : input[start..];
        }
        return null;
    }
}