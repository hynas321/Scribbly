namespace Scribbly.Application.Features.Rounds;

public record PrepareRoundResult(
    RoundCommandStatus Status,
    int CurrentRound = 0,
    IReadOnlyCollection<string>? RemainingDrawingCredentialHashes = null,
    string? CurrentDrawingCredentialHash = null);
