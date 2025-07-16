using WebApi.Api.Hubs.Static;
using WebApi.Api.Utilities;
using WebApi.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using WebApi.Api.Models.DTO;

namespace WebApi.Hubs;

public partial class HubConnection : Hub
{

    [HubMethodName(HubMessages.JoinGame)]
    public async Task JoinGame(string gameHash, string token, string username)
    {
        if (username.Length < 1)
        {
            string errorMessage = "Username is too short";

            _logger.LogError($"Game #{gameHash} JoinGame: {errorMessage}");
            await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnJoinGameError, errorMessage);
            return;
        }

        Game game = _gameManager.GetGame(gameHash);

        if (game is null)
        {
            string errorMessage = "Game does not exist";

            _logger.LogError($"Game #{gameHash} JoinGame: {errorMessage}");
            await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnJoinGameError, errorMessage);
            return;
        }

        Player player;
            
        if (token == game.HostToken)
        {
            player = new Player()
            {
                Username = username,
                Score = 0,
                Token = game.HostToken,
                ConnectionId = Context.ConnectionId
            };

            game.GameState.HostPlayerUsername = player.Username;
            _playerManager.AddPlayer(gameHash, player);
        }
        else if (!_playerManager.CheckIfPlayerExistsByUsername(gameHash, username))
        {
            player = new Player()
            {
                Username = username,
                Score = 0,
                Token = _hashManager.GenerateUserHash(),
                ConnectionId = Context.ConnectionId
            };

            _playerManager.AddPlayer(gameHash, player);
        }
        else if (_playerManager.CheckIfPlayerExistsByUsername(gameHash, username))
        {
            string errorMessage = "User with your username already exists";

            _logger.LogError($"Game #{gameHash} JoinGame: {errorMessage}");
            await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnJoinGameError, errorMessage);
            return;
        }
        else
        {
            string errorMessage = "Unexpected error, try again";

            _logger.LogError($"Game #{gameHash} JoinGame: {errorMessage}");
            await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnJoinGameError, errorMessage);
            return;
        }

        List<PlayerScore> playerScores = _playerManager.GetPlayerScores(gameHash);

        GameSettings settingsClient = new()
        {
            DrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds,
            RoundsCount = game.GameSettings.RoundsCount,
            WordLanguage = game.GameSettings.WordLanguage
        };

        GameStateDTO stateClient = new()
        {
            CurrentDrawingTimeSeconds = game.GameState.CurrentDrawingTimeSeconds,
            CurrentRound = game.GameState.CurrentRound,
            HiddenSecretWord = game.GameState.HiddenSecretWord,
            DrawingPlayerUsername = game.GameState.DrawingPlayerUsername,
            HostPlayerUsername = game.GameState.HostPlayerUsername,
            IsGameStarted = game.GameState.IsGameStarted,
            IsTimerVisible = game.GameState.IsTimerVisible,
            CorrectGuessPlayerUsernames = game.GameState.CorrectGuessPlayerUsernames
        };

        await Groups.AddToGroupAsync(Context.ConnectionId, gameHash);
        await Clients.Group(gameHash).SendAsync(HubMessages.OnUpdatePlayerScores, JsonHelper.Serialize(playerScores));
        await Clients.Client(Context.ConnectionId).SendAsync(
            HubMessages.OnJoinGame,
            JsonHelper.Serialize(player),
            JsonHelper.Serialize(settingsClient),
            JsonHelper.Serialize(stateClient)
        );

        await SendAnnouncement(gameHash, $"{player.Username} has joined the game", BootstrapColors.Green);

        _logger.LogInformation($"Game #{gameHash} JoinGame: {player.Username} joined the game.");
        _logger.LogInformation($"Game #{gameHash} Online players: {game.GameState.Players.Count}. Total players: {game.GameState.Players.Count}");
    }

    [HubMethodName(HubMessages.LeaveGame)]
    public async Task LeaveGame(string gameHash, string token)
    {
        Game game = _gameManager.GetGame(gameHash);
            
        if (game is null)
        {
            _logger.LogInformation($"Game #{gameHash} LeaveGame: Game does not exist");
            return;
        }

        Player player = _playerManager.GetPlayerByToken(gameHash, token);

        if (player is null)
        {
            _logger.LogError($"Game #{gameHash} LeaveGame: Player with the token {token} does not exist");
            return;
        }

        _playerManager.RemovePlayer(gameHash, token);

        if (game.GameState.Players.Count == 0)
        {
            _gameManager.RemoveGame(gameHash);
            _logger.LogInformation($"Game #{gameHash} LeaveGame: Game removed - no online players");
            return;
        }

        if (!game.GameState.IsGameStarted && token == game.HostToken)
        {
            _gameManager.RemoveGame(gameHash);

            AnnouncementMessage message = new()
            {
                Text = "Host left the lobby",
                BootstrapBackgroundColor = BootstrapColors.Red
            };

            await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubMessages.OnGameProblem, JsonHelper.Serialize(message));

            _logger.LogInformation($"Game #{gameHash} LeaveGame: Host left the unstarted game - game removed");
            return;
        }

        if (game.GameState.IsGameStarted && game.GameState.Players.Count < 2)
        {
            _gameManager.RemoveGame(gameHash);

            AnnouncementMessage message = new()
            {
                Text = "Not enough players to continue the game",
                BootstrapBackgroundColor = BootstrapColors.Red
            };

            await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubMessages.OnGameProblem, JsonHelper.Serialize(message));
        }

        List<PlayerScore> playerScores = _playerManager.GetPlayerScores(gameHash);
        string playerListSerialized = JsonHelper.Serialize(playerScores);

        _logger.LogInformation($"Game #{gameHash} LeaveGame: {player.Username} left the game");
        _logger.LogInformation($"Game #{gameHash} Online players: {game.GameState.Players.Count}. Total players: {game.GameState.Players.Count}");

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameHash);
        await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubMessages.OnUpdatePlayerScores, playerListSerialized);
        await SendAnnouncement(gameHash, $"{player.Username} has left the game", BootstrapColors.Red);
    }
}