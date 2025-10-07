using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Getting MongoDB parameters from env
var username = builder.AddParameter("mongo-username");
var password = builder.AddParameter("mongo-password");

var mongo = builder.AddMongoDB("mongo", 27017)
                    .WithDataVolume("mongo-data")
                    .WithDbGate();

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
                .WithReference(ollama)
                .WaitFor(ollama);

builder.AddProject<Projects.SemanticDocIngestor_AppHost_BlazorUI>("SemanticDocIngestor-ui")
        .WithExternalHttpEndpoints()
        .WithHttpHealthCheck("/health")
        .WithReference(mongo)
        .WaitFor(mongo)
        .WithReference(ollama)
        .WaitFor(ollama);

builder.Build().Run();
