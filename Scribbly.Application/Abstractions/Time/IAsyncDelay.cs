namespace Scribbly.Application.Abstractions.Time;

public interface IAsyncDelay
{
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);
}
