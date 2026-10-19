namespace Scribbly.Application.Features.Rounds;

public record StartTurnResult(
    RoundCommandStatus Status,
    string? DrawingCredentialHash = null,
    string? DrawingPlayerUsername = null,
    string? PromptLanguage = null,
    IReadOnlyCollection<string>? RemainingDrawingCredentialHashes = null,
    bool Resumed = false,
    long CanvasEpoch = 0);
