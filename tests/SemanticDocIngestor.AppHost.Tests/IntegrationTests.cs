using Aspire.Hosting;
using SemanticDocIngestor.Domain.Abstracts.Documents;
using SemanticDocIngestor.Domain.Contracts;
using SemanticDocIngestor.AppHost.ApiService.Models;
using SemanticDocIngestor.Core.Ollama;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using MongoDB.Bson;
using Moq;
using Ollama;
using System.Net.Http.Json;
using System.Threading;
using Xunit.v3;

namespace SemanticDocIngestor.AppHost.Tests;

[Collection("SequentialTests")]
public class IntegrationTests : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(1);

    private DistributedApplication? _appHost;
    private CancellationToken _cancellationToken;

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        (_appHost, _cancellationToken) = await SetupAppTestBuilder<Projects.SemanticDocIngestor_AppHost>();
    }

    private static async Task<(DistributedApplication app, CancellationToken cancellationToken)> SetupAppTestBuilder<T>() where T : class
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<T>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        return (app, cancellationToken);
    }

    [Fact]
    public async Task GetBlazorUIResourceRootReturnsOkStatusCode()
    {
        // Act
        var httpClient = _appHost.CreateHttpClient("SemanticDocIngestor-ui");
        await _appHost.ResourceNotifications.WaitForResourceHealthyAsync("SemanticDocIngestor-ui", _cancellationToken).WaitAsync(DefaultTimeout, _cancellationToken);
        var response = await httpClient.GetAsync("/", _cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWebApiCoversationsReturnsOkStatusCode()
    {
        // Act
        var httpClient = _appHost.CreateHttpClient("SemanticDocIngestor-api");
        await _appHost.ResourceNotifications.WaitForResourceHealthyAsync("SemanticDocIngestor-api", _cancellationToken).WaitAsync(DefaultTimeout, _cancellationToken);
        var response = await httpClient.GetAsync("/Chat/conversations", _cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWebApiOllamaInstalledModelsReturnsOkStatusCode()
    {
        // Act
        var httpClient = _appHost.CreateHttpClient("SemanticDocIngestor-api");
        await _appHost.ResourceNotifications.WaitForResourceHealthyAsync("SemanticDocIngestor-api", _cancellationToken).WaitAsync(DefaultTimeout, _cancellationToken);
        var response = await httpClient.GetAsync("/Ollama/models/installed", _cancellationToken).WithTimeout(DefaultTimeout);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ModelsResponse?>(cancellationToken: _cancellationToken);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetWebApiOllamaAvailableModelsReturnsOkStatusCode()
    {
        // Act
        var httpClient = _appHost.CreateHttpClient("SemanticDocIngestor-api");
        await _appHost.ResourceNotifications.WaitForResourceHealthyAsync("SemanticDocIngestor-api", _cancellationToken).WaitAsync(DefaultTimeout, _cancellationToken);

        var response = await httpClient.GetAsync("/Ollama/models/available", _cancellationToken).WithTimeout(DefaultTimeout);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<Ollama.Model>>(cancellationToken: _cancellationToken);
        Assert.NotNull(result);
    }

    async ValueTask System.IAsyncDisposable.DisposeAsync()
    {
        await _appHost.StopAsync(_cancellationToken);
        await _appHost.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}
