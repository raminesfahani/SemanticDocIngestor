using System.Net;
using Microsoft.Graph;
using SemanticDocIngestor.Domain.Abstractions.Factories;
using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Infrastructure.Factories.Docs;

public sealed class OneDriveFileResolver(GraphServiceClient graph) : ICloudFileResolver
{
    private readonly GraphServiceClient _graph = graph;

    public bool CanResolve(string input) =>
        input.StartsWith("onedrive://", StringComparison.OrdinalIgnoreCase) ||
        input.Contains("1drv.ms", StringComparison.OrdinalIgnoreCase) ||
        input.Contains("sharepoint.com", StringComparison.OrdinalIgnoreCase);

    public async Task<ResolvedCloudFile> ResolveAsync(string input, CancellationToken ct = default)
    {
        string driveId, itemId;

        if (input.StartsWith("onedrive://", StringComparison.OrdinalIgnoreCase))
        {
            var parts = input["onedrive://".Length..].Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) throw new ArgumentException("Invalid onedrive URI. Expected onedrive://{driveId}/{itemId}");
            driveId = parts[0];
            itemId = parts[1];
        }
        else
        {
            var encoded = WebUtility.UrlEncode(input);
            var driveItem = await _graph.Shares[$"u!{encoded}"].DriveItem.GetAsync(cancellationToken: ct)
                           ?? throw new InvalidOperationException("Unable to resolve OneDrive share link.");
            driveId = driveItem.ParentReference?.DriveId ?? throw new InvalidOperationException("DriveId missing.");
            itemId = driveItem.Id ?? throw new InvalidOperationException("ItemId missing.");
        }

        using var stream = await _graph.Drives[driveId].Items[itemId].Content.GetAsync(cancellationToken: ct)
                          ?? throw new InvalidOperationException("Download stream is null.");

        var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bin");
        await using (var fs = File.Create(temp))
            await stream.CopyToAsync(fs, ct);

        var identity = $"onedrive://{driveId}/{itemId}";
        return new ResolvedCloudFile(temp, identity, IngestionSource.OneDrive);
    }
}
