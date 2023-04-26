using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Dotnet.Server.Hubs;

public partial class GameHub : Hub
{
    [HubMethodName(HubEvents.LoadCanvas)]
    public async Task LoadCanvas(string gameHash)
    {
        try 
        {
            List<DrawnLine> drawnLines = gamesManager.Get(gameHash).DrawnLines;
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string drawnLinesSerialized = System.Text.Json.JsonSerializer.Serialize<List<DrawnLine>>(drawnLines, jsonSerializerOptions);
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