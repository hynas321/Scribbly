namespace Scribbly.Application.Features.Games;

public record GameClientState(
    int CurrentDrawingTimeSeconds,
    int CurrentRound,
    string HiddenPromptText,
    string PromptCategory,
    string PromptKind,
    string DrawingPlayerUsername,
    string HostPlayerUsername,
    bool IsGameStarted,
    bool IsTimerVisible,
    IReadOnlyCollection<string> CorrectGuessPlayerUsernames,
    long CanvasEpoch);
