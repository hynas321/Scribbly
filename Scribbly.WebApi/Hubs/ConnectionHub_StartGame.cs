using Microsoft.AspNetCore.SignalR;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.GameSessions;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.Hubs.Static;

namespace Scribbly.WebApi.Hubs;

public partial class HubConnection
{
    [HubMethodName(HubMessages.StartGame)]
    public async Task<string> StartGame(string gameHash, GameSettings settings)
    {
        if (settings is null || !GameSettingsPolicy.IsValid(settings, _settingsOptions))
        {
            throw new HubException("Invalid game settings.");
        }
        GameSessionStartResult result = await _sessionCoordinator.TryStartAuthorizedAsync(gameHash, RequireRoomAccessToken(), settings, Context.ConnectionAborted);
        if (result is GameSessionStartResult.Started or GameSessionStartResult.AlreadyRunning)
        {
            return result.ToString();
        }
        throw new HubException(result switch
        {
            GameSessionStartResult.TooFewPlayers => "At least two players are required.",
            GameSessionStartResult.GameNotFound => "Game does not exist.",
            GameSessionStartResult.GameFinished => "Game has already finished.",
            GameSessionStartResult.InvalidSettings => "Invalid game settings.",
            _ => "Only the host may start the game."
        });
    }
}
