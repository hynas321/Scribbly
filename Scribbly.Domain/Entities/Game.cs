namespace Scribbly.Domain.Entities;

public class Game
{
    public string GameHash { get; set; } = string.Empty;
    public string HostCredentialHash { get; set; } = string.Empty;
    public List<PendingJoinCredential> PendingJoinCredentials { get; set; } = new();
    public List<FinalPlayerScore> FinalScores { get; set; } = new();
    public bool ResultsCaptured { get; set; }
    public long Version { get; set; }
    public List<ChatMessage> ChatMessages { get; set; } = new();
    public GameSettings GameSettings { get; set; } = new();
    public GameState GameState { get; set; } = new();
}
