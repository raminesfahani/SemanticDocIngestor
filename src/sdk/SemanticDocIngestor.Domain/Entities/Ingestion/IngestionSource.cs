namespace SemanticDocIngestor.Domain.Entities.Ingestion
{
    /// <summary>
    /// Enumeration of supported document ingestion sources.
    /// Indicates where a document originated from during the ingestion process.
    /// </summary>
    public enum IngestionSource
    {
        /// <summary>
        /// Document from the local file system.
        /// </summary>
        Local,
        
        /// <summary>
        /// Document from Google Drive cloud storage.
        /// </summary>
        GoogleDrive,
        
        /// <summary>
        /// Document from Microsoft SharePoint.
        /// </summary>
        SharePoint,
    
        /// <summary>
        /// Document from Microsoft OneDrive cloud storage.
        /// </summary>
        OneDrive,
        
        /// <summary>
        /// Document from Dropbox cloud storage.
        /// </summary>
        Dropbox
    }
}
