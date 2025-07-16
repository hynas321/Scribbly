using WebApi.Api.Hubs.Static;
using WebApi.Api.Utilities;
using WebApi.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace WebApi.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubMessages.DrawOnCanvas)]
    public async Task DrawOnCanvas(string gameHash, string token, string drawnLineSerialized)
    {   
        Game game = _gameManager.GetGame(gameHash);

        if (game is null)
        {
            _logger.LogError($"Game #{gameHash} DrawOnCanvas: Game does not exist");
            return;
        }

        if (token != game.GameState.DrawingToken)
        {
            _logger.LogError($"DrawOnCanvas: Player with the token {token} cannot draw on canvas");
            return;
        }

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
    public async Task LoadCanvas(string gameHash)
    {
        Game game = _gameManager.GetGame(gameHash);

        if (game is null)
        {
            _logger.LogError($"Game #{gameHash} LoadCanvas: Game does not exist");
            return;
        }

        List<DrawnLine> drawnLines = game.GameState.DrawnLines;

        await Clients
            .Client(Context.ConnectionId)
            .SendAsync(HubMessages.OnLoadCanvas, JsonHelper.Serialize(drawnLines));
    }

    [HubMethodName(HubMessages.ClearCanvas)]
    public async Task ClearCanvas(string gameHash, string token)
    {   
        Game game = _gameManager.GetGame(gameHash);

        if (game is null)
        {
            _logger.LogError($"Game #{gameHash} ClearCanvas: Game does not exist");
            return;
        }

        if (token != game.GameState.DrawingToken)
        {
            _logger.LogError($"Game #{gameHash} ClearCanvas: Player with the token {token} cannot clear the canvas");
            return;
        }

        game.GameState.DrawnLines.Clear();

        await Clients.Group(gameHash).SendAsync(HubMessages.OnClearCanvas);
    }

    [HubMethodName(HubMessages.UndoLine)]
    public async Task UndoLine(string gameHash, string token)
    {
        Game game = _gameManager.GetGame(gameHash);

        if (game is null)
        {
            _logger.LogError($"Game #{gameHash} UndoLine: Game does not exist");
            return;
        }

        if (token != game.GameState.DrawingToken)
        {
            _logger.LogError($"Game #{gameHash} UndoLine: Player with the token {token} cannot undo line the canvas");
            return;
        }

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
