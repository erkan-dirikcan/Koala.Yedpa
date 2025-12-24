using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface IBudgetRatioService
{
    Task<ResponseDto<BudgetRatioDetailViewModel>> GetByIdAsync(string id);
    Task<ResponseDto<List<BudgetRatioListViewModel>>> GetAllAsync();
    Task<ResponseDto<List<BudgetRatioListViewModel>>> GetByYearAsync(string year);
    Task<ResponseDto<BudgetRatioDetailViewModel>> CreateAsync(CreateBudgetRatioViewModel model);
    Task<ResponseDto> UpdateAsync(UpdateBudgetRatioViewModel model);
    Task<ResponseDto> DeleteAsync(string id);
    Task<ResponseDto<bool>> CheckExistsAsync(string code, string year);
}

