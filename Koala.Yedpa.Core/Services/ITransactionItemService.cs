using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface ITransactionItemService
{
    // Transaction Item Management
    Task<ResponseDto<TransactionItemViewModel>> GetTransactionItemAsync(string itemId);
    Task<ResponseDto<List<TransactionItemViewModel>>> GetTransactionItemsAsync(string transactionId);
    Task<ResponseDto<List<TransactionItemViewModel>>> GetAllTransactionItemsAsync();
    Task<ResponseDto> AddTransactionItemAsync(CreateTransactionItemViewModel model);
    Task<ResponseDto> DeleteTransactionItemAsync(string itemId);

    // Advanced Queries
    Task<ResponseDto<List<TransactionItemViewModel>>> GetItemsByDateRangeAsync(DateTime startDate, DateTime endDate);
}
