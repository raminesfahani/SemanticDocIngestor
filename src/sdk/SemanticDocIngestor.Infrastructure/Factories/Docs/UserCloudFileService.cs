using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace SemanticDocIngestor.Infrastructure.Factories.Docs
{
    public sealed class UserCloudFileService(GraphServiceClient graph, IHttpContextAccessor http)
    {
        private readonly GraphServiceClient _graph = graph;
        private readonly IHttpContextAccessor _http = http;

        // OneDrive list files
        public async Task<IReadOnlyList<DriveItem>> ListOneDriveAsync(CancellationToken ct = default)
        {
            var res = await _graph.Me.Drive.GetAsync(cancellationToken: ct);
            return res?.Items ?? [];
        }

        // OneDrive download
        public async Task<Stream> DownloadOneDriveAsync(string driveId, string itemId, CancellationToken ct = default)
        {
            var stream = await _graph.Drives[driveId].Items[itemId].Content.GetAsync(cancellationToken: ct);
            return stream ?? throw new InvalidOperationException("OneDrive download returned null stream.");
        }

        // Google Drive list files (per-user token)
        public async Task<IList<Google.Apis.Drive.v3.Data.File>> ListGoogleDriveAsync(CancellationToken ct = default)
        {
            var svc = await CreateGoogleDriveServiceAsync(ct);
            var req = svc.Files.List();
            req.Fields = "files(id,name,mimeType,modifiedTime,size)";
            var res = await req.ExecuteAsync(ct);
            return res.Files ?? [];
        }

        // Google Drive download
        public async Task<Stream> DownloadGoogleDriveAsync(string fileId, CancellationToken ct = default)
        {
            var svc = await CreateGoogleDriveServiceAsync(ct);
            var ms = new MemoryStream();
            var prog = await svc.Files.Get(fileId).DownloadAsync(ms, ct);
            if (prog.Status != Google.Apis.Download.DownloadStatus.Completed)
                throw new InvalidOperationException($"Google Drive download failed: {prog.Status}");
            ms.Position = 0;
            return ms;
        }

        private async Task<DriveService> CreateGoogleDriveServiceAsync(CancellationToken ct)
        {
            var http = _http.HttpContext ?? throw new InvalidOperationException("No HttpContext.");
            var accessToken = await http.GetTokenAsync("access_token"); // from Google auth ticket
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new InvalidOperationException("Google access token not available. Sign in with Google.");

            var credential = GoogleCredential.FromAccessToken(accessToken)
                .CreateScoped(DriveService.Scope.DriveReadonly);

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "SemanticDocIngestor"
            });
        }
    }
}