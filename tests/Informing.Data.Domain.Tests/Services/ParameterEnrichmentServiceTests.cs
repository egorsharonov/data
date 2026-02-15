using System.Diagnostics;
using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Contracts.Camunda.Interfaces;
using Informing.Data.Domain.Contracts.Observability;
using Informing.Data.Domain.Models.Parameters;
using Informing.Data.Domain.Services;
using Informing.Data.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Informing.Data.Domain.Tests.Services;

public sealed class ParameterEnrichmentServiceTests
{
    [Fact]
    public async Task ProcessEnrichmentTasks_CompletesTaskWithResolvedExternalParameters()
    {
        var task = new ParameterProcessTaskContainer(
            Id: "task-1",
            ProcessInstanceId: "proc-1",
            RetriesLeft: 3,
            Variables: new ParameterTaskVariables("order-1", "sent-cdb", ["paramA", "paramB"]));

        var camunda = new FakeCamundaClient([task]);
        var resolver = new FakeRequirementsResolver(["paramA", "paramB"]);
        var providers = new IExternalParameterProvider[]
        {
            new FakeExternalParameterProvider("paramA", "value-a"),
            new FakeExternalParameterProvider("paramB", 42)
        };

        var service = new ParameterEnrichmentService(
            NullLogger<ParameterEnrichmentService>.Instance,
            camunda,
            new FakeWorkerInstrumentation(),
            resolver,
            providers);

        await service.ProcessEnrichmentTasks(CancellationToken.None);

        var completion = Assert.Single(camunda.CompletedTasks);
        Assert.Equal("task-1", completion.TaskId);
        Assert.True(completion.Variables.ContainsKey("externalParameters"));
        Assert.True(completion.Variables.ContainsKey("externalParametersRequested"));

        var externalParameters = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(completion.Variables["externalParameters"]);
        Assert.Equal("value-a", externalParameters["paramA"]);
        Assert.Equal(42, externalParameters["paramB"]);

        var requested = Assert.IsAssignableFrom<IReadOnlyList<string>>(completion.Variables["externalParametersRequested"]);
        Assert.Equal(["paramA", "paramB"], requested);

        Assert.Empty(camunda.FailedTasks);
    }

    [Fact]
    public async Task ProcessEnrichmentTasks_FailsTask_WhenProviderNotRegistered()
    {
        var task = new ParameterProcessTaskContainer(
            Id: "task-2",
            ProcessInstanceId: "proc-2",
            RetriesLeft: 2,
            Variables: new ParameterTaskVariables("order-2", "sent-cdb", ["unknownParam"]));

        var camunda = new FakeCamundaClient([task]);

        var service = new ParameterEnrichmentService(
            NullLogger<ParameterEnrichmentService>.Instance,
            camunda,
            new FakeWorkerInstrumentation(),
            new FakeRequirementsResolver(["unknownParam"]),
            Array.Empty<IExternalParameterProvider>());

        await service.ProcessEnrichmentTasks(CancellationToken.None);

        var failure = Assert.Single(camunda.FailedTasks);
        Assert.Equal("task-2", failure.TaskId);
        Assert.Equal(2, failure.Retries);
        Assert.Contains("No provider registered", failure.Error.Message);

        Assert.Empty(camunda.CompletedTasks);
    }

    private sealed class FakeCamundaClient(IReadOnlyList<ParameterProcessTaskContainer> tasks) : ICamundaClient
    {
        public List<(string TaskId, IReadOnlyDictionary<string, object?> Variables)> CompletedTasks { get; } = [];
        public List<(string TaskId, Exception Error, int Retries)> FailedTasks { get; } = [];

        public Task<IReadOnlyList<ParameterProcessTaskContainer>> FetchAndLockEnrichmentTasks(CancellationToken cancellationToken)
            => Task.FromResult(tasks);

        public Task CompleteTask(string taskId, IReadOnlyDictionary<string, object?> variables, CancellationToken cancellationToken)
        {
            CompletedTasks.Add((taskId, variables));
            return Task.CompletedTask;
        }

        public Task FailTask(string taskId, Exception taskError, int retries = 0, CancellationToken cancellationToken = default)
        {
            FailedTasks.Add((taskId, taskError, retries));
            return Task.CompletedTask;
        }

        public Task<bool> CheckConnection(CancellationToken cancellationToken) => Task.FromResult(true);
    }

    private sealed class FakeRequirementsResolver(IReadOnlyList<string> resolved) : IParameterRequirementsResolver
    {
        public IReadOnlyList<string> Resolve(ParameterTaskVariables variables) => resolved;
    }

    private sealed class FakeExternalParameterProvider(string key, object? value) : IExternalParameterProvider
    {
        public string ParameterKey => key;

        public Task<object?> GetParameterValue(ParameterRequestContext context, CancellationToken cancellationToken)
            => Task.FromResult(value);
    }

    private sealed class FakeWorkerInstrumentation : IWorkerInstrumentation
    {
        public string CamundaTaskNumberKey => "camunda.tasks.count";

        public void Dispose()
        {
        }

        public Activity? StartProcessCamundaTaskActivity(string taskId, string processInstaceId) => null;

        public Activity? StartProcessCamundaTasksActivity() => null;
    }
}
