using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class GameHub : Hub
{
    [HubMethodName("ClearCanvas")]
    public async Task ClearCanvas(string gameHash)
    {   
        await Clients.All.SendAsync("ApplyClearCanvas");

        logger.LogInformation($"Game #{gameHash}: Canvas was cleared.");
    }

    [HubMethodName("DrawOnCanvas")]
    public async Task DrawOnCanvas(string hash, string drawnLineSerialized)
    {   
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await Clients.All.SendAsync("UpdateCanvas", drawnLineSerialized);
    }
}