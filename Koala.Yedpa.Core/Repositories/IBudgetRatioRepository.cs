using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Core.Repositories;

public interface IBudgetRatioRepository
{
    Task<BudgetRatio> GetByIdAsync(string id);
    Task<IEnumerable<BudgetRatio>> GetAllAsync();
    Task<IEnumerable<BudgetRatio>> GetByYearAsync(int year);
    Task<IEnumerable<BudgetRatio>> GetByCodeAsync(string code);
    Task<IEnumerable<BudgetRatio>> GetByBudgetTypeAsync(BuggetTypeEnum budgetType);
    Task<string> AddAsync(BudgetRatio budgetRatio);
    Task UpdateAsync(BudgetRatio budgetRatio);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string code, int year);
}





