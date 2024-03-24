using Dotnet.Server.JsonConfig;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    private readonly GameManager gameManager = new GameManager();
    private readonly ILogger<HubConnection> logger;

    public HubConnection(ILogger<HubConnection> logger)
    {
        this.logger = logger;
    }

    public override async Task OnConnectedAsync()
    {   
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {   
        (Player removedPlayer, string gameHash) = gameManager.RemovePlayer(Context.ConnectionId);

        if (removedPlayer != null && gameHash != null)
        {
            List<PlayerScore> playerScores = gameManager.GetPlayerObjectsWithoutToken(gameHash);
            string playerListSerialized = JsonHelper.Serialize(playerScores);
            Game game = gameManager.GetGame(gameHash);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameHash);
            await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubEvents.OnUpdatePlayerScores, playerListSerialized);
            await SendAnnouncement(gameHash, $"{removedPlayer.Username} has left the game", BootstrapColors.Red);

            if (game.GameState.Players.Count == 0)
            {   
                gameManager.RemoveGame(gameHash);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}