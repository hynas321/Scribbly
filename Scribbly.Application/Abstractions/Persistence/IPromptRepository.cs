using Scribbly.Application.Features.Prompts;

namespace Scribbly.Application.Abstractions.Persistence;

public interface IPromptRepository
{
    Task<bool> AddAsync(GamePrompt prompt, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(GamePrompt prompt, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<GamePrompt>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetCategoriesAsync(string language, CancellationToken cancellationToken);
}
