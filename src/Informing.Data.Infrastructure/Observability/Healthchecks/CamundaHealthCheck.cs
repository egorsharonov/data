using Informing.Data.Domain.Contracts.Camunda.Interfaces;
using Informing.Data.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Informing.Data.Infrastructure.Observability.Healthchecks;

public class CamundaHealthCheck([FromKeyedServices(CamundaWorkerTag.ParameterService)] ICamundaClient camundaClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await camundaClient.CheckConnection(cancellationToken)
                ? HealthCheckResult.Healthy()
                : new HealthCheckResult(context.Registration.FailureStatus);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, ex.Message, ex);
        }
    }
}
