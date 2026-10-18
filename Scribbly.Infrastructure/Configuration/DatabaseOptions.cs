namespace Scribbly.Infrastructure.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";
    public const string DatabaseConnectionStringName = "DatabaseConnectionString";

    public string DatabaseConnectionString { get; set; } = string.Empty;
}
