using MongoDB.Bson;
using SemanticDocIngestor.Domain.Abstractions.Persistence;

namespace SemanticDocIngestor.Domain.Entities.Mongo
{
    public abstract class Document : IDocument
    {
        public ObjectId Id { get; set; }

        public DateTime CreatedAt => Id.CreationTime;
    }
}
