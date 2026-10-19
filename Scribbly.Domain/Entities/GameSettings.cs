namespace Scribbly.Domain.Entities;

public class GameSettings
{
    public int DrawingTimeSeconds { get; set; } = 75;
    public int RoundsCount { get; set; } = 6;
    public string PromptLanguage { get; set; } = "en";
    public int MaxPlayers { get; set; } = 6;
    public List<string> PromptCategories { get; set; } = new();
    public string HintSpeed { get; set; } = "off";
    public int HintPercentage { get; set; } = 25;
}
