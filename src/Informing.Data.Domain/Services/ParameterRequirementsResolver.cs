using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Services.Interfaces;
using Informing.Data.Domain.Configuration.Parameters;
using Microsoft.Extensions.Options;

namespace Informing.Data.Domain.Services;

public sealed class ParameterRequirementsResolver(IOptions<ParameterResolutionOptions> options) : IParameterRequirementsResolver
{
    private readonly ParameterResolutionOptions _options = options.Value;

    public IReadOnlyList<string> Resolve(ParameterTaskVariables variables)
    {
        if (variables.RequestedParameters.Count > 0)
        {
            return variables.RequestedParameters;
        }

        if (_options.EventTypeToParameters.TryGetValue(variables.EventType, out var mapped))
        {
            return mapped;
        }

        return _options.DefaultParameters;
    }
}
