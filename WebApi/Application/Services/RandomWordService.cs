using Newtonsoft.Json;
using WebApi.Api.Models.HttpResponse;
using WebApi.Domain.Static;
using WebApi.Infrastructure.Repositories.Interfaces;
using WebApi.Application.Managers.Interfaces;
using WebApi.Application.Services.Interfaces;
using WebApi.Api.Utilities;

namespace WebApi.Application.Services;

public class RandomWordService : IRandomWordService
{
    private readonly IConfiguration _configuration;
    private readonly IWordRepository _wordRepository;
    private readonly IGameManager _gameManager;

    public RandomWordService(
        IConfiguration configuration,
        IWordRepository wordRepository,
        IGameManager gameManager)
    {
        _configuration = configuration;
        _wordRepository = wordRepository;
        _gameManager = gameManager;
    }

    public async Task<string> FetchWordAsync(string gameHash)
    {
        var game = _gameManager.GetGame(gameHash);
        return game.GameSettings.WordLanguage switch
        {
            Languages.PL => await FetchPolishWordAsync(),
            Languages.EN => await FetchEnglishWordAsync(),
            _ => await FetchEnglishWordAsync()
        };
    }

    private async Task<string> FetchEnglishWordAsync()
    {
        string apiKey = _configuration[AppSettingsVariables.EnglishWordsApiKey];
        string apiUrl = $"https://api.wordnik.com/v4/words.json/randomWord" +
                        $"?hasDictionaryDef=true&includePartOfSpeech=noun&minCorpusCount=10000" +
                        $"&minDictionaryCount=1&minLength=5&includeTags=false&api_key={apiKey}";

        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return _wordRepository.GetRandomWord(Languages.EN);
            }

            var json = await response.Content.ReadAsStringAsync();
            var wordResponse = JsonConvert.DeserializeObject<RandomWordApiResponse>(json);
            return wordResponse?.Word?.ToLower();
        }
        catch
        {
            return null;
        }
    }

    private async Task<string> FetchPolishWordAsync()
    {
        string apiKey = _configuration[AppSettingsVariables.PolishWordsApiKey];
        int wordLength = new Random().Next(5, 20);
        string apiUrl = $"https://polish-words.p.rapidapi.com/noun/random/{wordLength}";

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "polish-words.p.rapidapi.com");

            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return _wordRepository.GetRandomWord(Languages.PL);
            }

            var json = await response.Content.ReadAsStringAsync();
            var wordResponse = JsonConvert.DeserializeObject<RandomWordApiResponse>(json);
            return wordResponse?.Word?.ToLower();
        }
        catch
        {
            return null;
        }
    }
}