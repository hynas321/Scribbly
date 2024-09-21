using WebApi.Api.Models.HttpRequest;

namespace WebApi.Infrastructure.Repositories.Interfaces;

public interface IWordRepository
{
    public bool AddWord(string text, string language);
    public bool DeleteWord(string text, string language);
    public List<WordBody> GetWords();
    public string GetRandomWord(string language);
}