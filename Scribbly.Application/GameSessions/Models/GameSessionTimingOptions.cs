namespace Scribbly.Application.GameSessions;

public record GameSessionTimingOptions(
    TimeSpan RoundIntroduction,
    TimeSpan DrawingPlayerIntroduction,
    TimeSpan TimerTick,
    TimeSpan AnswerReveal,
    TimeSpan GameFinished)
        {
    public static GameSessionTimingOptions Default { get; } = new(
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(8),
        TimeSpan.FromSeconds(10));
}
