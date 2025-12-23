using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Core.Repositories;

public interface ITransactionItemRepository
{
    // Basic CRUD Operations
    Task<TransactionItem?> GetByIdAsync(string id);
    Task<List<TransactionItem>> GetByTransactionIdAsync(string transactionId);
    Task<List<TransactionItem>> GetAllAsync();

    // Add/Update/Delete
    Task AddAsync(TransactionItem transactionItem);
    Task UpdateAsync(TransactionItem transactionItem);
    void Delete(TransactionItem transactionItem);

    // Bulk Operations
    Task AddRangeAsync(IEnumerable<TransactionItem> transactionItems);
    Task UpdateRangeAsync(IEnumerable<TransactionItem> transactionItems);

    // Specific Queries
    Task<bool> ExistsAsync(string id);
    Task<List<TransactionItem>> GetItemsByDateRangeAsync(DateTime startDate, DateTime endDate);
}


