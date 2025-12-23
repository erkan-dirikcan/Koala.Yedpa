using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Core.Repositories;

public interface ITransactionRepository
{
    // Basic CRUD Operations
    Task<Transaction?> GetByIdAsync(string id);
    Task<List<Transaction>> GetAllAsync();
    Task<List<Transaction>> GetByUserIdAsync(string userId);
    Task<List<Transaction>> GetByTransactionTypeIdAsync(string transactionTypeId);
    Task<List<Transaction>> GetCompletedTransactionsAsync();
    Task<List<Transaction>> GetPendingTransactionsAsync();

    // Add/Update/Delete
    Task AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    void Delete(Transaction transaction);

    // Bulk Operations
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
    Task UpdateRangeAsync(IEnumerable<Transaction> transactions);

    // Specific Queries
    Task<bool> ExistsAsync(string id);
    Task<int> CountAsync();
    Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
}


