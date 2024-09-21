using dotnet_server.Application.Managers.Interfaces;

namespace dotnet_server.Application.Managers;

class HashManager : IHashManager
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