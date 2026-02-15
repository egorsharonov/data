using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record TelcoAccountRef(
    [JsonProperty("id")] string? ID,
    [JsonProperty("msisdn")] string? Msisdn
);
