namespace Dotnet.Server.Models;

class Player
{
    public string? Username { get; set; }
    public int Score { get; set; }
    public bool Host { get; set; }
    public string? GameUrl { get; set; }
}