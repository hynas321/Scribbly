using WebApi.Api.Hubs.Static;
using WebApi.Api.Utilities;
using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

public partial class HubConnection : Hub
{
    private readonly IGameManager _gameManager;
    private readonly IPlayerManager _playerManager;
    private readonly IChatManager _chatManager;
    private readonly ILogger<HubConnection> _logger;

    public HubConnection(ILogger<HubConnection> logger, IGameManager gameManager, IPlayerManager playerManager, IChatManager chatManager)
    {
        _gameManager = gameManager;
        _logger = logger;
        _playerManager = playerManager;
        _chatManager = chatManager;
    }

    public override async Task OnConnectedAsync()
    {   
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {   
        try
        {
            (Player removedPlayer, string gameHash) = _playerManager.RemovePlayerByConnectionId(Context.ConnectionId);

            if (removedPlayer != null && gameHash != null)
            {
                List<PlayerScore> playerScores = _playerManager.GetPlayerScores(gameHash);
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