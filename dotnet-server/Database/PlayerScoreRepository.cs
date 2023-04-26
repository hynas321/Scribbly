using Dapper;
using Dotnet.Server.Database.Models;
using Dotnet.Server.JsonConfig;
using Microsoft.Data.Sqlite;

namespace Dotnet.Server.Database;

public class PlayerScoreRepository

{
    private readonly string connectionString;

    public PlayerScoreRepository()
    {
        ConfigHelper configHelper = new ConfigHelper();
        Config? config = configHelper.GetConfig();
        connectionString = config.DatabaseConnectionString ?? "null";
        
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            db.Open();
            string createTableQuery = $"CREATE TABLE IF NOT EXISTS PlayerScore (Username Text, Score INTEGER)";
            db.Execute(createTableQuery);
        }
            
    }

    public IEnumerable<PlayerScore> GetTopPlayerScores()
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            db.Open();
            string query = "SELECT * FROM PlayerScore ORDER BY Score DESC LIMIT 5";
            return db.Query<PlayerScore>(query);
        }
    }

    public void AddPlayerScore(PlayerScore playerScore)
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            db.Open();
            string query = "INSERT INTO PlayerScore (Username, Score) VALUES (@Username, @Score)";
            db.Execute(query, playerScore);
        }
    }
}