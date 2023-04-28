using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly HubConnectionManager hubManager = new HubConnectionManager();
    private readonly ILogger<HubConnection> logger;

    public HubConnection(ILogger<HubConnection> logger)
    {
        this.logger = logger;
    }
}