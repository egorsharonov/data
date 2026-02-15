using Informing.Data.Domain.Models.PortIn.Common;

namespace Informing.Data.Domain.Contracts.Camunda.Dto;

public record CamundaEnrichmentVariables(
    long OrderId,
    OrderStateCode EventType
);
