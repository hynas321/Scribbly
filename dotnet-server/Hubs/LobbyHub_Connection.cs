using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LobbyHub : Hub
{
    private readonly LobbyManager lobbyManager = new LobbyManager();
    private readonly ILogger<LobbyHub> logger;

    public LobbyHub(ILogger<LobbyHub> logger)
    {
        this.logger = logger;
    }

    public override async Task OnConnectedAsync()
    {   
        logger.LogInformation($"New client connected: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {   
        logger.LogInformation($"Client disconnected: {Context.ConnectionId}");

        await base.OnDisconnectedAsync(exception);
    }
}