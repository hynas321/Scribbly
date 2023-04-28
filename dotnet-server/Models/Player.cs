namespace Dotnet.Server.Models;

public class Player
{
    public string Username { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string GameHash { get; set; } = string.Empty;
    public int Score { get; set; } = 0;
}