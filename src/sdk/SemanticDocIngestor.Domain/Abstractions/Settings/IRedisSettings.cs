namespace SemanticDocIngestor.Domain.Abstractions.Settings
{
    public static partial class ServiceRegistration
    {
        public interface IRedisSettings
        {
            public string ConnectionString { get; set; }
        }
    }
}
