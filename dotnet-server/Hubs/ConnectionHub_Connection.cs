using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class ConnectionHub : Hub
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly HubConnectionManager hubManager = new HubConnectionManager();
    private readonly ILogger<ConnectionHub> logger;

    public ConnectionHub(ILogger<ConnectionHub> logger)
    {
        this.logger = logger;
    }
}