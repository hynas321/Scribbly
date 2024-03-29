using System.Text.Json.Serialization;

namespace Dotnet.Server.JsonConfig;

public class Config
{
    [JsonPropertyName("httpServerUrl")]
    public string HttpServerUrl { get; set; }

    [JsonPropertyName("webSocketPort")]
    public int WebSocketPort { get; set; }

    [JsonPropertyName("corsOrigin")]
    public string CorsOrigin { get; set; }

    [JsonPropertyName("databaseConnectionString")]
    public string DatabaseConnectionString { get; set; }

    [JsonPropertyName("englishWordsApiKey")]
    public string EnglishWordsApiKey { get; set; }

    [JsonPropertyName("polishWordsApiKey")]
    public string PolishWordsApiKey { get; set; }

    [JsonPropertyName("adminToken")]
    public string AdminToken { get; set; }
}
