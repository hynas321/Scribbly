namespace Dotnet.Server.Models;

public class GameState
{   
    //Client-side and server-side
    public int CurrentDrawingTimeSeconds { get; set; } = 75;
    public int CurrentRound { get; set; } = 1;
    public int WordLength { get; set; } = 10;

    //Server-side only
    public List<PlayerScore> PlayerScores = new List<PlayerScore>();
    public List<Player> Players = new List<Player>();
    public string DrawingToken { get; set; } = string.Empty;
    public bool IsStarted { get; set; } = false;
}