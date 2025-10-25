# Contributing to SemanticDocIngestor

First off, thank you for considering contributing to SemanticDocIngestor! 🎉

It's people like you that make SemanticDocIngestor such a great tool. We welcome contributions of all kinds: bug fixes, new features, documentation improvements, and more.

---

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Development Workflow](#development-workflow)
- [Coding Guidelines](#coding-guidelines)
- [Documentation Guidelines](#documentation-guidelines)
- [Pull Request Process](#pull-request-process)
- [Areas of Contribution](#areas-of-contribution)

---

## 📜 Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

---

## 🤝 How Can I Contribute?

### 🐛 Reporting Bugs

Before creating bug reports, please check the [existing issues](https://github.com/raminesfahani/SemanticDocIngestor/issues) to avoid duplicates.

When creating a bug report, include:
- **Clear title and description**
- **Steps to reproduce** the behavior
- **Expected behavior** vs actual behavior
- **Environment details** (OS, .NET version, package versions)
- **Code samples** or error logs
- **Screenshots** if applicable

**[Report a Bug](https://github.com/raminesfahani/SemanticDocIngestor/issues/new?template=bug_report.md)**

### ✨ Suggesting Features

Feature suggestions are welcome! Before creating a feature request:
- Check if the feature already exists
- Review existing feature requests
- Consider if it fits the project scope

When suggesting a feature:
- **Use case**: Explain the problem you're solving
- **Proposed solution**: How should it work?
- **Alternatives**: What other solutions did you consider?
- **Examples**: Code snippets or mockups

**[Request a Feature](https://github.com/raminesfahani/SemanticDocIngestor/issues/new?template=feature_request.md)**

### 📝 Improving Documentation

Documentation improvements are always appreciated:
- Fix typos or clarify explanations
- Add examples or use cases
- Improve API documentation
- Add tutorials or guides
- Translate documentation

### 💻 Code Contributions

We accept pull requests for:
- Bug fixes
- New features (discuss in an issue first for large features)
- Performance improvements
- Code quality improvements
- Test coverage improvements

---

## 🛠️ Development Setup

### Prerequisites

Ensure you have the following installed:

```bash
# Required
.NET 9 SDK or later
Docker Desktop (for infrastructure services)

# Recommended
Visual Studio 2022 or VS Code with C# extension
Git
```

### Fork and Clone

1. **Fork** the repository on GitHub
2. **Clone** your fork locally:

```bash
git clone https://github.com/YOUR_USERNAME/SemanticDocIngestor.git
cd SemanticDocIngestor
```

3. **Add upstream** remote:

```bash
git remote add upstream https://github.com/raminesfahani/SemanticDocIngestor.git
```

### Install Dependencies

```bash
# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run tests to ensure everything works
dotnet test
```

### Start Infrastructure Services

#### Using .NET Aspire (Recommended)

```bash
cd src/apps/SemanticDocIngestor.AppHost
dotnet run

# Access Aspire Dashboard at http://localhost:15888
```

#### Using Docker Compose

```bash
docker-compose up -d
```

---

## 🔄 Development Workflow

### 1. Create a Branch

Always create a new branch for your work:

```bash
# For bug fixes
git checkout -b fix/issue-123-description

# For features
git checkout -b feature/awesome-new-feature

# For documentation
git checkout -b docs/improve-readme
```

### 2. Make Your Changes

- Write clean, readable code
- Follow the coding guidelines (see below)
- Add or update tests as needed
- Update documentation for public API changes
- Add XML documentation comments

### 3. Test Your Changes

```bash
# Build the solution
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/SemanticDocIngestor.AppHost.Tests

# Check code formatting
dotnet format --verify-no-changes
```

### 4. Commit Your Changes

Write clear, descriptive commit messages:

```bash
git add .
git commit -m "Fix: Resolve issue with OneDrive file resolution (#123)

- Fixed path parsing for OneDrive share links
- Added unit tests for edge cases
- Updated documentation with examples"
```

**Commit Message Format:**
```
<type>: <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### 5. Push and Create Pull Request

```bash
# Push your branch
git push origin your-branch-name

# Create a pull request on GitHub
```

---

## 📏 Coding Guidelines

### C# Style Guide

Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

```csharp
// ✅ Good
public class DocumentProcessor : IDocumentProcessor
{
    private readonly ILogger<DocumentProcessor> _logger;
    
    public DocumentProcessor(ILogger<DocumentProcessor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<List<DocumentChunk>> ProcessDocument(
        string filePath, 
        int maxChunkSize = 500,
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}

// ❌ Bad - Inconsistent naming and formatting
public class documentprocessor : IDocumentProcessor
{
    private ILogger logger;
    public async Task<List<DocumentChunk>> processDocument(string path,int size,CancellationToken ct){
        // Implementation
    }
}
```

### Code Formatting

Use the provided `.editorconfig`:

```bash
# Format code
dotnet format

# Check formatting
dotnet format --verify-no-changes
```

### Best Practices

1. **Nullable Reference Types**: Enabled by default, handle nullability properly
2. **Async/Await**: Use `async`/`await` for I/O operations
3. **Dependency Injection**: Use constructor injection
4. **SOLID Principles**: Keep classes focused and cohesive
5. **Error Handling**: Use specific exceptions with meaningful messages
6. **Logging**: Use structured logging with Serilog

```csharp
// ✅ Good - Structured logging
_logger.LogInformation(
    "Processing document {FileName} with {ChunkCount} chunks",
    fileName, 
    chunkCount);

// ❌ Bad - String interpolation in logs
_logger.LogInformation($"Processing document {fileName} with {chunkCount} chunks");
```

### Testing Guidelines

- Write unit tests for new features
- Maintain or improve test coverage
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)

```csharp
[Fact]
public async Task IngestDocumentsAsync_WithValidPdf_ShouldCreateChunks()
{
    // Arrange
    var processor = new DocumentProcessor();
    var filePath = "test.pdf";
    
    // Act
    var chunks = await processor.ProcessDocument(filePath);
    
    // Assert
    Assert.NotEmpty(chunks);
    Assert.All(chunks, chunk => Assert.NotEmpty(chunk.Content));
}
```

---

## 📚 Documentation Guidelines

### XML Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Ingests multiple documents from various sources and stores them for search.
/// </summary>
/// <param name="documentPaths">
/// Collection of document paths or URIs. Supports local paths, OneDrive URIs, and Google Drive URIs.
/// </param>
/// <param name="maxChunkSize">Maximum size of each text chunk in tokens (default: 500).</param>
/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
/// <returns>A task representing the asynchronous ingestion operation.</returns>
/// <exception cref="ArgumentNullException">Thrown when documentPaths is null.</exception>
/// <exception cref="DirectoryNotFoundException">Thrown when a local path doesn't exist.</exception>
public async Task IngestDocumentsAsync(
    IEnumerable<string> documentPaths,
    int maxChunkSize = 500,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### README Updates

Update documentation when:
- Adding new public APIs
- Changing configuration options
- Adding new features
- Modifying behavior

Keep documentation:
- **Clear**: Easy to understand
- **Concise**: No unnecessary details
- **Current**: Always up-to-date
- **Complete**: Cover all scenarios

---

## 🔀 Pull Request Process

### Before Submitting

- [ ] Code builds without errors
- [ ] All tests pass
- [ ] Code is formatted (`dotnet format`)
- [ ] XML documentation is added/updated
- [ ] README/docs are updated if needed
- [ ] Commit messages are clear
- [ ] Branch is up-to-date with main

```bash
# Update your branch with latest main
git fetch upstream
git rebase upstream/main
```

### PR Description Template

```markdown
## Description
Brief description of the changes

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Related Issues
Fixes #123

## Changes Made
- Added X feature
- Fixed Y bug
- Updated Z documentation

## Testing
Describe how you tested the changes

## Screenshots (if applicable)
Add screenshots for UI changes

## Checklist
- [ ] My code follows the style guidelines
- [ ] I have performed a self-review
- [ ] I have commented my code where needed
- [ ] I have updated documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests
- [ ] New and existing tests pass
```

### Review Process

1. **Automated Checks**: CI/CD pipeline runs automatically
2. **Code Review**: Maintainers review your code
3. **Feedback**: Address any requested changes
4. **Approval**: Once approved, your PR will be merged

### After Merge

- Your contribution will be included in the next release
- You'll be added to the contributors list
- Thank you! 🎉

---

## 🎯 Areas of Contribution

### High Priority

- **Bug Fixes**: Issues labeled [`bug`](https://github.com/raminesfahani/SemanticDocIngestor/labels/bug)
- **Documentation**: Issues labeled [`documentation`](https://github.com/raminesfahani/SemanticDocIngestor/labels/documentation)
- **Good First Issues**: Perfect for beginners [`good first issue`](https://github.com/raminesfahani/SemanticDocIngestor/labels/good%20first%20issue)

### Feature Areas

#### Document Processing
- Support for additional file formats (HTML, RTF, CSV)
- Advanced chunking strategies (semantic, sliding window)
- OCR integration for scanned documents
- Metadata extraction improvements

#### Search & RAG
- Custom reranking implementations
- Advanced RAG techniques (HyDE, RAG-Fusion)
- Query optimization
- Result caching strategies

#### Cloud Integrations
- Azure Blob Storage resolver
- AWS S3 resolver
- Dropbox integration
- SharePoint improvements

#### Infrastructure
- Additional vector store implementations (Pinecone, Weaviate)
- Alternative embedding models
- Database migration tools
- Performance optimizations

#### UI/UX
- Blazor UI improvements
- Admin dashboard
- Search analytics
- Document preview

#### DevOps & Testing
- Additional integration tests
- Performance benchmarks
- CI/CD improvements
- Docker optimizations

#### Documentation & Samples
- Tutorial videos
- Sample applications
- Architecture decision records
- API usage examples

---

## 🎓 Learning Resources

### Project-Specific
- **[SDK Guide](src/sdk/SemanticDocIngestor.Core/README.md)** - Complete SDK documentation
- **[Architecture Overview](README.md#architecture--design)** - System design
- **[Documentation Summary](DOCUMENTATION_SUMMARY.md)** - Package details

### Technologies
- **[.NET Aspire](https://learn.microsoft.com/dotnet/aspire/)**
- **[Qdrant Documentation](https://qdrant.tech/documentation/)**
- **[Elasticsearch Guide](https://www.elastic.co/guide/)**
- **[Ollama Documentation](https://github.com/ollama/ollama)**

---

## ❓ Questions?

- **General Questions**: [GitHub Discussions](https://github.com/raminesfahani/SemanticDocIngestor/discussions)
- **Bug Reports**: [GitHub Issues](https://github.com/raminesfahani/SemanticDocIngestor/issues)
- **Security Issues**: [Security Policy](SECURITY.md)
- **Other**: [Support Guide](SUPPORT.md)

---

## 🙏 Recognition

All contributors will be recognized in:
- GitHub contributors page
- Release notes
- Project documentation

Your contributions, big or small, make a difference!

---

## 📄 License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).

---

**Thank you for contributing to SemanticDocIngestor! 🚀**
