using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record Individual(
    [JsonProperty("firstName")] string? FirstName,
    [JsonProperty("lastName")] string? LastName,
    [JsonProperty("middleName")] string? MiddleName,
    [JsonProperty("inn")] string? Inn,
    [JsonProperty("legalCategory")] string? LegalCategory,
    [JsonProperty("customer")] PartyRef? Customer,
    [JsonProperty("idDocuments")] List<IDDocument> IDDocuments,
    [JsonProperty("numbers")] List<string>? Numbers
);