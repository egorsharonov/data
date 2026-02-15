using Newtonsoft.Json;

namespace Informing.Data.Domain.Models.PortIn.Common;

public record IDDocument(
    [JsonProperty("docName")] string? DocName,
    [JsonProperty("docSeries")] string? DocSeries,
    [JsonProperty("docNumber")] string DocNumber,
    [JsonProperty("documentUrl")] string? DocumentURL,
    [JsonProperty("docType")] string? DocType
);
