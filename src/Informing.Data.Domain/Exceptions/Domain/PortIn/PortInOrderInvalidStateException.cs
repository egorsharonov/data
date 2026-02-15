
namespace Informing.Data.Domain.Exceptions.Domain.PortIn;

public class PortInOrderInvalidStateException : DomainException
{
    public long OrderID { get; init; }
    public PortInOrderInvalidStateException(string? message, long orderId) : base(message)
    {
        OrderID = orderId;
    }

    public PortInOrderInvalidStateException(string? message, long orderId, Exception? innerException) : base(message, innerException)
    {
        OrderID = orderId;
    }
}