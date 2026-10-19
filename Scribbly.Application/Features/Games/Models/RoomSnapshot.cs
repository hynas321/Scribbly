using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Games;

public record RoomSnapshot(
    string GameHash,
    string Username,
    bool IsHost,
    GameSettings Settings,
    GameClientState State,
    IReadOnlyCollection<PlayerScore> Scores,
    int PlayerCount,
    int MaxPlayers);
