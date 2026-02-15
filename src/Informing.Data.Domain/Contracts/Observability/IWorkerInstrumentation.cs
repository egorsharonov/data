using System.Diagnostics;

namespace Informing.Data.Domain.Contracts.Observability;

public interface IWorkerInstrumentation: IDisposable
{
    public string CamundaTaskNumberKey { get; }
    public Activity? StartProcessCamundaTasksActivity();
    public Activity? StartProcessCamundaTaskActivity(string taskId, string processInstaceId);
}