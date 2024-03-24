using Dotnet.Server.JsonConfig;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    private readonly HashManager hashManager = new HashManager();

    [HubMethodName(HubEvents.JoinGame)]
    public async Task JoinGame(string gameHash, string token, string username)
    {
        try 
        {
            if (username.Length < 1)
            {
                string errorMessage = "Username is too short";

                logger.LogError($"Game #{gameHash} JoinGame: {errorMessage}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnJoinGameError, errorMessage);
                return;
            }

            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                string errorMessage = "Game does not exist";

                logger.LogError($"Game #{gameHash} JoinGame: {errorMessage}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnJoinGameError, errorMessage);
                return;
            }

            Player player = null;
            
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
                gameManager.AddPlayer(gameHash, player);
            }
            else if (!gameManager.CheckIfPlayerExistsByUsername(gameHash, username))
            {
                player = new Player()
                {
                    Username = username,
                    Score = 0,
                    Token = hashManager.GenerateUserHash(),
                    ConnectionId = Context.ConnectionId
                };

                gameManager.AddPlayer(gameHash, player);
            }
            else if (gameManager.CheckIfPlayerExistsByUsername(gameHash, username))
            {
                string errorMessage = "User with your username already exists";

                logger.LogError($"Game #{gameHash} JoinGame: {errorMessage}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnJoinGameError, errorMessage);
                return;
            }
            else
            {
                string errorMessage = "Unexpected error, try again";

                logger.LogError($"Game #{gameHash} JoinGame: {errorMessage}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnJoinGameError, errorMessage);
                return;
            }

            PlayerScore playerScore = new PlayerScore()
            {
                Username = player.Username,
                Score = player.Score
            };

            List<PlayerScore> playerScores = gameManager.GetPlayerObjectsWithoutToken(gameHash);

            GameSettings settingsClient = new GameSettings()
            {
                DrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds,
                RoundsCount = game.GameSettings.RoundsCount,
                WordLanguage = game.GameSettings.WordLanguage
            };

            GameState stateClient = new GameState()
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
            await Clients.Group(gameHash).SendAsync(HubEvents.OnUpdatePlayerScores, JsonHelper.Serialize(playerScores));
            await Clients.Client(Context.ConnectionId).SendAsync(
                HubEvents.OnJoinGame,
                JsonHelper.Serialize(player),
                JsonHelper.Serialize(settingsClient),
                JsonHelper.Serialize(stateClient)
            );

            await SendAnnouncement(gameHash, $"{player.Username} has joined the game", BootstrapColors.Green);

            logger.LogInformation($"Game #{gameHash} JoinGame: {player.Username} joined the game.");
            logger.LogInformation($"Game #{gameHash} Online players: {game.GameState.Players.Count}. Total players: {game.GameState.Players.Count}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LeaveGame)]
    public async Task LeaveGame(string gameHash, string token)
    {
        try
        {
            Game game = gameManager.GetGame(gameHash);
            
            if (game == null)
            {
                logger.LogInformation($"Game #{gameHash} LeaveGame: Game does not exist");
                return;
            }

            Player player = gameManager.GetPlayerByToken(gameHash, token);

            if (player == null)
            {
                logger.LogError($"Game #{gameHash} LeaveGame: Player with the token {token} does not exist");
                return;
            }

            gameManager.RemovePlayer(gameHash, token);

            if (game.GameState.Players.Count == 0)
            {
                gameManager.RemoveGame(gameHash);
                logger.LogInformation($"Game #{gameHash} LeaveGame: Game removed - no online players");
                return;
            }

            if (!game.GameState.IsGameStarted && token == game.HostToken)
            {
                gameManager.RemoveGame(gameHash);

                AnnouncementMessage message = new AnnouncementMessage()
                {
                    Text = "Host left the lobby",
                    BootstrapBackgroundColor = BootstrapColors.Red
                };

                await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubEvents.OnGameProblem, JsonHelper.Serialize(message));

                logger.LogInformation($"Game #{gameHash} LeaveGame: Host left the unstarted game - game removed");
                return;
            }

            if (game.GameState.IsGameStarted && game.GameState.Players.Count < 2)
            {
                gameManager.RemoveGame(gameHash);

                AnnouncementMessage message = new AnnouncementMessage()
                {
                    Text = "Not enough players to continue the game",
                    BootstrapBackgroundColor = BootstrapColors.Red
                };

                await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubEvents.OnGameProblem, JsonHelper.Serialize(message));
            }

            List<PlayerScore> playerScores = gameManager.GetPlayerObjectsWithoutToken(gameHash);
            string playerListSerialized = JsonHelper.Serialize(playerScores);

            logger.LogInformation($"Game #{gameHash} LeaveGame: {player.Username} left the game");
            logger.LogInformation($"Game #{gameHash} Online players: {game.GameState.Players.Count}. Total players: {game.GameState.Players.Count}");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameHash);
            await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubEvents.OnUpdatePlayerScores, playerListSerialized);
            await SendAnnouncement(gameHash, $"{player.Username} has left the game", BootstrapColors.Red);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}