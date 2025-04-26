using WebApi.Api.Models.HttpRequest;

namespace WebApi.Infrastructure.Repositories.Interfaces;

public interface IWordRepository
{
    public Task<bool> AddWordAsync(string text, string language, CancellationToken cancellationToken);
    public Task<bool> DeleteWordAsync(string text, string language, CancellationToken cancellationToken);
    public Task<List<WordBody>> GetWordsAsync(CancellationToken cancellationToken);
    public Task<string> GetRandomWordAsync(string language, CancellationToken cancellationToken);
}