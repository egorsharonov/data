using System.Diagnostics;
using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Contracts.Camunda.Interfaces;
using Informing.Data.Domain.Contracts.Observability;
using Informing.Data.Domain.Enums;
using Informing.Data.Domain.Exceptions.Infrastructure.Camunda;
using Informing.Data.Domain.Models.Parameters;
using Informing.Data.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Informing.Data.Domain.Services;

public sealed class ParameterEnrichmentService : IParameterEnrichmentService
{
    private readonly ILogger<ParameterEnrichmentService> _logger;
    private readonly ICamundaClient _camundaClient;
    private readonly IWorkerInstrumentation _instrumentation;
    private readonly IParameterRequirementsResolver _requirementsResolver;
    private readonly IReadOnlyDictionary<string, IExternalParameterProvider> _providers;

    public ParameterEnrichmentService(
        ILogger<ParameterEnrichmentService> logger,
        [FromKeyedServices(CamundaWorkerTag.ParameterService)] ICamundaClient camundaClient,
        [FromKeyedServices(CamundaWorkerTag.ParameterService)] IWorkerInstrumentation instrumentation,
        IParameterRequirementsResolver requirementsResolver,
        IEnumerable<IExternalParameterProvider> providers)
    {
        _logger = logger;
        _camundaClient = camundaClient;
        _instrumentation = instrumentation;
        _requirementsResolver = requirementsResolver;
        _providers = providers.ToDictionary(x => x.ParameterKey, StringComparer.OrdinalIgnoreCase);
    }

    public async Task ProcessEnrichmentTasks(CancellationToken cancellationToken)
    {
        using var processTasksActivity = _instrumentation.StartProcessCamundaTasksActivity();

        IReadOnlyList<ParameterProcessTaskContainer> taskContainers =
            await _camundaClient.FetchAndLockEnrichmentTasks(cancellationToken);

        processTasksActivity?.SetTag(_instrumentation.CamundaTaskNumberKey, taskContainers.Count);

        foreach (var taskContainer in taskContainers)
        {
            await ProcessTask(taskContainer, cancellationToken);
        }
    }

    private async Task ProcessTask(ParameterProcessTaskContainer taskContainer, CancellationToken cancellationToken)
    {
        using var taskActivity = _instrumentation.StartProcessCamundaTaskActivity(taskContainer.Id, taskContainer.ProcessInstanceId);

        try
        {
            if (taskContainer.VariableException is not null)
            {
                throw taskContainer.VariableException;
            }

            if (taskContainer.Variables is null)
            {
                throw new CamundaTaskInvalidVariableException($"Task {taskContainer.Id} has no required variables", "", taskContainer.Id);
            }

            var requestedParameters = _requirementsResolver.Resolve(taskContainer.Variables);
            var context = new ParameterRequestContext(taskContainer.Variables.OrderId, taskContainer.Variables.EventType, taskContainer.Id, taskContainer.ProcessInstanceId);

            var resolved = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in requestedParameters)
            {
                if (!_providers.TryGetValue(key, out var provider))
                {
                    throw new InvalidOperationException($"No provider registered for external parameter '{key}'.");
                }

                resolved[key] = await provider.GetParameterValue(context, cancellationToken);
            }

            await _camundaClient.CompleteTask(taskContainer.Id, new Dictionary<string, object?>
            {
                ["externalParameters"] = resolved,
                ["externalParametersRequested"] = requestedParameters
            }, cancellationToken);

            _logger.LogInformation("Parameter task {TaskId} completed. Requested parameters count: {Count}", taskContainer.Id, requestedParameters.Count);
        }
        catch (Exception ex)
        {
            taskActivity?.AddException(ex);
            taskActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to process parameter task {TaskId}", taskContainer.Id);
            var retries = ex is CamundaTaskInvalidVariableException ? 0 : taskContainer.RetriesLeft;
            await _camundaClient.FailTask(taskContainer.Id, ex, retries, cancellationToken);
        }
    }
}
