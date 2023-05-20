using Dapper;
using Dotnet.Server.JsonConfig;
using Dotnet.Server.Models;
using Microsoft.Data.Sqlite;

namespace Dotnet.Server.Database;

public class AccountRepository
{
    private readonly string connectionString;

    public AccountRepository()
    {
        ConfigHelper configHelper = new ConfigHelper();
        Config config = configHelper.GetConfig();
        connectionString = config.DatabaseConnectionString ?? "null";
        
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string createTableQuery = $"CREATE TABLE IF NOT EXISTS Account" +
                "(Id TEXT PRIMARY KEY, AccessToken TEXT, Email TEXT, Name TEXT, GivenName TEXT, FamilyName TEXT, Score INTEGER)";

            db.Open();
            db.Execute(createTableQuery);
        } 
    }

    public bool AddAccountIfNotExists(Account account)
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string insertQuery = "INSERT OR IGNORE INTO Account (Id, AccessToken, Email, Name, GivenName, FamilyName, Score) " +
                "VALUES (@Id, @AccessToken, @Email, @Name, @GivenName, @FamilyName, 0)";

            db.Open();

            int rowsAffected = db.Execute(insertQuery, account);

            if (rowsAffected == 0)
            {   
                string updateQuery = "UPDATE Account SET AccessToken = @AccessToken WHERE ID = @Id";
                object parameters = new { Id = account.Id, AccessToken = account.AccessToken };

                db.Execute(updateQuery, parameters);
                return false;
            }

            return true;
        }
    }

    public void IncrementAccountScore(string accessToken, int number)
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string query = "UPDATE Account SET Score = Score + @Number WHERE AccessToken = @AccessToken";
            object parameters = new { AccessToken = accessToken, Number = number };

            db.Open();
            db.Execute(query, parameters);
        }
    }

    public int GetAccountScore(string accessToken)
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string query = "SELECT Score From Account Where AccessToken = @AccessToken";
            object parameters = new { AccessToken = accessToken };

            db.Open();

            return db.QueryFirstOrDefault<int>(query, parameters);
        }
    }

    public Account GetAccount(string id)
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string query = "SELECT * FROM Account WHERE Id = @Id";
            object parameters = new { Id = id };

            db.Open();

            return db.QueryFirstOrDefault<Account>(query, parameters);
        }
    }

    public IEnumerable<MainScoreboardScore> GetTopAccountPlayerScores()
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string query = "SELECT GivenName, Email, Score FROM Account ORDER BY Score DESC LIMIT 5";

            db.Open();

            return db.Query<MainScoreboardScore>(query);
        }
    }
}