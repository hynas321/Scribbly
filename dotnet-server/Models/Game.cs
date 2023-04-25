namespace Dotnet.Server.Models;

class Game : HostedEntity
{
    public List<DrawnLine> DrawnLines { get; set; } = new List<DrawnLine>();
    public GameState GameState { get; set; } = new GameState();
}
