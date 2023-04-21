using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public class LobbyHub : Hub
{
    private readonly ILogger<LobbyHub> logger;

    public LobbyHub(ILogger<LobbyHub> logger)
    {
        this.logger = logger;
    }

    public async Task JoinLobby(string message)
    {
        var userName = Context?.User?.Identity?.Name;
        logger.LogInformation("{UserName} sent message: {Message}", userName, message);

        await Clients.All.SendAsync("PlayerJoinedLobby", userName, message);
    }

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}
