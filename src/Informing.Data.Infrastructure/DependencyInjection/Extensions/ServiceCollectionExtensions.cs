using Informing.Data.CamundaApiClient;
using Informing.Data.Domain.Contracts.Camunda.Interfaces;
using Informing.Data.Domain.Contracts.Observability;
using Informing.Data.Domain.Enums;
using Informing.Data.Infrastructure.Camunda.Clients;
using Informing.Data.Infrastructure.Configuration;
using Informing.Data.Infrastructure.Configuration.Camunda;
using Informing.Data.Domain.Configuration.Parameters;
using Informing.Data.Infrastructure.Observability.Instrumentation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Informing.Data.Infrastructure.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CamundaPollingOptions>(configuration.GetRequiredSection("Infrastructure:Camunda:PollingOptions"));
        services.Configure<ParameterResolutionOptions>(configuration.GetRequiredSection("Infrastructure:Parameters:Resolution"));

        services.AddKeyedSingleton<CamundaWorkerOptions>(serviceKey: CamundaWorkerTag.ParameterService,
            implementationFactory: (_, _) => configuration.GetCamundaOptions(CamundaWorkerTag.ParameterService));

        return services;
    }

    public static IServiceCollection AddCamundaClient(this IServiceCollection services, IConfiguration configuration)
    {
        var pollingOptions = configuration.GetRequiredSection("Infrastructure:Camunda:PollingOptions").Get<CamundaPollingOptions>()
            ?? throw new ArgumentException("Camunda polling options are missing.");

        var baseUrl = pollingOptions.BaseUrl.EndsWith("/engine-rest/") ? pollingOptions.BaseUrl : $"{pollingOptions.BaseUrl}/engine-rest/";

        services.AddHttpClient<Client>(httpClient => httpClient.BaseAddress = new Uri(baseUrl));

        services.AddKeyedSingleton<ICamundaClient, CamundaClient>(CamundaWorkerTag.ParameterService,
            (serviceProvider, key) =>
            {
                var client = serviceProvider.GetRequiredService<Client>();
                client.BaseUrl = baseUrl;

                return new CamundaClient(
                    serviceProvider.GetRequiredService<ILogger<CamundaClient>>(),
                    serviceProvider.GetRequiredKeyedService<CamundaWorkerOptions>(key),
                    client);
            });

        return services;
    }

    public static IServiceCollection AddObservabilityInstrumentation(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IWorkerInstrumentation>(CamundaWorkerTag.ParameterService,
            new WorkerInstrumentation($"{CamundaWorkerTag.ParameterService}-Worker"));

        return services;
    }

    public static IServiceCollection AddExternalParameterProviders(this IServiceCollection services)
    {
        return services;
    }
}
