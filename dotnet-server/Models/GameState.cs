namespace Dotnet.Server.Models;

public class GameState
{   
    //Client-side and server-side
    public int CurrentDrawingTimeSeconds { get; set; } = 75;
    public int CurrentRound { get; set; } = 1;
    public string SecretWord { get; set; } = "";
    public string DrawingPlayerUsername { get; set; } = "";
    public string HostPlayerUsername { get; set; } = "";
    public bool IsGameStarted { get; set; } = false;

    //Server-side only
    public List<PlayerScore> PlayerScores = new List<PlayerScore>();
    public List<Player> Players = new List<Player>();
    public List<string> NoChatPermissionTokens = new List<string>();
    public string DrawingToken { get; set; } = string.Empty;
}