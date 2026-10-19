using System.Text.RegularExpressions;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Features.Prompts.Configuration;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Prompts;

public class GamePromptPolicyTests
{
    private static readonly PromptOptions Options = PromptTestOptions.Create();

    [Theory]
    [InlineData("two words", PromptKinds.Word, false)]
    [InlineData("apple", PromptKinds.Word, true)]
    [InlineData("apple", PromptKinds.Phrase, false)]
    [InlineData("red apple", PromptKinds.Phrase, true)]
    [InlineData("one", PromptKinds.Saying, false)]
    [InlineData("better late", PromptKinds.Saying, true)]
    [InlineData("", PromptKinds.Word, false)]
    [InlineData("   ", PromptKinds.Phrase, false)]
    public void PromptPolicy_EnforcesKindAgainstWordCount(string text, string kind, bool expected)
    {
        Assert.Equal(expected, GamePromptPolicy.IsValid(new GamePrompt(text, "en", "general", kind), Options));
    }

    [Fact]
    public void PromptPolicy_RejectsOverlongText()
    {
        var prompt = new GamePrompt(new string('a', Options.MaxTextLength + 1), "en", "general", PromptKinds.Word);
        Assert.False(GamePromptPolicy.IsValid(prompt, Options));
        Assert.True(GamePromptPolicy.IsValid(prompt with
        {
            Text = new string('a', Options.MaxTextLength)
        }, Options));
    }

    [Fact]
    public void PromptPolicy_RejectsEmptyOrOverlongCategory()
    {
        Assert.False(GamePromptPolicy.IsValid(new GamePrompt("apple", "en", "", PromptKinds.Word), Options));
        var overlongCategory = new string('c', Options.MaxCategoryLength + 1);
        Assert.False(GamePromptPolicy.IsValid(new GamePrompt("apple", "en", overlongCategory, PromptKinds.Word), Options));
    }

    [Theory]
    [InlineData("en", true)]
    [InlineData("pl", true)]
    [InlineData("de", false)]
    [InlineData("", false)]
    public void PromptPolicy_RejectsUnsupportedLanguages(string language, bool expected)
    {
        Assert.Equal(expected, GamePromptPolicy.IsValid(new GamePrompt("apple", language, "general", PromptKinds.Word), Options));
    }

    [Fact]
    public void PromptPolicy_RejectsUnsupportedKind()
    {
        Assert.False(GamePromptPolicy.IsValid(new GamePrompt("apple", "en", "general", "sentence"), Options));
    }

    [Fact]
    public void HiddenHint_ForWord_ContainsLetterCountWithoutKindCategoryOrAnswer()
    {
        var prompt = new GamePrompt("apple", "en", "food", PromptKinds.Word);
        Assert.Equal("_____", prompt.HiddenHint);
        Assert.DoesNotContain("word", prompt.HiddenHint);
        Assert.DoesNotContain("food", prompt.HiddenHint);
        Assert.DoesNotContain("apple", prompt.HiddenHint);
    }

    [Theory]
    [InlineData("walking the dog", "everyday activities", PromptKinds.Phrase)]
    [InlineData("better late than never", "proverbs", PromptKinds.Saying)]
    [InlineData("break   the ice", "idioms", PromptKinds.Saying)]
    public void HiddenHint_ForMultiWordPrompt_NeverLeaksAnyAnswerToken(string text, string category, string kind)
    {
        var prompt = new GamePrompt(text, "en", category, kind);

        string normalizedHint = Regex.Replace(prompt.HiddenHint, @"\s+", " ").ToLowerInvariant();
        string normalizedText = Regex.Replace(text.Trim(), @"\s+", " ").ToLowerInvariant();

        Assert.Matches(@"^_+( _+)*$", prompt.HiddenHint);
        Assert.DoesNotContain(kind, prompt.HiddenHint);
        Assert.DoesNotContain(category, prompt.HiddenHint);
        Assert.DoesNotContain(normalizedText, normalizedHint);
        foreach (string token in normalizedText.Split(' '))
        {
            Assert.DoesNotContain(token, normalizedHint);
        }
    }
}
