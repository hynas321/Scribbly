using Scribbly.Application.Features.Settings;
using Scribbly.Application.Features.Settings.Configuration;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Settings;

public class GameSettingsPolicyTests
{
    private static readonly GameSettingsOptions Options = GameSettingsTestOptions.Create();

    [Theory]
    [InlineData(30, true)]
    [InlineData(121, false)]
    public void DrawingTime_OnlyMultiplesOfFifteenWithinRangeAreValid(int seconds, bool expected) =>
        Assert.Equal(expected, GameSettingsPolicy.IsDrawingTimeValid(seconds, Options));

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void RoundsCount_OnlyRangeOneToSixIsValid(int rounds, bool expected) =>
        Assert.Equal(expected, GameSettingsPolicy.IsRoundsCountValid(rounds, Options));

    [Theory]
    [InlineData("en", true)]
    [InlineData("pl", true)]
    [InlineData("de", false)]
    public void PromptLanguage_OnlyEnglishAndPolishAreValid(string? language, bool expected) =>
        Assert.Equal(expected, GameSettingsPolicy.IsPromptLanguageValid(language));

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public void MaxPlayers_OnlyRangeTwoToTenIsValid(int maxPlayers, bool expected) =>
        Assert.Equal(expected, GameSettingsPolicy.IsMaxPlayersValid(maxPlayers, Options));

    [Theory]
    [InlineData("food", true)]
    [InlineData("food2", false)]
    public void PromptCategorySlug_OnlyLettersHyphenAndSpaceAreValid(string category, bool expected) =>
        Assert.Equal(expected, GameSettingsPolicy.IsPromptCategorySlugValid(category, Options));

    [Fact]
    public void PromptCategorySlug_OverSixtyFourCharacters_IsRejected()
    {
        Assert.False(GameSettingsPolicy.IsPromptCategorySlugValid(new string('a', Options.MaxPromptCategoryLength + 1), Options));
        Assert.True(GameSettingsPolicy.IsPromptCategorySlugValid(new string('a', Options.MaxPromptCategoryLength), Options));
    }
}
