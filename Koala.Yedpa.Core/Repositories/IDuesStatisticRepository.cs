using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Core.Repositories
{
    public interface IDuesStatisticRepository
    {
        Task<DuesStatistic> GetByIdAsync(string id);
        Task<IEnumerable<DuesStatistic>> GetByIdsAsync(List<string> ids);
        Task<IEnumerable<DuesStatistic>> GetAllAsync();
        Task<IEnumerable<DuesStatistic>> GetByYearAsync(string year);
        Task<IEnumerable<DuesStatistic>> GetByBudgetTypeAsync(BuggetTypeEnum budgetType);
        Task<IEnumerable<DuesStatistic>> GetByCodeAsync(string code);
        Task<DuesStatistic> GetByClientReferenceAsync(long clientReference, string year);
        Task<string> AddAsync(DuesStatistic duesStatistic);
        Task UpdateAsync(DuesStatistic duesStatistic);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string code, string year);
        Task BulkInsertAsync(IEnumerable<DuesStatistic> duesStatistics);
        Task DeleteByYearAsync(string year);
        Task<DuesStatistic> GetByWorkplaceCodeAsync(string workplaceCode, string year);
    }
}