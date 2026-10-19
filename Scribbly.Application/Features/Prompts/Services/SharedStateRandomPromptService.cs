using Scribbly.Application.Abstractions.Prompts;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Services.Interfaces;
using Scribbly.Domain.Static;

namespace Scribbly.Application.Services;

public class SharedStateRandomPromptService : IRandomPromptService
{
    private readonly IGameLifecycleService _games;
    private readonly IRandomPromptProvider _prompts;

    public SharedStateRandomPromptService(IGameLifecycleService games, IRandomPromptProvider prompts)
    {
        _games = games;
        _prompts = prompts;
    }

    public async Task<GamePrompt?> FetchPromptAsync(string gameHash, CancellationToken cancellationToken)
    {
        var snapshot = await _games.GetSnapshotAsync(gameHash, cancellationToken)
            ?? throw new InvalidOperationException("Game does not exist.");
        return await FetchPromptCoreAsync(snapshot.GameSettings.PromptLanguage, snapshot.GameSettings.PromptCategories, cancellationToken);
    }

    public Task<GamePrompt?> FetchPromptForLanguageAsync(string language, CancellationToken cancellationToken) =>
        FetchPromptCoreAsync(language, Array.Empty<string>(), cancellationToken);

    private Task<GamePrompt?> FetchPromptCoreAsync(
        string language,
        IReadOnlyCollection<string> categories,
        CancellationToken cancellationToken) =>
        _prompts.GetRandomPromptAsync(language is Languages.EN or Languages.PL ? language : Languages.EN, categories, cancellationToken);
}
