using Scribbly.Application.Features.Prompts;

namespace Scribbly.Application.Services.Interfaces;

public interface IRandomPromptService
{
    Task<GamePrompt?> FetchPromptAsync(string gameHash, CancellationToken cancellationToken);
    Task<GamePrompt?> FetchPromptForLanguageAsync(string language, CancellationToken cancellationToken);
}
