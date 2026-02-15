using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record AuthorizedPerson(
    [JsonProperty("firstName")] string FirstName,
    [JsonProperty("lastName")] string LastName,
    [JsonProperty("middleName")] string? MiddleName,
    [JsonProperty("position")] string? Position
);