using Dotnet.Server.JsonConfig;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{

    [HubMethodName(HubEvents.JoinGame)]
    public async Task JoinGame(string token, string username)
    {
        try 
        {
            if (username.Length < 1)
            {
                string errorMessage = "Username is too short";

                logger.LogError($"JoinGame: {errorMessage}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnJoinGameError, errorMessage);
                return;
            }

            Game game = gameManager.GetGame();

            if (game == null)
            {
                string errorMessage = "Game does not exist";

                logger.LogError($"JoinGame: {errorMessage}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnJoinGameError, errorMessage);
                return;
            }
            
            Player player = gameManager.GetPlayerByToken(token);

            if (player == null && token == game.HostToken)
            {
                player = new Player()
                {
                    Username = username,
                    Score = 0,
                    Token = game.HostToken,
                };

                game.GameState.HostPlayerUsername = player.Username;
                gameManager.AddPlayer(player);
            }
            else if (player != null && token == game.HostToken)
            {
                player = new Player()
                {
                    Username = player.Username,
                    Score = player.Score,
                    Token = game.HostToken,
                };
            }
            else if (player != null && token == player.Token)
            {
                player = new Player()
                {
                    Username = player.Username,
                    Score = player.Score,
                    Token = player.Token,
                };
            }
            else if (player == null && !gameManager.CheckIfPlayerExistsByUsername(username))
            {
                player = new Player()
                {
                    Username = username,
                    Score = 0,
                    Token = Guid.NewGuid().ToString().Replace("-", ""),
                };

                gameManager.AddPlayer(player);
            }
            else if (player == null && gameManager.CheckIfPlayerExistsByUsername(username))
            {
                string errorMessage = "User with your username already exists";

                logger.LogError($"JoinGame: {errorMessage}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnJoinGameError, errorMessage);
                return;
            }
            else
            {
                string errorMessage = "Unexpected error, try again";

                logger.LogError($"JoinGame: {errorMessage}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnJoinGameError, errorMessage);
                return;
            }

            PlayerScore playerScore = new PlayerScore()
            {
                Username = player.Username,
                Score = player.Score
            };

            gameManager.AddPlayerScore(playerScore);

            GameSettings settingsClient = new GameSettings()
            {
                NonAbstractNounsOnly = game.GameSettings.NonAbstractNounsOnly,
                DrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds,
                RoundsCount = game.GameSettings.RoundsCount,
                WordLanguage = game.GameSettings.WordLanguage
            };

            GameState stateClient = new GameState()
            {
                CurrentDrawingTimeSeconds = game.GameState.CurrentDrawingTimeSeconds,
                CurrentRound = game.GameState.CurrentRound,
                SecretWord = game.GameState.SecretWord,
                DrawingPlayerUsername = game.GameState.DrawingPlayerUsername,
                HostPlayerUsername = game.GameState.HostPlayerUsername,
                IsGameStarted = game.GameState.IsGameStarted
            };

            List<PlayerScore> playerScores = gameManager.GetPlayersWithoutToken();

            await Clients.All.SendAsync(HubEvents.OnPlayerJoinedGame, JsonHelper.Serialize(playerScores));
            await Clients.Client(Context.ConnectionId).SendAsync(
                HubEvents.OnJoinGame,
                JsonHelper.Serialize(player),
                JsonHelper.Serialize(settingsClient),
                JsonHelper.Serialize(stateClient)
            );

            await SendAnnouncement($"{player.Username} has joined the game", BootstrapColors.Green);

            logger.LogInformation($"JoinGame: {player.Username} joined the game.");
            logger.LogInformation($"Online players: {game.GameState.PlayerScores.Count}. Total players: {game.GameState.Players.Count}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LeaveGame)]
    public async Task LeaveGame(string token, bool leaveForGood)
    {
        try
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                return;
            }

            Player player = gameManager.GetPlayerByToken(token);

            if (player == null)
            {
                return;
            }

            if (leaveForGood)
            {
                gameManager.RemovePlayer(token);
                gameManager.RemovePlayerScore(player.Username);
            }
            else
            {
                gameManager.RemovePlayerScore(player.Username);
            }

            if (game.GameState.PlayerScores.Count == 0)
            {
                gameManager.SetGame(null);
                logger.LogInformation($"LeaveGame: Game removed - no online players");
                return;
            }

            if (!game.GameState.IsGameStarted && token == game.HostToken)
            {
                gameManager.SetGame(null);

                AnnouncementMessage message = new AnnouncementMessage()
                {
                    Text = "Host left the lobby",
                    BootstrapBackgroundColor = BootstrapColors.Red
                };

                await Clients.AllExcept(Context.ConnectionId).SendAsync(HubEvents.OnGameProblem, JsonHelper.Serialize(message));

                logger.LogInformation($"LeaveGame: Host left the unstarted game - game removed");
                return;
            }

            if (game.GameState.IsGameStarted && game.GameState.PlayerScores.Count < 2)
            {
                gameManager.SetGame(null);

                AnnouncementMessage message = new AnnouncementMessage()
                {
                    Text = "Not enough players to continue the game",
                    BootstrapBackgroundColor = BootstrapColors.Red
                };

                await Clients.AllExcept(Context.ConnectionId).SendAsync(HubEvents.OnGameProblem, JsonHelper.Serialize(message));
            }

            List<PlayerScore> playerScores = game.GameState.PlayerScores;
            string playerListSerialized = JsonHelper.Serialize(playerScores);

            logger.LogInformation($"JoinGame: {player.Username} left the game");
            logger.LogInformation($"Online players: {game.GameState.PlayerScores.Count}. Total players: {game.GameState.Players.Count}");

            await Clients.AllExcept(Context.ConnectionId).SendAsync(HubEvents.OnPlayerLeftGame, playerListSerialized);

            await SendAnnouncement($"{player.Username} has left the game", BootstrapColors.Red);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}