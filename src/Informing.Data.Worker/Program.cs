using Informing.Data.Domain.DependencyInjection.Extensions;
using Informing.Data.Domain.Enums;
using Informing.Data.Infrastructure.DependencyInjection.Extensions;
using Informing.Data.Infrastructure.Observability.Healthchecks;
using Informing.Data.Infrastructure.Observability.Instrumentation;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Trace;
using Salsa.Observability;
using Salsa.Observability.Extensions;
using Salsa.Observability.Tracing.Processors;

namespace Informing.Data.Worker;

public sealed class Program
{
    public static async Task Main()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .UseObservability(
                setupTrace: (obsBuilder, traceBuilder) =>
                {
                    var filterEmptyCamundaSpans = obsBuilder.Configuration.GetValue<bool>("Observability:FilterEmptyTasksSpans");

                    traceBuilder
                        .AddHttpClientInstrumentation(options =>
                        {
                            if (filterEmptyCamundaSpans)
                            {
                                options.EnrichWithHttpResponseMessage = WorkerInstrumentation.DetermineCamundaIgnorableTasks;
                            }
                        })
                        .AddSource($"{CamundaWorkerTag.ParameterService}-Worker");

                    if (filterEmptyCamundaSpans)
                    {
                        traceBuilder
                            .AddProcessor(new FilteringProcessor(WorkerInstrumentation.FilterEmptyCamundaTasksSpans))
                            .AddProcessor(new FilteringProcessor(WorkerInstrumentation.FilterCamundaHealthcheckSpans));
                    }
                })
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .AddObservabilityInstrumentation()
                    .AddInfrastructureConfiguration(hostContext.Configuration)
                    .AddCamundaClient(hostContext.Configuration)
                    .AddExternalParameterProviders()
                    .AddDomainServices()
                    .AddHostedService<ParameterWorker>();

                services
                    .AddTCPHealthchecks()
                    .AddCheck<CamundaHealthCheck>("Camunda", HealthStatus.Unhealthy, timeout: TimeSpan.FromMilliseconds(5000));
            });

        await hostBuilder.Build().RunAsync();
    }
}
