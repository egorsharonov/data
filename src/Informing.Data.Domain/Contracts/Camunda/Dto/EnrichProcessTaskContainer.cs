using Informing.Data.Domain.Exceptions.Infrastructure.Camunda;

namespace Informing.Data.Domain.Contracts.Camunda.Dto;

public record EnrichProcessTaskContainer(
    string Id,
    string ProcessInstanceId,
    int RetriesLeft,
    CamundaEnrichmentVariables? EnrichmentTaskVariables,
    CamundaTaskInvalidVariableException? VariableException = null
);