namespace Informing.Data.Domain.Configuration.Parameters;

public sealed class ParameterResolutionOptions
{
    public Dictionary<string, List<string>> EventTypeToParameters { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> DefaultParameters { get; init; } = [];
}
