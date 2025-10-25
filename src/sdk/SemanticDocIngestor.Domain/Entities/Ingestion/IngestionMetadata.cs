namespace SemanticDocIngestor.Domain.Entities.Ingestion
{
    /// <summary>
    /// Represents metadata associated with an ingested document chunk.
    /// Provides contextual information about the source document, location within the document, and ingestion details.
    /// </summary>
    public class IngestionMetadata
    {
        /// <summary>
        /// Gets or sets the name of the source file (e.g., "report.pdf").
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the file type or extension (e.g., "pdf", "docx").
        /// </summary>
        public string FileType { get; set; } = string.Empty;
   
        /// <summary>
        /// Gets or sets the full file path or stable identity URI for the source document.
        /// For cloud sources, this is the stable URI (e.g., "onedrive://driveId/itemId", "gdrive://fileId").
        /// For local files, this is the file system path.
        /// </summary>
        public string? FilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the page number within the source document where this chunk originated.
        /// Applicable primarily to PDF and similar paginated formats.
        /// </summary>
        public string? PageNumber { get; set; }
      
        /// <summary>
        /// Gets or sets the section title or heading associated with this chunk.
        /// Useful for structured documents with named sections.
        /// </summary>
        public string? SectionTitle { get; set; }
        
        /// <summary>
        /// Gets or sets the sheet name for spreadsheet documents (e.g., Excel workbooks).
        /// Indicates which worksheet the chunk came from.
        /// </summary>
        public string? SheetName { get; set; }
        
        /// <summary>
        /// Gets or sets the row index for spreadsheet or tabular data.
        /// Zero-based index indicating the row position in the source sheet.
        /// </summary>
        public long? RowIndex { get; set; }
    
        /// <summary>
        /// Gets or sets the ingestion source indicating where the document originated.
        /// Can be Local, OneDrive, GoogleDrive, or other cloud sources.
        /// </summary>
        public IngestionSource Source { get; set; } = IngestionSource.Local;
    }
}
