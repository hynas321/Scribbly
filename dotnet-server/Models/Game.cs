namespace Dotnet.Server.Models;

public class Game
{
    public string HostToken { get; set; } = Guid.NewGuid().ToString().Replace("-", "");
    public List<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public GameSettings GameSettings { get; set; } = new GameSettings();
    public List<DrawnLine> DrawnLines { get; set; } = new List<DrawnLine>();
    public GameState GameState { get; set; } = new GameState();
}
