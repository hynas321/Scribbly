using Dotnet.Server.Models;
using dotnet_server.Utilities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubMessages.DrawOnCanvas)]
    public async Task DrawOnCanvas(string gameHash, string token, string drawnLineSerialized)
    {   
        try 
        {
            Game game = _gameManager.GetGame(gameHash);

            if (game == null)
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

            if (drawnLine == null)
            {
                _logger.LogError($"Game #{gameHash} DrawOnCanvas: Serialized drawnline {drawnLineSerialized} has an incorrect format");
                return;
            }

            game.GameState.DrawnLines.Add(drawnLine);

            await Clients.Group(gameHash).SendAsync(HubMessages.OnDrawOnCanvas, JsonHelper.Serialize(drawnLine));
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubMessages.LoadCanvas)]
    public async Task LoadCanvas(string gameHash, string token)
    {
        try 
        {
            Game game = _gameManager.GetGame(gameHash);

            if (game == null)
            {
                _logger.LogError($"Game #{gameHash} LoadCanvas: Game does not exist");
                return;
            }

            List<DrawnLine> drawnLines = game.GameState.DrawnLines;

            await Clients
                .Client(Context.ConnectionId)
                .SendAsync(HubMessages.OnLoadCanvas, JsonHelper.Serialize(drawnLines));

        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubMessages.ClearCanvas)]
    public async Task ClearCanvas(string gameHash, string token)
    {   
        try 
        {
            Game game = _gameManager.GetGame(gameHash);

            if (game == null)
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
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubMessages.UndoLine)]
    public async Task UndoLine(string gameHash, string token)
    {
        try 
        {
            Game game = _gameManager.GetGame(gameHash);

            if (game == null)
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
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }
}
