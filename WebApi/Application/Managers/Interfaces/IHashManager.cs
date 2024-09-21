namespace WebApi.Application.Managers.Interfaces;

public interface IHashManager
{
    public string GenerateGameHash();
    public string GenerateUserHash();
}