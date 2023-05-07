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

            if (settings.DrawingTimeSeconds < 25 ||
                settings.DrawingTimeSeconds > 120 ||
                settings.RoundsCount < 1 ||
                settings.RoundsCount < 6
            )
            {
                logger.LogError($"StartGame: Incorrect settings data");
                return;
            }

            gameManager.RemoveChatMessages();
            game.GameState.IsStarted = true;

            game.GameSettings.NonAbstractNounsOnly = settings.NonAbstractNounsOnly;
            game.GameSettings.DrawingTimeSeconds = settings.DrawingTimeSeconds;
            game.GameSettings.RoundsCount = settings.RoundsCount;
            game.GameSettings.WordLanguage = settings.WordLanguage;

            await hubContext.Clients.All.SendAsync(HubEvents.OnStartGame);
            logger.LogInformation($"StartGame: Game started");

            Context.Abort();

            await Task.Run(() => UpdateTimer(game.HostToken));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    public async Task ManageGameFlow()
    {
        await Task.Run(async () =>
        {
            Game game = gameManager.GetGame();

            //await SendAnnouncement("Game has started", BootstrapColors.Yellow);
            while (true)
            {

            }
        });
    }

    public async Task UpdateTimer(string token)
    {
        try
        {  
            Game game = gameManager.GetGame();

            if (game == null)
            {
                return;
            }

            int initialTime = game.GameState.CurrentDrawingTimeSeconds;
            int currentTime = initialTime;
            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            for (int i = 0; i < initialTime; i++)
            {   
                logger.LogInformation(Convert.ToString(currentTime));
                await hubContext.Clients.All.SendAsync(HubEvents.OnUpdateTimer, currentTime);

                currentTime--;

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);

                if (currentTime <= 0 || game == null)
                {
                    cancellationToken.Cancel();
                    break;
                }
            }
        }
        catch(Exception ex)
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