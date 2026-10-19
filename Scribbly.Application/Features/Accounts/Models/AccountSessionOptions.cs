using Scribbly.Application.Features.Accounts.Configuration;

namespace Scribbly.Application.Features.Accounts;

public record AccountSessionOptions(TimeSpan Lifetime)
{
    public static AccountSessionOptions FromOptions(AuthenticationOptions options) =>
        new(TimeSpan.FromHours(Math.Clamp(options.SessionLifetimeHours, 1, 24 * 30)));
}
