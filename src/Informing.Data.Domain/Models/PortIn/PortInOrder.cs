using Informing.Data.Domain.Models.PortIn.Common;

namespace Informing.Data.Domain.Models.PortIn;

public record PortInOrder(
    string ID,
    string? CdbProcesID,
    string? Source,
    DateTimeOffset? DueDate,
    string? Comment,
    Operator? Donor,
    Operator? Recipient,
    Person? Person,
    Company? Company,
    Government? Government,
    Individual? Individual,
    MnpDocumentRef Contract,
    List<PortationNumber> PortationNumbers,
    OrderState State
);