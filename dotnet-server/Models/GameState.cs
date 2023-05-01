namespace Dotnet.Server.Models;

public class GameState
{
    public int CurrentDrawingTimeSeconds { get; set; } = 75;
    public int CurrentRound { get; set; } = 1;
    public int WordLength { get; set; } = 10;
    public List<Player> Players = new List<Player>();
    public List<PlayerScore> PlayerList = new List<PlayerScore>();
}