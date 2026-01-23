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
    private IEmailTemplateRepository? _emailTemplateRepository;

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

    // Email Repositories
    public IEmailTemplateRepository EmailTemplateRepository =>
        _emailTemplateRepository ??= new EmailTemplateRepository(context as AppDbContext ?? throw new ArgumentException("Context must be AppDbContext"));

    public void Commit()
    {
        context.SaveChanges();
    }

    public async Task CommitAsync()
    {
        await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}