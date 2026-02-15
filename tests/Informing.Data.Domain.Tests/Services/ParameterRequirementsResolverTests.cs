using Informing.Data.Domain.Configuration.Parameters;
using Informing.Data.Domain.Contracts.Camunda.Dto;
using Informing.Data.Domain.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace Informing.Data.Domain.Tests.Services;

public sealed class ParameterRequirementsResolverTests
{
    [Fact]
    public void Resolve_ReturnsRequestedParameters_FromTaskVariables()
    {
        var resolver = new ParameterRequirementsResolver();

        var variables = new ParameterTaskVariables("order-1", "sent-cdb", ["requestedA", "requestedB"]);

        var resolved = resolver.Resolve(variables);

        Assert.Equal(new[] { "requestedA", "requestedB" }, resolved);
    }

    [Fact]
    public void Resolve_ReturnsEmptyList_WhenRequestedParametersAbsent()
    {
        var resolver = new ParameterRequirementsResolver();

        var variables = new ParameterTaskVariables("order-1", "sent-cdb", []);

        var resolved = resolver.Resolve(variables);

        Assert.Empty(resolved);
    }
}
