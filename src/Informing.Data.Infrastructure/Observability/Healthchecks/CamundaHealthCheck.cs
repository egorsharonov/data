using Informing.Data.Domain.Contracts.Camunda.Interfaces;
using Informing.Data.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Informing.Data.Infrastructure.Observability.Healthchecks;

public class CamundaHealthCheck : IHealthCheck
{
    private readonly ICamundaClient _camundaClient;

    public CamundaHealthCheck(
        [FromKeyedServices(CamundaWorkerTag.PortIn)]
        ICamundaClient camundaClient
    )
    {
        _camundaClient = camundaClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            bool camundaConnected = await _camundaClient.CheckConnection(cancellationToken);

            if (!camundaConnected)
            {
                return new HealthCheckResult(
                status: context.Registration.FailureStatus);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(
                status: context.Registration.FailureStatus,
                description: ex.Message,
                exception: ex);
        }
    }
}