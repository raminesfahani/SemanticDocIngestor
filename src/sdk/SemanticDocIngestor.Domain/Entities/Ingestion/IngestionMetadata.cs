namespace SemanticDocIngestor.Domain.Entities.Ingestion
{
    public class IngestionMetadata
    {
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string? FilePath { get; set; } = string.Empty;
        public string? PageNumber { get; set; }
        public string? SectionTitle { get; set; }
        public string? SheetName { get; set; }
        public long? RowIndex { get; set; }
        public IngestionSource Source { get; set; } = IngestionSource.Local;
    }
}
