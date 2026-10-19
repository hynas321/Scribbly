using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Security;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Critical;

public class SecurityAndPromptTests
{
    [Fact]
    public void RoomCredential_IsOpaqueAndHashIsDeterministic()
    {
        var service = new RoomCredentialService();
        string token = service.GenerateAccessToken();
        string other = service.GenerateAccessToken();
        Assert.NotEqual(token, other);
        Assert.True(token.Length >= 40);
        Assert.Equal(64, service.ComputeCredentialHash(token).Length);
        Assert.True(service.Matches(token, service.ComputeCredentialHash(token)));
        Assert.False(service.Matches(other, service.ComputeCredentialHash(token)));
        Assert.Equal(16, service.GenerateRoomCode().Length);
    }

    [Theory]
    [InlineData("apple", "en", "food", "word", true)]
    [InlineData("walking the dog", "en", "activities", "phrase", true)]
    [InlineData("better late than never", "en", "proverbs", "saying", true)]
    [InlineData("jabłko", "pl", "jedzenie", "word", true)]
    [InlineData("robić śniadanie", "pl", "czynności", "phrase", true)]
    [InlineData("single", "en", "general", "phrase", false)]
    [InlineData("two words", "en", "general", "word", false)]
    public void PromptPolicy_ValidatesWordsPhrasesAndSayings(
        string text,
        string language,
        string category,
        string kind,
        bool expected)
    {
        Assert.Equal(expected, GamePromptPolicy.IsValid(new GamePrompt(text, language, category, kind), PromptTestOptions.Create()));
    }

    [Fact]
    public void PromptHint_ShowsLetterBlanksWithoutKindCategoryOrAnswerForMultiWordPrompt()
    {
        var prompt = new GamePrompt("break the ice", "en", "idioms", PromptKinds.Saying);
        Assert.Equal("_____ ___ ___", prompt.HiddenHint);
        Assert.DoesNotContain("saying", prompt.HiddenHint);
        Assert.DoesNotContain("idioms", prompt.HiddenHint);
        Assert.DoesNotContain("break the ice", prompt.HiddenHint);
    }
}
