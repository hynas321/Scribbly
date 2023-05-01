namespace Dotnet.Server.Models;

public class Game
{
    public string GameHash { get; set; } = string.Empty;
    public string HostToken { get; set; } = string.Empty;
    public List<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public GameSettings GameSettings { get; set; } = new GameSettings();
    public List<DrawnLine> DrawnLines { get; set; } = new List<DrawnLine>();
    public GameState GameState { get; set; } = new GameState();
}
