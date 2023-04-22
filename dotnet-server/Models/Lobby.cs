namespace Dotnet.Server.Models;

class Lobby 
{
    public List<Player> players { get; set; } = new List<Player>();
    public List<ChatMessage> chatMessages { get; set; } = new List<ChatMessage>();
    public GameSettings? gameSettings { get; set; }
}