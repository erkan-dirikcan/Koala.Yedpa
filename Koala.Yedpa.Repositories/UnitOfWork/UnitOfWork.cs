using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.UnitOfWork;

public class UnitOfWork<TContext>(TContext context) : IUnitOfWork<TContext>
    where TContext : DbContext
{
    // Context
    public TContext Context => context;

    // Lazy-loaded repositories
    private ITransactionRepository? _transactionRepository;
    private ITransactionItemRepository? _transactionItemRepository;
    private ITransactionTypeRepository? _transactionTypeRepository;
    private IBudgetRatioRepository? _budgetRatioRepository;
    private IDuesStatisticRepository? _duesStatisticRepository;

    // Transaction Repositories
    public ITransactionRepository TransactionRepository =>
        _transactionRepository ??= new TransactionRepository(context as AppDbContext ?? throw new ArgumentException("Context must be AppDbContext"));

    public ITransactionItemRepository TransactionItemRepository =>
        _transactionItemRepository ??= new TransactionItemRepository(context as AppDbContext ?? throw new ArgumentException("Context must be AppDbContext"));

    public ITransactionTypeRepository TransactionTypeRepository =>
        _transactionTypeRepository ??= new TransactionTypeRepository(context as AppDbContext ?? throw new ArgumentException("Context must be AppDbContext"));

    // Additional Repositories
    public IBudgetRatioRepository BudgetRatioRepository =>
        _budgetRatioRepository ??= new BudgetRatioRepository(context as AppDbContext ?? throw new ArgumentException("Context must be AppDbContext"));

    public IDuesStatisticRepository DuesStatisticRepository =>
        _duesStatisticRepository ??= new DuesStatisticRepository(context as AppDbContext ?? throw new ArgumentException("Context must be AppDbContext"));

    public void Commit()
    {
        context.SaveChanges();
    }

    public async Task CommitAsync()
    {
        try
        {
            var result = await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public void Dispose()
    {
        context.Dispose();
    }
}