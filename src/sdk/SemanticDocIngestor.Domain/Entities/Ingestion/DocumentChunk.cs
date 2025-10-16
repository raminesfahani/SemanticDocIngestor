using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Domain.Entities.Ingestion
{
    public class DocumentChunk
    {
        public string Content { get; set; } = string.Empty;
        public float[]? Embedding { get; set; }
        public IngestionMetadata Metadata { get; set; } = new();
        public int Index { get; set; }
        // New: transient score from retrieval (vector similarity or hybrid)
        public double? RetrievalScore { get; set; }
    }
}
