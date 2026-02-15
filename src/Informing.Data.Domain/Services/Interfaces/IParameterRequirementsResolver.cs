using Informing.Data.Domain.Contracts.Camunda.Dto;

namespace Informing.Data.Domain.Services.Interfaces;

public interface IParameterRequirementsResolver
{
    IReadOnlyList<string> Resolve(ParameterTaskVariables variables);
}
