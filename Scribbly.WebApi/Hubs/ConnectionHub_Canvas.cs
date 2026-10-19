using Microsoft.AspNetCore.SignalR;
using Scribbly.Application.Features.Canvas;
using Scribbly.WebApi.Hubs.Static;

namespace Scribbly.WebApi.Hubs;

public partial class HubConnection
{
    [HubMethodName(HubMessages.AppendCanvasChunk)]
    public async Task<CanvasCommandResult> AppendCanvasChunk(string gameHash, CanvasStrokeChunk chunk)
    {
        CanvasCommandResult result = await _canvas.AppendChunkAsync(gameHash, RequireRoomAccessToken(), chunk, Context.ConnectionAborted);
        if (result.Status == CanvasCommandStatus.Applied)
        {
            await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubMessages.OnCanvasChunk, chunk, Context.ConnectionAborted);
        }
        return result;
    }

    [HubMethodName(HubMessages.LoadCanvas)]
    public async Task<CanvasSnapshot> LoadCanvas(string gameHash) =>
        await _canvas.GetSnapshotAsync(gameHash, RequireRoomAccessToken(), Context.ConnectionAborted)
        ?? throw new HubException("You are not a member of this room.");

    [HubMethodName(HubMessages.ClearCanvas)]
    public async Task<CanvasCommandResult> ClearCanvas(string gameHash)
    {
        CanvasCommandResult result = await _canvas.ClearAsync(gameHash, RequireRoomAccessToken(), Context.ConnectionAborted);
        if (result.Status == CanvasCommandStatus.Applied)
        {
            await Clients.Group(gameHash).SendAsync(HubMessages.OnCanvasCleared, result.Epoch, Context.ConnectionAborted);
        }
        return result;
    }

    [HubMethodName(HubMessages.UndoStroke)]
    public async Task<CanvasCommandResult> UndoStroke(string gameHash)
    {
        CanvasCommandResult result = await _canvas.UndoAsync(gameHash, RequireRoomAccessToken(), Context.ConnectionAborted);
        if (result.Status == CanvasCommandStatus.Applied && result.RemovedStrokeId is not null)
        {
            await Clients.Group(gameHash).SendAsync(HubMessages.OnCanvasStrokeUndone, result.RemovedStrokeId, result.Epoch, Context.ConnectionAborted);
        }
        return result;
    }
}
