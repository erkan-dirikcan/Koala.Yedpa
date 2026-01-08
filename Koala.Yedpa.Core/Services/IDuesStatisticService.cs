using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface IDuesStatisticService
{
    Task<ResponseDto<DuesStatistic>> GetByIdAsync(string id);
    Task<ResponseDto<IEnumerable<DuesStatistic>>> GetAllAsync();
    Task<ResponseDto<IEnumerable<DuesStatistic>>> GetByYearAsync(string year);
    Task<ResponseDto<DuesStatistic>> CreateAsync(DuesStatistic duesStatistic);
    Task<ResponseDto> UpdateAsync(DuesStatistic duesStatistic);
    Task<ResponseDto> DeleteAsync(string id);
    Task<ResponseDto<int>> ImportFromSourceDatabaseAsync(string year, BuggetTypeEnum budgetType = BuggetTypeEnum.Budget);
    Task<ResponseDto<bool>> SyncYearDataAsync(string year);
    Task<ResponseDto<DuesStatisticSummaryViewModel>> GetYearSummaryAsync(string year);
    Task<ResponseDto<IEnumerable<YearOverviewViewModel>>> GetYearOverviewsAsync();
    Task<ResponseDto<IEnumerable<MonthlySummaryViewModel>>> GetMonthlySummaryAsync(string year);
    Task<ResponseDto<DuesStatistic>> GetByWorkplaceCodeAsync(string workplaceCode, string year);
    Task<ResponseDto<bool>> CheckExistsAsync(string code, string year);
    Task<ResponseDto<List<int>>> GetDistinctYearsAsync();
    Task<ResponseDto<MonthlyBudgetSummaryViewModel>> GetMonthlyBudgetSummaryAsync(int year, BuggetTypeEnum budgetType);
}