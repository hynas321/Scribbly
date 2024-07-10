using Dotnet.Server.Models;

namespace Dotnet.Server.Database;

public interface IAccountRepository {
    public bool AddAccount(Account account);
    public void IncrementAccountScore(string accessToken, int number);
    public int GetAccountScore(string accessToken);
    public Account GetAccount(string id);
    public IEnumerable<MainScoreboardScore> GetTopAccountPlayerScores();
}