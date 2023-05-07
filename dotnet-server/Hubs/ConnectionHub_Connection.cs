using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    private readonly GameManager gameManager = new GameManager(25);
    private readonly HubConnectionManager hubManager = new HubConnectionManager();
    private readonly ILogger<HubConnection> logger;

    public HubConnection(ILogger<HubConnection> logger)
    {
        this.logger = logger;
    }

    public override async Task OnConnectedAsync()
    {   
        await base.OnConnectedAsync();

        hubManager.Connections++;
        
        logger.LogInformation(Context.ConnectionId);
        //logger.LogInformation($"New connection {Context.ConnectionId} established. Total connections: {hubManager.Connections}");
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {   
        await base.OnDisconnectedAsync(exception);

        if (hubManager.Connections != 0)
        {
            hubManager.Connections--;
        }

        //logger.LogInformation($"Connection {Context.ConnectionId} terminated. Total clients connected: {hubManager.Connections}");
    }
}