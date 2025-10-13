using SemanticDocIngestor.Domain.Entities.Ingestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Domain.Abstractions.Factories
{
    public interface IDocumentProcessor
    {
        List<string> SupportedFileExtensions { get; }
        Task<List<DocumentChunk>> ProcessDocument(string filePath, int maxChunkSize = 500, CancellationToken cancellationToken = default);
    }
}
