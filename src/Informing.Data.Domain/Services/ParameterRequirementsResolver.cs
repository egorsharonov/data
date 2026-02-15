using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Services.Interfaces;

namespace Informing.Data.Domain.Services;

public sealed class ParameterRequirementsResolver : IParameterRequirementsResolver
{
    public IReadOnlyList<string> Resolve(ParameterTaskVariables variables)
    {
        return variables.RequestedParameters;
    }
}
