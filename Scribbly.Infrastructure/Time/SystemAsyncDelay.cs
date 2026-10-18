using Scribbly.Application.Abstractions.Time;

namespace Scribbly.Infrastructure.Time;

public class SystemAsyncDelay : IAsyncDelay
{
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default) =>
        Task.Delay(delay, cancellationToken);
}
