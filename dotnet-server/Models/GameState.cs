namespace Dotnet.Server.Models;

public class GameState
{   
    //Client-side and server-side
    public int CurrentDrawingTimeSeconds { get; set; } = 75;
    public int CurrentRound { get; set; } = 1;
    public string HiddenSecretWord { get; set; } = "? ? ?";
    public string DrawingPlayerUsername { get; set; } = "";
    public string HostPlayerUsername { get; set; } = "";
    public bool IsGameStarted { get; set; } = false;
    public bool IsTimerVisible { get; set; } = false;
    public List<string> CorrectGuessPlayerUsernames = new List<string>();

    //Server-side only
    public List<Player> Players = new List<Player>();
    public List<DrawnLine> DrawnLines { get; set; } = new List<DrawnLine>();
    public string DrawingToken { get; set; } = "";
    public string ActualSecretWord { get; set; } = "";
    public List<string> DrawingPlayersTokens = new List<string>();
    public List<string> NoChatPermissionTokens = new List<string>();
    public int CorrectAnswerCount = 0;
}