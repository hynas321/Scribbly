namespace Scribbly.Application.Features.Games.Configuration;

public sealed class GameOptions
{
    public const string SectionName = "Game";

    public int ReconnectGracePeriodSeconds { get; set; }
    public int DisconnectedPlayerCleanupIntervalSeconds { get; set; }
    public int JoinCredentialLifetimeSeconds { get; set; }
    public int MaxPlayers { get; set; }
}
