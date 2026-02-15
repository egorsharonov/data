
namespace Informing.Data.Domain.Exceptions.Infrastructure.Camunda;

public class CamundaTaskInvalidVariableException : InfrastructureException
{
    public string VariableKey { get; init; }
    public string? InvalidVariableValue { get; init; }
    public string TaskId { get; init; }

    public CamundaTaskInvalidVariableException(string? message, string variableKey, string taskId, string? invalidVariableValue = null) : base(message)
    {
        InvalidVariableValue = invalidVariableValue;
        VariableKey = variableKey;
        TaskId = taskId;
    }

    public CamundaTaskInvalidVariableException(string? message, string variableKey, string taskId, Exception? innerException, string? invalidVariableValue = null) : base(message, innerException)
    {
        InvalidVariableValue = invalidVariableValue;
        VariableKey = variableKey;
        TaskId = taskId;
    }
}