using Moq;
using WebApi.Application.Managers;
using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;
using WebApi.Repositories.Interfaces;

namespace WebApi.UnitTests;

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
        ChatMessage chatMessage = new() { Username = "user1", Text = "Hello!" };

        _mockGameRepository.Setup(repo => repo.GetGame(It.IsAny<string>())).Returns((Game)null);

        // Act and Assert
        Assert.Throws<KeyNotFoundException>(() => _chatManager.AddChatMessage(_gameHash, chatMessage));
    }

    [Fact]
    public void AddChatMessage_ShouldAddMessage_WhenGameExists()
    {
        // Arrange
        Game game = new Game { ChatMessages = new List<ChatMessage>() };
        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);
        ChatMessage chatMessage = new() { Username = "user1", Text = "Hello!" };

        // Act
        _chatManager.AddChatMessage(_gameHash, chatMessage);

        // Assert
        Assert.Contains(game.ChatMessages, m => m.Text == "Hello!");
    }

    [Fact]
    public void AddChatMessage_ShouldRemoveOldestMessage_WhenMaxCountExceeded()
    {
        Game game = new() { ChatMessages = [] };

        for (int i = 0; i < 25; i++)
        {
            game.ChatMessages.Add(new ChatMessage { Text = $"Message {i}" });
        }

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        ChatMessage newMessage = new() { Username = "user1", Text = "New Message" };

        // Act
        _chatManager.AddChatMessage(_gameHash, newMessage);

        // Assert
        Assert.Equal(25, game.ChatMessages.Count);
        Assert.DoesNotContain(game.ChatMessages, m => m.Text == "Message 0");
        Assert.Contains(game.ChatMessages, m => m.Text == "New Message");
    }

    [Fact]
    public void AddAnnouncementMessage_ShouldAddMessage_WithNullUsername()
    {
        // Arrange
        Game game = new() { ChatMessages = [] };
        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        AnnouncementMessage announcementMessage = new()
        {
            Text = "Game starting soon!",
            BootstrapBackgroundColor = "alert-info"
        };

        // Act
        _chatManager.AddAnnouncementMessage(_gameHash, announcementMessage);

        // Assert
        ChatMessage? addedMessage = game.ChatMessages.Find(m => m.Text == "Game starting soon!");

        Assert.NotNull(addedMessage);
        Assert.Null(addedMessage.Username);
        Assert.Equal("alert-info", addedMessage.BootstrapBackgroundColor);
    }
}