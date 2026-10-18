using Scribbly.Application.Features.Settings.Configuration;

namespace Scribbly.WebApi.UnitTests.TestDoubles;

public static class GameSettingsTestOptions
{
    public static GameSettingsOptions Create() => new()
    {
        MinDrawingTimeSeconds = 30,
        MaxDrawingTimeSeconds = 120,
        DrawingTimeStepSeconds = 15,
        MinRounds = 1,
        MaxRounds = 6,
        MinPlayers = 2,
        MaxPlayers = 10,
        MaxPromptCategoryLength = 64,
        MinHintPercentage = 25,
        MaxHintPercentage = 75,
        HintPercentageStep = 25
    };
}
