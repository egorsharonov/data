using Informing.Data.Domain.Enums;
using Informing.Data.Infrastructure.Configuration.Camunda;
using Microsoft.Extensions.Configuration;

namespace Informing.Data.Infrastructure.Configuration;

internal static class ConfigurationResolver
{
    internal static CamundaWorkerOptions GetCamundaOptions(this IConfiguration configuration, CamundaWorkerTag workerTag)
    {
        return configuration
                   .GetSection($"Infrastructure:Camunda:WorkerOptions:{workerTag}")
                   .Get<CamundaWorkerOptions>()
               ?? throw new ArgumentException($"Camunda {workerTag} worker options are missing.");
    }
}
