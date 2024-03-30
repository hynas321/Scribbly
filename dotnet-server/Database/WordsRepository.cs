using Dapper;
using Dotnet.Server.Models;
using Microsoft.Data.Sqlite;

namespace Dotnet.Server.Database;

public class WordsRepository
{
    private readonly string connectionString;

    public WordsRepository(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DatabaseConnectionString");       
        
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string createTableQuery = @"CREATE TABLE IF NOT EXISTS Word (Text TEXT, Language TEXT);";

            db.Open();
            db.Execute(createTableQuery);
        } 
    }

    public bool AddWord(string text, string language)
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string insertQuery = "INSERT INTO Word (Text, Language) VALUES (@Text, @Language)";
            object parameters = new { Text = text, Language = language };

            db.Open();

            int rowsAffected = db.Execute(insertQuery, parameters);

            return rowsAffected > 0;
        }
    }

    public bool DeleteWord(string text, string language)
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string deleteQuery = "DELETE FROM Word WHERE Text = @Text AND Language = @Language";
            object parameters = new { Text = text, Language = language };

            db.Open();

            int rowsAffected = db.Execute(deleteQuery, parameters);

            return rowsAffected > 0;
        }
    }

    public WordBody[] GetWords()
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string query = "SELECT Text, Language FROM Word";

            db.Open();

            var words = db.Query<WordBody>(query).ToArray();

            return words;
        }
    }

    public string GetRandomWord(string language)
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string query = "SELECT Text From Word Where Language = @Language ORDER BY RANDOM() LIMIT 1";
            object parameters = new { Language = language };

            db.Open();

            return db.QueryFirstOrDefault<string>(query, parameters);
        }
    }
}
