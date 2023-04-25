using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class GameHub : Hub
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly HubConnectionManager hubManager = new HubConnectionManager();
    private readonly ILogger<LobbyHub> logger;

    public GameHub(ILogger<LobbyHub> logger)
    {
        this.logger = logger;
    }

    public override async Task OnConnectedAsync()
    {   
        await base.OnConnectedAsync();

        hubManager.Connections++;
        
        logger.LogInformation($"GameHub: New connection {Context.ConnectionId} established. Total connections: {hubManager.Connections}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {   
        await base.OnDisconnectedAsync(exception);

        if (hubManager.Connections != 0)
        {
            hubManager.Connections--;
        }

        logger.LogInformation($"GameHub: Connection {Context.ConnectionId} terminated. Total clients connected: {hubManager.Connections}");
    }
}