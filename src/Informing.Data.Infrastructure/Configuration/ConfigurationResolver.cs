using Informing.Data.Domain.Enums;
using Informing.Data.Infrastructure.Configuration.Camunda;
using Informing.Data.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Informing.Data.Infrastructure.Configuration;

internal static class ConfigurationResolver
{
    private const string EnvironmentKeysBase = "INF_DATA_";
    private const string PgUsernameEnvKey = "PG_USERNAME";
    private const string PgPasswordEnvKey = "PG_PASSWORD";
    private const string PgAppUsernameEnvKey = "APP_USERNAME";
    private const string PgAppPasswordEnvKey = "APP_PASSWORD";

    /// <summary>
    /// Строка подключения для служебного пользователя БД
    /// </summary>
    internal static string GetPostgreConnectionString(
        this IConfiguration configuration,
        IHostEnvironment environment)
    {
        PostgresConnectionOptions connectionOptions = configuration.GetSection("Infrastructure:Postgres:ConnectionOptions")
                                                                   .Get<PostgresConnectionOptions>()
                                                                    ?? throw new ArgumentException("PostgreSQL connection options are missing");

        PostgresCredentialsOptions credentialsOptions =
                            environment.IsProduction() ?
                            GetProductionPostgreCredentials() :
                            GetPostgreCredentials(configuration);

        return
            $"USER ID={credentialsOptions.Username};Password={credentialsOptions.Password};Host={connectionOptions.Host};Port={connectionOptions.Port};Database={connectionOptions.Database};SearchPath={connectionOptions.Schema},public;Pooling=true";
    }

    /// <summary>
    /// Строка подключения для пользователя БД, используемого в бизнес-логике
    /// </summary>
    internal static string GetPostgreAppConnectionString(
        this IConfiguration configuration,
        IHostEnvironment environment)
    {
        PostgresConnectionOptions connectionOptions = configuration.GetSection("Infrastructure:Postgres:ConnectionOptions")
                                                                   .Get<PostgresConnectionOptions>()
                                                                    ?? throw new ArgumentException("PostgreSQL connection options are missing");

        PostgresCredentialsOptions credentialsOptions =
                            environment.IsProduction() ?
                            GetProductionPostgreCredentials() :
                            GetPostgreCredentials(configuration);

        return
            $"USER ID={credentialsOptions.AppUsername};Password={credentialsOptions.AppPassword};Host={connectionOptions.Host};Port={connectionOptions.Port};Database={connectionOptions.Database};SearchPath={connectionOptions.Schema},public;Pooling=true";
    }

    internal static CamundaWorkerOptions GetCamundaOptions(this IConfiguration configuration, CamundaWorkerTag workerTag)
    {
        return configuration
                        .GetSection($"Infrastructure:Camunda:WorkerOptions:{workerTag}")
                        .Get<CamundaWorkerOptions>()
                ?? throw new ArgumentException($"Camunda {workerTag} worker options are missing.");
    }

    private static PostgresCredentialsOptions GetPostgreCredentials(IConfiguration configuration)
    {
        return configuration.GetSection("Infrastructure:Postgres:CredentialsOptions").Get<PostgresCredentialsOptions>()
                ?? throw new ArgumentException("PostgreSQL credentials options are missing");
    }

    private static PostgresCredentialsOptions GetProductionPostgreCredentials()
    {
        return new PostgresCredentialsOptions
        {
            Username = GetEnvironmetVariable($"{EnvironmentKeysBase}{PgUsernameEnvKey}"),
            Password = GetEnvironmetVariable($"{EnvironmentKeysBase}{PgPasswordEnvKey}"),
            AppUsername = GetEnvironmetVariable($"{EnvironmentKeysBase}{PgAppUsernameEnvKey}"),
            AppPassword = GetEnvironmetVariable($"{EnvironmentKeysBase}{PgAppPasswordEnvKey}")
        };
    }

    private static string GetEnvironmetVariable(string envKey)
    {
        return Environment.GetEnvironmentVariable(envKey) ?? throw new ArgumentException($"{envKey} env variable is missing.");
    }
}