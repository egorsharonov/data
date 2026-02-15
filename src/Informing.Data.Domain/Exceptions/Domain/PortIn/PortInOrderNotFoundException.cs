
using Informing.Data.Domain.Exceptions.Infrastructure;

namespace Informing.Data.Domain.Exceptions.Domain.PortIn;

public class PortInOrderNotFoundException : DomainException
{
    public long OrderId { get; }

    public PortInOrderNotFoundException(string? message, long orderId, EntityNotFoundException innerException) : base(message, innerException)
    {
        OrderId = orderId;
    }
}