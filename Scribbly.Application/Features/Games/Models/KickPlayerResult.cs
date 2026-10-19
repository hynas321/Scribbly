namespace Scribbly.Application.Features.Games;

public record KickPlayerResult(
    KickPlayerStatus Status,
    string? Username = null,
    long CanvasEpoch = 0,
    bool GameFinished = false);
