
var builder = DistributedApplication.CreateBuilder(args);

// Getting ElasticSearch parameters from env
var elasticPassword = builder.AddParameter("elastic-password");

var elasticsearch = builder
                    .AddElasticsearch("elasticsearch", elasticPassword)
                    //.WithLifetime(ContainerLifetime.Persistent)
                    .WithDataVolume("elastic-data");

var qdrant = builder.AddQdrant("qdrant")
                    //.WithLifetime(ContainerLifetime.Persistent)
                    .WithDataVolume("qdrant-data");

var ollama = builder.AddOllama("ollama", 11434)
                    .WithImageTag("latest")
                    .WithDataVolume("ollama-data")
                    //.WithLifetime(ContainerLifetime.Persistent)
                    .WithGPUSupport();

builder.AddProject<Projects.SemanticDocIngestor_AppHost_ApiService>("SemanticDocIngestor-api")
                .WithHttpHealthCheck("/health")
                .WithHttpEndpoint(name: "external", port: 5001)
                .WithReference(elasticsearch)
                .WaitFor(elasticsearch)
                .WithReference(qdrant)
                .WaitFor(qdrant)
                .WithReference(ollama)
                .WaitFor(ollama);

builder.Build().Run();
