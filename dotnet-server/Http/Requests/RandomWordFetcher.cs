using Dotnet.Server.JsonConfig;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;

namespace Dotnet.Server.Http.Requests;

class RandomWordFetcher
{
    public static async Task<string> FetchWordAsync()
    {
        GameManager gameManager = new GameManager();
        Game game = gameManager.GetGame();

        switch (game.GameSettings.WordLanguage)
        {
            case "en":
                return await FetchEnglishWord(game);
            //case "pl":
                //return await FetchPolishWord(game);
            default:
                return await FetchEnglishWord(game);
        }
    }

    public static async Task<string> FetchEnglishWord(Game game)
    {
        GameManager gameManager = new GameManager();
        ConfigHelper configHelper = new ConfigHelper();
        Config config = configHelper.GetConfig();
        
        string apiUrl = "";

        if (game.GameSettings.NounsOnly)
        {
            apiUrl = $"https://api.wordnik.com/v4/words.json/randomWord?hasDictionaryDef=true&includePartOfSpeech=noun&minCorpusCount=10000&maxCorpusCount=-1&minDictionaryCount=1&maxDictionaryCount=-1&minLength=5&maxLength=-1&includeTags=false&api_key=" + config.EnglishWordsApiKey;
        }
        else
        {
            apiUrl = $"https://api.wordnik.com/v4/words.json/randomWord?hasDictionaryDef=true&includePartOfSpeech=noun,adjective,verb&minCorpusCount=10000&maxCorpusCount=-1&minDictionaryCount=1&maxDictionaryCount=-1&minLength=5&maxLength=-1&includeTags=false&api_key=" + config.EnglishWordsApiKey;
        }

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    RandomWordApiResponse wordnikResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<RandomWordApiResponse>(json);

                    return wordnikResponse.Word;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }

    //Api does not work (Status code 503)
    public static async Task<string> FetchPolishWord(Game game)
    {
        GameManager gameManager = new GameManager();
        ConfigHelper configHelper = new ConfigHelper();
        Config config = configHelper.GetConfig();
        
        string apiUrl = "";

        Random random = new Random();
        int wordLength = random.Next(5, 20);

        if (game.GameSettings.NounsOnly)
        {
            apiUrl = $"https://polish-words.p.rapidapi.com/noun/random/{wordLength}";
        }
        else
        {
            apiUrl = $"https://polish-words.p.rapidapi.com/word/random/{wordLength}";
        }

        using (HttpClient client = new HttpClient())
        {
            try
            {
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", config.PolishWordsApiKey);
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "polish-words.p.rapidapi.com");

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    RandomWordApiResponse wordnikResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<RandomWordApiResponse>(json);

                    return wordnikResponse.Word;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}