using Microsoft.AspNetCore.SignalR;
using Scribbly.Application.GameSessions;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.Hubs.Static;
using Scribbly.WebApi.Hubs;

namespace Scribbly.WebApi.GameSessions;

public class SignalRGameSessionEvents : IGameSessionEvents
{
    private readonly IHubContext<HubConnection> _hub;

    public SignalRGameSessionEvents(IHubContext<HubConnection> hub) => _hub = hub;

    public Task GameStartedAsync(string gameHash, CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnGameStarted, cancellationToken);
    public Task GameEndedAsync(string gameHash, CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnGameEnded, cancellationToken);
    public Task GameSessionFailedAsync(string gameHash, string message, CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnGameSessionFailed, message, cancellationToken);
    public Task CanvasClearedAsync(string gameHash, long epoch, CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnCanvasCleared, epoch, cancellationToken);
    public Task CanvasTextSetAsync(string gameHash, AnnouncementMessage message, CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnCanvasText, message, cancellationToken);
    public Task CorrectGuessPlayersUpdatedAsync(
        string gameHash,
        IReadOnlyCollection<string> usernames,
        CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnCorrectGuessPlayersUpdated, usernames, cancellationToken);
    public Task DrawingPlayerUpdatedAsync(string gameHash, string username, CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnDrawingPlayerUpdated, username, cancellationToken);
    public Task PromptRefreshRequestedAsync(string gameHash, CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnPromptRefreshRequested, cancellationToken);
    public Task TimerVisibilityUpdatedAsync(string gameHash, bool visible, CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnTimerVisibilityUpdated, visible, cancellationToken);
    public Task TimerUpdatedAsync(string gameHash, int seconds, CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnTimerUpdated, seconds, cancellationToken);
    public Task PlayerScoresUpdatedAsync(
        string gameHash,
        IReadOnlyCollection<PlayerScore> scores,
        CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnPlayerScoresUpdated, scores, cancellationToken);
    public Task AnnouncementSentAsync(string gameHash, AnnouncementMessage message, CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnAnnouncement, message, cancellationToken);
    public Task CurrentRoundUpdatedAsync(string gameHash, int round, CancellationToken cancellationToken) => SendAsync(gameHash, HubMessages.OnCurrentRoundUpdated, round, cancellationToken);

    private Task SendAsync(string gameHash, string method, CancellationToken cancellationToken) =>
        _hub.Clients.Group(gameHash).SendAsync(method, cancellationToken);

    private Task SendAsync<T>(string gameHash, string method, T payload, CancellationToken cancellationToken) =>
        _hub.Clients.Group(gameHash).SendAsync(method, payload, cancellationToken);
}
