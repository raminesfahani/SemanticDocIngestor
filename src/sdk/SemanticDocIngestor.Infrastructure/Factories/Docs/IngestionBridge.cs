using SemanticDocIngestor.Domain.Abstractions.Services;

namespace SemanticDocIngestor.Infrastructure.Factories.Docs
{
    public sealed class IngestionBridge(UserCloudFileService cloud, IDocumentIngestorService ingestor)
    {
        private readonly UserCloudFileService _cloud = cloud;
        private readonly IDocumentIngestorService _ingestor = ingestor;

        public async Task IngestOneDriveAsync(string driveId, string itemId, CancellationToken ct = default)
        {
            await using var src = await _cloud.DownloadOneDriveAsync(driveId, itemId, ct);
            var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bin");
            await using (var fs = File.Create(temp)) await src.CopyToAsync(fs, ct);

            // Identity URI ensures dedupe in ES/Vector stores
            await _ingestor.IngestDocumentsAsync([$"onedrive://{driveId}/{itemId}"], cancellationToken: ct);
        }

        public async Task IngestGoogleAsync(string fileId, CancellationToken ct = default)
        {
            await using var src = await _cloud.DownloadGoogleDriveAsync(fileId, ct);
            var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bin");
            await using (var fs = File.Create(temp)) await src.CopyToAsync(fs, ct);

            await _ingestor.IngestDocumentsAsync([$"gdrive://{fileId}"], cancellationToken: ct);
        }
    }
}