namespace SemanticDocIngestor.Domain.Abstracts.Settings
{
    public interface IMongoDbSettings
    {
        string DatabaseName { get; set; }
    }
}
