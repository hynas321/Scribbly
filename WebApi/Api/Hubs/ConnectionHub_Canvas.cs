using WebApi.Api.Hubs.Static;
using WebApi.Api.Utilities;
using WebApi.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using WebApi.Api.Hubs.Attributes;

namespace WebApi.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubMessages.DrawOnCanvas)]
    [ValidateHubArgument("gameHash", ValidationType.GameHash)]
    [ValidateHubArgument("token", ValidationType.DrawingToken)]
    public async Task DrawOnCanvas(string gameHash, string token, string drawnLineSerialized)
    {
        Game game = Context.Items["Game"] as Game;
        DrawnLine drawnLine = JsonConvert.DeserializeObject<DrawnLine>(drawnLineSerialized);

        if (drawnLine is null)
        {
            _logger.LogError($"Game #{gameHash} DrawOnCanvas: Serialized drawnline {drawnLineSerialized} has an incorrect format");
            return;
        }

        game.GameState.DrawnLines.Add(drawnLine);

        await Clients.Group(gameHash).SendAsync(HubMessages.OnDrawOnCanvas, JsonHelper.Serialize(drawnLine));
    }

    [HubMethodName(HubMessages.LoadCanvas)]
    [ValidateHubArgument("gameHash", ValidationType.GameHash)]
    public async Task LoadCanvas(string gameHash)
    {
        Game game = Context.Items["Game"] as Game;
        List<DrawnLine> drawnLines = game.GameState.DrawnLines;

        await Clients
            .Client(Context.ConnectionId)
            .SendAsync(HubMessages.OnLoadCanvas, JsonHelper.Serialize(drawnLines));
    }

    [HubMethodName(HubMessages.ClearCanvas)]
    [ValidateHubArgument("gameHash", ValidationType.GameHash)]
    [ValidateHubArgument("token", ValidationType.DrawingToken)]
    public async Task ClearCanvas(string gameHash, string token)
    {
        Game game = Context.Items["Game"] as Game;
        game.GameState.DrawnLines.Clear();

        await Clients.Group(gameHash).SendAsync(HubMessages.OnClearCanvas);
    }

    [HubMethodName(HubMessages.UndoLine)]
    [ValidateHubArgument("gameHash", ValidationType.GameHash)]
    [ValidateHubArgument("token", ValidationType.DrawingToken)]
    public async Task UndoLine(string gameHash, string token)
    {
        Game game = Context.Items["Game"] as Game;

        List<DrawnLine> drawnLines = game.GameState.DrawnLines;

        if (drawnLines.Count > 0)
        {
            int lastNumber = drawnLines.Last().CurrentLine;

            drawnLines.RemoveAll(line => line.CurrentLine == lastNumber);

            await Clients.Group(gameHash).SendAsync(HubMessages.OnClearCanvas);
            await Clients.Group(gameHash).SendAsync(HubMessages.OnLoadCanvas, JsonHelper.Serialize(drawnLines));
        }
    }
}
