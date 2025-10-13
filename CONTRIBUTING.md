# Contributing to SemanticDocIngestor

Thanks for your interest in contributing!

## How to contribute
1. Fork the repository and create a feature branch.
2. Follow the repo’s coding conventions (C#/.NET style; nullable enabled; analyzers as configured).
3. Keep PRs focused and small; include/adjust unit tests where appropriate.
4. Add/update XML docs for public APIs and update README/docs when public surfaces change.
5. Open a pull request with a clear description and rationale.

## Development workflow
- Requires .NET 9 SDK.
- Build and test:
  - `dotnet build`
  - `dotnet test`
- For UI/API changes, run the Blazor UI and API Service. If using Aspire, you can orchestrate services locally.
- Ensure Elasticsearch and any optional dependencies (Ollama, MongoDB) are reachable when testing affected features.

## Code style & formatting
- Use `dotnet format` (EditorConfig is provided).
- Prefer small, cohesive methods and DI for testability.
- Keep public API surface consistent and documented.

## Areas of contribution
- Document ingestion pipeline (chunking, metadata, dedupe)
- Source integrations (OneDrive/Google Drive)
- Keyword/vector store implementations
- Blazor UI/UX improvements
- Resiliency and observability
- Docs and samples

## Commit & PR guidelines
- Reference related issues where relevant.
- Include tests for bug fixes and new features when practical.
- Update docs (README, samples) for user-facing changes.

Thank you — your contributions make the project better!
