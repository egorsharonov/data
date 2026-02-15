using Informing.Data.Domain.Configuration.Parameters;
using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Services;
using Microsoft.Extensions.Options;

namespace Informing.Data.Domain.Tests.Services;

public sealed class ParameterRequirementsResolverTests
{
    [Fact]
    public void Resolve_ReturnsRequestedParameters_FromTaskVariablesFirst()
    {
        var resolver = CreateResolver(new ParameterResolutionOptions
        {
            EventTypeToParameters = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["sent-cdb"] = ["fromConfig"]
            },
            DefaultParameters = ["default"]
        });

        var variables = new ParameterTaskVariables("order-1", "sent-cdb", ["requestedA", "requestedB"]);

        var resolved = resolver.Resolve(variables);

        Assert.Equal(new[] { "requestedA", "requestedB" }, resolved);
    }

    [Fact]
    public void Resolve_UsesEventTypeMapping_WhenNoRequestedParametersProvided()
    {
        var resolver = CreateResolver(new ParameterResolutionOptions
        {
            EventTypeToParameters = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["sent-cdb"] = ["configA", "configB"]
            },
            DefaultParameters = ["default"]
        });

        var variables = new ParameterTaskVariables("order-1", "sent-cdb", []);

        var resolved = resolver.Resolve(variables);

        Assert.Equal(new[] { "configA", "configB" }, resolved);
    }

    [Fact]
    public void Resolve_FallsBackToDefault_WhenNoRequestedAndNoEventMapping()
    {
        var resolver = CreateResolver(new ParameterResolutionOptions
        {
            DefaultParameters = ["defaultA"]
        });

        var variables = new ParameterTaskVariables("order-1", "unknown-event", []);

        var resolved = resolver.Resolve(variables);

        Assert.Equal(new[] { "defaultA" }, resolved);
    }

    private static ParameterRequirementsResolver CreateResolver(ParameterResolutionOptions options)
    {
        return new ParameterRequirementsResolver(Options.Create(options));
    }
}
