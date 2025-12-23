using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories;

public class TransactionItemRepository(AppDbContext context) : ITransactionItemRepository
{
    private readonly DbSet<TransactionItem> _dbSet = context.Set<TransactionItem>();

    public async Task<TransactionItem?> GetByIdAsync(string id)
    {
        return await _dbSet
            .Include(ti => ti.Transaction)
            .FirstOrDefaultAsync(ti => ti.Id == id);
    }

    public async Task<List<TransactionItem>> GetByTransactionIdAsync(string transactionId)
    {
        return await _dbSet
            .Include(ti => ti.Transaction)
            .Where(ti => ti.TransactionId == transactionId)
            .OrderByDescending(ti => ti.CreateTime)
            .ToListAsync();
    }

    public async Task<List<TransactionItem>> GetAllAsync()
    {
        return await _dbSet
            .Include(ti => ti.Transaction)
            .OrderByDescending(ti => ti.CreateTime)
            .ToListAsync();
    }

    public async Task AddAsync(TransactionItem transactionItem)
    {
        await _dbSet.AddAsync(transactionItem);
    }

    public async Task UpdateAsync(TransactionItem transactionItem)
    {
        _dbSet.Update(transactionItem);
    }

    public void Delete(TransactionItem transactionItem)
    {
        _dbSet.Remove(transactionItem);
    }

    public async Task AddRangeAsync(IEnumerable<TransactionItem> transactionItems)
    {
        await _dbSet.AddRangeAsync(transactionItems);
    }

    public async Task UpdateRangeAsync(IEnumerable<TransactionItem> transactionItems)
    {
        _dbSet.UpdateRange(transactionItems);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _dbSet.AnyAsync(ti => ti.Id == id);
    }

    public async Task<List<TransactionItem>> GetItemsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(ti => ti.Transaction)
            .Where(ti => ti.CreateTime >= startDate && ti.CreateTime <= endDate)
            .OrderByDescending(ti => ti.CreateTime)
            .ToListAsync();
    }
}


