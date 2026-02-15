using Informing.Data.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Npgsql.NameTranslation;

namespace Informing.Data.Infrastructure.Dal;

internal static class Postgres
{
    private static readonly INpgsqlNameTranslator Translator = new NpgsqlSnakeCaseNameTranslator();

    public static void ConfigureTypeMapOptions()
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public static void AddDataSource(IServiceCollection services, string postgreConnectionString, IHostEnvironment hostEnvironment)
    {
        services.AddNpgsqlDataSource(
            connectionString: postgreConnectionString,
            builder =>
            {
                builder.MapEnum<OrderType>("order_type_enum", Translator);          

                if (hostEnvironment.IsDevelopment())
                {
                    builder.EnableParameterLogging();
                }
            }
        );
    }    
}