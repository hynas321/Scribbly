using System.Text.Json;

namespace Dotnet.Server.JsonConfig;

class ConfigHelper
{   
    private readonly Config? config;

    public ConfigHelper(string configFilePath)
    {
        string json = File.ReadAllText(configFilePath);
        config = JsonSerializer.Deserialize<Config>(json);
    }

    public Config GetConfig()
    {   
        return config ?? throw new NullReferenceException();
    }
}