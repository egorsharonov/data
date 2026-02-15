
namespace Informing.Data.Domain.Exceptions.Domain.PortIn;

public class PortInOrderDataInvalidFormatException : DomainException
{
    public long OrderId { get; init; }
    public PortInOrderDataInvalidFormatException(string? message, long orderId) : base(message)
    {
        OrderId = orderId;
    }
    public PortInOrderDataInvalidFormatException(string? message, long orderId, Exception? innerException) : base(message, innerException)
    {
        OrderId = orderId;
    }

}