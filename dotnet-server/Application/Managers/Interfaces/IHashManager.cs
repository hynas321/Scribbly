namespace dotnet_server.Application.Managers.Interfaces;

public interface IHashManager
{
    public string GenerateGameHash();
    public string GenerateUserHash();
}