# SemanticDocIngestor.Persistence

> Part of the [SemanticDocIngestor.Core](https://github.com/raminesfahani/SemanticDocIngestor) SDK

## 📦 NuGet

[![NuGet](https://img.shields.io/nuget/v/SemanticDocIngestor.Persistence)](https://www.nuget.org/packages/SemanticDocIngestor.Persistence)

**SemanticDocIngestor.Persistence** provides a clean and extensible MongoDB-based persistence layer for the SemanticDocIngestor ecosystem. It powers features like chat history, conversation storage, and data caching in SemanticDocIngestor-based applications.

## ✨ Features

- MongoDB support for chat conversations and messages
- Repository pattern abstraction for testability and clean architecture
- Pluggable via DI using SemanticDocIngestor.Core
- Optional caching support
- Built for high-performance and scalable data access

## 📦 Installation

To use `SemanticDocIngestor.Persistence`, install the required NuGet package, or include the project reference in your solution.

```bash
dotnet add package SemanticDocIngestor.Extensions
```

## 🔧 Service Registration

In your `Program.cs` or inside a service registration method (already is done in SemanticDocIngestor.Core):

```csharp
services.AddSemanticDocIngestorMemoryCache();
services.AddSemanticDocIngestorMongoDb(configuration);
```

## 📄 License

MIT License