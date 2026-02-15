using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record Person(
    [JsonProperty("firstName")] string FirstName,
    [JsonProperty("lastName")] string LastName,
    [JsonProperty("middleName")] string? MiddleName,
    [JsonProperty("legalCategory")] string? LegalCategory,
    [JsonProperty("customer")] PartyRef? Customer,
    [JsonProperty("idDocuments")] List<IDDocument> IDDocuments,
    [JsonProperty("numbers")] List<string>? Numbers
);