# Support

Thank you for using **SemanticDocIngestor**! We're here to help you get the most out of the SDK and reference applications.

---

## ?? Documentation

Before seeking support, please check our comprehensive documentation:

### Primary Resources
- **[SDK Guide](src/sdk/SemanticDocIngestor.Core/README.md)** - Complete SDK documentation with:
  - Installation and quick start guides
  - Configuration options (Aspire and manual)
  - Connection string management
  - API reference with examples
  - Troubleshooting guides
  - Deployment scenarios
  
- **[Main README](README.md)** - Solution overview, architecture, and getting started
- **[Documentation Summary](DOCUMENTATION_SUMMARY.md)** - Package preparation and documentation checklist
- **[Contributing Guide](CONTRIBUTING.md)** - How to contribute to the project

### Troubleshooting Guides
- **Connection Issues**: See [SDK Guide - Troubleshooting](src/sdk/SemanticDocIngestor.Core/README.md#troubleshooting)
- **Ollama Setup**: See [SDK Guide - Ollama Issues](src/sdk/SemanticDocIngestor.Core/README.md#ollama-issues)
- **Performance Tips**: See [SDK Guide - Performance](src/sdk/SemanticDocIngestor.Core/README.md#performance-tips)
- **Deployment Scenarios**: See [SDK Guide - Deployment](src/sdk/SemanticDocIngestor.Core/README.md#deployment-scenarios)

---

## ?? Community Support

### GitHub Discussions
The best place for questions, ideas, and community interaction.

**[Visit GitHub Discussions](https://github.com/raminesfahani/SemanticDocIngestor/discussions)**

- ?? **Ideas & Feature Requests**: Share your ideas for new features
- ? **Q&A**: Ask questions and get help from the community
- ?? **Show and Tell**: Share what you've built with SemanticDocIngestor
- ?? **Announcements**: Stay updated with project news

### Stack Overflow
For technical questions with detailed code examples.

- Tag your questions with: `semanticdocingestor`, `.net`, `rag`, `vector-search`
- **[Browse existing questions](https://stackoverflow.com/questions/tagged/semanticdocingestor)**

---

## ?? Bug Reports

If you've found a bug, please help us fix it by opening a detailed issue.

**[Open a Bug Report](https://github.com/raminesfahani/SemanticDocIngestor/issues/new?template=bug_report.md)**

### What to Include

Please provide as much detail as possible:

#### 1. Environment
```
- OS: Windows 11 / macOS 14 / Ubuntu 22.04
- .NET SDK Version: 9.0.x
- SemanticDocIngestor.Core Version: 1.0.7
- Elasticsearch Version: 8.11.0
- Qdrant Version: 1.7.0
- Ollama Version: 0.1.x (if applicable)
- Deployment: Local / Docker / Kubernetes / Azure
```

#### 2. Description
- What you tried to do
- What you expected to happen
- What actually happened

#### 3. Steps to Reproduce
Provide minimal, complete code to reproduce the issue:

```csharp
// Example reproduction code
var documentIngestor = serviceProvider.GetRequiredService<IDocumentIngestorService>();
await documentIngestor.IngestDocumentsAsync(new[] { "test.pdf" });
// Error occurs here...
```

#### 4. Logs and Errors
- Full error messages and stack traces
- Relevant log output (use structured logging)
- Screenshots if applicable

#### 5. Configuration
Share your relevant configuration (remove sensitive data):

```json
{
  "AppSettings": {
    "Ollama": {
    "ChatModel": "llama3.2",
      "EmbeddingModel": "nomic-embed-text"
    }
  }
}
```

---

## ? Feature Requests

Have an idea for a new feature or improvement?

**[Open a Feature Request](https://github.com/raminesfahani/SemanticDocIngestor/issues/new?template=feature_request.md)**

### What to Include
- **Use Case**: Describe the problem you're trying to solve
- **Proposed Solution**: How you envision the feature working
- **Alternatives**: Other solutions you've considered
- **Examples**: Code examples or mockups if applicable

---

## ?? Security Issues

**Do NOT open public issues for security vulnerabilities.**

For security concerns, please see our [Security Policy](SECURITY.md) or contact the maintainers directly:

- **Email**: [Create a private security advisory](https://github.com/raminesfahani/SemanticDocIngestor/security/advisories/new)
- We aim to respond within 48 hours
- We'll work with you to understand and address the issue

---

## ?? Commercial & Enterprise Support

### Self-Service Resources
- **[SDK Guide](src/sdk/SemanticDocIngestor.Core/README.md)** - Comprehensive documentation
- **[Architecture Guide](README.md#architecture--design)** - System design and extensibility
- **[Deployment Guides](src/sdk/SemanticDocIngestor.Core/README.md#deployment-scenarios)** - Production deployment options

### Custom Support Options
For enterprise deployments or custom development:

- **Priority Support**: Faster response times for critical issues
- **Custom Development**: Features specific to your use case
- **Consulting**: Architecture review, performance optimization, integration assistance
- **Training**: Workshops and training sessions for your team
- **SLA Options**: Service level agreements for production systems

To discuss enterprise support options:
1. Open an issue with the `enterprise-support` label
2. Or contact via email: [See GitHub profile](https://github.com/raminesfahani)

We'll respond with available options and pricing.

---

## ?? Direct Contact

For issues that don't fit the above categories:

- **GitHub**: [@raminesfahani](https://github.com/raminesfahani)
- **Email**: See [GitHub profile](https://github.com/raminesfahani)

**Response Times:**
- Community Issues: Best effort (usually 1-3 business days)
- Bug Reports: 1-5 business days depending on severity
- Security Issues: Within 48 hours
- Enterprise Support: As per SLA (if applicable)

---

## ?? Contributing

Want to help others by improving the project?

- **Fix Bugs**: Find issues labeled [`good first issue`](https://github.com/raminesfahani/SemanticDocIngestor/labels/good%20first%20issue)
- **Improve Documentation**: Help make the docs even better
- **Answer Questions**: Help others in Discussions or Stack Overflow
- **Share Examples**: Submit example applications or use cases

See our **[Contributing Guide](CONTRIBUTING.md)** for details.

---

## ?? Additional Resources

### Official Resources
- **NuGet Package**: https://www.nuget.org/packages/SemanticDocIngestor.Core
- **GitHub Repository**: https://github.com/raminesfahani/SemanticDocIngestor
- **Release Notes**: [GitHub Releases](https://github.com/raminesfahani/SemanticDocIngestor/releases)

### Related Technologies
- **[.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)**
- **[Qdrant Documentation](https://qdrant.tech/documentation/)**
- **[Elasticsearch Documentation](https://www.elastic.co/guide/)**
- **[Ollama Documentation](https://github.com/ollama/ollama/blob/main/README.md)**

### Community Content
- Sample projects using SemanticDocIngestor (coming soon)
- Blog posts and tutorials (coming soon)
- Video walkthroughs (coming soon)

---

## ?? Show Your Support

If SemanticDocIngestor has been helpful:

- ? **Star the repository** on GitHub
- ?? **Share** with others who might benefit
- ?? **Write** about your experience (blog, social media)
- ?? **Engage** in discussions and help others
- ?? **Contribute** code, docs, or examples

---

## ?? Project Health

Check the current state of the project:

- **Build Status**: [![Build](https://github.com/raminesfahani/SemanticDocIngestor/actions/workflows/nuget-packages.yml/badge.svg)](https://github.com/raminesfahani/SemanticDocIngestor/actions)
- **Open Issues**: [View Issues](https://github.com/raminesfahani/SemanticDocIngestor/issues)
- **Latest Release**: [View Releases](https://github.com/raminesfahani/SemanticDocIngestor/releases)
- **Package Version**: [![NuGet](https://img.shields.io/nuget/v/SemanticDocIngestor.Core)](https://www.nuget.org/packages/SemanticDocIngestor.Core)

---

Thank you for being part of the SemanticDocIngestor community! ??
