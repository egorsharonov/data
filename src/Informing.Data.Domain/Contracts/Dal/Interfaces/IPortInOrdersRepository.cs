using Informing.Data.Domain.Contracts.Dal.Entities;

namespace Informing.Data.Domain.Contracts.Dal.Interfaces;

public interface IPortInOrdersRepository: IDbRepository
{
    public Task<PortInOrderEntity> GetByOrderId(long orderId, CancellationToken cancellationToken);
}