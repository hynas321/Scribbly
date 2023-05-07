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
}
