using Informing.Data.Domain.Contracts.Camunda.Dto;

namespace Informing.Data.Domain.Contracts.Camunda.Interfaces;

public interface ICamundaClient
{
    Task<IReadOnlyList<ParameterProcessTaskContainer>> FetchAndLockEnrichmentTasks(CancellationToken cancellationToken);
    Task CompleteTask(string taskId, IReadOnlyDictionary<string, object?> variables, CancellationToken cancellationToken);
    Task FailTask(string taskId, Exception taskError, int retries = 0, CancellationToken cancellationToken = default);
    Task<bool> CheckConnection(CancellationToken cancellationToken);
}
