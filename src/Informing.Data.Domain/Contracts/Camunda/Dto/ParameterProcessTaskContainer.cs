using Informing.Data.Domain.Exceptions.Infrastructure.Camunda;

namespace Informing.Data.Domain.Contracts.Camunda.Dto;

public sealed record ParameterProcessTaskContainer(
    string Id,
    string ProcessInstanceId,
    int RetriesLeft,
    ParameterTaskVariables? Variables,
    CamundaTaskInvalidVariableException? VariableException = null
);
