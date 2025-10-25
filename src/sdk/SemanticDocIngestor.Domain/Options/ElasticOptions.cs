namespace SemanticDocIngestor.Domain.Options
{
    public class ElasticOptions
    {
        public string SemanticDocIndexName { get; set; } = "semantic_docs";
        public string DocRepoIndexName { get; set; } = "docs_repo";
    }
}
