using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Security;
using Scribbly.Application.Services;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Prompts;

public class SharedStateRandomPromptServiceTests
{
    private static async Task<(InMemoryGameStateStore Games, GameLifecycleService Lifecycle, CreateGameResult Room)> CreateGame()
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 4), GameSettingsTestOptions.Create());
        CreateGameResult room = await lifecycle.CreateAsync("host");
        return (games, lifecycle, room);
    }

    [Fact]
    public async Task FetchPromptAsync_ReturnsPromptMatchingGameLanguage()
    {
        var (games, lifecycle, room) = await CreateGame();
        await games.UpdateAsync(room.GameHash, game =>
        {
            game.GameSettings.PromptLanguage = "pl";
            return 0;
        });
        var provider = new FakeRandomPromptProvider(
            new GamePrompt("apple", "en", "food", PromptKinds.Word),
            new GamePrompt("jabłko", "pl", "jedzenie", PromptKinds.Word));
        var service = new SharedStateRandomPromptService(lifecycle, provider);

        GamePrompt? prompt = await service.FetchPromptAsync(room.GameHash, CancellationToken.None);

        Assert.NotNull(prompt);
        Assert.Equal("pl", prompt!.Language);
        Assert.Equal("jabłko", prompt.Text);
        Assert.Equal("pl", provider.LastRequestedLanguage);
    }

    [Fact]
    public async Task FetchPromptForLanguageAsync_UnsupportedLanguage_FallsBackToEnglish()
    {
        var (_, lifecycle, _) = await CreateGame();
        var provider = new FakeRandomPromptProvider(new GamePrompt("apple", "en", "food", PromptKinds.Word));
        var service = new SharedStateRandomPromptService(lifecycle, provider);

        GamePrompt? prompt = await service.FetchPromptForLanguageAsync("de", CancellationToken.None);

        Assert.Equal("en", provider.LastRequestedLanguage);
        Assert.Equal("apple", prompt!.Text);
    }

    [Fact]
    public async Task FetchPromptAsync_UnknownGame_Throws()
    {
        var (_, lifecycle, _) = await CreateGame();
        var provider = new FakeRandomPromptProvider(new GamePrompt("apple", "en", "food", PromptKinds.Word));
        var service = new SharedStateRandomPromptService(lifecycle, provider);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.FetchPromptAsync("UNKNOWNHASH00000", CancellationToken.None));
    }

    [Fact]
    public async Task FetchPromptForLanguageAsync_EmptyCatalog_ReturnsNull()
    {
        var (_, lifecycle, _) = await CreateGame();
        var service = new SharedStateRandomPromptService(lifecycle, new FakeRandomPromptProvider());

        GamePrompt? prompt = await service.FetchPromptForLanguageAsync("en", CancellationToken.None);

        Assert.Null(prompt);
    }
}
