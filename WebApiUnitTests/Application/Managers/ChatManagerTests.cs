using FluentAssertions;
using Moq;
using WebApi.Application.Managers;
using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;
using WebApi.Repositories.Interfaces;
using Xunit;

namespace WebApiUnitTests.Application.Managers;

public class ChatManagerTests
{
    private readonly IChatManager _chatManager;
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly string _gameHash = "game123";

    public ChatManagerTests()
    {
        _mockGameRepository = new Mock<IGameRepository>();
        _chatManager = new ChatManager(_mockGameRepository.Object);
    }

    [Fact]
    public void AddChatMessage_ShouldThrowException_WhenGameNotFound()
    {
        // Arrange
        var chatMessage = new ChatMessage { Username = "user1", Text = "Hello!" };

        _mockGameRepository.Setup(repo => repo.GetGame(It.IsAny<string>())).Returns((Game)null);

        // Act
        Action act = () => _chatManager.AddChatMessage(_gameHash, chatMessage);

        // Assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("Game not found.");
    }

    [Fact]
    public void AddChatMessage_ShouldAddMessage_WhenGameExists()
    {
        // Arrange
        var game = new Game { ChatMessages = new List<ChatMessage>() };
        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);
        var chatMessage = new ChatMessage { Username = "user1", Text = "Hello!" };

        // Act
        _chatManager.AddChatMessage(_gameHash, chatMessage);

        // Assert
        game.ChatMessages.Should().Contain(m => m.Text == "Hello!");
        game.ChatMessages.Should().HaveCount(1);
    }

    [Fact]
    public void AddChatMessage_ShouldRemoveOldestMessage_WhenMaxCountExceeded()
    {
        // Arrange
        var game = new Game { ChatMessages = new List<ChatMessage>() };

        for (int i = 0; i < 25; i++)
        {
            game.ChatMessages.Add(new ChatMessage { Text = $"Message {i}" });
        }

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        var newMessage = new ChatMessage { Username = "user1", Text = "New Message" };

        // Act
        _chatManager.AddChatMessage(_gameHash, newMessage);

        // Assert
        game.ChatMessages.Should().HaveCount(25);
        game.ChatMessages.Should().NotContain(m => m.Text == "Message 0");
        game.ChatMessages.Should().Contain(m => m.Text == "New Message");
    }

    [Fact]
    public void AddChatMessage_ShouldNotRemoveMessage_WhenBelowMaxCount()
    {
        // Arrange
        var game = new Game { ChatMessages = new List<ChatMessage>() };

        for (int i = 0; i < 24; i++)
        {
            game.ChatMessages.Add(new ChatMessage { Text = $"Message {i}" });
        }

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        var newMessage = new ChatMessage { Username = "user1", Text = "New Message" };

        // Act
        _chatManager.AddChatMessage(_gameHash, newMessage);

        // Assert
        game.ChatMessages.Should().HaveCount(25);
        game.ChatMessages.Should().Contain(m => m.Text == "Message 0");
        game.ChatMessages.Should().Contain(m => m.Text == "New Message");
    }

    [Fact]
    public void AddAnnouncementMessage_ShouldAddMessage_WithNullUsername()
    {
        // Arrange
        var game = new Game { ChatMessages = new List<ChatMessage>() };
        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        var announcementMessage = new AnnouncementMessage
        {
            Text = "Game starting soon!",
            BootstrapBackgroundColor = "alert-info"
        };

        // Act
        _chatManager.AddAnnouncementMessage(_gameHash, announcementMessage);

        // Assert
        var addedMessage = game.ChatMessages.Find(m => m.Text == "Game starting soon!");

        addedMessage.Should().NotBeNull();
        addedMessage.Username.Should().BeNull();
        addedMessage.BootstrapBackgroundColor.Should().Be("alert-info");
    }

    [Fact]
    public void AddAnnouncementMessage_ShouldUseTextAndColorFromMessage()
    {
        // Arrange
        var game = new Game { ChatMessages = new List<ChatMessage>() };
        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        var announcementMessage = new AnnouncementMessage
        {
            Text = "Warning!",
            BootstrapBackgroundColor = "alert-danger"
        };

        // Act
        _chatManager.AddAnnouncementMessage(_gameHash, announcementMessage);

        // Assert
        game.ChatMessages.Should().ContainSingle();
        game.ChatMessages[0].Text.Should().Be("Warning!");
        game.ChatMessages[0].BootstrapBackgroundColor.Should().Be("alert-danger");
    }
}
