namespace Informing.Data.Infrastructure.Configuration.Camunda;

public class CamundaPollingOptions
{
    public string BaseUrl { get; init; } = string.Empty;
    public long NormalPollingInterval { get; init; }
    public long ErrorPollingInterval { get; init; }
}