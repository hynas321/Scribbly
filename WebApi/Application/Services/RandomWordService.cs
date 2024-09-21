using WebApi.Api.Models.HttpResponse;
using WebApi.Api.Utilities;
using WebApi.Application.Managers.Interfaces;
using WebApi.Application.Services.Interfaces;
using WebApi.Domain.Entities;
using WebApi.Domain.Static;
using WebApi.Infrastructure.Repositories.Interfaces;

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
        Game game = _gameManager.GetGame(gameHash);

        switch (game.GameSettings.WordLanguage)
        {
            case Languages.EN:
                return await FetchEnglishWord();
            case Languages.PL:
                return await FetchPolishWord();
            default:
                return await FetchEnglishWord();
        }
    }

    public async Task<string> FetchEnglishWord()
    {
        try
        {
            string englishWordApiKey = _configuration[AppSettingsVariables.EnglishWordsApiKey];
            string apiUrl = $"https://api.wordnik.com/v4/words.json/randomWord?hasDictionaryDef=true&includePartOfSpeech=noun&minCorpusCount=10000&maxCorpusCount=-1&minDictionaryCount=1&maxDictionaryCount=-1&minLength=5&maxLength=-1&includeTags=false&api_key=" + englishWordApiKey;

            using HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                RandomWordApiResponse randomWordResponse =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<RandomWordApiResponse>(json);

                return randomWordResponse.Word.ToLower();
            }
            else
            {
                return _wordRepository.GetRandomWord(Languages.EN);
            }
        }
        catch
        {
            return null;
        }
    }

    //Api does not work (Status code 503)
    public async Task<string> FetchPolishWord()
    {
        try
        {
            Random random = new Random();
            int wordLength = random.Next(5, 20);

            string polishWordApiKey = _configuration[AppSettingsVariables.PolishWordsApiKey];
            string apiUrl = $"https://polish-words.p.rapidapi.com/noun/random/{wordLength}";

            using HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", polishWordApiKey);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "polish-words.p.rapidapi.com");

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                RandomWordApiResponse randomWordResponse =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<RandomWordApiResponse>(json);

                return randomWordResponse.Word.ToLower();
            }
            else
            {
                return _wordRepository.GetRandomWord(Languages.PL);
            }
        }
        catch
        {
            return null;
        }
    }
}