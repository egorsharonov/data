namespace Informing.Data.Domain.Services.Interfaces;

public interface IParameterEnrichmentService
{
    Task ProcessEnrichmentTasks(CancellationToken cancellationToken);
}
