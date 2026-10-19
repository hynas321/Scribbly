using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Features.Settings.Configuration;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Prompts;

public class PromptHintTests
{
    private static readonly GameSettingsOptions Options = GameSettingsTestOptions.Create();

    [Theory]
    [InlineData("slow", 60, 36, 12)]
    [InlineData("normal", 60, 27, 9)]
    [InlineData("fast", 60, 18, 6)]
    [InlineData("normal", 30, 14, 6)]
    [InlineData("fast", 120, 36, 12)]
    public void Schedule_ScalesFirstRevealAndSpacing(string speed, int total, int first, int interval)
    {
        var settings = new GameSettings { DrawingTimeSeconds = total, HintSpeed = speed, HintPercentage = 50 };
        Assert.Equal(0, PromptHintSchedule.RevealCount(settings, total - first + 1, 100, Options));
        Assert.Equal(1, PromptHintSchedule.RevealCount(settings, total - first, 100, Options));
        Assert.Equal(1, PromptHintSchedule.RevealCount(settings, total - first - interval + 1, 100, Options));
        Assert.Equal(2, PromptHintSchedule.RevealCount(settings, total - first - interval, 100, Options));
    }

    [Theory]
    [InlineData("off", 50)]
    [InlineData("off", 25)]
    [InlineData("invalid", 50)]
    public void DisabledSpeed_DisablesHints(string speed, int percentage)
    {
        var settings = new GameSettings { HintSpeed = speed, HintPercentage = percentage };
        Assert.Equal(0, PromptHintSchedule.RevealCount(settings, 0, 100, Options));
    }

    [Fact]
    public void Schedule_NeverExceedsPercentageOrAddsHintsInFinalFiveSeconds()
    {
        foreach (string speed in new[] { "slow", "normal", "fast" })
        {
            for (int total = 30; total <= 120; total += 15)
            {
                foreach (int percentage in new[] { 25, 50, 75 })
                {
                    for (int letters = 1; letters <= 120; letters++)
                    {
                        var settings = new GameSettings { DrawingTimeSeconds = total, HintSpeed = speed, HintPercentage = percentage };
                        int previous = 0;
                        for (int remaining = total; remaining >= 0; remaining--)
                        {
                            int count = PromptHintSchedule.RevealCount(settings, remaining, letters, Options);
                            Assert.InRange(count, previous, letters * percentage / 100);
                            if (remaining < 5)
                            {
                                Assert.Equal(previous, count);
                            }
                            previous = count;
                        }
                    }
                }
            }
        }
    }

    [Theory]
    [InlineData(3, 25, 0)]
    [InlineData(7, 25, 1)]
    [InlineData(12, 25, 3)]
    [InlineData(12, 50, 6)]
    [InlineData(8, 75, 6)]
    public void Percentage_IsBasedOnLengthAndRoundedDown(int letters, int percentage, int expected)
    {
        var settings = new GameSettings { DrawingTimeSeconds = 120, HintSpeed = "fast", HintPercentage = percentage };
        Assert.Equal(expected, PromptHintSchedule.RevealCount(settings, 0, letters, Options));
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(5, 5)]
    [InlineData(20, 5)]
    [InlineData(35, 5)]
    public void OlderRoomPercentages_UseTheNormalizedCap(int percentage, int expected)
    {
        var settings = new GameSettings { DrawingTimeSeconds = 120, HintSpeed = "fast", HintPercentage = percentage };
        Assert.Equal(expected, PromptHintSchedule.RevealCount(settings, 0, 20, Options));
    }

    [Fact]
    public void Phrase_RevealsAcrossWordsAndAlwaysKeepsTwoLettersHiddenPerWord()
    {
        const string answer = "walking the dog";
        string full = PromptHintMask.FullMask(answer);
        var order = PromptHintMask.ShuffledRevealOrder(full, new Random(42));
        string[] firstHints = PromptHintMask.WithRevealedLetters(answer, order, 3).Split(' ');
        Assert.All(firstHints, word => Assert.Single(word.Where(char.IsLetter)));
        string all = PromptHintMask.WithRevealedLetters(answer, order, 100);
        Assert.All(all.Split(' '), word => Assert.Equal(2, word.Count(c => c == '_')));
        Assert.Equal(order.Count, order.Distinct().Count());
        Assert.Equal(all, PromptHintMask.WithRevealedLetters(answer, order, 100));
    }

    [Theory]
    [InlineData("a to elephant")]
    [InlineData("zażółć   gęślą  jaźń!")]
    [InlineData("well-known phrase")]
    public void Mask_HandlesShortWordsAccentsPunctuationAndRepeatedSpaces(string answer)
    {
        string normalized = string.Join(' ', answer.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        string full = PromptHintMask.FullMask(answer);
        var order = PromptHintMask.ShuffledRevealOrder(full, new Random(1));
        string mask = PromptHintMask.WithRevealedLetters(answer, order, 100);
        Assert.Equal(normalized.Length, mask.Length);
        for (int i = 0; i < mask.Length; i++)
        {
            Assert.True(mask[i] == '_' || mask[i] == normalized[i]);
        }
        foreach (string word in mask.Split(' '))
        {
            Assert.True(word.Count(c => c == '_') >= Math.Min(2, word.Count(c => c == '_' || char.IsLetter(c))));
        }
    }

    [Fact]
    public void Mask_ProtectsWordsWithLegacyOrDuplicateRevealOrder()
    {
        const string answer = "go cat";
        var order = new[] { 0, 0, 1, 3, 4, 5, -1, 200 };
        Assert.Equal("__ c__", PromptHintMask.WithRevealedLetters(answer, order, 100));
    }
}
