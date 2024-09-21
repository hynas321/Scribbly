using WebApi.Application.Managers.Interfaces;

namespace WebApi.Application.Managers;

public class HashManager : IHashManager
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