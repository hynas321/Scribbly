namespace dotnet_server.Services.Interfaces;

public interface IRandomWordService
{
    public Task<string> FetchWordAsync(string gameHash);
    public Task<string> FetchEnglishWord();
    public Task<string> FetchPolishWord();
}