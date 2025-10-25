using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;
using SemanticDocIngestor.Domain.Abstractions.Persistence;
using SemanticDocIngestor.Domain.DTOs;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using SemanticDocIngestor.Infrastructure.Configurations;
using SharpCompress.Common;
using System.Security.Cryptography;
using System.Text;

namespace SemanticDocIngestor.Infrastructure.Persistence.ElasticSearch
{
    public class ElasticStore(ElasticsearchClient client, IOptions<AppSettings> options) : IElasticStore
    {
        private readonly ElasticsearchClient _client = client;
        private readonly string _semanticDocIndexName = options.Value.Elastic.SemanticDocIndexName;
        private readonly string _docRepoIndexName = options.Value.Elastic.DocRepoIndexName;
        private static readonly string[] fields = ["content", "metadata.*"];

        public async Task EnsureSemanticDocIndexExistsAsync()
        {
            var exists = await _client.Indices.ExistsAsync(_semanticDocIndexName);
            if (exists.Exists) return;

            var create = await _client.Indices.CreateAsync(_semanticDocIndexName);
            if (!create.IsValidResponse)
                throw new InvalidOperationException($"Failed to create index '{_semanticDocIndexName}': {create.DebugInformation}");
        }

        private async Task EnsureDocRepoIndexExistsAsync()
        {
            var exists = await _client.Indices.ExistsAsync(_docRepoIndexName);
            if (exists.Exists) return;

            var create = await _client.Indices.CreateAsync(_docRepoIndexName);
            if (!create.IsValidResponse)
                throw new InvalidOperationException($"Failed to create index '{_docRepoIndexName}': {create.DebugInformation}");
        }

        public async Task<List<DocumentRepoItemDto>> GetIngestedDocumentsAsync(CancellationToken cancellationToken = default)
        {
            await EnsureDocRepoIndexExistsAsync();
            // Implementation for retrieving ingested documents can be added here.

            var response = await _client.SearchAsync<DocumentRepoItemDto>(s => s
                                        .Index(_docRepoIndexName), cancellationToken);

            if (response.IsValidResponse)
            {
                var documents = response.Documents;
                return [.. response.Documents.OrderByDescending(d => d.UpdatedAt)];
            }

            return [];
        }

        private async Task UpdateIngestedDocsRepoAsync(List<DocumentChunk> chunks, CancellationToken cancellationToken = default)
        {
            await EnsureDocRepoIndexExistsAsync();

            // Update list of ingested documents, e.g., for tracking purposes.
            var files = chunks.GroupBy(c => c.Metadata?.FilePath)
                              .Select(g => g.First().Metadata)
                              .ToList();

            // 2) Bulk index using deterministic IDs to prevent future duplicates.
            var bulk = new BulkRequest(_docRepoIndexName)
            {
                Operations = new List<IBulkOperation>(files.Count),
                Refresh = Refresh.WaitFor
            };

            foreach (var c in chunks.Where(x => x.Embedding != null && !string.IsNullOrWhiteSpace(x.Content)))
            {
                // if filePath is existed, don't add again
                var existsResponse = await _client.ExistsAsync<DocumentRepoItemDto>(_docRepoIndexName, c.Metadata.FilePath ?? c.Metadata.FileName, cancellationToken: cancellationToken);
                if (existsResponse.Exists)
                {
                    // Update the UpdatedAt field
                    var updateResponse = await _client.UpdateAsync<DocumentRepoItemDto, DocumentRepoItemDto>(c.Metadata.FilePath ?? c.Metadata.FileName, u => u
                        .Index(_docRepoIndexName)
                        .Doc(new DocumentRepoItemDto
                        {
                            Metadata = c.Metadata,
                            UpdatedAt = DateTime.UtcNow
                        }), cancellationToken);
                }
                else
                {
                    var op = new BulkIndexOperation<DocumentRepoItemDto>(new DocumentRepoItemDto() { Metadata = c.Metadata })
                    {
                        Id = c.Metadata.FilePath ?? c.Metadata.FileName,
                    };

                    bulk.Operations.Add(op);
                }
            }

            if (bulk.Operations.Count == 0)
                return;

            var response = await _client.BulkAsync(bulk, cancellationToken);
        }

        public async Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default)
        {
            var response = await _client.Indices.DeleteAsync(_semanticDocIndexName, cancellationToken: cancellationToken);
            await _client.Indices.DeleteAsync(_docRepoIndexName, cancellationToken: cancellationToken);
            return response.IsValidResponse && response.IsSuccess();
        }

        public async Task<bool> UpsertAsync(DocumentChunk chunk, CancellationToken ct = default)
        {
            if (chunk is null || chunk.Embedding == null || string.IsNullOrWhiteSpace(chunk.Content))
                return false;

            var id = BuildChunkId(chunk);
            var response = await _client.IndexAsync(chunk, d => d
                .Index(_semanticDocIndexName)
                .Id(id)
                .Refresh(Refresh.WaitFor), ct);

            if (!response.IsValidResponse)
                throw new InvalidOperationException($"Failed to index document chunk into '{_semanticDocIndexName}': {response.DebugInformation}");

            await UpdateIngestedDocsRepoAsync([chunk], ct);

            return response.IsValidResponse;
        }

        public async Task<bool> UpsertAsync(List<DocumentChunk> chunks, CancellationToken ct = default)
        {
            if (chunks is null || chunks.Count == 0) return true;

            // 2) Bulk index using deterministic IDs to prevent future duplicates.
            var bulk = new BulkRequest(_semanticDocIndexName)
            {
                Operations = new List<IBulkOperation>(chunks.Count),
                Refresh = Refresh.WaitFor
            };

            foreach (var c in chunks.Where(x => x.Embedding != null && !string.IsNullOrWhiteSpace(x.Content)))
            {
                var op = new BulkIndexOperation<DocumentChunk>(c)
                {
                    Id = BuildChunkId(c)
                };
                bulk.Operations.Add(op);
            }

            if (bulk.Operations.Count == 0)
                return true;

            var response = await _client.BulkAsync(bulk, ct);
            if (!response.IsValidResponse || response.Errors)
                throw new InvalidOperationException($"Failed to bulk index into '{_semanticDocIndexName}': {response.DebugInformation}");

            await UpdateIngestedDocsRepoAsync(chunks, ct);

            return true;
        }

        public async Task DeleteExistingChunks(List<DocumentChunk> chunks, CancellationToken ct)
        {
            var docKeys = chunks
                .Select(c => GetDocumentIdentityKey(c))
                .Where(k => k is not null)
                .Distinct()!
                .ToList();

            foreach (var key in docKeys)
                await DeleteExistingDocumentChunksAsync(key!, ct);
        }

        public async Task DeleteExistingChunks(string filePath, CancellationToken ct)
        {
            // No-op if index does not exist yet
            var exists = await _client.Indices.ExistsAsync(_semanticDocIndexName, ct);
            if (!exists.Exists) return;

            var request = new DeleteByQueryRequest(_semanticDocIndexName)
            {
                Refresh = true,
                Query = Query.Bool(new BoolQuery
                {
                    Must =
                    [
                        // Prefer filePath, else fileName. We use should + minimum_should_match:1 inside a bool to match either.
                        Query.Bool(new BoolQuery
                        {
                            Should =
                            [
                                Query.MatchPhrase(new MatchPhraseQuery("metadata.filePath") { Query = filePath })
                            ],
                            MinimumShouldMatch = 1
                        })
                    ]
                })
            };

            var resp = await _client.DeleteByQueryAsync(request, ct);
            // Intentionally ignore index_not_found scenarios; startup race can occur
        }

        public async Task<IEnumerable<DocumentChunk>> SearchAsync(string query, int size = 10, CancellationToken ct = default)
        {
            var response = await _client.SearchAsync<DocumentChunk>(s => s
                .Index(_semanticDocIndexName)
                .Size(size)
                .Query(q => q.MultiMatch(m => m
                    .Fields(fields)
                    .Query(query)
                ))
            , ct);

            if (!response.IsValidResponse)
                throw new InvalidOperationException($"Search failed on '{_semanticDocIndexName}': {response.DebugInformation}");

            return response.Hits
                            .OrderByDescending(x => x.Score)
                            .Where(h => h.Source is not null)
                            .Select(h => h.Source!);
        }

        // --------------------
        // Helpers
        // --------------------

        // Deletes all chunks for a given document identity.
        private async Task DeleteExistingDocumentChunksAsync(string documentKey, CancellationToken ct)
        {
            // No-op if index does not exist yet
            var exists = await _client.Indices.ExistsAsync(_semanticDocIndexName, ct);
            if (!exists.Exists) return;

            var request = new DeleteByQueryRequest(_semanticDocIndexName)
            {
                Refresh = true,
                Query = Query.Bool(new BoolQuery
                {
                    Must = BuildDocIdentityMustQueries(documentKey)
                })
            };

            var resp = await _client.DeleteByQueryAsync(request, ct);
            // Ignore index_not_found scenarios to be resilient at startup
            if (!resp.IsValidResponse)
            {
                // Silently ignore; callers treat delete as best-effort
                return;
            }
        }

        // Build "must" queries to match a document identity.
        // Identity is:
        // - metadata.filePath (preferred) OR metadata.fileName
        // - AND metadata.source
        private static List<Query> BuildDocIdentityMustQueries(string docKey)
        {
            var must = new List<Query>
            {
                // Prefer filePath, else fileName. We use should + minimum_should_match:1 inside a bool to match either.
                Query.Bool(new BoolQuery
                {
                    Should =
                    [
                        Query.MatchPhrase(new MatchPhraseQuery("metadata.filePath") { Query = docKey }),
                        Query.MatchPhrase(new MatchPhraseQuery("metadata.fileName") { Query = docKey }),
                    ],
                    MinimumShouldMatch = 1
                })
            };

            // If the docKey encodes source (see GetDocumentIdentityKey), also filter by source.
            var source = ExtractSourceFromDocKey(docKey);
            if (source is not null)
            {
                must.Add(Query.Term(new TermQuery("metadata.source") { Value = (int)source.Value }));
            }

            return must;
        }

        // Deterministic ID for a chunk to prevent duplicates on future upserts.
        private static string BuildChunkId(DocumentChunk chunk)
        {
            var docKey = GetDocumentIdentityKey(chunk) ?? $"content:{HashToBase64Url(chunk.Content ?? string.Empty)}";
            return $"{docKey}#idx:{chunk.Index}";
        }

        // Derive a stable document identity key. Encodes source to reduce accidental collisions across different sources.
        private static string? GetDocumentIdentityKey(DocumentChunk chunk)
        {
            var src = (int?)chunk.Metadata?.Source;
            var path = chunk.Metadata?.FilePath?.Trim();
            var name = chunk.Metadata?.FileName?.Trim();

            var baseKey = path ?? name;
            if (string.IsNullOrWhiteSpace(baseKey)) return null;

            return $"src:{src?.ToString() ?? "null"}|doc:{baseKey}";
        }

        private static IngestionSource? ExtractSourceFromDocKey(string key)
        {
            // key format: "src:{intOrNull}|doc:{...}"
            // We try to parse the src part if present.
            var prefix = "src:";
            var sep = "|doc:";
            var i = key.IndexOf(prefix, StringComparison.Ordinal);
            var j = key.IndexOf(sep, StringComparison.Ordinal);
            if (i != 0 || j <= prefix.Length) return null;

            var srcStr = key.Substring(prefix.Length, j - prefix.Length);
            if (int.TryParse(srcStr, out var srcVal))
                return (IngestionSource)srcVal;

            return null;
        }

        private static string HashToBase64Url(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
