namespace SemanticDocIngestor.Domain.Abstractions.Settings
{
    public interface IMongoDbSettings
    {
        string DatabaseName { get; set; }
    }
}
