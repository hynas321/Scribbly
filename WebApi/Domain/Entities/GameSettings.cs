namespace WebApi.Domain.Entities;

public class GameSettings
{
    //Client-side and server-side
    public int DrawingTimeSeconds { get; set; } = 75;
    public int RoundsCount { get; set; } = 6;
    public string WordLanguage { get; set; } = "en";
}