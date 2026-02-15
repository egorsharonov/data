using Informing.Data.Domain.Services.Interfaces;
using Informing.Data.Infrastructure.Configuration.Camunda;
using Microsoft.Extensions.Options;
using Salsa.Observability.Healthchecks;

namespace Informing.Data.Worker;

public class PortInDataWorker : BackgroundService
{
    private readonly ILogger<PortInDataWorker> _logger;
    private readonly TCPHealthCheckServer _healthCheckServer;
    private readonly CamundaPollingOptions _pollingOptions;
    private readonly IServiceProvider _serviceProvider;

    public PortInDataWorker(
        ILogger<PortInDataWorker> logger,
        IOptions<CamundaPollingOptions> camundaOptions,
        TCPHealthCheckServer healthCheckServer,
        IServiceProvider serviceProvider
    )
    {
        _logger = logger;
        _healthCheckServer = healthCheckServer;
        _pollingOptions = camundaOptions.Value;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _healthCheckServer.CompleteStartupStep("PortIn data worker");
        
        _logger.LogInformation("PortIn-Data Worker started executing for address: {camundaAddress}", _pollingOptions.BaseUrl);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcessCamundaTasks(cancellationToken);

                await Task.Delay(TimeSpan.FromMilliseconds(_pollingOptions.NormalPollingInterval), cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "PortIn-Data Worker stopped executing");
            }
            catch (Exception)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(_pollingOptions.ErrorPollingInterval), cancellationToken);
            }
        }
    }

    private async Task ProcessCamundaTasks(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var portInEnrichmentService = scope.ServiceProvider.GetRequiredService<IPortInDataEnrichmentService>();

        await portInEnrichmentService.ProcessEnrichmentTasks(cancellationToken);
    }
}