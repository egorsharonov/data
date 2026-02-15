namespace Informing.Data.Domain.Models.Parameters;

public sealed record ParameterRequestContext(
    string OrderId,
    string EventType,
    string TaskId,
    string ProcessInstanceId
);
