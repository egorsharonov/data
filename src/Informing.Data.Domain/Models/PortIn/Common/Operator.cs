using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record Operator(
    [JsonProperty("rn")] string RN,
    [JsonProperty("mnc")] string? MNC,
    [JsonProperty("name")] string? Name,
    [JsonProperty("region")] Region? Region,
    [JsonProperty("networkOperator")] string? NetworkOperator,
    [JsonProperty("cdbCode")] string? CdbCode
);