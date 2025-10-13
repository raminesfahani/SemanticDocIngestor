namespace SemanticDocIngestor.Domain.Options
{
    public class QdrantOptions
    {
        public string CollectionName { get; set; } = "semantic_docs";
        public ulong VectorSize { get; set; } = 768;
        public bool UseCosine { get; set; } = true;
    }
}
