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

            gameManager.RemoveChatMessages();
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
            List<string> drawingPlayersLeft = new List<string>();

            while (true)
            {
                drawingPlayersLeft = gameManager.GetOnlinePlayersTokens();

                await hubContext.Clients.All.SendAsync(HubEvents.OnClearCanvas);
                await SetCanvasText($"Round {game.GameState.CurrentRound}", BootstrapColors.Green);
                await Task.Delay(2000);

                while (drawingPlayersLeft.Count != 0)
                {
                    game.GameState.DrawingToken = null;

                    Random random = new Random();
                    int randomTokenIndex = random.Next(drawingPlayersLeft.Count);
                    string drawingToken = drawingPlayersLeft[randomTokenIndex];
                    string secretWord = FetchWord();

                    drawingPlayersLeft.RemoveAt(randomTokenIndex);

                    game.GameState.SecretWord = secretWord;

                    string drawingPlayerUsername = gameManager.GetPlayerByToken(drawingToken).Username;

                    await hubContext.Clients.All.SendAsync(HubEvents.OnUpdateDrawingPlayer, drawingPlayerUsername);
                    await SetCanvasText($"{drawingPlayerUsername} is going to draw in 5s", BootstrapColors.Green);
                    await Task.Delay(5000);

                    game.GameState.DrawingToken = drawingToken;

                    await hubContext.Clients.All.SendAsync(HubEvents.OnUpdateTimerVisibility, true);
                    await Task.Run(() => UpdateTimer());
                    await hubContext.Clients.All.SendAsync(HubEvents.OnUpdateTimerVisibility, false);
                    await SetCanvasText($"Time is up, the correct answer was: {secretWord}", BootstrapColors.Green);

                    game.GameState.DrawingToken = null;

                    await Task.Delay(3000);
                }

                game.GameState.CurrentRound++;

                if (game.GameState.CurrentRound > game.GameSettings.RoundsCount)
                {   
                    await SetCanvasText($"The end", BootstrapColors.Green);
                    break;
                }

                await hubContext.Clients.All.SendAsync(HubEvents.OnUpdateCurrentRound, game.GameState.CurrentRound);
            }
            }
            catch (Exception ex)
            {
                logger.LogInformation(Convert.ToString(ex));
            }
        
    }

    public async Task UpdateTimer()
    {
        try
        {  
            Game game = gameManager.GetGame();

            if (game == null)
            {
                return;
            }

            game.GameState.CurrentDrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds;

            int initialTime = game.GameState.CurrentDrawingTimeSeconds;
            int currentTime = initialTime;
            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            for (int i = 0; i < initialTime; i++)
            {   
                if (currentTime < 0 || game == null)
                {
                    cancellationToken.Cancel();
                    break;
                }

                logger.LogInformation(Convert.ToString(currentTime));
                await hubContext.Clients.All.SendAsync(HubEvents.OnUpdateTimer, currentTime);

                currentTime--;

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);
            }
        }
        catch(Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
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
}