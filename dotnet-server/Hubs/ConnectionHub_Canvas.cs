using Dotnet.Server.JsonConfig;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.DrawOnCanvas)]
    public async Task DrawOnCanvas(string token, string drawnLineSerialized)
    {   
        try 
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"DrawOnCanvas: Game does not exist");
                return;
            }

            if (token != game.GameState.DrawingToken)
            {
                logger.LogError($"DrawOnCanvas: Player with the token {token} cannot draw on canvas");
                return;
            }

            DrawnLine drawnLine = JsonConvert.DeserializeObject<DrawnLine>(drawnLineSerialized);

            if (drawnLine == null)
            {
                logger.LogError($"DrawOnCanvas: Serialized drawnline {drawnLineSerialized} has an incorrect format");
                return;
            }

            game.GameState.DrawnLines.Add(drawnLine);

            await Clients.All.SendAsync(HubEvents.OnDrawOnCanvas, JsonHelper.Serialize(drawnLine));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LoadCanvas)]
    public async Task LoadCanvas(string token)
    {
        try 
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"LoadCanvas: Game does not exist");
                return;
            }

            List<DrawnLine> drawnLines = game.GameState.DrawnLines;

            await Clients
                .Client(Context.ConnectionId)
                .SendAsync(HubEvents.OnLoadCanvas, JsonHelper.Serialize(drawnLines));

        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.ClearCanvas)]
    public async Task ClearCanvas(string token)
    {   
        try 
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"ClearCanvas: Game does not exist");
                return;
            }

            if (token != game.GameState.DrawingToken)
            {
                logger.LogError($"ClearCanvas: Player with the token {token} cannot clear the canvas");
                return;
            }

            await Clients.All.SendAsync(HubEvents.OnClearCanvas);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}
