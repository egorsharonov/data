using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record PartyRef(
    [JsonProperty("id")] string ID
);
