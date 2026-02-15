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
        var hostBuilder =
        Host.CreateDefaultBuilder()
            .UseObservability(
                setupTrace: (obsBuilder, traceBuilder) =>
                {
                    bool filterEmptyCamundaSpans = obsBuilder.Configuration.GetValue<bool>("Observability:FilterEmptyTasksSpans");

                    traceBuilder
                        .AddHttpClientInstrumentation(options =>
                        {
                            if (filterEmptyCamundaSpans)
                            {
                                options.EnrichWithHttpResponseMessage = WorkerInstrumentation.DetermineCamundaIgnorableTasks;
                            }
                        })
                        .AddSource(
                            names: [
                                $"{CamundaWorkerTag.PortIn}-Worker",
                                "Npgsql"
                            ]
                        );

                    if (filterEmptyCamundaSpans)
                    {
                        traceBuilder
                            .AddProcessor(new FilteringProcessor(
                                filter: WorkerInstrumentation.FilterEmptyCamundaTasksSpans
                            ))
                            .AddProcessor(new FilteringProcessor(
                                filter: WorkerInstrumentation.FilterCamundaHealthcheckSpans
                            ));
                    }
                }
            )
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .AddObservabilityInstrumentation(
                        configration: hostContext.Configuration
                    )
                    .AddInfrastructureConfiguration(
                        configuration: hostContext.Configuration
                    )
                    .AddDalInfrastructure(
                        configuration: hostContext.Configuration,
                        hostEnvironment: hostContext.HostingEnvironment
                    )
                    .AddDalRepositories()
                    .AddCamundaClient(
                        configuration: hostContext.Configuration
                    )
                    .AddDomainServices()
                    .AddHostedService<PortInDataWorker>();

                services
                    .AddTCPHealthchecks()
                    .AddCheck<CamundaHealthCheck>(
                        name: "Camunda",
                        failureStatus: HealthStatus.Unhealthy,
                        timeout: TimeSpan.FromMilliseconds(5000)
                    )
                    .AddNpgSql(
                        name: "MNP Portin DB"
                    );
            });

        await hostBuilder
                    .Build()
                    .RunAsync();
    }
}