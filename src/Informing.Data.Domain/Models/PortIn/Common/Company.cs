using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;


public record Company(
    [JsonProperty("name")] string Name,
    [JsonProperty("inn")] string Inn,
    [JsonProperty("customer")] PartyRef? Customer,
    [JsonProperty("idDocuments")] List<IDDocument>? IDDocuments,
    [JsonProperty("numbers")] List<string>? Numbers,
    [JsonProperty("authorizedPerson")] AuthorizedPerson? AuthorizedPerson
);