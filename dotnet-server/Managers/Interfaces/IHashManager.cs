namespace Dotnet.Server.Managers;

public interface IHashManager {
    public string GenerateGameHash();
    public string GenerateUserHash();
}