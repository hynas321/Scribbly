namespace dotnet_server.Domain.Entities;

public class Player
{
    public string Username { get; set; } = string.Empty;
    public int Score { get; set; } = 0;
    public string Token { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
}