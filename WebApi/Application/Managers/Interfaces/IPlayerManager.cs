using WebApi.Domain.Entities;

namespace WebApi.Application.Managers.Interfaces;

public interface IPlayerManager
{
    void AddPlayer(string gameHash, Player player);
    void RemovePlayer(string gameHash, string token);
    (Player player, string gameHash) RemovePlayerByConnectionId(string connectionId);
    Player GetPlayerByToken(string gameHash, string token);
    List<PlayerScore> GetPlayerScores(string gameHash);
    List<string> GetOnlinePlayersTokens(string gameHash);
    bool CheckIfPlayerExistsByToken(string gameHash, string token);
    bool CheckIfPlayerExistsByUsername(string gameHash, string username);
    void UpdatePlayerScore(string gameHash, string token, int score);
}