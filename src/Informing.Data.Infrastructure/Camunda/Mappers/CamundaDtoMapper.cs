using Informing.Data.CamundaApiClient;
using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Exceptions.Infrastructure.Camunda;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Informing.Data.Infrastructure.Camunda.Mappers;

internal static class CamundaDtoMapper
{
    public static ParameterProcessTaskContainer MapToEnrichTask(this LockedExternalTaskDto taskDto, int retriesOnFailure)
    {
        var taskId = taskDto.Id;
        CamundaTaskInvalidVariableException? variableException = null;
        ParameterTaskVariables? taskVariables;

        try
        {
            taskVariables = MapToVariablesModel(taskId, taskDto);
        }
        catch (CamundaTaskInvalidVariableException ex)
        {
            taskVariables = null;
            variableException = ex;
        }

        var retriesLeft = taskDto.Retries == null ? retriesOnFailure : taskDto.Retries.Value - 1;

        return new ParameterProcessTaskContainer(taskId, taskDto.ProcessInstanceId, retriesLeft, taskVariables, variableException);
    }

    public static string GetRequiredValue(string key, string taskId, IDictionary<string, VariableValueDto> taskVariables)
    {
        if (!taskVariables.TryGetValue(key, out var targetVariableDto) || string.IsNullOrWhiteSpace(targetVariableDto.Value?.ToString()))
        {
            throw new CamundaTaskInvalidVariableException($"Task with id: {taskId} missing required variable {key}", key, taskId);
        }

        return targetVariableDto.Value!.ToString()!;
    }

    private static ParameterTaskVariables MapToVariablesModel(string taskId, LockedExternalTaskDto taskDto)
    {
        var taskVariables = taskDto.Variables
            ?? throw new CamundaTaskInvalidVariableException($"Task with id: {taskId} does not contain required variables", "", taskId);

        var orderId = GetRequiredValue("orderId", taskId, taskVariables);
        var eventType = GetRequiredValue("eventType", taskId, taskVariables);
        var requested = ParseOptionalList("requestedParameters", taskVariables);

        return new ParameterTaskVariables(orderId, eventType, requested);
    }

    private static IReadOnlyList<string> ParseOptionalList(string key, IDictionary<string, VariableValueDto> taskVariables)
    {
        if (!taskVariables.TryGetValue(key, out var dto) || dto.Value is null)
        {
            return [];
        }

        var raw = dto.Value.ToString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        if (dto.Value is JArray jArray)
        {
            return jArray.Values<string>().Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        if (raw.StartsWith("["))
        {
            try
            {
                var arr = JsonConvert.DeserializeObject<List<string>>(raw);
                if (arr is not null)
                {
                    return arr.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                }
            }
            catch
            {
                // fallback to csv parsing
            }
        }

        return raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                  .Distinct(StringComparer.OrdinalIgnoreCase)
                  .ToList();
    }
}
