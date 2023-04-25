using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class GameHub : Hub
{
    [HubMethodName("StartProgressBarTimer")]
    public async Task StartProgressBarTimer(string hash)
    {
        try
        {  
            Game game = gamesManager.Get(hash);
            int initialTime = game.GameState.CurrentDrawingTimeSeconds;
            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                for (int i = 0; i < initialTime; i++)
                {   
                    logger.LogInformation(Convert.ToString(game.GameState.CurrentDrawingTimeSeconds));
                    await Clients.Client(Context.ConnectionId).SendAsync("OnUpdateProgressBarTimer", game.GameState.CurrentDrawingTimeSeconds);
                    game.GameState.CurrentDrawingTimeSeconds--;

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);

                    if (game.GameState.CurrentDrawingTimeSeconds <= 0)
                    {
                        cancellationToken.Cancel();
                        break;
                    }
                }
            });
        }
        catch(Exception ex)
        {
            logger.LogInformation($"Game #{hash}: Progress bar timer could not be started. {ex}");
        }
    }
}