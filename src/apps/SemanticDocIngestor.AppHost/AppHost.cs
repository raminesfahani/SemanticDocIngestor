using Aspire.Hosting;
using Aspire.Hosting.Elasticsearch;

var builder = DistributedApplication.CreateBuilder(args);

// Getting MongoDB parameters from env
var mongoUsername = builder.AddParameter("mongo-username");
var mongoPassword = builder.AddParameter("mongo-password");

// Getting ElasticSearch parameters from env
var elasticUsername = builder.AddParameter("elastic-username");
var elasticPassword = builder.AddParameter("elastic-password");

var mongo = builder.AddMongoDB("mongo", 27017)
                    .WithDataVolume("mongo-data")
                    .WithDbGate();

var elasticsearch = builder
                    .AddElasticsearch("elasticsearch", elasticPassword)
                    .WithDataVolume("elastic-data");

var qdrant = builder.AddQdrant("qdrant")
                    .WithDataVolume("qdrant-data");

var ollama = builder.AddOllama("ollama", 11434)
                    .WithImageTag("latest")
                    .WithDataVolume("ollama-data")
                    .WithGPUSupport();

// Optional
//ollama.AddModel("llama3"); // Download a sample ollama model

builder.AddProject<Projects.SemanticDocIngestor_AppHost_ApiService>("SemanticDocIngestor-api")
                .WithHttpHealthCheck("/health")
                .WithHttpEndpoint(name: "external", port: 5001)
                .WithReference(mongo)
                .WaitFor(mongo)
                .WithReference(elasticsearch)
                .WaitFor(elasticsearch)
                .WithReference(qdrant)
                .WaitFor(qdrant)
                .WithReference(ollama)
                .WaitFor(ollama);

//builder.AddProject<Projects.SemanticDocIngestor_AppHost_BlazorUI>("SemanticDocIngestor-ui")
//        .WithExternalHttpEndpoints()
//        .WithHttpHealthCheck("/health")
//        .WithReference(mongo)
//        .WaitFor(mongo)
//        .WithReference(qdrant)
//        .WaitFor(qdrant)
//        .WithReference(ollama)
//        .WaitFor(ollama);

builder.Build().Run();
