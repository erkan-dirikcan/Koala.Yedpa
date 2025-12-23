using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories
{
    public class DuesStatisticRepository : IDuesStatisticRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<DuesStatistic> _dbSet;

        public DuesStatisticRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<DuesStatistic>();
        }

        public async Task<DuesStatistic> GetByIdAsync(string id)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<DuesStatistic>> GetAllAsync()
        {
            return await _dbSet
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Code)
                .ToListAsync();
        }

        public async Task<IEnumerable<DuesStatistic>> GetByYearAsync(string year)
        {
            return await _dbSet
                .Where(d => d.Year == year)
                .OrderBy(d => d.Code)
                .ToListAsync();
        }

        public async Task<IEnumerable<DuesStatistic>> GetByBudgetTypeAsync(BuggetTypeEnum budgetType)
        {
            return await _dbSet
                .Where(d => d.BudgetType == budgetType)
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Code)
                .ToListAsync();
        }

        public async Task<IEnumerable<DuesStatistic>> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Where(d => d.Code == code)
                .OrderByDescending(d => d.Year)
                .ToListAsync();
        }

        public async Task<DuesStatistic> GetByClientReferenceAsync(long clientReference, string year)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.ClientRef == clientReference && d.Year == year);
        }

        public async Task<string> AddAsync(DuesStatistic duesStatistic)
        {
            duesStatistic.CreateTime = DateTime.UtcNow;
            _dbSet.Add(duesStatistic);
            return duesStatistic.Id;
        }

        public async Task UpdateAsync(DuesStatistic duesStatistic)
        {
            var existing = await _context.DuesStatistics.FindAsync(duesStatistic.Id);
            if (existing == null)
                return;// false;

            existing.DivName = duesStatistic.DivName;
            existing.BudgetType = duesStatistic.BudgetType;
            existing.January = duesStatistic.January;
            existing.February = duesStatistic.February;
            existing.March = duesStatistic.March;
            existing.April = duesStatistic.April;
            existing.May = duesStatistic.May;
            existing.June = duesStatistic.June;
            existing.July = duesStatistic.July;
            existing.August = duesStatistic.August;
            existing.September = duesStatistic.September;
            existing.October = duesStatistic.October;
            existing.November = duesStatistic.November;
            existing.December = duesStatistic.December;
            existing.Total = duesStatistic.Total;
            existing.LastUpdateTime = DateTime.UtcNow;

            _dbSet.Update(existing);
        }

        public async Task DeleteAsync(string id)
        {
            var duesStatistic = await _context.DuesStatistics.FindAsync(id);
            if (duesStatistic == null)
                return;// false;

            _dbSet.Remove(duesStatistic);
        }

        public async Task<bool> ExistsAsync(string code, string year)
        {
            return await _dbSet
                .AnyAsync(d => d.Code == code && d.Year == year);
        }

        public async Task BulkInsertAsync(IEnumerable<DuesStatistic> duesStatistics)
        {
            var now = DateTime.UtcNow;
            foreach (var item in duesStatistics)
            {
                item.CreateTime = now;
            }

            await _dbSet.AddRangeAsync(duesStatistics);
        }

        public async Task DeleteByYearAsync(string year)
        {
            var itemsToDelete = await _context.DuesStatistics
                .Where(d => d.Year == year)
                .ToListAsync();

            if (!itemsToDelete.Any())
                return;// false;

            _dbSet.RemoveRange(itemsToDelete);
        }

        public async Task<DuesStatistic> GetByWorkplaceCodeAsync(string workplaceCode, string year)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.DivCode == workplaceCode && d.Year == year);
        }
    }
}