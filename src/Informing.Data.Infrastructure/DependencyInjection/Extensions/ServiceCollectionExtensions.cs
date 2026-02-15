using Informing.Data.CamundaApiClient;
using Informing.Data.Domain.Contracts.Camunda.Interfaces;
using Informing.Data.Domain.Contracts.Dal.Interfaces;
using Informing.Data.Domain.Contracts.Observability;
using Informing.Data.Domain.Enums;
using Informing.Data.Infrastructure.Camunda.Clients;
using Informing.Data.Infrastructure.Configuration;
using Informing.Data.Infrastructure.Configuration.Camunda;
using Informing.Data.Infrastructure.Dal;
using Informing.Data.Infrastructure.Dal.Repositories;
using Informing.Data.Infrastructure.Observability.Instrumentation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Informing.Data.Infrastructure.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDalInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        Postgres.ConfigureTypeMapOptions();
        Postgres.AddDataSource(
            services: services,
            postgreConnectionString: configuration.GetPostgreAppConnectionString(hostEnvironment),
            hostEnvironment: hostEnvironment
        );

        return services;
    }

    public static IServiceCollection AddDalRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPortInOrdersRepository, PortInOrdersRepository>();

        return services;
    }

    public static IServiceCollection AddCamundaClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var camundaPollingOptions = configuration.GetSection("Infrastructure:Camunda:PollingOptions").Get<CamundaPollingOptions>()
                                    ?? throw new ArgumentException("Camunda polling options are missing.");

        string fixedBaseUrl = camundaPollingOptions.BaseUrl;
        if (!fixedBaseUrl.EndsWith("/engine-rest/"))
        {
            fixedBaseUrl += "/engine-rest/";
        }

        services.AddHttpClient<Client>(httpClient =>
            {
                httpClient.BaseAddress = new Uri(fixedBaseUrl);
            }
        );

        services.AddKeyedSingleton<ICamundaClient, CamundaClient>(
            serviceKey: CamundaWorkerTag.PortIn,
            (serviceprovider, key) =>
            {
                var camundaClient = serviceprovider.GetRequiredService<Client>();
                camundaClient.BaseUrl = fixedBaseUrl;

                return new CamundaClient(
                    logger: serviceprovider.GetRequiredService<ILogger<CamundaClient>>(),
                    workerOptions: serviceprovider.GetRequiredKeyedService<CamundaWorkerOptions>(key),
                    client: camundaClient
                );
            }
        );

        services.AddKeyedSingleton<ICamundaClient, CamundaClient>(
            serviceKey: CamundaWorkerTag.PortOut,
            (serviceprovider, key) =>
            {
                var camundaClient = serviceprovider.GetRequiredService<Client>();
                camundaClient.BaseUrl = fixedBaseUrl;

                return new CamundaClient(
                    logger: serviceprovider.GetRequiredService<ILogger<CamundaClient>>(),
                    workerOptions: serviceprovider.GetRequiredKeyedService<CamundaWorkerOptions>(key),
                    client: camundaClient
                );
            }
        );
        return services;
    }

    public static IServiceCollection AddInfrastructureConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CamundaPollingOptions>(
            configuration.GetSection("Infrastructure:Camunda:PollingOptions")
            ?? throw new ArgumentException("Camunda polling options are missing.")
        );

        // Именнованные синглетоны для настроек клиентов portIn и portOut в Camunda
        // Потому что IOptions<T> не поддерживает версионирование, а IOptionsSnapshot<T>
        // не может являться зависимостью Singleton сервиса
        services.AddKeyedSingleton<CamundaWorkerOptions>(
            serviceKey: CamundaWorkerTag.PortIn,
            (serviceProvider, _) =>
            {
                return configuration.GetCamundaOptions(CamundaWorkerTag.PortIn);
            }
        );


        services.AddKeyedSingleton<CamundaWorkerOptions>(
            serviceKey: CamundaWorkerTag.PortOut,
            (serviceProvider, _) =>
            {
                return configuration.GetCamundaOptions(CamundaWorkerTag.PortOut);
            }
        );

        return services;
    }

    public static IServiceCollection AddObservabilityInstrumentation(
        this IServiceCollection services,
        IConfiguration configration)
    {
        services.AddKeyedSingleton<IWorkerInstrumentation>(
            serviceKey: CamundaWorkerTag.PortIn,
            implementationInstance: new WorkerInstrumentation(
                activitySourceName: $"{CamundaWorkerTag.PortIn}-Worker"
            )
        );

        services.AddKeyedSingleton<IWorkerInstrumentation>(
            serviceKey: CamundaWorkerTag.PortOut,
            implementationInstance: new WorkerInstrumentation(
                activitySourceName: $"{CamundaWorkerTag.PortOut}-Worker"
            )
        );

        return services;
    }
}