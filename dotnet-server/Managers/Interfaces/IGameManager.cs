using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

public interface IGameManager
{
    void CreateGame(Game game, string gameHash);
    Game GetGame(string gameHash);
    void RemoveGame(string gameHash);
    void AddPlayer(string gameHash, Player player);
    void RemovePlayer(string gameHash, string token);
    Player GetPlayerByToken(string gameHash, string token);
    List<PlayerScore> GetPlayerObjectsWithoutToken(string gameHash);
    List<string> GetOnlinePlayersTokens(string gameHash);
    bool CheckIfPlayerExistsByToken(string gameHash, string token);
    bool CheckIfPlayerExistsByUsername(string gameHash, string username);
    void AddChatMessage(string gameHash, ChatMessage chatMessage);
    void AddChatMessage(string gameHash, AnnouncementMessage message);
    void UpdatePlayerScore(string gameHash, string token, int score);
    (Player player, string gameHash) RemovePlayer(string connectionId);
}