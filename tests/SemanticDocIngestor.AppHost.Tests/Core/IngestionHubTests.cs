using Microsoft.AspNetCore.SignalR;
using Moq;
using SemanticDocIngestor.Core.Hubs;
using SemanticDocIngestor.Domain.Abstractions.Hubs;
using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Tests.Core;

/// <summary>
/// Unit tests for IngestionHub SignalR hub.
/// </summary>
public class IngestionHubTests
{
    private readonly Mock<IHubCallerClients<IIngestionHubClient>> _mockClients;
    private readonly Mock<IIngestionHubClient> _mockCaller;
    private readonly IngestionHub _hub;

    public IngestionHubTests()
    {
        _mockClients = new Mock<IHubCallerClients<IIngestionHubClient>>();
        _mockCaller = new Mock<IIngestionHubClient>();
        _hub = new IngestionHub
        {
            Clients = _mockClients.Object
        };

        _mockClients.Setup(c => c.Caller).Returns(_mockCaller.Object);
    }

    [Fact]
    public async Task OnConnectedAsync_SendsWelcomeMessage()
    {
        // Arrange
        _mockCaller.Setup(c => c.ReceiveMessage(It.IsAny<string>()))
                  .Returns(Task.CompletedTask);

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _mockCaller.Verify(c => c.ReceiveMessage("Connected to Ingestion Hub"), Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_CompletesSuccessfully()
    {
        // Arrange & Act
        var exception = await Record.ExceptionAsync(async () =>
            await _hub.OnDisconnectedAsync(null));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WithException_CompletesSuccessfully()
    {
        // Arrange
        var testException = new Exception("Test disconnection error");

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await _hub.OnDisconnectedAsync(testException));

        // Assert
        Assert.Null(exception);
    }
}
