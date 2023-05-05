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
            }

            if (token != game.HostToken)
            {
                logger.LogError($"StartGame: Token is not a host token");
            }

            if (settings.DrawingTimeSeconds < 25 ||
                settings.DrawingTimeSeconds > 120 ||
                settings.RoundsCount < 1 ||
                settings.RoundsCount < 6
            )
            {
                logger.LogError($"StartGame: Incorrect settings data");
            }

            gameManager.RemoveChatMessages();
            game.GameState.IsStarted = true;

            game.GameSettings.NonAbstractNounsOnly = settings.NonAbstractNounsOnly;
            game.GameSettings.DrawingTimeSeconds = settings.DrawingTimeSeconds;
            game.GameSettings.RoundsCount = settings.RoundsCount;
            game.GameSettings.WordLanguage = settings.WordLanguage;
            
            await Clients.All.SendAsync(HubEvents.OnStartGame);
            await SendAnnouncement("Game has started", BootstrapColors.Yellow);
            await StartTimer(token);

            logger.LogInformation($"StartGame: Game started");

        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.StartTimer)]
    public async Task StartTimer(string token)
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

            // await Task.Run(async () =>
            // {
            //     for (int i = 0; i < initialTime; i++)
            //     {   
            //         await Clients.All.SendAsync(HubEvents.OnStartTimer, currentTime);

            //         currentTime--;

            //         await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);

            //         if (currentTime <= 0)
            //         {
            //             cancellationToken.Cancel();
            //             break;
            //         }
            //     }
            // });

            await Clients.All.SendAsync(HubEvents.OnStartTimer, 25);
        }
        catch(Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}