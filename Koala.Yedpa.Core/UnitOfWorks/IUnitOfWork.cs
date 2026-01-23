using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Core.UnitOfWorks;

public interface IUnitOfWork<TContext> : IDisposable where TContext : DbContext
{
    // Context
    TContext Context { get; }

    // Transaction Repositories
    ITransactionRepository TransactionRepository { get; }
    ITransactionItemRepository TransactionItemRepository { get; }
    ITransactionTypeRepository TransactionTypeRepository { get; }

    // Budget Repositories
    IBudgetRatioRepository BudgetRatioRepository { get; }
    IDuesStatisticRepository DuesStatisticRepository { get; }

    // Email Repositories
    IEmailTemplateRepository EmailTemplateRepository { get; }

    // Commit Operations
    Task CommitAsync();
    void Commit();
}