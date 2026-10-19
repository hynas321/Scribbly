using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Rounds;

public record CompleteTurnResult(
    RoundCommandStatus Status,
    string? DrawingPlayerUsername = null,
    string? ActualPromptText = null,
    int DrawingBonus = 0,
    IReadOnlyCollection<PlayerScore>? Scores = null);
