using dotnet_server.Domain.Entities;

namespace dotnet_server.Application.Managers.Interfaces
{
    public interface IChatManager
    {
        void AddChatMessage(string gameHash, ChatMessage chatMessage);
        void AddAnnouncementMessage(string gameHash, AnnouncementMessage message);
    }
}