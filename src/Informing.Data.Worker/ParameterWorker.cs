using Informing.Data.Domain.Services.Interfaces;
using Informing.Data.Infrastructure.Configuration.Camunda;
using Microsoft.Extensions.Options;
using Salsa.Observability.Healthchecks;

namespace Informing.Data.Worker;

public class ParameterWorker : BackgroundService
{
    private readonly ILogger<ParameterWorker> _logger;
    private readonly TCPHealthCheckServer _healthCheckServer;
    private readonly CamundaPollingOptions _pollingOptions;
    private readonly IServiceProvider _serviceProvider;

    public ParameterWorker(
        ILogger<ParameterWorker> logger,
        IOptions<CamundaPollingOptions> camundaOptions,
        TCPHealthCheckServer healthCheckServer,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _healthCheckServer = healthCheckServer;
        _pollingOptions = camundaOptions.Value;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _healthCheckServer.CompleteStartupStep("Parameter worker");
        _logger.LogInformation("Parameter worker started for Camunda address {Address}", _pollingOptions.BaseUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IParameterEnrichmentService>();
                await service.ProcessEnrichmentTasks(stoppingToken);
                await Task.Delay(TimeSpan.FromMilliseconds(_pollingOptions.NormalPollingInterval), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Parameter worker stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parameter worker iteration failed");
                await Task.Delay(TimeSpan.FromMilliseconds(_pollingOptions.ErrorPollingInterval), stoppingToken);
            }
        }
    }
}
