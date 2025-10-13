namespace SemanticDocIngestor.Domain.Abstractions.Factories;

public interface ICloudFileResolver
{
    bool CanResolve(string input);
    Task<ResolvedCloudFile> ResolveAsync(string input, CancellationToken ct = default);
}
