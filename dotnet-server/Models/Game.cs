namespace Dotnet.Server.Models;

class Game
{
    public string? Url { get; set; }
    public string? HostUsername { get; set; }
    public GameSettings? gameSettings { get; set; }
}
