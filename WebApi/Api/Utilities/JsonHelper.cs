using System.Text.Json;

namespace WebApi.Api.Utilities;

public class JsonHelper
{
    public static string Serialize(object o)
    {
        JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(o, jsonSerializerOptions);
    }
}