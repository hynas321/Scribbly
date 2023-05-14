namespace Dotnet.Server.Models;

public class GameSettings
{
    //Client-side and server-side
    public bool NounsOnly { get; set; } = true;
    public int DrawingTimeSeconds { get; set; } = 75;
    public int RoundsCount { get; set; } = 6;
    public string WordLanguage { get; set; } = "en";
}