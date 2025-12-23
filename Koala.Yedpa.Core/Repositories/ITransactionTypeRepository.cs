using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Core.Repositories;

public interface ITransactionTypeRepository
{
    // Basic CRUD Operations
    Task<TransactionType?> GetByIdAsync(string id);
    Task<List<TransactionType>> GetAllAsync();
    Task<List<TransactionType>> GetActiveTransactionTypesAsync();

    // Add/Update/Delete
    Task AddAsync(TransactionType transactionType);
    Task UpdateAsync(TransactionType transactionType);
    void Delete(TransactionType transactionType);

    // Specific Queries
    Task<bool> ExistsAsync(string id);
    Task<TransactionType?> GetByNameAsync(string name);
    Task<List<TransactionType>> GetByStatusAsync(StatusEnum status);
}


