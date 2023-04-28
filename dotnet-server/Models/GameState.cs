namespace Dotnet.Server.Models;

public class GameState
{
    public int CurrentDrawingTimeSeconds { get; set; } = 75;
    public int CurrentRound { get; set; } = 1;
    public int wordLength { get; set; } = 10;
}