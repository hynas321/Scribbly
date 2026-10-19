using Scribbly.Application.Features.Games.Configuration;

namespace Scribbly.Application.Features.Games;

public record GameLifecycleOptions(
    TimeSpan ReconnectGracePeriod,
    TimeSpan DisconnectedPlayerCleanupInterval,
    TimeSpan JoinCredentialLifetime,
    int MaxPlayers)
        {
    public static GameLifecycleOptions Default { get; } = new(
        TimeSpan.FromMinutes(2),
        TimeSpan.FromSeconds(15),
        TimeSpan.FromSeconds(45),
        12);

    public static GameLifecycleOptions FromOptions(GameOptions options)
    {
        return new(
            TimeSpan.FromSeconds(Math.Clamp(options.ReconnectGracePeriodSeconds, 15, 3600)),
            TimeSpan.FromSeconds(Math.Clamp(options.DisconnectedPlayerCleanupIntervalSeconds, 1, 300)),
            TimeSpan.FromSeconds(Math.Clamp(options.JoinCredentialLifetimeSeconds, 10, 300)),
            Math.Clamp(options.MaxPlayers, 2, 50));
    }
}
