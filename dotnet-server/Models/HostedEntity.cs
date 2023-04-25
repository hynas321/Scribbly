namespace Dotnet.Server.Models;

class HostedEntity
{
    public string Hash { get; set; } = "";
    public string HostToken { get; set; } = "";
    public List<Player> Players { get; set; } = new List<Player>();
    public List<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public GameSettings? GameSettings { get; set; } = new GameSettings();
}