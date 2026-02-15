using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record Government(
    [JsonProperty("name")] string Name,
    [JsonProperty("inn")] string Inn,
    [JsonProperty("customer")] PartyRef? Customer,
    [JsonProperty("idDocuments")] List<IDDocument>? IDDocuments,
    [JsonProperty("tenderId")] string? TenderID,
    [JsonProperty("tradingFloor")] string? TradingFloor,
    [JsonProperty("contractDueDate")] DateTimeOffset? ContractDueDate,
    [JsonProperty("numbers")] List<string>? Numbers,
    [JsonProperty("authorizedPerson")] AuthorizedPerson? AuthorizedPerson
);