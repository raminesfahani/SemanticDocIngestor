using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Domain.Abstractions.Factories;

public sealed record ResolvedCloudFile(string LocalPath, string IdentityPath, IngestionSource Source) : IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        try { if (File.Exists(LocalPath)) File.Delete(LocalPath); }
        catch { /* best effort */ }
        return ValueTask.CompletedTask;
    }
}
