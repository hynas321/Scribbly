using Microsoft.AspNetCore.SignalR;
using Scribbly.WebApi.Hubs.Static;

namespace Scribbly.WebApi.Hubs;

public partial class HubConnection
{
    [HubMethodName(HubMessages.GetPrompt)]
    public async Task<PromptView> GetPrompt(string gameHash)
    {
        string token = RequireRoomAccessToken();
        var player = await _games.GetPlayerAsync(gameHash, token, Context.ConnectionAborted)
            ?? throw new HubException("You are not a member of this room.");
        var game = await _games.GetSnapshotAsync(gameHash, Context.ConnectionAborted)
            ?? throw new HubException("Game does not exist.");
        bool drawing = player.CredentialHash == game.GameState.DrawingCredentialHash;
        return new PromptView(
            drawing ? game.GameState.ActualPromptText : game.GameState.HiddenPromptText,
            game.GameState.PromptCategory,
            game.GameState.PromptKind,
            drawing);
    }
}
