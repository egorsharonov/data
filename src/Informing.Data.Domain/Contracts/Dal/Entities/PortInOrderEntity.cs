using Informing.Data.Domain.Enums;

namespace Informing.Data.Domain.Contracts.Dal.Entities;

public class PortInOrderEntity
{
    public long OrderId { get; init; }
    public long? CdbProcessId { get; init; }
    public DateTimeOffset CreationDate { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public int State { get; init; }
    public OrderType OrderType { get; init; }
    public string CustomerId { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    /// <summary>
    /// JSON Полный объект заявки PortIn - <see cref="Models.PortIn.PortInOrder"/>
    /// </summary>
    public string OrderData { get; init; } = string.Empty;
    public DateTimeOffset? ChangingDate { get; init; }
    public string? ChangedByUser { get; init; }
}