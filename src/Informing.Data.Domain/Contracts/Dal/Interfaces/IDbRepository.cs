using System.Transactions;

namespace Informing.Data.Domain.Contracts.Dal.Interfaces;

public interface IDbRepository
{
    public TransactionScope CreateTransactionScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
}