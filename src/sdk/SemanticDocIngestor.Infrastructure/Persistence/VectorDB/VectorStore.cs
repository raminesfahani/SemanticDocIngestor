using Elastic.Clients.Elasticsearch;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SemanticDocIngestor.Domain.Abstractions.Factories;
using SemanticDocIngestor.Domain.Abstractions.Persistence;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using SemanticDocIngestor.Domain.Options;
using SemanticDocIngestor.Infrastructure.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Infrastructure.Persistence.VectorDB
{
    public class VectorStore(QdrantClient client,
                       IOptions<AppSettings> options,
                       IOllamaServiceFactory embeddingProvider) : IVectorStore
    {
        private readonly QdrantClient _client = client;
        private readonly QdrantOptions _settings = options.Value.Qdrant;
        private readonly IOllamaServiceFactory _embeddingProvider = embeddingProvider;

        public async Task EnsureCollectionExistsAsync()
        {
            var exist = await _client.CollectionExistsAsync(_settings.CollectionName);
            if (exist != false)
                return;

            var vectorParams = new VectorParams
            {
                Size = _settings.VectorSize,
                Distance = _settings.UseCosine ? Distance.Cosine : Distance.Euclid,
                Datatype = Datatype.Float32,
                OnDisk = true,
            };

            await _client.CreateCollectionAsync(_settings.CollectionName, vectorParams);
        }

        public async Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default)
        {
            await _client.DeleteCollectionAsync(_settings.CollectionName, cancellationToken: cancellationToken);
            return true;
        }

        public async Task UpsertAsync(IEnumerable<DocumentChunk> chunks,
                                      CancellationToken cancellationToken = default)
        {
            await EnsureCollectionExistsAsync();

            // Group chunks by file path to handle deletion per document
            var chunksByFile = chunks.GroupBy(c => c.Metadata.FilePath ?? string.Empty);

            foreach (var fileGroup in chunksByFile)
            {
                var filePath = fileGroup.Key;
                var fileChunks = fileGroup.ToList();

                // Insert new chunks for this file
                var points = fileChunks.Where(x => x.Embedding != null).Select(chunk =>
                {
                    return new PointStruct()
                    {
                        Id = Guid.NewGuid(),
                        Vectors = chunk.Embedding ?? [],
                        Payload = {
                            ["content"] = chunk.Content,
                            ["fileName"] = chunk.Metadata.FileName,
                            ["fileType"] = chunk.Metadata.FileType,
                            ["filePath"] = chunk.Metadata.FilePath ?? "",
                            ["source"] = chunk.Metadata.Source.ToString(),
                            ["pageNumber"] = chunk.Metadata.PageNumber ?? "",
                            ["sectionTitle"] = chunk.Metadata.SectionTitle ?? "",
                            ["sheetName"] = chunk.Metadata.SheetName ?? "",
                            ["rowIndex"] = chunk.Metadata.RowIndex ?? -1
                        }
                    };
                }).ToList();

                if (points.Count > 0)
                {
                    await _client.UpsertAsync(_settings.CollectionName, points, cancellationToken: cancellationToken);
                }
            }
        }

        public async Task DeleteExistingChunksAsync(string filePath, CancellationToken cancellationToken)
        {
            var deleteFilter = new Filter
            {
                Must = {
                            new Condition
                            {
                                Field = new FieldCondition
                                {
                                    Key = "filePath",
                                    Match = new Match { Text = filePath }
                                }
                            }
                        }
            };

            var resp = await _client.DeleteAsync(_settings.CollectionName,
                                      deleteFilter,
                                      cancellationToken: cancellationToken);
        }

        public async Task<List<DocumentChunk>> SearchAsync(string query,
                                                           ulong topK = 5,
                                                           CancellationToken cancellationToken = default)
        {
            var embedding = await _embeddingProvider.GetEmbeddingAsync(query, cancellationToken);
            if (embedding == null || embedding.Count == 0)
                return [];

            var searchResult = await _client.QueryAsync(collectionName: _settings.CollectionName,
                                                        query: embedding.ToArray(),
                                                        limit: topK,
                                                        payloadSelector: true,
                                                        cancellationToken: cancellationToken);

            return [.. searchResult.Select(item =>
            {
                var payload = item.Payload;
                return new DocumentChunk
                {
                    Content = payload.GetValueOrDefault("content")?.StringValue ?? "",
                    Embedding = null,
                    Metadata = new IngestionMetadata
                    {
                        FileName = payload.GetValueOrDefault("fileName")?.StringValue ?? "",
                        FileType = payload.GetValueOrDefault("fileType")?.StringValue ?? "",
                        FilePath = payload.GetValueOrDefault("filePath")?.StringValue,
                        PageNumber = payload.GetValueOrDefault("pageNumber")?.StringValue,
                        SectionTitle = payload.GetValueOrDefault("sectionTitle")?.StringValue,
                        SheetName = payload.GetValueOrDefault("sheetName")?.StringValue,
                        Source = Enum.TryParse<IngestionSource>(payload.GetValueOrDefault("source")?.StringValue, out var source) ? source : IngestionSource.Local,
                        RowIndex = payload.GetValueOrDefault("rowIndex")?.IntegerValue,
                    }
                };
            })];
        }
    }
}
