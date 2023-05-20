namespace Dotnet.Server.Models;

public class GameState
{   
    //Client-side and server-side
    public int CurrentDrawingTimeSeconds { get; set; } = 75;
    public int CurrentRound { get; set; } = 1;
    public string HiddenSecretWord { get; set; } = "? ? ?";
    public string DrawingPlayerUsername { get; set; } = string.Empty;
    public string HostPlayerUsername { get; set; } = string.Empty;
    public bool IsGameStarted { get; set; } = false;
    public bool IsTimerVisible { get; set; } = false;
    public List<string> CorrectGuessPlayerUsernames { get; set; } = new List<string>();

    //Server-side only
    public List<Player> Players = new List<Player>();
    public List<DrawnLine> DrawnLines { get; set; } = new List<DrawnLine>();
    public string DrawingToken { get; set; } = string.Empty;
    public string ActualSecretWord { get; set; } = string.Empty;
    public List<string> DrawingPlayersTokens { get; set; } = new List<string>();
    public List<string> NoChatPermissionTokens { get; set; }= new List<string>();
    public int CorrectAnswerCount { get; set; } = 0;
}