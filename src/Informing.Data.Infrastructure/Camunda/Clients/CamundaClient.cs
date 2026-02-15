using System.Diagnostics;
using Informing.Data.CamundaApiClient;
using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Contracts.Camunda.Interfaces;
using Informing.Data.Domain.Models.Rtm;
using Informing.Data.Infrastructure.Camunda.Mappers;
using Informing.Data.Infrastructure.Configuration.Camunda;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Informing.Data.Infrastructure.Camunda.Clients;

public class CamundaClient : ICamundaClient
{
    private readonly ILogger<CamundaClient> _logger;
    private readonly CamundaWorkerOptions _workerOptions;
    private readonly IClient _client;

    public CamundaClient(
        ILogger<CamundaClient> logger,
        CamundaWorkerOptions workerOptions,
        IClient client
    )
    {
        _logger = logger;
        _workerOptions = workerOptions;
        _client = client;
    }

    public async Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        try
        {
            var versionDTO = await _client.GetRestAPIVersionAsync(cancellationToken);

            return versionDTO is not null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to determine camunda REST API version");
            throw;
        }
    }

    public async Task CompleteTask(string taskId, RtmMessage rtmMessage, CancellationToken cancellationToken)
    {
        try
        {
            await _client.CompleteExternalTaskResourceAsync(
                id: taskId,
                body: new CompleteExternalTaskDto
                {
                    WorkerId = _workerOptions.WorkerId,
                    Variables = new Dictionary<string, VariableValueDto>
                    {
                        ["rtmMessageKey"] = new VariableValueDto { Value = new JValue(rtmMessage.Key) },
                        ["rtmMessagePayload"] = new VariableValueDto { Value = new JValue(rtmMessage.Serialize()) }
                    }
                },
                cancellationToken: cancellationToken
            );

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete task with id: {taskId}", taskId);
            throw;
        }
    }

    public async Task FailTask(
        string taskId,
        Exception taskError,
        int retries = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.HandleFailureAsync(
                id: taskId,
                body: new ExternalTaskFailureDto
                {
                    WorkerId = _workerOptions.WorkerId,
                    ErrorMessage = taskError.Message,
                    ErrorDetails = taskError.ToString(),
                    Retries = retries
                },
                cancellationToken: cancellationToken
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to mark task with id: {taskId} as failed", taskId);
        }
        finally
        {
            Activity.Current?.AddException(taskError);
            Activity.Current?.SetStatus(ActivityStatusCode.Error, taskError.Message);
        }
    }

    public async Task<IReadOnlyList<EnrichProcessTaskContainer>> FetchAndLockEnrichmentTasks(CancellationToken cancellationToken)
    {
        var fetchAndLockRequest = new FetchExternalTasksDto
        {
            WorkerId = _workerOptions.WorkerId,
            MaxTasks = _workerOptions.MaxBatchTasks,
            UsePriority = true,
            AsyncResponseTimeout = _workerOptions.LongPollingWaitDurationMs,
            Topics = [
                new FetchExternalTaskTopicDto
                {
                    TopicName = _workerOptions.TopicName,
                    LockDuration = _workerOptions.LockDurationMs,
                    TenantIdIn = [
                        _workerOptions.TenantId
                    ],
                    LocalVariables = true,
                }
            ]
        };

        var enrichTasks = await _client.FetchAndLockAsync(
            body: fetchAndLockRequest,
            cancellationToken: cancellationToken
        );


        return (enrichTasks == null || enrichTasks.Count == 0) ? [] :
                enrichTasks.Select(taskDto => taskDto.MapToEnrichTask(_workerOptions.RetriesOnFailure)).ToList();
    }
}