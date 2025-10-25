# SemanticDocIngestor.Tests

Comprehensive unit and integration tests for the SemanticDocIngestor.Core SDK.

## Test Coverage

This test project provides extensive coverage for the SemanticDocIngestor.Core SDK, including:

### Core Services Tests

#### `DocumentIngestorServiceTests.cs`
- ? Document ingestion with valid files
- ? Folder ingestion with non-existent directories
- ? Hybrid search functionality
- ? RAG response generation
- ? Progress tracking
- ? Flush operations
- ? Listing ingested documents
- ? Event handling (OnProgress, OnCompleted)
- ? Proper disposal

#### `DocumentIngestorServiceIntegrationTests.cs`
- ? Cloud file resolver integration
- ? Multi-file ingestion with progress events
- ? Existing chunk deletion before re-ingestion
- ? Cancellation token handling
- ? Result deduplication
- ? Streaming RAG responses
- ? SignalR client notifications
- ? Empty query handling
- ? Edge cases and error scenarios

#### `ServiceCollectionExtensionsTests.cs`
- ? Dependency injection configuration
- ? Service registration validation
- ? Singleton lifetime verification
- ? AutoMapper configuration
- ? Configuration validation

#### `IngestionHubTests.cs`
- ? SignalR hub connection handling
- ? Welcome message on connect
- ? Disconnection with/without exceptions

### Domain Entity Tests

#### `IngestionProgressTests.cs`
- ? Default values
- ? Property setters
- ? Percentage calculations
- ? Progress tracking scenarios

#### `DocumentChunkTests.cs`
- ? Default values
- ? Property initialization
- ? Embedding dimensions
- ? Metadata population
- ? All ingestion sources support
- ? Empty embedding handling

#### `IngestionMetadataTests.cs`
- ? Default values
- ? All property setters
- ? Excel-specific metadata (SheetName, RowIndex)
- ? Page number handling
- ? Cloud source information
- ? Nullable properties

#### `DocumentChunkDtoTests.cs`
- ? DTO default values
- ? AutoMapper bidirectional mapping
- ? Configuration validity
- ? SearchAndGetRagResponseDto functionality
- ? DocumentRepoItemDto timestamps
- ? Multiple reference handling

#### `ResolvedCloudFileTests.cs`
- ? Record creation
- ? All ingestion sources
- ? Local vs identity path differences
- ? Value equality (record semantics)
- ? Async disposal

#### `IngestionSourceTests.cs`
- ? Enum value verification
- ? String conversion (ToString/Parse)
- ? All values defined
- ? Switch expression support

### Integration Tests

#### `IntegrationTests.cs`
- ? .NET Aspire integration
- ? Full application stack testing
- ? Service health checks

## Running Tests

### All Tests
```bash
dotnet test
```

### Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~DocumentIngestorServiceTests"
```

### With Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Verbose Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Structure

```
SemanticDocIngestor.Tests/
??? Core/
?   ??? DocumentIngestorServiceTests.cs
?   ??? DocumentIngestorServiceIntegrationTests.cs
?   ??? ServiceCollectionExtensionsTests.cs
?   ??? IngestionHubTests.cs
??? Domain/
?   ??? DocumentChunkTests.cs
?   ??? DocumentChunkDtoTests.cs
?   ??? IngestionMetadataTests.cs
?   ??? IngestionProgressTests.cs
?   ??? IngestionSourceTests.cs
?   ??? ResolvedCloudFileTests.cs
??? IntegrationTests.cs
```

## Testing Frameworks

- **xUnit v3** - Testing framework
- **Moq** - Mocking framework
- **Aspire.Hosting.Testing** - Integration testing for .NET Aspire
- **AutoMapper** - Object mapping testing
- **Coverlet** - Code coverage

## Mock Objects

The tests use Moq to create mock implementations of:
- `IDocumentProcessor` - Document processing operations
- `IVectorStore` - Vector database operations
- `IElasticStore` - Elasticsearch operations
- `IRagService` - RAG and LLM operations
- `ICloudFileResolver` - Cloud storage resolution
- `HybridCache` - Caching operations
- `IHubContext<T>` - SignalR hub operations
- `IMapper` - AutoMapper operations

## Test Categories

### Unit Tests
Fast, isolated tests that verify individual components without external dependencies.

### Integration Tests
Tests that verify multiple components working together, including .NET Aspire integration.

## Best Practices

1. **Arrange-Act-Assert Pattern**: All tests follow the AAA pattern for clarity
2. **Descriptive Names**: Test names clearly describe what is being tested
3. **One Assert Per Concept**: Each test verifies a single concept or behavior
4. **Mock Verification**: Verify that mocked methods are called as expected
5. **Edge Cases**: Include tests for boundary conditions and error scenarios
6. **Async/Await**: Proper async test patterns throughout

## Code Coverage Goals

- **Core Services**: 90%+ coverage
- **Domain Entities**: 95%+ coverage
- **DTOs**: 95%+ coverage
- **Integration Points**: 80%+ coverage

## Contributing

When adding new tests:

1. Follow the existing test structure
2. Use descriptive test names
3. Include XML documentation comments for test classes
4. Group related tests in the same file
5. Use Theory/InlineData for parameterized tests
6. Ensure tests are deterministic and isolated
7. Clean up resources in Dispose methods when needed

## Continuous Integration

These tests are designed to run in CI/CD pipelines:

```yaml
# Example GitHub Actions workflow
- name: Run Tests
  run: dotnet test --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"

- name: Upload Coverage
  uses: codecov/codecov-action@v3
  with:
    files: ./coverage.cobertura.xml
```

## Known Limitations

1. **External Services**: Some tests mock external services (Elasticsearch, Qdrant, Ollama). For true integration testing, use the `IntegrationTests.cs` with .NET Aspire.
2. **File System**: File system operations are mocked or use temporary files that are cleaned up.
3. **Network Operations**: Cloud file resolvers are mocked to avoid network dependencies in unit tests.

## Future Enhancements

- [ ] Performance benchmarks
- [ ] Load testing scenarios
- [ ] Additional cloud provider tests
- [ ] More complex RAG scenarios
- [ ] Error injection testing
- [ ] Resilience testing (circuit breaker, retry)

## Related Documentation

- [Core SDK Documentation](../../src/sdk/SemanticDocIngestor.Core/README.md)
- [Contributing Guide](../../CONTRIBUTING.md)
- [Main README](../../README.md)

---

**Test Coverage Report**: Run `dotnet test --collect:"XPlat Code Coverage"` to generate detailed coverage reports.
