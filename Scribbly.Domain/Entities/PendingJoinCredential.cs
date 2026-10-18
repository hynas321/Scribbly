namespace Scribbly.Domain.Entities;

public class PendingJoinCredential
{
    public string CredentialHash { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
}
