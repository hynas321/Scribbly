namespace Scribbly.Domain.Entities;

public class GameState
{
    public int CurrentDrawingTimeSeconds { get; set; }
    public int CurrentRound { get; set; } = 1;
    public string HiddenPromptText { get; set; } = "? ? ?";
    public string PromptCategory { get; set; } = string.Empty;
    public string PromptKind { get; set; } = string.Empty;
    public string DrawingPlayerUsername { get; set; } = string.Empty;
    public string HostPlayerUsername { get; set; } = string.Empty;
    public bool IsGameStarted { get; set; }
    public bool IsTimerVisible { get; set; }
    public List<string> CorrectGuessPlayerUsernames { get; set; } = new();
    public List<Player> Players { get; set; } = new();
    public List<Player> DisconnectedPlayers { get; set; } = new();
    public string DrawingCredentialHash { get; set; } = string.Empty;
    public string PendingDrawingCredentialHash { get; set; } = string.Empty;
    public string ActualPromptText { get; set; } = string.Empty;
    public List<string> DrawingPlayerCredentialHashes { get; set; } = new();
    public List<string> NoChatPermissionCredentialHashes { get; set; } = new();
    public List<string> KickedCredentialHashes { get; set; } = new();
    public int CorrectAnswerCount { get; set; }
    public List<int> PromptLetterRevealOrder { get; set; } = new();
    public long CanvasEpoch { get; set; } = 1;
}
