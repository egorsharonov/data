using Informing.Data.Domain.Contracts.Camunda.Dto;

namespace Informing.Data.Domain.Services.Interfaces;

public interface IProcessDataEnrichmentService
{
    public Task ProcessEnrichmentTasks(CancellationToken cancellationToken);
    public Task ProcessEnrichmentTask(EnrichProcessTaskContainer taskContainer, CancellationToken cancellationToken);
}