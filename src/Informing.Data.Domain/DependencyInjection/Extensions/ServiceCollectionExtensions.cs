using Informing.Data.Domain.Services;
using Informing.Data.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Informing.Data.Domain.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IParameterEnrichmentService, ParameterEnrichmentService>();
        services.AddSingleton<IParameterRequirementsResolver, ParameterRequirementsResolver>();
        return services;
    }
}
