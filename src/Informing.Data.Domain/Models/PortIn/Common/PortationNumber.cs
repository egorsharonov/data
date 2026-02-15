using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record PortationNumber(
    [JsonProperty("msisdn")] string Msisdn,
    [JsonProperty("telcoAccount")] TelcoAccountRef TelcoAccount,
    [JsonProperty("status")] PortNumState? Status
);

/// <summary>
/// В state портируемого номера существуют статусы, не входящие в OrderStateCode,
/// поэтому используется отдельная модель.
/// </summary>
public record PortNumState(
    [JsonProperty("code")] string Code,
    [JsonProperty("message")] string? Message,
    [JsonProperty("statusDate")] DateTimeOffset? StatusDate,
    [JsonProperty("name")] string? Name
);