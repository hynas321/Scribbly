namespace Scribbly.Application.Features.Rounds;

public record TimerTickResult(
    RoundCommandStatus Status,
    int CurrentDrawingTimeSeconds = 0,
    bool Completed = false,
    bool HintChanged = false);
