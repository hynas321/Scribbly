using WebApi.Application.Managers;
using WebApi.Application.Managers.Interfaces;

namespace WebApi.UnitTests;

public class HashManagerTests
{
    private readonly IHashManager _hashManager;

    public HashManagerTests()
    {
        _hashManager = new HashManager();
    }

    [Fact]
    public void GenerateGameHash_ShouldReturnHashOfCorrectLength()
    {
        // Act
        string hash = _hashManager.GenerateGameHash();

        // Assert
        Assert.Equal(8, hash.Length);
    }

    [Fact]
    public void GenerateUserHash_ShouldReturnHashOfCorrectLength()
    {
        // Act
        string hash = _hashManager.GenerateUserHash();

        // Assert
        Assert.Equal(16, hash.Length);
    }
}