namespace Scribbly.Application.Features.Accounts.Configuration;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public int SessionLifetimeHours { get; set; }
    public string GoogleClientId { get; set; } = string.Empty;
    public string AdminToken { get; set; } = string.Empty;
}
