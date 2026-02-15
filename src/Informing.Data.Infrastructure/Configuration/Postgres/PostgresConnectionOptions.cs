namespace Informing.Data.Infrastructure.Configuration.Options;

public class PostgresConnectionOptions
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public string Database { get; init; } = string.Empty;
    public string Schema { get; init; } = string.Empty;
}