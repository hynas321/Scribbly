using Microsoft.AspNetCore.SignalR;
using Scribbly.Application.Features.Games;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.Hubs.Static;

namespace Scribbly.WebApi.Hubs;

public partial class HubConnection
{
    [HubMethodName(HubMessages.JoinGame)]
    public async Task<RoomSnapshot> JoinGame(string gameHash)
    {
        JoinGameResult result = await _games.JoinAsync(gameHash, RequireRoomAccessToken(), Context.ConnectionId, Context.ConnectionAborted);
        if (result.Status != JoinGameStatus.Joined || result.Snapshot is null)
        {
            throw new HubException(JoinError(result.Status));
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, gameHash, Context.ConnectionAborted);
        await Clients.Group(gameHash).SendAsync(HubMessages.OnPlayerScoresUpdated, result.Snapshot.Scores, Context.ConnectionAborted);
        await Clients.Group(gameHash).SendAsync(HubMessages.OnAnnouncement,
            new AnnouncementMessage { Text = $"{result.Snapshot.Username} joined the game", BootstrapBackgroundColor = BootstrapColors.Green },
            Context.ConnectionAborted);
        _logger.LogInformation("Game #{GameHash}: {Username} joined; {PlayerCount}/{MaxPlayers}",
            gameHash, result.Snapshot.Username, result.Snapshot.PlayerCount, result.Snapshot.MaxPlayers);
        return result.Snapshot;
    }

    [HubMethodName(HubMessages.ReconnectGame)]
    public async Task<RoomSnapshot?> ReconnectGame(string gameHash)
    {
        JoinGameResult result = await _games.ReconnectAsync(gameHash, RequireRoomAccessToken(), Context.ConnectionId, Context.ConnectionAborted);
        if (result.Status is JoinGameStatus.InvalidCredential or JoinGameStatus.GameNotFound)
        {
            return null;
        }
        if (result.Status != JoinGameStatus.Resumed || result.Snapshot is null)
        {
            throw new HubException(JoinError(result.Status));
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, gameHash, Context.ConnectionAborted);
        await Clients.Group(gameHash).SendAsync(HubMessages.OnPlayerScoresUpdated, result.Snapshot.Scores, Context.ConnectionAborted);
        return result.Snapshot;
    }

    [HubMethodName(HubMessages.LeaveGame)]
    public async Task LeaveGame(string gameHash)
    {
        LeaveGameResult result = await _games.LeaveAsync(gameHash, RequireRoomAccessToken(), Context.ConnectionAborted);
        if (result.Status != LeaveGameStatus.Left)
        {
            return;
        }
        if (result.GameRemovedBecauseEmpty || result.HostLeftLobby || result.NotEnoughPlayers)
        {
            await _sessionCoordinator.RequestCancellationAsync(gameHash, CancellationToken.None);
        }
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameHash, Context.ConnectionAborted);
        if (result.GameRemovedBecauseEmpty)
        {
            return;
        }
        if (result.HostLeftLobby || result.NotEnoughPlayers)
        {
            await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubMessages.OnGameProblem,
                new AnnouncementMessage
                {
                    Text = result.HostLeftLobby ? "Host left the game — the lobby was closed." : "Not enough players to continue the game",
                    BootstrapBackgroundColor = BootstrapColors.Red
                }, Context.ConnectionAborted);
            if (result.HostLeftLobby)
            {
                return;
            }
        }
        await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubMessages.OnPlayerScoresUpdated,
            result.Scores ?? Array.Empty<PlayerScore>(), Context.ConnectionAborted);
        if (result.Username is not null)
        {
            await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(HubMessages.OnAnnouncement,
                new AnnouncementMessage { Text = $"{result.Username} left the game", BootstrapBackgroundColor = BootstrapColors.Yellow },
                Context.ConnectionAborted);
        }
        if (result.NewHostUsername is not null)
        {
            await Clients.GroupExcept(gameHash, Context.ConnectionId).SendAsync(
                HubMessages.OnHostChanged, result.NewHostUsername, Context.ConnectionAborted);
        }
    }

    private static string JoinError(JoinGameStatus status) => status switch
    {
        JoinGameStatus.GameAlreadyStarted => "Game has already started.",
        JoinGameStatus.ResumeExpired => "Reconnect window has expired.",
        JoinGameStatus.GameNotFound => "Game does not exist.",
        _ => "Room credential is invalid or expired."
    };
}
