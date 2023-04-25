namespace Dotnet.Server.Models;

class Player
{
    public string? Username { get; set; }
    public string? Token { get; set; }
    public string? gameHash { get; set; }
    public int Score { get; set; }
}