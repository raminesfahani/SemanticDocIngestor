# SemanticDocIngestor - Quick Reference Guide

Quick reference for common tasks and configurations.

---

## ?? Installation

```bash
dotnet add package SemanticDocIngestor.Core
```

---

## ? Quick Setup

### With .NET Aspire
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddSemanticDocIngestorCore(builder.Configuration);

var app = builder.Build();
app.UseSemanticDocIngestorCore(app.Configuration, 
    app.Services.GetRequiredService<ILoggerFactory>());
app.Run();
```

### Without Aspire
```json
{
  "ConnectionStrings": {
    "elasticsearch": "http://localhost:9200",
    "qdrant": "http://localhost:6333",
    "ollama": "http://localhost:11434"
  }
}
```

---

## ?? Common Tasks

### Ingest Documents
```csharp
// Local files
await documentIngestor.IngestDocumentsAsync(new[] { "file.pdf" });

// OneDrive
await documentIngestor.IngestDocumentsAsync(new[] { "onedrive://drive/item" });

// Google Drive
await documentIngestor.IngestDocumentsAsync(new[] { "gdrive://fileId" });

// Folder
await documentIngestor.IngestFolderAsync(@"C:\docs");
```

### Search
```csharp
// Hybrid search
var results = await documentIngestor.SearchDocumentsAsync("query", limit: 10);

// RAG answer
var answer = await documentIngestor.SearchAndGetRagResponseAsync("question", limit: 5);

// Streaming
var stream = await documentIngestor.SearchAndGetRagStreamResponseAsync("question");
```

### Progress Tracking
```csharp
// Events
documentIngestor.OnProgress += (s, e) => 
    Console.WriteLine($"{e.Completed}/{e.Total}");

// Query
var progress = await documentIngestor.GetProgressAsync();
```

### List Documents
```csharp
var docs = await documentIngestor.ListIngestedDocumentsAsync();
```

### Clear All
```csharp
await documentIngestor.FlushAsync();
```

---

## ??? Configuration Quick Reference

### Ollama Settings
| Setting | Default | Description |
|---------|---------|-------------|
| `ChatModel` | `llama3.2` | LLM for answers |
| `EmbeddingModel` | `nomic-embed-text` | Embedding model |
| `Temperature` | `0.7` | Randomness (0.0-1.0) |
| `MaxTokens` | `2048` | Max response length |

### Qdrant Settings
| Setting | Default | Description |
|---------|---------|-------------|
| `CollectionName` | `documents` | Collection name |
| `VectorSize` | `768` | Embedding dimensions |
| `Distance` | `Cosine` | Distance metric |

### Elasticsearch Settings
| Setting | Default | Description |
|---------|---------|-------------|
| `SemanticDocIndexName` | `semantic_docs` | Chunk index |
| `DocRepoIndexName` | `docs_repo` | Metadata index |

### Resiliency Settings
| Setting | Default | Description |
|---------|---------|-------------|
| `RetryCount` | `3` | Retry attempts |
| `TimeoutSeconds` | `30` | Request timeout |
| `ExceptionsAllowedBeforeCircuitBreaking` | `5` | Circuit threshold |
| `CircuitBreakingDurationSeconds` | `60` | Circuit open time |

---

## ?? Connection Strings

### Formats
```json
{
  "ConnectionStrings": {
 "elasticsearch": "http://localhost:9200",
    "elasticsearch": "http://user:pass@localhost:9200",
    "elasticsearch": "https://cloud.elastic.co:9243",
    
  "qdrant": "http://localhost:6333",
    "qdrant": "http://localhost:6333;ApiKey=key",
    "qdrant": "https://cloud.qdrant.io:6333;ApiKey=key",
    
    "ollama": "http://localhost:11434",
    "ollama": "http://remote-server:11434"
  }
}
```

### Environment Variables
```bash
export ConnectionStrings__elasticsearch="http://localhost:9200"
export ConnectionStrings__qdrant="http://localhost:6333"
export ConnectionStrings__ollama="http://localhost:11434"
```

---

## ?? Cloud Ingestion URIs

### OneDrive
```
onedrive://{driveId}/{itemId}
https://1drv.ms/u/s!xxxxx
https://contoso.sharepoint.com/:b:/g/documents/file.pdf
```

### Google Drive
```
gdrive://{fileId}
https://drive.google.com/file/d/{fileId}/view
```

---

## ?? Troubleshooting

### Connection Issues
```bash
# Test Elasticsearch
curl http://localhost:9200

# Test Qdrant
curl http://localhost:6333/collections

# Test Ollama
curl http://localhost:11434/api/tags
```

### Pull Ollama Models
```bash
ollama pull llama3.2
ollama pull nomic-embed-text
ollama list
```

### Check Service Health
- Aspire Dashboard: `http://localhost:15888`
- API Health: `http://localhost:5000/health`

---

## ?? Deployment

### Docker Compose
```yaml
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    ports: ["9200:9200"]
  
  qdrant:
    image: qdrant/qdrant:latest
    ports: ["6333:6333"]
  
  ollama:
    image: ollama/ollama:latest
    ports: ["11434:11434"]
```

### Start Services
```bash
docker-compose up -d
```

---

## ?? Documentation Links

- **[Complete SDK Guide](src/sdk/SemanticDocIngestor.Core/README.md)** - Full documentation
- **[Main README](README.md)** - Project overview
- **[Contributing](CONTRIBUTING.md)** - How to contribute
- **[Support](SUPPORT.md)** - Getting help

---

## ?? Quick Help

| I want to... | See... |
|--------------|--------|
| Install the SDK | [Installation](#installation) |
| Get started quickly | [Quick Setup](#quick-setup) |
| Ingest documents | [Ingest Documents](#ingest-documents) |
| Search documents | [Search](#search) |
| Configure settings | [Configuration](#configuration-quick-reference) |
| Deploy to production | [Deployment](#deployment) |
| Fix connection issues | [Troubleshooting](#troubleshooting) |
| Get detailed help | [SDK Guide](src/sdk/SemanticDocIngestor.Core/README.md) |

---

**For complete documentation, see: [SDK Guide](src/sdk/SemanticDocIngestor.Core/README.md)**
