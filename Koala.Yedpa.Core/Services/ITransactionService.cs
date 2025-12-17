using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface ITransactionService
{
    // Transaction Management
    Task<ResponseDto<Transaction>> CreateTransactionAsync(CreateTransactionViewModel model);
    Task<ResponseDto<TransactionDetailViewModel>> GetTransactionAsync(string transactionId);
    Task<ResponseDto<List<TransactionListViewModel>>> GetAllTransactionsAsync();
    Task<ResponseDto<List<TransactionListViewModel>>> GetUserTransactionsAsync(string userId);
    Task<ResponseDto<List<TransactionListViewModel>>> GetTransactionsByTypeAsync(string transactionTypeId);
    Task<ResponseDto> UpdateTransactionAsync(UpdateTransactionViewModel model);
    Task<ResponseDto> CompleteTransactionAsync(CompleteTransactionViewModel model);
    Task<ResponseDto> DeleteTransactionAsync(DeleteTransactionViewModel model);

    // Transaction Status Operations
    Task<ResponseDto<List<TransactionListViewModel>>> GetPendingTransactionsAsync();
    Task<ResponseDto<List<TransactionListViewModel>>> GetCompletedTransactionsAsync();
    Task<ResponseDto<int>> GetTransactionCountAsync();

    // Advanced Queries
    Task<ResponseDto<List<TransactionListViewModel>>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<ResponseDto<List<TransactionListViewModel>>> GetRecentTransactionsAsync(int count = 10);
    Task<ResponseDto<List<TransactionListViewModel>>> SearchTransactionsAsync(TransactionSearchViewModel model);

    // Transaction Items Management
    Task<ResponseDto> AddTransactionItemAsync(CreateTransactionItemViewModel model);
    Task<ResponseDto<List<TransactionItemViewModel>>> GetTransactionItemsAsync(string transactionId);
}
