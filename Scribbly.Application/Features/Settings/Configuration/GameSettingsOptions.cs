namespace Scribbly.Application.Features.Settings.Configuration;

public sealed class GameSettingsOptions
{
    public const string SectionName = "GameSettings";

    public int MinDrawingTimeSeconds { get; set; }
    public int MaxDrawingTimeSeconds { get; set; }
    public int DrawingTimeStepSeconds { get; set; }
    public int MinRounds { get; set; }
    public int MaxRounds { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int MaxPromptCategoryLength { get; set; }
    public int MinHintPercentage { get; set; }
    public int MaxHintPercentage { get; set; }
    public int HintPercentageStep { get; set; }
}
