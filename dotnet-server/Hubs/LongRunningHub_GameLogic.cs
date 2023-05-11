using Dotnet.Server.JsonConfig;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LongRunningHubConnection : Hub
{
    private readonly GameManager gameManager = new GameManager(25);
    private readonly ILogger<HubConnection> logger;
    private readonly IHubContext<HubConnection> hubContext;

    public LongRunningHubConnection(ILogger<HubConnection> logger, IHubContext<HubConnection> hubContext)
    {
        this.logger = logger;
        this.hubContext = hubContext;
    }

    [HubMethodName(HubEvents.StartGame)]
    public async Task StartGame(string token, GameSettings settings)
    {
        try
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"StartGame: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                logger.LogError($"StartGame: Token is not a host token");
                return;
            }

            if (game.GameState.PlayerScores.Count < 2)
            {
                logger.LogError($"StartGame: Too few players to start the game");
                return;
            }

            if (settings.DrawingTimeSeconds < 30 ||
                settings.DrawingTimeSeconds > 120 ||
                settings.RoundsCount < 1 ||
                settings.RoundsCount > 6
            )
            {
                logger.LogError($"StartGame: Incorrect settings data");
                return;
            }

            game.ChatMessages.Clear();
            game.GameState.IsGameStarted = true;

            game.GameSettings.NonAbstractNounsOnly = settings.NonAbstractNounsOnly;
            game.GameSettings.DrawingTimeSeconds = settings.DrawingTimeSeconds;
            game.GameSettings.RoundsCount = settings.RoundsCount;
            game.GameSettings.WordLanguage = settings.WordLanguage;

            await hubContext.Clients.All.SendAsync(HubEvents.OnStartGame);
            logger.LogInformation($"StartGame: Game started");

            Context.Abort();

            await Task.Run(() => ManageGameFlow());
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    public async Task ManageGameFlow()
    {
        try
        {
            Game game = gameManager.GetGame();

            while (true)
            {
                game.GameState.DrawingPlayersTokens = gameManager.GetOnlinePlayersTokens();

                await hubContext.Clients.All.SendAsync(HubEvents.OnClearCanvas);
                await SetCanvasText($"Round {game.GameState.CurrentRound}", BootstrapColors.Green);
                await Task.Delay(5000);

                while (game.GameState.DrawingPlayersTokens.Count != 0)
                {
                    List<string> drawingPlayersTokens = game.GameState.DrawingPlayersTokens;
                    List<string> onlinePlayerTokens = gameManager.GetOnlinePlayersTokens();

                    if (drawingPlayersTokens.Count != onlinePlayerTokens.Count)
                    {
                        game.GameState.DrawingPlayersTokens = drawingPlayersTokens
                            .Where(token => onlinePlayerTokens.Contains(token))
                            .ToList();
                    }

                    game.GameState.DrawingPlayerUsername = "";
                    game.GameState.DrawingToken = "";
                    game.GameState.NoChatPermissionTokens.Clear();
                    game.GameState.DrawnLines.Clear();
                    game.GameState.CorrectAnswerCount = 0;

                    await hubContext.Clients.All.SendAsync(HubEvents.OnClearCanvas);

                    Random random = new Random();

                    int randomTokenIndex = random.Next(game.GameState.DrawingPlayersTokens.Count);
                    string drawingToken = game.GameState.DrawingPlayersTokens[randomTokenIndex];
                    string drawingPlayerUsername = gameManager.GetPlayerByToken(drawingToken).Username;

                    string actualSecretWord = FetchWord();
                    string hiddenSecretWord = Convert.ToString(actualSecretWord.Length);

                    game.GameState.DrawingPlayerUsername = drawingPlayerUsername;
                    game.GameState.DrawingPlayersTokens.RemoveAt(randomTokenIndex);
                    game.GameState.ActualSecretWord = actualSecretWord;
                    game.GameState.HiddenSecretWord = $"Secret word length: {hiddenSecretWord}";
                    game.GameState.DrawingToken = drawingToken;
                    game.GameState.IsTimerVisible = true;

                    await hubContext.Clients.All.SendAsync(HubEvents.OnUpdateDrawingPlayer, drawingPlayerUsername);
                    await SetCanvasText($"{drawingPlayerUsername} is going to draw in 5s", BootstrapColors.Green);
                    await Task.Delay(5000);

                    await hubContext.Clients.All.SendAsync(HubEvents.OnRequestSecretWord);
                    await hubContext.Clients.All.SendAsync(HubEvents.OnUpdateTimerVisibility, true);

                    bool isTimerFinishedSuccessfuly = await Task.Run(() => UpdateTimer());

                    if (!isTimerFinishedSuccessfuly)
                    {
                        return;
                    }

                    if (game.GameState.CorrectAnswerCount > 0)
                    {
                        int pointsForDrawing = 10;

                        gameManager.UpdatePlayerScore(drawingToken, pointsForDrawing);

                        await SendAnnouncement($"{drawingPlayerUsername} received the drawing bonus (+{pointsForDrawing} points)", BootstrapColors.Green);
                    }
                    else
                    {
                        await SendAnnouncement($"{drawingPlayerUsername} received no drawing bonus", BootstrapColors.Red);
                    }

                    List<PlayerScore> playerScores = gameManager.GetPlayerObjectsWithoutToken();

                    await hubContext.Clients.All.SendAsync(HubEvents.OnUpdatePlayerScores, JsonHelper.Serialize(playerScores));

                    game.GameState.DrawingToken = "";
                    game.GameState.IsTimerVisible = false;

                    await hubContext.Clients.All.SendAsync(HubEvents.OnUpdateTimerVisibility, false);
                    await SetCanvasText($"The drawing phase has ended", BootstrapColors.Green);
                    await SendAnnouncement($"The answer was: {actualSecretWord}", BootstrapColors.Yellow);
                    await Task.Delay(8000);
                }

                game.GameState.CurrentRound++;

                if (game.GameState.CurrentRound > game.GameSettings.RoundsCount)
                {   
                    await SetCanvasText($"Thank you for playing! Automatic disconnection in 15s", BootstrapColors.Green);
                    await Task.Delay(15000);
                    await hubContext.Clients.All.SendAsync(HubEvents.OnEndGame);

                    gameManager.RemoveGame();
                    break;
                }

                await hubContext.Clients.All.SendAsync(HubEvents.OnUpdateCurrentRound, game.GameState.CurrentRound);
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation(Convert.ToString(ex));
            await hubContext.Clients.All.SendAsync(HubEvents.OnEndGame);
            gameManager.RemoveGame();
        }
        
    }

    public async Task<bool> UpdateTimer()
    {
        try
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                return false;
            }

            game.GameState.CurrentDrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds;

            int initialTime = game.GameState.CurrentDrawingTimeSeconds;
            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            for (int i = 0; i < initialTime; i++)
            {   
                if (gameManager.GetGame() == null)
                {
                    cancellationToken.Cancel();
                    return false;
                }

                if (game.GameState.CorrectAnswerCount == game.GameState.PlayerScores.Count - 1)
                {
                    cancellationToken.Cancel();
                    return true;
                }

                if (game.GameState.CurrentDrawingTimeSeconds < 0)
                {
                    cancellationToken.Cancel();
                    return true;
                }

                await hubContext.Clients.All.SendAsync(HubEvents.OnUpdateTimer, game.GameState.CurrentDrawingTimeSeconds);

                game.GameState.CurrentDrawingTimeSeconds--;

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);
            }

            return true;
        }
        catch(Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
            return false;
        }
    }

    public async Task SetCanvasText(string text, string color)
    {
        try
        {
            Game game = new Game();

            if (game == null)
            {
                return;
            }

            AnnouncementMessage message = new AnnouncementMessage()
            {
                Text = text,
                BootstrapBackgroundColor = color
            };

            await hubContext.Clients.All.SendAsync(HubEvents.OnSetCanvasText, JsonHelper.Serialize(message));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    public string FetchWord()
    {
        List<string> words = new List<string>()
        {
            "application", "server", "project"
        };

        Random random = new Random();

        return words[random.Next(words.Count)];
    }

    public async Task SendAnnouncement(string text, string backgroundColor)
    {
        try 
        {
            Game game = new Game();

            if (game == null)
            {
                logger.LogError($"SendAnnouncement: Game does not exist");
                return;
            }

            AnnouncementMessage message = new AnnouncementMessage()
            {
                Text = text,
                BootstrapBackgroundColor = backgroundColor
            };

            await hubContext.Clients.All.SendAsync(HubEvents.OnSendAnnouncement, JsonHelper.Serialize(message));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}