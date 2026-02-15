using Informing.Data.Domain.Models.Parameters;

namespace Informing.Data.Domain.Services.Interfaces;

public interface IExternalParameterProvider
{
    string ParameterKey { get; }
    Task<object?> GetParameterValue(ParameterRequestContext context, CancellationToken cancellationToken);
}
