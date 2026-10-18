using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Games;

public record LeaveGameResult(
    LeaveGameStatus Status,
    string? Username = null,
    IReadOnlyCollection<PlayerScore>? Scores = null,
    int PlayerCount = 0,
    bool GameRemovedBecauseEmpty = false,
    bool HostLeftLobby = false,
    bool NotEnoughPlayers = false,
    string? NewHostUsername = null);
