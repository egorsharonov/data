using Dapper;
using Informing.Data.Domain.Contracts.Dal.Entities;
using Informing.Data.Domain.Contracts.Dal.Interfaces;
using Informing.Data.Domain.Exceptions.Infrastructure;
using Npgsql;

namespace Informing.Data.Infrastructure.Dal.Repositories;

public class PortInOrdersRepository : BaseRepository, IPortInOrdersRepository
{
    public PortInOrdersRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource)
    {
    }

    public async Task<PortInOrderEntity> GetByOrderId(long orderId, CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
SELECT * FROM orders
    WHERE order_id = @OrderId
";

        var sqlParameters = new
        {
            OrderId = orderId
        };

        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);

        var entity = await connection.QueryFirstOrDefaultAsync<PortInOrderEntity>(
            new CommandDefinition(
                commandText: sqlQuery,
                parameters: sqlParameters,
                cancellationToken: cancellationToken
            )
        ) 
        ?? throw new EntityNotFoundException("PortIn order could not be found.");

        
        return entity;
    }
}