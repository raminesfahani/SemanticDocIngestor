using SemanticDocIngestor.Domain.Entities.Ingestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Domain.DTOs
{
    public class DocumentChunkDto
    {
        public string Content { get; set; } = string.Empty;
        public IngestionMetadata Metadata { get; set; } = new();
        public int Index { get; set; }
    }

    public class DocumentChunkMappingProfile : AutoMapper.Profile
    {
        public DocumentChunkMappingProfile()
        {
            CreateMap<DocumentChunkDto, DocumentChunk>().ReverseMap();
        }
    }
}
