using Ollama;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Domain.DTOs
{
    /// <summary>
    /// Data transfer object representing a single chunk of a document with its content and metadata.
    /// Used for transferring document chunks between layers and in API responses.
    /// </summary>
    public class DocumentChunkDto
    {
        /// <summary>
        /// Gets or sets the text content of this document chunk.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the metadata associated with this document chunk,
        /// including file name, source, page number, and other contextual information.
        /// </summary>
        public IngestionMetadata Metadata { get; set; } = new();

        /// <summary>
        /// Gets or sets the sequential index of this chunk within the source document.
        /// Zero-based index indicating the chunk's position in the document.
        /// </summary>
        public int Index { get; set; }
    }

    /// <summary>
    /// AutoMapper profile for mapping between <see cref="DocumentChunkDto"/> and <see cref="DocumentChunk"/> entities.
    /// </summary>
    public class DocumentChunkMappingProfile : AutoMapper.Profile
    {
        public DocumentChunkMappingProfile()
        {
            CreateMap<DocumentChunkDto, DocumentChunk>().ReverseMap();
        }
    }

    /// <summary>
    /// Response DTO containing a generated answer from RAG and references to source documents.
    /// </summary>
    public class SearchAndGetRagResponseDto
    {
        /// <summary>
        /// Gets or sets the AI-generated answer to the user's question based on retrieved context.
        /// </summary>
        public string Answer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the dictionary of source document references used to generate the answer.
        /// Key is the file path, value is the list of chunks from that document.
        /// </summary>
        public Dictionary<string, List<DocumentChunkDto>> ReferencesPath { get; set; } = [];
    }

    /// <summary>
    /// Response DTO containing a streaming answer from RAG and references to source documents.
    /// Allows real-time display of answers as they are generated token by token.
    /// </summary>
    public class SearchAndGetRagStreamingResponseDto
    {
        /// <summary>
        /// Gets or sets the async enumerable stream of chat completion responses from the LLM.
        /// Each response contains a token or chunk of the generated answer.
        /// </summary>
        public IAsyncEnumerable<GenerateChatCompletionResponse>? Answer { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of source document references used to generate the answer.
        /// Key is the file path, value is the list of chunks from that document.
        /// </summary>
        public Dictionary<string, List<DocumentChunkDto>> ReferencesPath { get; set; } = [];
    }

    /// <summary>
    /// DTO representing a document repository item with metadata and timestamps.
    /// Used for listing all ingested documents in the system.
    /// </summary>
    public class DocumentRepoItemDto
    {
        /// <summary>
        /// Gets or sets the metadata for this document, including file name, type, and source information.
        /// </summary>
        public IngestionMetadata Metadata { get; set; } = new();

        /// <summary>
        /// Gets or sets the UTC timestamp when this document was first ingested.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the UTC timestamp when this document was last updated or re-ingested.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
