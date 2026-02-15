using System.Diagnostics;
using System.Text;
using Informing.Data.Domain.Contracts.Observability;
using Salsa.Observability.Tracing.Instrumentation;

namespace Informing.Data.Infrastructure.Observability.Instrumentation;

public class WorkerInstrumentation(string activitySourceName)
    : InstrumentationBase(activitySourceName), IWorkerInstrumentation
{
    internal const string CamundaTasksNumberKey = "camunda.tasks.number";
    internal const string CamundaEmptyTasksKey = "camunda.tasks.empty";
    internal const string CamundaHealthCheckKey = "camunda.healthcheck";
    internal const string CamundaTaskIdKey = "camunda.task.id";
    internal const string CamundaProcessInstanceIdKey = "camunda.process.instance.id";
    public string CamundaTaskNumberKey => CamundaTasksNumberKey;

    public Activity? StartProcessCamundaTasksActivity()
    {
        return ActivitySource.StartActivity(
            name: "ProcessCamundaTasks",
            kind: ActivityKind.Internal
        );
    }

    public Activity? StartProcessCamundaTaskActivity(string taskId, string processInstaceId)
    {
        return ActivitySource.StartActivity(
            name: "ProcessTask",
            kind: ActivityKind.Internal,
            tags: [
                new KeyValuePair<string, object?>(
                    key: CamundaTaskIdKey,
                    value: taskId   
                ),
                new KeyValuePair<string, object?>(
                    key: CamundaProcessInstanceIdKey,
                    value: processInstaceId
                )
            ]
        );
    }

    public static void DetermineCamundaIgnorableTasks(Activity activity, HttpResponseMessage httpResponseMessage)
    {

        bool isFetchAndLock = activity.GetTagItem("url.full")?.ToString()?.Split('/')[^1] == "fetchAndLock";

        bool isEngineVersionHealthcheck = activity.GetTagItem("url.full")?.ToString()?.Split('/')[^1] == "version";

        if (isFetchAndLock &&
            httpResponseMessage.IsSuccessStatusCode &&
            httpResponseMessage.Content.Headers.ContentType?.ToString() == "application/json")
        {
            var originalContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

            activity.SetTag(CamundaEmptyTasksKey, originalContent.Length >= 2 && originalContent.StartsWith("[]"));

            httpResponseMessage.Content = new StringContent(
                originalContent,
                Encoding.UTF8,
                "application/json"
            );
        }

        if (isEngineVersionHealthcheck
            && httpResponseMessage.Content.Headers.ContentType?.ToString() == "application/json")
        {
            activity.SetTag(CamundaHealthCheckKey, true);
        }
    }

    public static bool FilterCamundaHealthcheckSpans(Activity activity)
    {
        var camundaHealthCheck = activity.GetTagItem(CamundaHealthCheckKey)?.ToString();

        return !string.IsNullOrEmpty(camundaHealthCheck)
                && bool.TryParse(camundaHealthCheck, out var isHealthCheck)
                && isHealthCheck;
    }

    public static bool FilterEmptyCamundaTasksSpans(Activity activity)
    {
        return FilterProcessCamundaTasksSpan(activity)
                || FilterCamundaTasksPollRequestSpan(activity);
    }

    private static bool FilterCamundaTasksPollRequestSpan(Activity activity)
    {
        var camundaEmptyTasks = activity.GetTagItem(CamundaEmptyTasksKey)?.ToString();

        return !string.IsNullOrEmpty(camundaEmptyTasks)
                && bool.TryParse(camundaEmptyTasks, out var emptyTasks)
                && emptyTasks;
    }

    private static bool FilterProcessCamundaTasksSpan(Activity activity)
    {
        var tasksNumberTagValue = activity.GetTagItem(CamundaTasksNumberKey)?.ToString();

        return !string.IsNullOrEmpty(tasksNumberTagValue)
                && int.TryParse(tasksNumberTagValue, out var numberOfTasks)
                && numberOfTasks == 0;
    }
}