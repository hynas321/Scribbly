using dotnet_server.Api.Models.HttpRequest;

namespace dotnet_server.Infrastructure.Repositories.Interfaces;

public interface IWordRepository
{
    public bool AddWord(string text, string language);
    public bool DeleteWord(string text, string language);
    public List<WordBody> GetWords();
    public string GetRandomWord(string language);
}