namespace Dotnet.Server.Models;

class GameSettings
{
    public bool NonAbstractNounsOnly { get; set; } = true;
    public int DrawingTimeSeconds { get; set; } = 75;
    public int RoundsCount { get; set; } = 6;
    public string? WordLanguage { get; set; } = "en";
}