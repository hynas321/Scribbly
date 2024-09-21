using WebApi.Domain.Entities;

namespace WebApi.Application.Managers.Interfaces;

public interface IChatManager
{
    void AddChatMessage(string gameHash, ChatMessage chatMessage);
    void AddAnnouncementMessage(string gameHash, AnnouncementMessage message);
}