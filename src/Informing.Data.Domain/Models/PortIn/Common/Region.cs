using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record Region(
    [JsonProperty("code")] string Code,
    [JsonProperty("kladr")] string? Kladr,
    [JsonProperty("name")] string? Name
);

