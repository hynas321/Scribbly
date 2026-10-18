using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Features.Prompts;

namespace Scribbly.WebApi.UnitTests.TestDoubles;

public class FakePromptRepository : IPromptRepository
{
    private readonly Dictionary<string, string[]> _categories;

    public FakePromptRepository(params (string Language, string[] Categories)[] catalog) =>
        _categories = catalog.ToDictionary(entry => entry.Language, entry => entry.Categories, StringComparer.Ordinal);

    public Task<bool> AddAsync(GamePrompt prompt, CancellationToken cancellationToken) => Task.FromResult(true);

    public Task<bool> DeleteAsync(GamePrompt prompt, CancellationToken cancellationToken) => Task.FromResult(true);

    public Task<IReadOnlyCollection<GamePrompt>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<GamePrompt>>(Array.Empty<GamePrompt>());

    public Task<IReadOnlyCollection<string>> GetCategoriesAsync(string language, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<string>>(
            _categories.TryGetValue(language, out string[]? categories) ? categories : Array.Empty<string>());
}
