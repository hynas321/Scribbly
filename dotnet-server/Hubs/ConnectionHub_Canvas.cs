using System.Text.Json;
using Dotnet.Server.Json;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Dotnet.Server.Hubs;

public partial class ConnectionHub : Hub
{
    [HubMethodName(HubEvents.LoadCanvas)]
    public async Task LoadCanvas(string gameHash)
    {
        try 
        {
            List<DrawnLine> drawnLines = gamesManager.GetGame(gameHash).DrawnLines;
            string drawnLinesSerialized = JsonHelper.Serialize(drawnLines);
            
            await Clients.All.SendAsync(HubEvents.OnLoadCanvas, drawnLinesSerialized);
        }
        catch (Exception ex)
        {
            logger.LogError($"Game #{gameHash}: Could not load canvas. {ex}");
        }
    }

    [HubMethodName(HubEvents.DrawOnCanvas)]
    public async Task DrawOnCanvas(string gameHash, string drawnLineSerialized)
    {   
        try 
        {
            DrawnLine? drawnLineDeserialized = JsonConvert.DeserializeObject<DrawnLine>(drawnLineSerialized);

            if (drawnLineDeserialized == null) {
                return;
            }

            gamesManager.AddDrawnLine(gameHash, drawnLineDeserialized);
            await Clients.All.SendAsync(HubEvents.OnDrawOnCanvas, drawnLineSerialized);
        }
        catch { }
    }

    [HubMethodName(HubEvents.ClearCanvas)]
    public async Task ClearCanvas(string gameHash)
    {   
        try 
        {
            gamesManager.RemoveAllDrawnLines(gameHash);
            await Clients.All.SendAsync(HubEvents.OnClearCanvas);
        }
        catch (Exception ex)
        {
            logger.LogError($"Game #{gameHash}: Could not clear the canvas. {ex}");
        }
    }
}