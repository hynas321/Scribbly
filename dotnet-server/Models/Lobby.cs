namespace Dotnet.Server.Models;

class Lobby 
{   
    public string? Hash { get; set; } = "TestLobbyHash";
    public string? HostUsername { get; set; } = "Test";
    public List<Player> Players { get; set; } = new List<Player>();
    public List<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public GameSettings? GameSettings { get; set; } = new GameSettings();
}