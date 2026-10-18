namespace Scribbly.Application.Features.Rounds;

public record AdvanceRoundResult(RoundCommandStatus Status, int CurrentRound = 0, bool GameFinished = false);
