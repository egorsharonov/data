using Informing.Data.CamundaApiClient;
using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Exceptions.Infrastructure.Camunda;
using Informing.Data.Domain.Models.PortIn.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Informing.Data.Infrastructure.Camunda.Mappers;

internal static class CamundaDtoMapper
{
    private static readonly JsonSerializerSettings _serializerSettings = new()
    {
        Converters = new List<JsonConverter>
        {
            new StringEnumConverter()
        }
    };

    public static EnrichProcessTaskContainer MapToEnrichTask(this LockedExternalTaskDto taskDto, int retriesOnFailure)
    {
        string taskId = taskDto.Id;
        CamundaTaskInvalidVariableException? variableException = null;
        CamundaEnrichmentVariables? taskVariablesModel;

        try
        {
            taskVariablesModel = MapToVariablesModel(taskId, taskDto);
        }
        catch (CamundaTaskInvalidVariableException ex)
        {
            taskVariablesModel = null;
            variableException = ex;
        }

        int retriesLeft = taskDto.Retries == null ? retriesOnFailure : (taskDto.Retries.Value - 1);

        return new EnrichProcessTaskContainer(
            Id: taskId,
            RetriesLeft: retriesLeft,
            ProcessInstanceId: taskDto.ProcessInstanceId,
            EnrichmentTaskVariables: taskVariablesModel,
            VariableException: variableException
        );
    }

    public static string GetVariableDtoValue(string key, string taskId, IDictionary<string, VariableValueDto> taskVariables)
    {
        if (!taskVariables.TryGetValue(key, out var targetVariableDto) ||
            string.IsNullOrEmpty(targetVariableDto.Value.ToString()))
        {
            throw new CamundaTaskInvalidVariableException(
                message: $"Task with id: {taskId} missing required variable {key}",
                variableKey: key,
                taskId: taskId
            );
        }

        return targetVariableDto.Value.ToString();
    }

    private static CamundaEnrichmentVariables MapToVariablesModel(string taskId, LockedExternalTaskDto taskDto)
    {
        var taskVariables = taskDto.Variables ??
        throw new CamundaTaskInvalidVariableException(
                message: $"Task with id: {taskId} doesnt contain any required variable.",
                variableKey: "",
                taskId: taskId
        );

        var orderIdPin = GetVariableDtoValue("orderId", taskId, taskVariables);

        if (orderIdPin.StartsWith("pin"))
        {
            if (orderIdPin.Length <= 3)
            {
                throw new CamundaTaskInvalidVariableException(
               message: $"Task with id: {taskId} has invalid orderId variable value: {orderIdPin}. Missing order id.",
               variableKey: "orderId",
               taskId: taskId,
               invalidVariableValue: orderIdPin
           );
            }

            orderIdPin = orderIdPin[3..];
        }

        if (!long.TryParse(orderIdPin, out var orderIdLong))
        {
            throw new CamundaTaskInvalidVariableException(
               message: $"Task with id: {taskId} has invalid orderId variable value type: {orderIdPin}. Expecxted: long",
               variableKey: "orderId",
               taskId: taskId,
               invalidVariableValue: orderIdPin
           );
        }

        var eventType = GetVariableDtoValue("eventType", taskId, taskVariables);
        OrderStateCode eventStateCode = default;
        try
        {
            eventStateCode = JsonConvert.DeserializeObject<OrderStateCode>(
                value: $"\"{eventType}\"",
                settings: _serializerSettings
            );
        }
        catch (Exception ex)
        {
            throw new CamundaTaskInvalidVariableException(
               message: $"Task with id: {taskId} has unsupported eventType variable value type: {orderIdPin}",
               variableKey: "eventType",
               taskId: taskId,
               invalidVariableValue: eventType,
               innerException: ex
           );
        }

        return new CamundaEnrichmentVariables(
            OrderId: orderIdLong,
            EventType: eventStateCode
        );
    }
}