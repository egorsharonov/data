using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record MnpDocumentRef(
    [JsonProperty("id")] string? ID,
    [JsonProperty("documentDate")] DateTimeOffset? DocumentDate,
    [JsonProperty("documentUrl")] string DocumentURL
);