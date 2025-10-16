using Microsoft.AspNetCore.SignalR;
using SemanticDocIngestor.Domain.Abstractions.Hubs;
using SemanticDocIngestor.Domain.Entities.Ingestion;

namespace SemanticDocIngestor.Core.Hubs
{
    public class IngestionHub : Hub<IIngestionHubClient>
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.ReceiveMessage("Connected to Ingestion Hub");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}