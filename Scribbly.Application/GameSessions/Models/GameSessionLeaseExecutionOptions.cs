namespace Scribbly.Application.GameSessions;

public record GameSessionLeaseExecutionOptions(TimeSpan RenewalInterval)
{
    public static GameSessionLeaseExecutionOptions FromLeaseDuration(TimeSpan leaseDuration)
    {
        if (leaseDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(leaseDuration),
                "The session lease duration must be greater than zero.");
        }

        TimeSpan interval = TimeSpan.FromTicks(Math.Max(1, leaseDuration.Ticks / 3));
        return new GameSessionLeaseExecutionOptions(interval);
    }
}
