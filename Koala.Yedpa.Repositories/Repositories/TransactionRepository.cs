using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories;

public class TransactionRepository(AppDbContext context) : ITransactionRepository
{
    private readonly DbSet<Transaction> _dbSet = context.Set<Transaction>();

    public async Task<Transaction?> GetByIdAsync(string id)
    {
        return await _dbSet
            .Include(t => t.TransactionItems)
            .Include(t => t.TransactionType)
            .Include(t => t.AppUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Transaction>> GetAllAsync()
    {
        return await _dbSet
            .Include(t => t.TransactionItems)
            .Include(t => t.TransactionType)
            .Include(t => t.AppUser)
            .OrderByDescending(t => t.CreateTime)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(t => t.TransactionItems)
            .Include(t => t.TransactionType)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreateTime)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetByTransactionTypeIdAsync(string transactionTypeId)
    {
        return await _dbSet
            .Include(t => t.TransactionItems)
            .Include(t => t.TransactionType)
            .Where(t => t.TransactionTypeId == transactionTypeId)
            .OrderByDescending(t => t.CreateTime)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetCompletedTransactionsAsync()
    {
        return await _dbSet
            .Include(t => t.TransactionItems)
            .Include(t => t.TransactionType)
            .Where(t => t.IsComplated)
            .OrderByDescending(t => t.CreateTime)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetPendingTransactionsAsync()
    {
        return await _dbSet
            .Include(t => t.TransactionItems)
            .Include(t => t.TransactionType)
            .Where(t => !t.IsComplated)
            .OrderByDescending(t => t.CreateTime)
            .ToListAsync();
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _dbSet.AddAsync(transaction);
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _dbSet.Update(transaction);
    }

    public void Delete(Transaction transaction)
    {
        _dbSet.Remove(transaction);
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        await _dbSet.AddRangeAsync(transactions);
    }

    public async Task UpdateRangeAsync(IEnumerable<Transaction> transactions)
    {
        _dbSet.UpdateRange(transactions);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _dbSet.AnyAsync(t => t.Id == id);
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(t => t.TransactionItems)
            .Include(t => t.TransactionType)
            .Where(t => t.CreateTime >= startDate && t.CreateTime <= endDate)
            .OrderByDescending(t => t.CreateTime)
            .ToListAsync();
    }
}
