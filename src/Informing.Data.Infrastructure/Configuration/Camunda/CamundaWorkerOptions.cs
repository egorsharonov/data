namespace Informing.Data.Infrastructure.Configuration.Camunda;

public class CamundaWorkerOptions
{
    public string WorkerId { get; init; } = string.Empty;
    public string TopicName { get; init; } = string.Empty;
    public int LockDurationMs { get; init; }
    public long LongPollingWaitDurationMs { get; init; }
    public int MaxBatchTasks { get; init; }
    public int RetriesOnFailure { get; init; }
    public string TenantId { get; init; } = string.Empty;
}