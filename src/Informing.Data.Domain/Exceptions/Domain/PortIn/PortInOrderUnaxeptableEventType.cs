
using Informing.Data.Domain.Models.PortIn.Common;

namespace Informing.Data.Domain.Exceptions.Domain.PortIn;

public class PortInOrderUnacceptableEventType : DomainException
{
    public long OrderId { get; init; }
    public OrderStateCode UnacceptableEventType { get; init; }

    public PortInOrderUnacceptableEventType(string? message, long orderId, OrderStateCode unnaceptableEventType) : base(message)
    {
        OrderId = orderId;
        UnacceptableEventType = unnaceptableEventType;
    }

    public PortInOrderUnacceptableEventType(string? message, long orderId, OrderStateCode unnaceptableEventType, Exception? innerException) : base(message, innerException)
    {
        OrderId = orderId;
        UnacceptableEventType = unnaceptableEventType;
    }
}