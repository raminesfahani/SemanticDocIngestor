# SemanticDocIngestor.Core SDK

[![NuGet](https://img.shields.io/nuget/v/SemanticDocIngestor.Core)](https://www.nuget.org/packages/SemanticDocIngestor.Core)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/raminesfahani/SemanticDocIngestor/blob/master/LICENSE)

A powerful .NET 9 SDK for document ingestion, semantic search, and retrieval-augmented generation (RAG) with hybrid search capabilities. Build intelligent document processing pipelines with vector and keyword search powered by Qdrant and Elasticsearch.

---

## Features

- **📄 Multi-Format Document Processing**: Support for PDF, DOCX, XLSX, PPTX, TXT, and more
- **🔍 Hybrid Search**: Combine vector similarity and keyword search for optimal results
- **☁️ Multi-Source Ingestion**: Local files, OneDrive (Microsoft Graph), and Google Drive
- **🤖 RAG Integration**: Built-in support for Ollama LLM models with streaming responses
- **📊 Progress Tracking**: Real-time ingestion progress with event-driven architecture
- **🔄 De-duplication**: Deterministic chunk IDs prevent duplicate ingestion
- **⚡ Resilient**: Built-in retry policies, circuit breakers, and timeout handling with Polly
- **🧩 Extensible**: Clean abstractions for custom processors, stores, and cloud resolvers
- **📡 SignalR Ready**: Real-time progress notifications for web applications
- **🎯 .NET Aspire Ready**: First-class support for .NET Aspire orchestration

---

## Installation

Install the NuGet package:

```bash
dotnet add package SemanticDocIngestor.Core
```

### Prerequisites

The SDK requires:
- **.NET 9.0** or later
- **Elasticsearch** (for keyword search)
- **Qdrant** (for vector search)
- **Ollama** (optional, for RAG/LLM features)

---

## Quick Start

### Option 1: With .NET Aspire (Recommended)

.NET Aspire automatically manages service discovery and connection strings. The SDK integrates seamlessly with Aspire's orchestration.

#### 1. Add Aspire Service Defaults

In your `Program.cs`:

```csharp
using SemanticDocIngestor.Core;

var builder = WebApplication.CreateBuilder(args);

// Add .NET Aspire service defaults (includes service discovery, telemetry, health checks)
builder.AddServiceDefaults();

// Add SemanticDocIngestor services
// Connection strings are automatically discovered via Aspire service discovery
builder.Services.AddSemanticDocIngestorCore(builder.Configuration);

var app = builder.Build();

// Map default Aspire endpoints (health checks, metrics)
app.MapDefaultEndpoints();

// Use SemanticDocIngestor middleware
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
app.UseSemanticDocIngestorCore(app.Configuration, loggerFactory);

app.Run();
```

#### 2. Configure Aspire AppHost

In your Aspire `AppHost` project (e.g., `Program.cs`):

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add infrastructure services
var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithDataVolume();

var qdrant = builder.AddQdrant("qdrant")
    .WithDataVolume();

var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithOpenWebUI(); // Optional: Add Open WebUI for model management

// Add your API service with references
var apiService = builder.AddProject<Projects.SemanticDocIngestor_AppHost_ApiService>("apiservice")
    .WithReference(elasticsearch)
    .WithReference(qdrant)
    .WithReference(ollama);

builder.Build().Run();
```

#### 3. Minimal Configuration

With Aspire, your `appsettings.json` only needs SDK-specific settings:

```json
{
  "AppSettings": {
    "Ollama": {
      "ChatModel": "llama3.2",
      "EmbeddingModel": "nomic-embed-text",
      "Temperature": 0.7,
"MaxTokens": 2048
    },
    "Qdrant": {
      "CollectionName": "documents",
      "VectorSize": 768,
      "Distance": "Cosine"
    },
    "Elastic": {
      "SemanticDocIndexName": "semantic_docs",
      "DocRepoIndexName": "docs_repo"
    }
  },
  "ResiliencyMiddlewareOptions": {
    "RetryCount": 3,
    "TimeoutSeconds": 30,
    "ExceptionsAllowedBeforeCircuitBreaking": 5,
  "CircuitBreakingDurationSeconds": 60
  }
}
```

**Note:** Connection strings for `elasticsearch`, `qdrant`, and `ollama` are automatically injected by Aspire service discovery.

---

### Option 2: Without .NET Aspire (Manual Configuration)

If you're not using .NET Aspire, you need to manually configure connection strings.

#### 1. Configure Services

```csharp
using SemanticDocIngestor.Core;

var builder = WebApplication.CreateBuilder(args);

// Add SemanticDocIngestor services
builder.Services.AddSemanticDocIngestorCore(builder.Configuration);

var app = builder.Build();

// Use SemanticDocIngestor middleware
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
app.UseSemanticDocIngestorCore(app.Configuration, loggerFactory);

app.Run();
```

#### 2. Complete Configuration

Add all connection strings and settings to your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "elasticsearch": "http://localhost:9200",
    "qdrant": "http://localhost:6333",
    "ollama": "http://localhost:11434"
  },
  "AppSettings": {
    "Ollama": {
      "ChatModel": "llama3.2",
   "EmbeddingModel": "nomic-embed-text",
      "Temperature": 0.7,
      "MaxTokens": 2048
    },
    "Qdrant": {
 "CollectionName": "documents",
    "VectorSize": 768,
   "Distance": "Cosine"
    },
    "Elastic": {
 "SemanticDocIndexName": "semantic_docs",
    "DocRepoIndexName": "docs_repo"
    }
  },
  "ResiliencyMiddlewareOptions": {
    "RetryCount": 3,
    "TimeoutSeconds": 30,
    "ExceptionsAllowedBeforeCircuitBreaking": 5,
    "CircuitBreakingDurationSeconds": 60
  }
}
```

---

## Connection String Management

### Connection String Formats

The SDK expects the following connection string names in `appsettings.json` or via .NET Aspire:

#### Elasticsearch
```json
{
  "ConnectionStrings": {
    "elasticsearch": "http://localhost:9200"
  }
}
```

**With Authentication:**
```json
{
  "ConnectionStrings": {
    "elasticsearch": "http://username:password@localhost:9200"
  }
}
```

**Cloud/Elastic Cloud:**
```json
{
  "ConnectionStrings": {
    "elasticsearch": "https://my-deployment.es.us-central1.gcp.cloud.es.io:9243"
  }
}
```

#### Qdrant
```json
{
  "ConnectionStrings": {
    "qdrant": "http://localhost:6333"
  }
}
```

**With API Key:**
```json
{
  "ConnectionStrings": {
    "qdrant": "http://localhost:6333;ApiKey=your-api-key-here"
  }
}
```

**Qdrant Cloud:**
```json
{
  "ConnectionStrings": {
    "qdrant": "https://xyz-example.qdrant.io:6333;ApiKey=your-cloud-api-key"
  }
}
```

#### Ollama
```json
{
  "ConnectionStrings": {
    "ollama": "http://localhost:11434"
  }
}
```

**Remote Ollama:**
```json
{
  "ConnectionStrings": {
    "ollama": "http://your-ollama-server:11434"
  }
}
```

### Environment-Specific Configuration

#### Development
Use `appsettings.Development.json` for local development:

```json
{
  "ConnectionStrings": {
    "elasticsearch": "http://localhost:9200",
    "qdrant": "http://localhost:6333",
    "ollama": "http://localhost:11434"
  }
}
```

#### Production
Use `appsettings.Production.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "elasticsearch": "https://prod-es.company.com:9243",
    "qdrant": "https://prod-qdrant.company.com:6333;ApiKey=${QDRANT_API_KEY}",
    "ollama": "http://ollama-service:11434"
  }
}
```

#### Using Environment Variables

Set connection strings via environment variables (useful for Docker/Kubernetes):

```bash
export ConnectionStrings__elasticsearch="http://elasticsearch:9200"
export ConnectionStrings__qdrant="http://qdrant:6333"
export ConnectionStrings__ollama="http://ollama:11434"
```

**In Docker Compose:**
```yaml
services:
  api:
    image: your-api-image
    environment:
      - ConnectionStrings__elasticsearch=http://elasticsearch:9200
      - ConnectionStrings__qdrant=http://qdrant:6333
      - ConnectionStrings__ollama=http://ollama:11434
    depends_on:
      - elasticsearch
      - qdrant
  - ollama
```

### Azure Configuration

When deploying to Azure, use Azure App Configuration or Key Vault:

```csharp
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(builder.Configuration["ConnectionStrings:AppConfig"])
           .UseFeatureFlags();
});

// Or use Key Vault
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Connection String Priority

The SDK resolves connection strings in the following order:
1. **.NET Aspire Service Discovery** (if using Aspire)
2. **Environment Variables** (`ConnectionStrings__elasticsearch`)
3. **appsettings.{Environment}.json** (e.g., `appsettings.Production.json`)
4. **appsettings.json**
5. **User Secrets** (development only)

---

## .NET Aspire Integration Details

### Service Discovery

When using .NET Aspire, the SDK automatically discovers services by their registered names:

| Service Name | SDK Uses For | Default Port |
|--------------|--------------|--------------|
| `elasticsearch` | Keyword search and metadata storage | 9200 |
| `qdrant` | Vector embeddings and semantic search | 6333 |
| `ollama` | LLM chat and embedding generation | 11434 |

### Aspire Container Resources

The SDK works seamlessly with Aspire's container resources:

```csharp
// In AppHost Program.cs
var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var qdrant = builder.AddContainer("qdrant", "qdrant/qdrant")
    .WithBindMount("./qdrant_data", "/qdrant/storage")
    .WithHttpEndpoint(port: 6333, targetPort: 6333, name: "qdrant");

var ollama = builder.AddContainer("ollama", "ollama/ollama")
    .WithBindMount("./ollama_data", "/root/.ollama")
    .WithHttpEndpoint(port: 11434, targetPort: 11434, name: "ollama");
```

### Health Checks with Aspire

The SDK supports Aspire's health check infrastructure:

```csharp
// In your API service
builder.Services.AddHealthChecks()
    .AddElasticsearch(builder.Configuration.GetConnectionString("elasticsearch")!)
    .AddQdrant(builder.Configuration.GetConnectionString("qdrant")!);

// In AppHost, health checks are automatically monitored
var apiService = builder.AddProject<Projects.YourApi>("api")
    .WithReference(elasticsearch)
    .WithReference(qdrant)
    .WithHealthCheck(); // Monitors the health endpoints
```

### Aspire Dashboard

When running with Aspire, you can monitor the SDK's operations in the Aspire Dashboard:

- **Traces**: View document ingestion and search operations
- **Metrics**: Monitor throughput, latency, and error rates
- **Logs**: Structured logging from all SDK components
- **Resources**: See connection status and resource health

Access the dashboard at: `http://localhost:15888` (default)

---

## Basic Usage

### 3. Ingesting Documents

```csharp
using SemanticDocIngestor.Domain.Abstractions.Services;

public class DocumentController : ControllerBase
{
    private readonly IDocumentIngestorService _documentIngestor;
    
    public DocumentController(IDocumentIngestorService documentIngestor)
    {
        _documentIngestor = documentIngestor;
    }
    
    [HttpPost("ingest")]
    public async Task<IActionResult> IngestDocuments(
        [FromBody] List<string> filePaths,
      CancellationToken cancellationToken)
    {
    // Ingest local files
   await _documentIngestor.IngestDocumentsAsync(
 filePaths,
          maxChunkSize: 500,
     cancellationToken: cancellationToken);
        
        return Ok("Ingestion completed");
    }
}
```

### Searching Documents

```csharp
[HttpGet("search")]
public async Task<IActionResult> Search(
    [FromQuery] string query,
    [FromQuery] ulong limit = 10,
    CancellationToken cancellationToken = default)
{
    var results = await _documentIngestor.SearchDocumentsAsync(
        query,
        limit: limit,
        cancellationToken: cancellationToken);
    
    return Ok(results);
}
```

### RAG with Streaming Response

```csharp
[HttpGet("ask")]
public async Task<IActionResult> AskQuestion(
 [FromQuery] string question,
    [FromQuery] ulong contextLimit = 5,
    CancellationToken cancellationToken = default)
{
    var response = await _documentIngestor.SearchAndGetRagResponseAsync(
        question,
        limit: contextLimit,
        cancellationToken: cancellationToken);
    
    return Ok(new
    {
        answer = response.Answer,
      sources = response.ReferencesPath.Keys
    });
}
```

---

## Advanced Features

### Multi-Source Ingestion

The SDK supports ingesting from multiple sources:

#### Local Files
```csharp
var localFiles = new[]
{
    @"C:\Documents\report.pdf",
    @"C:\Documents\presentation.pptx"
};

await _documentIngestor.IngestDocumentsAsync(localFiles);
```

#### OneDrive (Microsoft Graph)
```csharp
// Using OneDrive URIs
var oneDriveFiles = new[]
{
    "onedrive://{driveId}/{itemId}",
    "https://1drv.ms/u/s!xxxxxxxxxxxxxx",
    "https://contoso.sharepoint.com/:b:/g/documents/report.pdf"
};

await _documentIngestor.IngestDocumentsAsync(oneDriveFiles);
```

**OneDrive Configuration** (add to appsettings.json):
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "contoso.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "CallbackPath": "/signin-oidc"
  }
}
```

Required Microsoft Graph permissions: `Files.Read`, `Files.Read.All`

#### Google Drive
```csharp
// Using Google Drive URIs
var googleDriveFiles = new[]
{
    "gdrive://{fileId}",
    "https://drive.google.com/file/d/{fileId}/view"
};

await _documentIngestor.IngestDocumentsAsync(googleDriveFiles);
```

**Google Drive Configuration** (add to appsettings.json):
```json
{
  "Google": {
"ClientId": "your-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-client-secret",
    "ApplicationName": "YourAppName"
  }
}
```

Required Google API scope: `https://www.googleapis.com/auth/drive.readonly`

### Progress Tracking

Monitor ingestion progress with events:

```csharp
public class IngestionService
{
    private readonly IDocumentIngestorService _documentIngestor;
    
    public IngestionService(IDocumentIngestorService documentIngestor)
    {
        _documentIngestor = documentIngestor;
  
        // Subscribe to progress events
    _documentIngestor.OnProgress += OnIngestionProgress;
 _documentIngestor.OnCompleted += OnIngestionCompleted;
    }
    
    private async void OnIngestionProgress(object? sender, IngestionProgress e)
    {
        Console.WriteLine($"Progress: {e.Completed}/{e.Total} - {e.FilePath}");
  }
    
    private async void OnIngestionCompleted(object? sender, IngestionProgress e)
    {
        Console.WriteLine($"Ingestion completed: {e.Total} documents processed");
    }
}
```

Or query progress directly:

```csharp
var progress = await _documentIngestor.GetProgressAsync(cancellationToken);
Console.WriteLine($"{progress.Completed} of {progress.Total} files processed");
```

### List Ingested Documents

```csharp
var ingestedDocs = await _documentIngestor.ListIngestedDocumentsAsync(cancellationToken);

foreach (var doc in ingestedDocs)
{
    Console.WriteLine($"File: {doc.Metadata.FileName}");
    Console.WriteLine($"Source: {doc.Metadata.Source}");
    Console.WriteLine($"Path: {doc.Metadata.FilePath}");
    Console.WriteLine($"Ingested: {doc.CreatedAt}");
}
```

### Flush All Documents

```csharp
// Remove all ingested documents from both vector and keyword stores
await _documentIngestor.FlushAsync(cancellationToken);
```

---

## Architecture

### Core Components

```
SemanticDocIngestor.Core
??? Services
?   ??? DocumentIngestorService  # Main orchestration service
??? Domain
?   ??? Abstractions
?   ?   ??? Services
?   ?   ?   ??? IDocumentIngestorService
?   ?   ?   ??? IRagService
?   ?   ??? Factories
?   ?   ?   ??? IDocumentProcessor
?   ?   ?   ??? ICloudFileResolver
?   ?   ??? Persistence
?   ?     ??? IVectorStore
?   ?       ??? IElasticStore
?   ??? Entities
?   ?   ??? DocumentChunk
?   ??? DTOs
??? Infrastructure
    ??? Factories
    ???? DocumentProcessor     # PDF, DOCX, XLSX processing
    ? ??? OneDriveFileResolver     # Microsoft Graph integration
    ?   ??? GoogleDriveFileResolver  # Google Drive API integration
    ??? Persistence
    ?   ??? VectorStore         # Qdrant implementation
    ?   ??? ElasticStore             # Elasticsearch implementation
    ??? Middlewares
        ??? ResiliencyMiddleware     # Polly-based resilience
        ??? RequestLoggingMiddleware
  ??? ExceptionHandlingMiddleware
```

### Document Processing Pipeline

1. **Input Resolution**: Local files or cloud URIs are resolved to local paths
2. **Document Processing**: Files are parsed and chunked with metadata extraction
3. **Embedding Generation**: Text chunks are converted to vector embeddings via Ollama
4. **Dual Storage**: Chunks are stored in both Qdrant (vectors) and Elasticsearch (keywords)
5. **Deduplication**: Deterministic IDs ensure no duplicate chunks across re-ingestions

### Hybrid Search Flow

1. **Parallel Search**: Query runs against both vector store (semantic) and keyword store (BM25)
2. **Result Merging**: Results are deduplicated and merged
3. **Optional Reranking**: Results can be reranked for relevance
4. **Context Assembly**: Top results are assembled for RAG context

---

## API Reference

### IDocumentIngestorService

Main service interface for document ingestion and search.

#### Methods

**IngestDocumentsAsync**
```csharp
Task IngestDocumentsAsync(
    IEnumerable<string> documentPaths,
    int maxChunkSize = 500,
    CancellationToken cancellationToken = default)
```
Ingest documents from local paths or cloud URIs.

**IngestFolderAsync**
```csharp
Task IngestFolderAsync(
    string folderPath,
    CancellationToken cancellationToken = default)
```
Recursively ingest all supported documents from a folder.

**SearchDocumentsAsync**
```csharp
Task<List<DocumentChunkDto>> SearchDocumentsAsync(
    string query,
    ulong limit = 10,
 CancellationToken cancellationToken = default)
```
Perform hybrid search across ingested documents.

**SearchAndGetRagResponseAsync**
```csharp
Task<SearchAndGetRagResponseDto> SearchAndGetRagResponseAsync(
    string search,
    ulong limit = 5,
    CancellationToken cancellationToken = default)
```
Search documents and generate an AI-powered answer using RAG.

**SearchAndGetRagStreamResponseAsync**
```csharp
Task<SearchAndGetRagStreamingResponseDto> SearchAndGetRagStreamResponseAsync(
    string search,
    ulong limit = 5,
    CancellationToken cancellationToken = default)
```
Search documents and stream an AI-powered answer in real-time.

**ListIngestedDocumentsAsync**
```csharp
Task<List<DocumentRepoItemDto>> ListIngestedDocumentsAsync(
    CancellationToken cancellationToken = default)
```
Get a list of all ingested documents with metadata.

**GetProgressAsync**
```csharp
Task<IngestionProgress?> GetProgressAsync(
    CancellationToken cancellationToken = default)
```
Get current ingestion progress.

**FlushAsync**
```csharp
Task FlushAsync(CancellationToken cancellationToken = default)
```
Delete all ingested documents from both stores.

#### Events

**OnProgress**
```csharp
event EventHandler<IngestionProgress>? OnProgress
```
Raised during ingestion with progress updates.

**OnCompleted**
```csharp
event EventHandler<IngestionProgress>? OnCompleted
```
Raised when ingestion completes.

---

## Configuration Options

### Ollama Settings

| Property | Description | Default |
|----------|-------------|---------|
| `ChatModel` | Model for RAG chat completions | `llama3.2` |
| `EmbeddingModel` | Model for vector embeddings | `nomic-embed-text` |
| `Temperature` | Response randomness (0.0-1.0) | `0.7` |
| `MaxTokens` | Maximum response tokens | `2048` |

### Qdrant Settings

| Property | Description | Default |
|----------|-------------|---------|
| `CollectionName` | Vector collection name | `documents` |
| `VectorSize` | Embedding vector dimensions | `768` |
| `Distance` | Distance metric | `Cosine` |

### Elasticsearch Settings

| Property | Description | Default |
|----------|-------------|---------|
| `SemanticDocIndexName` | Index for document chunks | `semantic_docs` |
| `DocRepoIndexName` | Index for document metadata | `docs_repo` |

### Resiliency Settings

| Property | Description | Default |
|----------|-------------|---------|
| `RetryCount` | Number of retry attempts | `3` |
| `TimeoutSeconds` | Request timeout | `30` |
| `ExceptionsAllowedBeforeCircuitBreaking` | Circuit breaker threshold | `5` |
| `CircuitBreakingDurationSeconds` | Circuit breaker open duration | `60` |

---

## Real-World Examples

### ASP.NET Core API

Complete example from the reference implementation:

```csharp
using SemanticDocIngestor.Core;
using SemanticDocIngestor.Domain.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add SemanticDocIngestor
builder.Services.AddSemanticDocIngestorCore(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
app.UseSemanticDocIngestorCore(app.Configuration, loggerFactory);

app.MapControllers();
app.Run();

// Controller
[ApiController]
[Route("[controller]")]
public class IngestionController : ControllerBase
{
    private readonly IDocumentIngestorService _documentIngestor;
  private readonly IWebHostEnvironment _env;
    
    public IngestionController(
        IDocumentIngestorService documentIngestor,
        IWebHostEnvironment env)
    {
        _documentIngestor = documentIngestor;
   _env = env;
    }

    [HttpPost("ingest-files")]
    public async Task<IActionResult> IngestFiles(
     [FromBody] List<string> filesPath,
        CancellationToken cancellationToken)
    {
        var fullPaths = filesPath.Select(f => 
            Path.Combine(_env.WebRootPath, f));
      
   await _documentIngestor.IngestDocumentsAsync(
     fullPaths,
   cancellationToken: cancellationToken);
            
        return Created();
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search(
     [FromQuery] string search,
        [FromQuery] ulong limit = 5,
        CancellationToken cancellationToken = default)
    {
        var response = await _documentIngestor
     .SearchAndGetRagResponseAsync(
     search,
  limit,
   cancellationToken);
  
        return Ok(response);
    }
    
    [HttpGet("ingested-files")]
    public async Task<IActionResult> GetIngestedFiles(
        CancellationToken cancellationToken)
    {
   var files = await _documentIngestor
       .ListIngestedDocumentsAsync(cancellationToken);
            
        return Ok(files);
    }
    
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress(
        CancellationToken cancellationToken)
    {
   var progress = await _documentIngestor
 .GetProgressAsync(cancellationToken);
          
        return Ok(progress);
    }
    
    [HttpDelete("flush-db")]
    public async Task<IActionResult> Flush(
        CancellationToken cancellationToken)
    {
 await _documentIngestor.FlushAsync(cancellationToken);
        return Ok();
}
}
```

### Console Application

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SemanticDocIngestor.Core;
using SemanticDocIngestor.Domain.Abstractions.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddSemanticDocIngestorCore(configuration);

var serviceProvider = services.BuildServiceProvider();
var documentIngestor = serviceProvider
    .GetRequiredService<IDocumentIngestorService>();

// Subscribe to progress
documentIngestor.OnProgress += (sender, progress) =>
{
    Console.WriteLine($"Progress: {progress.Completed}/{progress.Total}");
};

// Ingest documents
var files = new[]
{
  @"C:\docs\report.pdf",
    @"C:\docs\presentation.pptx"
};

await documentIngestor.IngestDocumentsAsync(files);

// Search with RAG
var question = "What are the key findings in the report?";
var response = await documentIngestor
    .SearchAndGetRagResponseAsync(question, limit: 5);

Console.WriteLine($"Answer: {response.Answer}");
Console.WriteLine($"Sources: {string.Join(", ", response.ReferencesPath.Keys)}");
```

---

## Extensibility

### Custom Document Processor

Implement `IDocumentProcessor` for custom file types:

```csharp
public class CustomDocumentProcessor : IDocumentProcessor
{
    public List<string> SupportedFileExtensions => new() { ".custom" };
    
    public async Task<List<DocumentChunk>> ProcessDocument(
 string filePath,
        int maxChunkSize = 500,
        CancellationToken cancellationToken = default)
    {
        // Your custom processing logic
   var chunks = new List<DocumentChunk>();
        // ... parse and chunk the document
        return chunks;
    }
}

// Register in DI
builder.Services.AddSingleton<IDocumentProcessor, CustomDocumentProcessor>();
```

### Custom Cloud File Resolver

Implement `ICloudFileResolver` for custom cloud storage:

```csharp
public class S3FileResolver : ICloudFileResolver
{
    public bool CanResolve(string input)
    {
        return input.StartsWith("s3://");
    }
 
    public async Task<ResolvedCloudFile> ResolveAsync(
        string input,
        CancellationToken ct = default)
    {
        // Download from S3 to temp location
        var localPath = await DownloadFromS3(input, ct);
        return new ResolvedCloudFile(
       localPath,
      input, // identity
     IngestionSource.Cloud);
 }
}

// Register in DI
builder.Services.AddSingleton<ICloudFileResolver, S3FileResolver>();
```

---

## Troubleshooting

### Connection Issues

#### With .NET Aspire

**Service not discovered:**
```bash
# Check Aspire dashboard (http://localhost:15888)
# Ensure services are running and healthy
# Verify service names match in AppHost configuration
```

**Container startup issues:**
```bash
# View container logs in Aspire dashboard
# Check Docker Desktop or container runtime
# Verify port availability (9200, 6333, 11434)
```

#### Without .NET Aspire

**Elasticsearch not reachable:**
```bash
# Test connection
curl http://localhost:9200

# Check connection string in appsettings.json
# Verify Elasticsearch is running: docker ps
```

**Qdrant not reachable:**
```bash
# Test connection
curl http://localhost:6333/collections

# Check connection string in appsettings.json
# Verify Qdrant is running: docker ps
```

**Ollama not reachable:**
```bash
# Test connection
curl http://localhost:11434/api/tags

# Check if Ollama is running
# Verify models are installed: ollama list
```

### Connection String Validation

Add connection string validation at startup:

```csharp
var elasticConnection = builder.Configuration.GetConnectionString("elasticsearch");
if (string.IsNullOrEmpty(elasticConnection))
{
    throw new InvalidOperationException("Elasticsearch connection string is not configured");
}

var qdrantConnection = builder.Configuration.GetConnectionString("qdrant");
if (string.IsNullOrEmpty(qdrantConnection))
{
    throw new InvalidOperationException("Qdrant connection string is not configured");
}
```

### Ollama Issues

**Model not found:**
```bash
# Pull required models
ollama pull llama3.2
ollama pull nomic-embed-text

# Verify models are available
ollama list
```

**Embedding dimension mismatch:**
```
Ensure VectorSize in Qdrant settings matches your embedding model output:
- nomic-embed-text: 768 dimensions
- all-minilm: 384 dimensions
- text-embedding-ada-002: 1536 dimensions
```

### Docker Compose Example

For manual deployment without Aspire:

```yaml
version: '3.8'

services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
    ports:
      - "9200:9200"
    volumes:
   - elasticsearch-data:/usr/share/elasticsearch/data

  qdrant:
    image: qdrant/qdrant:latest
    ports:
    - "6333:6333"
    volumes:
      - qdrant-data:/qdrant/storage

  ollama:
    image: ollama/ollama:latest
    ports:
      - "11434:11434"
    volumes:
      - ollama-data:/root/.ollama

  api:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ConnectionStrings__elasticsearch=http://elasticsearch:9200
      - ConnectionStrings__qdrant=http://qdrant:6333
      - ConnectionStrings__ollama=http://ollama:11434
    depends_on:
      - elasticsearch
      - qdrant
  - ollama

volumes:
  elasticsearch-data:
  qdrant-data:
  ollama-data:
```

### Performance Tips

1. **Chunk Size**: Smaller chunks (300-500 tokens) work better for RAG precision
2. **Batch Processing**: Ingest large document sets in batches for better progress tracking
3. **Caching**: The SDK uses HybridCache for progress; configure appropriately
4. **Resiliency**: Adjust retry/timeout settings based on your infrastructure
5. **Connection Pooling**: Elasticsearch and Qdrant clients use connection pooling automatically
6. **Aspire Observability**: Use Aspire dashboard to identify bottlenecks

---

## Deployment Scenarios

### Local Development with Aspire
- Use `dotnet run` with AppHost project
- Services auto-configured via service discovery
- Dashboard available at http://localhost:15888

### Docker with Aspire
- Build and publish with `dotnet publish`
- Use Aspire container publishing
- Connection strings injected via service discovery

### Kubernetes
- Use Aspire's Kubernetes manifest generation
- Or manually configure connection strings via ConfigMaps/Secrets
- Consider using Helm charts for infrastructure services

### Azure Container Apps with Aspire
- Deploy using `azd up` (Azure Developer CLI)
- Connection strings managed by Aspire
- Uses Azure service connectors automatically

### Traditional Hosting
- Configure connection strings in appsettings or Key Vault
- Ensure Elasticsearch, Qdrant, and Ollama are accessible
- Use environment variables for secrets

---

## License

Licensed under the [MIT License](https://github.com/raminesfahani/SemanticDocIngestor/blob/master/LICENSE).

---

## Support

- **Issues**: [GitHub Issues](https://github.com/raminesfahani/SemanticDocIngestor/issues)
- **Discussions**: [GitHub Discussions](https://github.com/raminesfahani/SemanticDocIngestor/discussions)
- **Documentation**: [GitHub Wiki](https://github.com/raminesfahani/SemanticDocIngestor/wiki)

---

## Acknowledgments

Built with:
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) - Cloud-ready app stack for .NET
- [Qdrant](https://qdrant.tech/) - Vector database
- [Elasticsearch](https://www.elastic.co/) - Search engine
- [Ollama](https://ollama.ai/) - Local LLM runtime
- [Polly](https://github.com/App-vNext/Polly) - Resilience and transient-fault-handling

---

**Made with ❤️ by [Ramin Esfahani](https://github.com/raminesfahani)**
