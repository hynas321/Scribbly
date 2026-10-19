namespace Scribbly.Infrastructure.Redis;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public bool Enabled { get; set; }
    public string ConnectionString { get; set; } = "localhost:6379,abortConnect=false";
    public string KeyPrefix { get; set; } = "scribbly";
    public int GameTtlHours { get; set; } = 24;
    public int LockTimeoutSeconds { get; set; } = 5;
    public int LockLeaseSeconds { get; set; } = 15;
    public int SessionLeaseSeconds { get; set; } = 30;
}
