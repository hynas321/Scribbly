using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Games;

public record ExpiredDisconnectedPlayersResult(
    string? GameHash,
    IReadOnlyCollection<string> Usernames,
    IReadOnlyCollection<PlayerScore> Scores,
    int PlayerCount,
    bool GameRemoved,
    bool HostExpiredLobby = false,
    bool NotEnoughPlayers = false);
