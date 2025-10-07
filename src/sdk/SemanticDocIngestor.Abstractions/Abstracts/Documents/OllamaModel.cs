﻿using SemanticDocIngestor.Domain.Abstracts.Persistence;
using SemanticDocIngestor.Domain.Abstracts.Persistence;
using MongoDB.Bson;

namespace SemanticDocIngestor.Domain.Abstracts.Documents
{
    [BsonCollection("ollama_models")]
    public class OllamaModel : IDocument
    {
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
}
