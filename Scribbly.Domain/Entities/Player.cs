namespace Scribbly.Domain.Entities;

public class Player
{
    public string Username { get; set; } = string.Empty;
    public int Score { get; set; }
    public string CredentialHash { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public DateTimeOffset? DisconnectedAtUtc { get; set; }
}
