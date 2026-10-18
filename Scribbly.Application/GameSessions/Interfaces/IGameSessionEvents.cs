using Scribbly.Domain.Entities;

namespace Scribbly.Application.GameSessions;

public interface IGameSessionEvents
{
    Task GameStartedAsync(string gameHash, CancellationToken cancellationToken);
    Task GameEndedAsync(string gameHash, CancellationToken cancellationToken);
    Task GameSessionFailedAsync(string gameHash, string message, CancellationToken cancellationToken);
    Task CanvasClearedAsync(string gameHash, long epoch, CancellationToken cancellationToken);
    Task CanvasTextSetAsync(string gameHash, AnnouncementMessage message, CancellationToken cancellationToken);
    Task CorrectGuessPlayersUpdatedAsync(
        string gameHash,
        IReadOnlyCollection<string> usernames,
        CancellationToken cancellationToken);
    Task DrawingPlayerUpdatedAsync(string gameHash, string username, CancellationToken cancellationToken);
    Task PromptRefreshRequestedAsync(string gameHash, CancellationToken cancellationToken);
    Task TimerVisibilityUpdatedAsync(string gameHash, bool visible, CancellationToken cancellationToken);
    Task TimerUpdatedAsync(string gameHash, int seconds, CancellationToken cancellationToken);
    Task PlayerScoresUpdatedAsync(
        string gameHash,
        IReadOnlyCollection<PlayerScore> scores,
        CancellationToken cancellationToken);
    Task AnnouncementSentAsync(string gameHash, AnnouncementMessage message, CancellationToken cancellationToken);
    Task CurrentRoundUpdatedAsync(string gameHash, int round, CancellationToken cancellationToken);
}
