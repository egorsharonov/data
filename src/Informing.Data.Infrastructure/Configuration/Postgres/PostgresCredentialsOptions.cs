namespace Informing.Data.Infrastructure.Configuration.Options;

public class PostgresCredentialsOptions
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string AppUsername { get; init; } = string.Empty;
    public string AppPassword { get; init; } = string.Empty;
}