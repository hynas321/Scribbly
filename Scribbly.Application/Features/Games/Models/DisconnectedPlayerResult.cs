using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Games;

public record DisconnectedPlayerResult(
    string? Username,
    string? GameHash,
    IReadOnlyCollection<PlayerScore> Scores,
    int PlayerCount,
    bool GameRemoved,
    bool HostExpiredLobby = false,
    bool NotEnoughPlayers = false);
