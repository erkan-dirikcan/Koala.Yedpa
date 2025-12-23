using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface ITransactionTypeService
{
    // Transaction Type Management
    Task<ResponseDto<TransactionTypeViewModel>> CreateTransactionTypeAsync(CreateTransactionTypeViewModel model);
    Task<ResponseDto<TransactionTypeViewModel>> GetTransactionTypeAsync(string typeId);
    Task<ResponseDto<List<TransactionTypeViewModel>>> GetAllTransactionTypesAsync();
    Task<ResponseDto<List<TransactionTypeViewModel>>> GetActiveTransactionTypesAsync();
    Task<ResponseDto> UpdateTransactionTypeAsync(UpdateTransactionTypeViewModel model);
    Task<ResponseDto> DeleteTransactionTypeAsync(string typeId);

    // Advanced Queries
    Task<ResponseDto<TransactionTypeViewModel>> GetByNameAsync(string name);
    Task<ResponseDto<List<TransactionTypeViewModel>>> GetByStatusAsync(StatusEnum status);
}


