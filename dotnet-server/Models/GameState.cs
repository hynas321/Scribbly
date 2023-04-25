namespace Dotnet.Server.Models;

class GameState
{
    public int CurrentDrawingTimeSeconds { get; set; } = 50;
    public int CurrentRound { get; set; } = 1;
    public List<Player> Players { get; set; } = new List<Player>();
    public List<string> OnlinePlayersTokens { get; set; } = new List<string>();
}