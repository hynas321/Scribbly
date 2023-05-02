using Dotnet.Server.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.DrawOnCanvas)]
    public async Task DrawOnCanvas(
        string token,
        string gameHash,
        string drawnLineSerialized
    )
    {   
        try 
        {
            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                return;
            }

            if (token != game.GameState.DrawingToken)
            {
                return;
            }

            DrawnLine drawnLine = JsonConvert.DeserializeObject<DrawnLine>(drawnLineSerialized);

            if (drawnLine == null)
            {
                return;
            }

            game.DrawnLines.Add(drawnLine);

            await Clients
                .Group(gameHash)
                .SendAsync(HubEvents.OnDrawOnCanvas, JsonHelper.Serialize(drawnLine));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LoadCanvas)]
    public async Task LoadCanvas(
        string token,
        string gameHash
    )
    {
        try 
        {
            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                return;
            }

            List<DrawnLine> drawnLines = gamesManager.GetGameByHash(gameHash).DrawnLines;

            await Clients
                .Client(Context.ConnectionId)
                .SendAsync(HubEvents.OnLoadCanvas, JsonHelper.Serialize(drawnLines));

        }
        catch (Exception ex)
        {
            logger.LogInformation(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.ClearCanvas)]
    public async Task ClearCanvas(
        string token,
        string gameHash
    )
    {   
        try 
        {
            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                return;
            }

            if (token != game.GameState.DrawingToken)
            {
                return;
            }

            await Clients
                .Group(gameHash)
                .SendAsync(HubEvents.OnClearCanvas);
        }
        catch (Exception ex)
        {
            logger.LogInformation(Convert.ToString(ex));
        }
    }
}
