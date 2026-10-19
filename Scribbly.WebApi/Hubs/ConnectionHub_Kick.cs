using Microsoft.AspNetCore.SignalR;
using Scribbly.Application.Features.Games;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.Hubs.Static;

namespace Scribbly.WebApi.Hubs;

public partial class HubConnection
{
    [HubMethodName(HubMessages.KickPlayer)]
    public async Task KickPlayer(string gameHash, string username)
    {
        KickPlayerResult result = await _games.KickAsync(gameHash, RequireRoomAccessToken(), username, Context.ConnectionAborted);
        if (result.Status != KickPlayerStatus.Applied)
        {
            throw new HubException(result.Status switch
            {
                KickPlayerStatus.GameNotFound => "Game does not exist.",
                KickPlayerStatus.PlayerNotFound => "Player does not exist in this room.",
                KickPlayerStatus.CannotKickHost => "The host cannot be kicked.",
                _ => "Only the host may kick players."
            });
        }
        if (result.GameFinished)
        {
            await _sessionCoordinator.RequestCancellationAsync(gameHash, CancellationToken.None);
            await _results.CaptureAsync(gameHash, CancellationToken.None);
        }
        await Clients.Group(gameHash).SendAsync(HubMessages.OnPlayerKicked, result.Username, Context.ConnectionAborted);
        var snapshot = await _games.GetSnapshotAsync(gameHash, Context.ConnectionAborted);
        if (snapshot is not null)
        {
            var scores = snapshot.GameState.Players
                .Select(p => new PlayerScore { Username = p.Username, Score = p.Score })
                .ToList();
            await Clients.Group(gameHash).SendAsync(HubMessages.OnPlayerScoresUpdated, scores, Context.ConnectionAborted);
        }
        await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubMessages.OnAnnouncement,
            new AnnouncementMessage { Text = $"{result.Username} left the game", BootstrapBackgroundColor = BootstrapColors.Yellow },
            Context.ConnectionAborted);
        if (result.GameFinished)
        {
            await Clients.Group(gameHash).SendAsync(HubMessages.OnGameEnded, Context.ConnectionAborted);
            await _games.DeleteAsync(gameHash, CancellationToken.None);
        }
    }
}
