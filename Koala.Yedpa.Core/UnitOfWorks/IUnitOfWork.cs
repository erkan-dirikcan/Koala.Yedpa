using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Core.UnitOfWorks;

public interface IUnitOfWork<TContext> : IDisposable where TContext : DbContext
{
    // Transaction Repositories
    ITransactionRepository TransactionRepository { get; }
    ITransactionItemRepository TransactionItemRepository { get; }
    ITransactionTypeRepository TransactionTypeRepository { get; }

    // Commit Operations
    Task CommitAsync();
    void Commit();
}