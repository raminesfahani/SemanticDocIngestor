using SemanticDocIngestor.Domain.Entities.Ingestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Domain.Abstractions.Factories
{
    /// <summary>
    /// Interface for document processing and chunking operations.
    /// Implementations handle parsing of various document formats and splitting content into manageable chunks.
    /// </summary>
    public interface IDocumentProcessor
    {
        /// <summary>
        /// Gets the list of file extensions supported by this document processor.
        /// Extensions should include the dot (e.g., ".pdf", ".docx").
        /// </summary>
        List<string> SupportedFileExtensions { get; }

        /// <summary>
        /// Processes a document file, extracting text content and splitting it into chunks with metadata.
        /// Each chunk includes information about its source, page number, section, and other relevant metadata.
        /// </summary>
        /// <param name="filePath">Full path to the document file to process.</param>
        /// <param name="maxChunkSize">Maximum size of each text chunk in tokens (default: 500).</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>List of document chunks with extracted content and metadata.</returns>
        /// <exception cref="UnsupportedFileTypeException">Thrown when the file type is not supported.</exception>
        Task<List<DocumentChunk>> ProcessDocument(string filePath, int maxChunkSize = 500, CancellationToken cancellationToken = default);
    }
}
