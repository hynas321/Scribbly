namespace Dotnet.Server.Models;

public class Game
{
    public string HostToken { get; set; } = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
    public string AnnouncementToken { get; set; } = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
    public List<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public GameSettings GameSettings { get; set; } = new GameSettings();
    public GameState GameState { get; set; } = new GameState();
}
