using Dotnet.Server.Database;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using Dotnet.Server.Models.Static;

namespace Dotnet.Server.Http;

public class RandomWordFetcher
{
    private readonly IConfiguration configuration;
    private readonly WordsRepository wordsRepository;

    public RandomWordFetcher(IConfiguration configuration)
    {
        this.configuration = configuration;
        this.wordsRepository = new WordsRepository(configuration);
    }

    public async Task<string> FetchWordAsync(string gameHash)
    {
        GameManager gameManager = new GameManager();
        Game game = gameManager.GetGame(gameHash);

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
        string englishWordApiKey = configuration[AppSettingsVariables.EnglishWordsApiKey];
        string apiUrl = $"https://api.wordnik.com/v4/words.json/randomWord?hasDictionaryDef=true&includePartOfSpeech=noun&minCorpusCount=10000&maxCorpusCount=-1&minDictionaryCount=1&maxDictionaryCount=-1&minLength=5&maxLength=-1&includeTags=false&api_key=" + englishWordApiKey;

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    RandomWordApiResponse randomWordResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<RandomWordApiResponse>(json);

                    return randomWordResponse.Word.ToLower();
                }
                else
                {
                    return wordsRepository.GetRandomWord(Languages.EN);
                }
            }
            catch
            {
                return null;
            }
        }
    }

    //Api does not work (Status code 503)
    public async Task<string> FetchPolishWord()
    {
        Random random = new Random();
        int wordLength = random.Next(5, 20);

        string polishWordApiKey = configuration[AppSettingsVariables.PolishWordsApiKey];
        string apiUrl = $"https://polish-words.p.rapidapi.com/noun/random/{wordLength}";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", polishWordApiKey);
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "polish-words.p.rapidapi.com");

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    RandomWordApiResponse randomWordResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<RandomWordApiResponse>(json);

                    return randomWordResponse.Word.ToLower();
                }
                else
                {
                    return wordsRepository.GetRandomWord(Languages.PL);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}