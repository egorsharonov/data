using System.Diagnostics;
using Informing.Data.CamundaApiClient;
using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Contracts.Camunda.Interfaces;
using Informing.Data.Infrastructure.Camunda.Mappers;
using Informing.Data.Infrastructure.Configuration.Camunda;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Informing.Data.Infrastructure.Camunda.Clients;

public sealed class CamundaClient : ICamundaClient
{
    private readonly ILogger<CamundaClient> _logger;
    private readonly CamundaWorkerOptions _workerOptions;
    private readonly IClient _client;

    public CamundaClient(ILogger<CamundaClient> logger, CamundaWorkerOptions workerOptions, IClient client)
    {
        _logger = logger;
        _workerOptions = workerOptions;
        _client = client;
    }

    public async Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        var version = await _client.GetRestAPIVersionAsync(cancellationToken);
        return version is not null;
    }

    public async Task<IReadOnlyList<ParameterProcessTaskContainer>> FetchAndLockEnrichmentTasks(CancellationToken cancellationToken)
    {
        var request = new FetchExternalTasksDto
        {
            WorkerId = _workerOptions.WorkerId,
            MaxTasks = _workerOptions.MaxBatchTasks,
            UsePriority = true,
            AsyncResponseTimeout = _workerOptions.LongPollingWaitDurationMs,
            Topics =
            [
                new FetchExternalTaskTopicDto
                {
                    TopicName = _workerOptions.TopicName,
                    LockDuration = _workerOptions.LockDurationMs,
                    TenantIdIn = [_workerOptions.TenantId],
                    LocalVariables = true,
                }
            ]
        };

        var tasks = await _client.FetchAndLockAsync(request, cancellationToken);
        return tasks is null || tasks.Count == 0 ? [] : tasks.Select(x => x.MapToEnrichTask(_workerOptions.RetriesOnFailure)).ToList();
    }

    public async Task CompleteTask(string taskId, IReadOnlyDictionary<string, object?> variables, CancellationToken cancellationToken)
    {
        var mapped = variables.ToDictionary(
            x => x.Key,
            x => new VariableValueDto { Value = x.Value is null ? null : JToken.FromObject(x.Value) });

        await _client.CompleteExternalTaskResourceAsync(taskId, new CompleteExternalTaskDto
        {
            WorkerId = _workerOptions.WorkerId,
            Variables = mapped
        }, cancellationToken);
    }

    public async Task FailTask(string taskId, Exception taskError, int retries = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.HandleFailureAsync(taskId, new ExternalTaskFailureDto
            {
                WorkerId = _workerOptions.WorkerId,
                ErrorMessage = taskError.Message,
                ErrorDetails = taskError.ToString(),
                Retries = retries
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark task with id: {TaskId} as failed", taskId);
        }
        finally
        {
            Activity.Current?.AddException(taskError);
            Activity.Current?.SetStatus(ActivityStatusCode.Error, taskError.Message);
        }
    }
}
