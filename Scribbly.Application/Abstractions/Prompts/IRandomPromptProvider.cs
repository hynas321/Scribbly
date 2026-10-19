using Scribbly.Application.Features.Prompts;

namespace Scribbly.Application.Abstractions.Prompts;

public interface IRandomPromptProvider
{
    Task<GamePrompt?> GetRandomPromptAsync(
        string language,
        IReadOnlyCollection<string> categories,
        CancellationToken cancellationToken = default);
}
