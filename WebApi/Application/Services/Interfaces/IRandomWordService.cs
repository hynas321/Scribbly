namespace WebApi.Application.Services.Interfaces;

public interface IRandomWordService
{
    public Task<string> FetchWordAsync(string gameHash);
}