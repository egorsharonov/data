using System.Transactions;
using Informing.Data.Domain.Contracts.Dal.Interfaces;
using Npgsql;

namespace Informing.Data.Infrastructure.Dal.Repositories;

public abstract class BaseRepository : IDbRepository
{
    private readonly NpgsqlDataSource _npgsqlDataSource;

    protected BaseRepository(NpgsqlDataSource npgsqlDataSource)
    {
        _npgsqlDataSource = npgsqlDataSource;
    }

    protected async Task<NpgsqlConnection> GetAndOpenConnectionAsync(CancellationToken cancellationToken)
    {
        return await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
    }

    public TransactionScope CreateTransactionScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        return new TransactionScope(
            scopeOption: TransactionScopeOption.Required,
            transactionOptions: new TransactionOptions
            {
                IsolationLevel = isolationLevel,
                Timeout = TimeSpan.FromSeconds(5)
            },
            asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled
        );
    }
}