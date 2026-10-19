using Scribbly.Application.Abstractions.Canvas;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Security;
using Scribbly.Application.Persistence;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.Features.Settings.Configuration;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Games;

public partial class GameLifecycleService
{
private RoomSnapshot BuildSnapshot(Game game, Player player) => new(
        game.GameHash, player.Username, player.CredentialHash == game.HostCredentialHash,
        Copy(game.GameSettings), ToClientState(game.GameState), ToScores(game), ReconnectableCount(game), _options.MaxPlayers);

    private int ReservedSlots(Game game)
    {
        int hostReservation = AllPlayers(game).Any(p => p.CredentialHash == game.HostCredentialHash) ? 0 : 1;
        return ReconnectableCount(game) + game.PendingJoinCredentials.Count + hostReservation;
    }

    private bool PrunePending(Game game, DateTimeOffset now) => game.PendingJoinCredentials.RemoveAll(p => p.ExpiresAtUtc <= now) > 0;

    private static IEnumerable<Player> AllPlayers(Game game) => game.GameState.Players.Concat(game.GameState.DisconnectedPlayers);

    private static int ReconnectableCount(Game game) => game.GameState.Players.Count + game.GameState.DisconnectedPlayers.Count;

    private static bool HasPendingCompletion(Game game) => game.GameState.CurrentRound > game.GameSettings.RoundsCount;

    private static string NormalizeUsername(string value)
    {
        string normalized = (value ?? string.Empty).Trim();

        if (normalized.Length is < 1 or > 25)
        {
            throw new ArgumentException("Username must contain 1-25 characters.", nameof(value));
        }
        return normalized;
    }

    private static bool EqualsName(string left, string right) => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    private GameSettings Copy(GameSettings s) => new()
    {
        DrawingTimeSeconds = s.DrawingTimeSeconds,
        RoundsCount = s.RoundsCount,
        PromptLanguage = s.PromptLanguage,
        MaxPlayers = s.MaxPlayers,
        PromptCategories = new List<string>(s.PromptCategories),
        HintSpeed = s.HintSpeed,
        HintPercentage = GameSettingsPolicy.NormalizeHintPercentage(s.HintPercentage, _settingsOptions)
    };

    private static Player Copy(Player p) => new()
    {
        Username = p.Username,
        Score = p.Score,
        CredentialHash = p.CredentialHash,
        ConnectionId = p.ConnectionId,
        DisconnectedAtUtc = p.DisconnectedAtUtc
    };

    private static GameClientState ToClientState(GameState s) => new(s.CurrentDrawingTimeSeconds, s.CurrentRound,
        s.HiddenPromptText, s.PromptCategory, s.PromptKind, s.DrawingPlayerUsername, s.HostPlayerUsername,
        s.IsGameStarted, s.IsTimerVisible, s.CorrectGuessPlayerUsernames.ToArray(), s.CanvasEpoch);

    private static IReadOnlyCollection<PlayerScore> ToScores(Game game) =>
        AllPlayers(game).OrderByDescending(p => p.Score).Select(p => new PlayerScore { Username = p.Username, Score = p.Score }).ToArray();

    private static GameMutation<T> MutationFor<T>(bool persist, T value) =>
        persist ? GameMutation<T>.Persist(value) : GameMutation<T>.NoChange(value);

    private static DisconnectedPlayerResult EmptyDisconnected() => new(null, null, Array.Empty<PlayerScore>(), 0, false);

    private static ExpiredDisconnectedPlayersResult EmptyExpired(string gameHash) =>
        new(gameHash, Array.Empty<string>(), Array.Empty<PlayerScore>(), 0, false);
}
