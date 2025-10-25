namespace SemanticDocIngestor.Domain.Entities.Ingestion
{
    /// <summary>
    /// Represents the progress of a document ingestion operation.
    /// Provides real-time updates about the number of documents processed.
    /// </summary>
    public class IngestionProgress
    {
        /// <summary>
        /// Gets or sets the file path of the document currently being processed.
        /// Empty string if no specific file is being processed.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of documents that have been successfully processed.
        /// </summary>
        public int Completed { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of documents to be processed in this ingestion operation.
        /// </summary>
        public int Total { get; set; } = 0;
    }
}
