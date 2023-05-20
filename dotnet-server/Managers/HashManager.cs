namespace Dotnet.Server.Managers;

class HashManager
{
    public string GenerateGameHash()
    {
        return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
    }

    public string GenerateUserHash()
    {
        return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
    }
}