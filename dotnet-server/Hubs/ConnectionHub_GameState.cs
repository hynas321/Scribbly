using Dotnet.Server.JsonConfig;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
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
                string errorMessage = "Too few players to start the game";

                ChatMessage message = new ChatMessage()
                {
                    Username = null,
                    Text = errorMessage,
                    BootstrapBackgroundColor = BootstrapColors.Red
                };

                await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnSendAnnouncement, JsonHelper.Serialize(message));
                logger.LogError($"StartGame: {errorMessage}");
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
            
            await Clients.All.SendAsync(HubEvents.OnStartGame);
            await SendAnnouncement("Game has started", BootstrapColors.Yellow);
            logger.LogInformation($"StartGame: Game started");

            ManageGameFlow();

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

            while (true)
            {

            }
        });
    }

    public async Task SetTimerValues(string token)
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

            await Task.Run(async () =>
            {
                 for (int i = 0; i < initialTime; i++)
                 {   
                    //await Clients.All.SendAsync(HubEvents.OnStartTimer, currentTime);

                    currentTime--;

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);

                    if (currentTime <= 0 || game == null)
                    {
                        cancellationToken.Cancel();
                        break;
                    }
                 }
             });
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