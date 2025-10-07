# SemanticDocIngestor.AppHost.Tests

**SemanticDocIngestor.AppHost.Tests** — Host application for running SemanticDocIngestor services and UI.

## Usage

```bash
cd src/SemanticDocIngestor.AppHost/SemanticDocIngestor.AppHost.Tests
dotnet build
dotnet run
```

(if applicable — library projects are meant to be referenced from other projects, not run directly)

## Features
- Designed to integrate with other SemanticDocIngestor projects seamlessly.
- Written in C# targeting .NET 8.0+.
- Part of the modular SemanticDocIngestor ecosystem.

## Development notes
- Keep code clean and well-documented.
- Add/update unit tests in related `*.Tests` projects when making changes.
- Configuration is usually read from `appsettings.json` or environment variables.

---

_Last updated: 2025-09-29 17:06 UTC_
