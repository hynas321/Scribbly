using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using dotnet_server.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    private readonly IGameManager _gameManager;
    private readonly ILogger<HubConnection> _logger;

    public HubConnection(ILogger<HubConnection> logger, IGameManager gameManager)
    {
        _gameManager = gameManager;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {   
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {   
        try
        {
            (Player removedPlayer, string gameHash) = _gameManager.RemovePlayer(Context.ConnectionId);

            if (removedPlayer != null && gameHash != null)
            {
                List<PlayerScore> playerScores = _gameManager.GetPlayerObjectsWithoutToken(gameHash);
                string playerListSerialized = JsonHelper.Serialize(playerScores);
                Game game = _gameManager.GetGame(gameHash);

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameHash);
                await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubMessages.OnUpdatePlayerScores, playerListSerialized);
                await SendAnnouncement(gameHash, $"{removedPlayer.Username} has left the game", BootstrapColors.Red);

                if (game.GameState.Players.Count == 0)
                {
                    _gameManager.RemoveGame(gameHash);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }
}