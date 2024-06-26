using Dotnet.Server.Http;
using Dotnet.Server.JsonConfig;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LongRunningHubConnection : Hub
{
    private readonly GameManager gameManager = new GameManager();
    private readonly ILogger<HubConnection> logger;
    private readonly IHubContext<HubConnection> hubContext;
    private readonly IHubContext<AccountHubConnection> accountHubContext;
    private readonly RandomWordFetcher randomWordFetcher;

    public LongRunningHubConnection(
        ILogger<HubConnection> logger,
        IHubContext<HubConnection> hubContext,
        IHubContext<AccountHubConnection> accountHubContext,
        IConfiguration configuration
    )
    {
        this.logger = logger;
        this.hubContext = hubContext;
        this.accountHubContext = accountHubContext;

        randomWordFetcher = new RandomWordFetcher(configuration);
    }

    [HubMethodName(HubEvents.StartGame)]
    public async Task StartGame(string gameHash, string token, GameSettings settings)
    {
        try
        {
            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError($"Game #{gameHash} StartGame: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                logger.LogError($"Game #{gameHash} StartGame: Token is not a host token");
                return;
            }

            if (game.GameState.Players.Count < 2)
            {
                logger.LogError($"Game #{gameHash} StartGame: Too few players to start the game");
                return;
            }

            if (settings.DrawingTimeSeconds < 30 ||
                settings.DrawingTimeSeconds > 120 ||
                settings.RoundsCount < 1 ||
                settings.RoundsCount > 6
            )
            {
                logger.LogError($"Game #{gameHash} StartGame: Incorrect settings data");
                return;
            }

            game.ChatMessages.Clear();
            game.GameState.IsGameStarted = true;

            game.GameSettings.DrawingTimeSeconds = settings.DrawingTimeSeconds;
            game.GameSettings.RoundsCount = settings.RoundsCount;
            game.GameSettings.WordLanguage = settings.WordLanguage;

            await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnStartGame);
            logger.LogInformation($"Game #{gameHash} StartGame: Game started");

            Context.Abort();

            await Task.Run(() => ManageGameFlow(gameHash));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    public async Task ManageGameFlow(string gameHash)
    {
        try
        {
            Game game = gameManager.GetGame(gameHash);

            while (true)
            {
                game.GameState.DrawingPlayersTokens = gameManager.GetOnlinePlayersTokens(gameHash);

                await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnClearCanvas);
                await SetCanvasText(gameHash, $"Round {game.GameState.CurrentRound}", BootstrapColors.Green);
                await Task.Delay(5000);

                while (game.GameState.DrawingPlayersTokens.Count != 0)
                {
                    List<string> drawingPlayersTokens = game.GameState.DrawingPlayersTokens;
                    List<string> onlinePlayerTokens = gameManager.GetOnlinePlayersTokens(gameHash);

                    if (drawingPlayersTokens.Count != onlinePlayerTokens.Count)
                    {
                        game.GameState.DrawingPlayersTokens = drawingPlayersTokens
                            .Where(onlinePlayerTokens.Contains)
                            .ToList();
                    }

                    game.GameState.DrawingPlayerUsername = "";
                    game.GameState.DrawingToken = "";
                    game.GameState.NoChatPermissionTokens.Clear();
                    game.GameState.DrawnLines.Clear();
                    game.GameState.CorrectAnswerCount = 0;
                    game.GameState.CorrectGuessPlayerUsernames.Clear();

                    await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnClearCanvas);

                    Random random = new Random();

                    int randomTokenIndex = random.Next(game.GameState.DrawingPlayersTokens.Count);
                    string drawingToken = game.GameState.DrawingPlayersTokens[randomTokenIndex];
                    string drawingPlayerUsername = gameManager.GetPlayerByToken(gameHash, drawingToken).Username;
                    string actualSecretWord = await randomWordFetcher.FetchWordAsync(gameHash) ?? throw new NullReferenceException();
                    string hiddenSecretWord = Convert.ToString(actualSecretWord.Length);

                    game.GameState.DrawingPlayerUsername = drawingPlayerUsername;
                    game.GameState.DrawingPlayersTokens.RemoveAt(randomTokenIndex);
                    game.GameState.IsTimerVisible = true;
                    game.GameState.HiddenSecretWord = "? ? ?";

                    await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.onUpdateCorrectGuessPlayerUsernames, JsonHelper.Serialize(game.GameState.CorrectGuessPlayerUsernames));
                    await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnUpdateDrawingPlayer, drawingPlayerUsername);
                    await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnRequestSecretWord);
                    await SetCanvasText(gameHash, $"{drawingPlayerUsername} is going to draw in 5s", BootstrapColors.Green);
                    await Task.Delay(5000);

                    game.GameState.ActualSecretWord = actualSecretWord;
                    game.GameState.HiddenSecretWord = $"Secret word length: {hiddenSecretWord}";

                    game.GameState.DrawingToken = drawingToken;

                    await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnRequestSecretWord);
                    await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnUpdateTimerVisibility, true);

                    bool isTimerFinishedSuccessfuly = await Task.Run(() => UpdateTimer(gameHash));

                    if (!isTimerFinishedSuccessfuly)
                    {
                        return;
                    }

                    int correctAnswers = game.GameState.CorrectAnswerCount;
                    int playersCount = game.GameState.Players.Count;

                    if (correctAnswers == playersCount - 1)
                    {
                        int pointsForDrawing = 10;

                        gameManager.UpdatePlayerScore(gameHash, drawingToken, pointsForDrawing);

                        await SendAnnouncement(gameHash, $"{drawingPlayerUsername} received the drawing bonus (+{pointsForDrawing} points)", BootstrapColors.Green);
                    }
                    else if (correctAnswers < playersCount - 1 && correctAnswers > 0)
                    {
                        int pointsForDrawing = 5;

                        gameManager.UpdatePlayerScore(gameHash, drawingToken, pointsForDrawing);

                        await SendAnnouncement(gameHash, $"{drawingPlayerUsername} received the drawing bonus (+{pointsForDrawing} points)", BootstrapColors.Green);
                    }
                    else
                    {
                        await SendAnnouncement(gameHash, $"{drawingPlayerUsername} received no drawing bonus", BootstrapColors.Red);
                    }

                    List<PlayerScore> playerScores = gameManager.GetPlayerObjectsWithoutToken(gameHash);

                    await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnUpdatePlayerScores, JsonHelper.Serialize(playerScores));

                    game.GameState.DrawingToken = "";
                    game.GameState.IsTimerVisible = false;

                    await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnUpdateTimerVisibility, false);
                    await SetCanvasText(gameHash, $"The drawing phase has ended", BootstrapColors.Green);
                    await SendAnnouncement(gameHash, $"The answer was: {actualSecretWord}", BootstrapColors.Yellow);

                    game.GameState.HiddenSecretWord = actualSecretWord;
                    await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnRequestSecretWord);
                    await Task.Delay(8000);

                    game.GameState.HiddenSecretWord = "? ? ?";
                    await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnRequestSecretWord);
                }

                game.GameState.CurrentRound++;

                if (game.GameState.CurrentRound > game.GameSettings.RoundsCount)
                {   
                    await accountHubContext.Clients.All.SendAsync(HubEvents.OnUpdateAccountScore, gameHash);
                    await SetCanvasText(gameHash, $"Thank you for playing! You may leave the game :)", BootstrapColors.Green);
                    await Task.Delay(10000);
                    gameManager.RemoveGame(gameHash);
                    //await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnEndGame);
                    break;
                }

                await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnUpdateCurrentRound, game.GameState.CurrentRound);
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation(Convert.ToString(ex));
            await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnEndGame);
            gameManager.RemoveGame(gameHash);
        }
    }

    public async Task<bool> UpdateTimer(string gameHash)
    {
        try
        {
            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                return false;
            }

            game.GameState.CurrentDrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds;

            int initialTime = game.GameState.CurrentDrawingTimeSeconds;
            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            for (int i = 0; i < initialTime; i++)
            {   
                if (gameManager.GetGame(gameHash) == null)
                {
                    cancellationToken.Cancel();
                    return false;
                }

                if (game.GameState.CorrectAnswerCount == game.GameState.Players.Count - 1)
                {
                    cancellationToken.Cancel();
                    return true;
                }

                if (game.GameState.CurrentDrawingTimeSeconds < 0)
                {
                    cancellationToken.Cancel();
                    return true;
                }

                await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnUpdateTimer, game.GameState.CurrentDrawingTimeSeconds);

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

    public async Task SetCanvasText(string gameHash, string text, string color)
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

            await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnSetCanvasText, JsonHelper.Serialize(message));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    public async Task SendAnnouncement(string gameHash, string text, string backgroundColor)
    {
        try 
        {
            Game game = new Game();

            if (game == null)
            {
                logger.LogError($"Game #{gameHash} SendAnnouncement: Game does not exist");
                return;
            }

            AnnouncementMessage message = new AnnouncementMessage()
            {
                Text = text,
                BootstrapBackgroundColor = backgroundColor
            };

            //gameManager.AddChatMessage(message);

            await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnSendAnnouncement, JsonHelper.Serialize(message));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}