namespace Informing.Data.Domain.Contracts.Camunda.Dto;

public sealed record ParameterTaskVariables(
    string OrderId,
    string EventType,
    IReadOnlyList<string> RequestedParameters
);
