namespace SemanticDocIngestor.Domain.Entities.Ingestion
{
    public class IngestionProgress
    {
        public string FilePath { get; set; } = string.Empty;
        public int Completed { get; set; } = 0;
        public int Total { get; set; } = 0;
    }
}
