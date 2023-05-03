using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.StartGame)]
    public async Task StartGame(string token)
    {
        try
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                return;
            }

            if (token != game.HostToken)
            {
                return;
            }

            game.GameState.IsStarted = true;

            await Clients.All.SendAsync(HubEvents.OnStartGame);

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

            await Task.Run(async () =>
            {
                for (int i = 0; i < initialTime; i++)
                {   
                    await Clients.All.SendAsync(HubEvents.OnStartTimer, currentTime);

                    currentTime--;

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);

                    if (currentTime <= 0)
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
}