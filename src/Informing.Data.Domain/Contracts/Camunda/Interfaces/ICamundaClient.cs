using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Models.Rtm;

namespace Informing.Data.Domain.Contracts.Camunda.Interfaces;

public interface ICamundaClient
{
    public Task<IReadOnlyList<EnrichProcessTaskContainer>> FetchAndLockEnrichmentTasks(CancellationToken cancellationToken);
    public Task CompleteTask(string taskId, RtmMessage rtmMessage, CancellationToken cancellationToken);
    public Task FailTask(string taskId, Exception taskError, int retries = 0, CancellationToken cancellationToken = default);
    public Task<bool> CheckConnection(CancellationToken cancellationToken);
}