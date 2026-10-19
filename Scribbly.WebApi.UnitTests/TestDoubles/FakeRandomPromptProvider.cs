using Scribbly.Application.Abstractions.Prompts;
using Scribbly.Application.Features.Prompts;

namespace Scribbly.WebApi.UnitTests.TestDoubles;

public class FakeRandomPromptProvider : IRandomPromptProvider
{
    private readonly object _sync = new();
    private readonly Dictionary<string, Queue<GamePrompt>> _prompts = new(StringComparer.Ordinal);

    public string? LastRequestedLanguage { get; private set; }
    public IReadOnlyCollection<string>? LastRequestedCategories { get; private set; }

    public FakeRandomPromptProvider(params GamePrompt[] prompts)
    {
        foreach (GamePrompt prompt in prompts)
        {
            if (!_prompts.TryGetValue(prompt.Language, out Queue<GamePrompt>? queue))
            {
                queue = new Queue<GamePrompt>();
                _prompts[prompt.Language] = queue;
            }
            queue.Enqueue(prompt);
        }
    }

    public Task<GamePrompt?> GetRandomPromptAsync(
        string language,
        IReadOnlyCollection<string> categories,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            LastRequestedLanguage = language;
            LastRequestedCategories = categories.ToArray();
            GamePrompt? prompt = _prompts.TryGetValue(language, out Queue<GamePrompt>? queue) && queue.Count > 0
                ? queue.Dequeue()
                : null;
            return Task.FromResult(prompt);
        }
    }
}
