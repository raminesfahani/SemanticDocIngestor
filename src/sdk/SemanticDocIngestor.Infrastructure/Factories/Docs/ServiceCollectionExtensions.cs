using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Azure.Identity;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using SemanticDocIngestor.Domain.Abstractions.Factories;
using SemanticDocIngestor.Domain.Abstractions.Services;

namespace SemanticDocIngestor.Infrastructure.Factories.Docs
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterDocumentProcessorFactory(this IServiceCollection services)
        {
            services.AddSingleton<IDocumentProcessor, DocumentProcessor>();

            // OneDrive / Graph authentication using client credentials
            services.AddSingleton<GraphServiceClient>(sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var tenantId = cfg["AzureAd:TenantId"] ?? throw new InvalidOperationException("AzureAd:TenantId is not configured");
                var clientId = cfg["AzureAd:ClientId"] ?? throw new InvalidOperationException("AzureAd:ClientId is not configured");
                var clientSecret = cfg["AzureAd:ClientSecret"] ?? throw new InvalidOperationException("AzureAd:ClientSecret is not configured");
                var scopes = new[] { "https://graph.microsoft.com/.default" };

                var cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
                return new GraphServiceClient(cred, scopes);
            });

            // Google Drive authentication using service account or OAuth client credentials
            services.AddSingleton<DriveService>(sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var json = cfg["Google:CredentialsJson"];
                var appName = cfg["Google:ApplicationName"] ?? "SemanticDocIngestor";

                if (string.IsNullOrWhiteSpace(json))
                    throw new InvalidOperationException("Google:CredentialsJson is not configured");

                GoogleCredential credential = GoogleCredential.FromJson(json)
                    .CreateScoped(DriveService.Scope.DriveReadonly);

                return new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = appName
                });
            });

            services.AddSingleton<ICloudFileResolver, OneDriveFileResolver>();
            services.AddSingleton<ICloudFileResolver, GoogleDriveFileResolver>();

            return services;
        }
    }
}
