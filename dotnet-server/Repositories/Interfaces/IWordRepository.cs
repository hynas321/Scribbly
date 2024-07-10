using dotnet_server.Models.Http.Request;

namespace Dotnet.Server.Database;

public interface IWordRepository {
    public bool AddWord(string text, string language);
    public bool DeleteWord(string text, string language);
    public List<WordBody> GetWords();
    public string GetRandomWord(string language);
}