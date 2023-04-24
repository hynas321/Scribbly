using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LobbyHub : Hub
{
    private readonly LobbiesManager lobbiesManager = new LobbiesManager(25);
    private readonly HubConnectionManager hubManager = new HubConnectionManager();
    private readonly ILogger<LobbyHub> logger;

    public LobbyHub(ILogger<LobbyHub> logger)
    {
        this.logger = logger;
    }

    public override async Task OnConnectedAsync()
    {   
        await base.OnConnectedAsync();

        hubManager.Connections++;
        
        logger.LogInformation($"LobbyHub: New connection {Context.ConnectionId} established. Total connections: {hubManager.Connections}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {   
        await base.OnDisconnectedAsync(exception);

        hubManager.Connections--;

        logger.LogInformation($"LobbyHub: Connection {Context.ConnectionId} terminated. Total clients connected: {hubManager.Connections}");
    }
}