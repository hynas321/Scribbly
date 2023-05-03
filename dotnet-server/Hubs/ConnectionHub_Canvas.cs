using Dotnet.Server.Json;
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
                return;
            }

            List<DrawnLine> drawnLines = game.DrawnLines;

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
    public async Task ClearCanvas(string token)
    {   
        try 
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                return;
            }

            if (token != game.GameState.DrawingToken)
            {
                return;
            }

            await Clients.All.SendAsync(HubEvents.OnClearCanvas);
        }
        catch (Exception ex)
        {
            logger.LogInformation(Convert.ToString(ex));
        }
    }
}
