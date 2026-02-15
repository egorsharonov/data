using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record OrderState(
    [JsonProperty("code")] OrderStateCode Code,
    [JsonProperty("message")] string? Message,
    [JsonProperty("statusDate")] DateTimeOffset? StatusDate,
    [JsonProperty("name")] string? Name
);