using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories;

public class TransactionTypeRepository(AppDbContext context) : ITransactionTypeRepository
{
    private readonly DbSet<TransactionType> _dbSet = context.Set<TransactionType>();

    public async Task<TransactionType?> GetByIdAsync(string id)
    {
        return await _dbSet
            .Include(tt => tt.Transactions)
            .FirstOrDefaultAsync(tt => tt.Id == id);
    }

    public async Task<List<TransactionType>> GetAllAsync()
    {
        return await _dbSet
            .Include(tt => tt.Transactions)
            .OrderBy(tt => tt.Name)
            .ToListAsync();
    }

    public async Task<List<TransactionType>> GetActiveTransactionTypesAsync()
    {
        return await _dbSet
            .Include(tt => tt.Transactions)
            .Where(tt => tt.Status == StatusEnum.Active)
            .OrderBy(tt => tt.Name)
            .ToListAsync();
    }

    public async Task AddAsync(TransactionType transactionType)
    {
        await _dbSet.AddAsync(transactionType);
    }

    public async Task UpdateAsync(TransactionType transactionType)
    {
        _dbSet.Update(transactionType);
    }

    public void Delete(TransactionType transactionType)
    {
        _dbSet.Remove(transactionType);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _dbSet.AnyAsync(tt => tt.Id == id);
    }

    public async Task<TransactionType?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Include(tt => tt.Transactions)
            .FirstOrDefaultAsync(tt => tt.Name.ToLower() == name.ToLower());
    }

    public async Task<List<TransactionType>> GetByStatusAsync(StatusEnum status)
    {
        return await _dbSet
            .Include(tt => tt.Transactions)
            .Where(tt => tt.Status == status)
            .OrderBy(tt => tt.Name)
            .ToListAsync();
    }
}


