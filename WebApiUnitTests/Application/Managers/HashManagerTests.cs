using FluentAssertions;
using WebApi.Application.Managers;
using WebApi.Application.Managers.Interfaces;
using Xunit;

namespace WebApiUnitTests.Application.Managers;

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
        hash.Length.Should().Be(8);
    }

    [Fact]
    public void GenerateUserHash_ShouldReturnHashOfCorrectLength()
    {
        // Act
        string hash = _hashManager.GenerateUserHash();

        // Assert
        hash.Length.Should().Be(16);
    }

    [Fact]
    public void GenerateGameHash_ShouldReturnUniqueHashes()
    {
        // Act
        string hash1 = _hashManager.GenerateGameHash();
        string hash2 = _hashManager.GenerateGameHash();

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GenerateUserHash_ShouldReturnUniqueHashes()
    {
        // Act
        string hash1 = _hashManager.GenerateUserHash();
        string hash2 = _hashManager.GenerateUserHash();

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GenerateGameHash_ShouldContainOnlyAlphanumericCharacters()
    {
        // Act
        string hash = _hashManager.GenerateGameHash();

        // Assert
        hash.Should().MatchRegex("^[a-zA-Z0-9]*$");
    }

    [Fact]
    public void GenerateUserHash_ShouldContainOnlyAlphanumericCharacters()
    {
        // Act
        string hash = _hashManager.GenerateUserHash();

        // Assert
        hash.Should().MatchRegex("^[a-zA-Z0-9]*$");
    }
}
