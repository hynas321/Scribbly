namespace dotnet_server.Application.Services.Interfaces;

public interface IRandomWordService
{
    public Task<string> FetchWordAsync(string gameHash);
    public Task<string> FetchEnglishWord();
    public Task<string> FetchPolishWord();
}