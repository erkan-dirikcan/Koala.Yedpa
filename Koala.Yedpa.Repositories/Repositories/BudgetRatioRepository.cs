using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories;

public class BudgetRatioRepository : IBudgetRatioRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<BudgetRatio> _dbSet;

    public BudgetRatioRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<BudgetRatio>();
    }

    public async Task<BudgetRatio> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<BudgetRatio>> GetAllAsync()
    {
        return await _dbSet
            .OrderBy(b => b.Year)
            .ThenBy(b => b.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<BudgetRatio>> GetByYearAsync(int year)
    {
        return await _dbSet
            .Where(b => b.Year == year)
            .OrderBy(b => b.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<BudgetRatio>> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Where(b => b.Code == code)
            .OrderByDescending(b => b.Year)
            .ToListAsync();
    }

    public async Task<IEnumerable<BudgetRatio>> GetByBudgetTypeAsync(BuggetTypeEnum budgetType)
    {
        return await _dbSet
            .Where(b => b.BuggetType == budgetType)
            .OrderBy(b => b.Year)
            .ThenBy(b => b.Code)
            .ToListAsync();
    }

    public async Task<string> AddAsync(BudgetRatio budgetRatio)
    {
        budgetRatio.CreateTime = DateTime.UtcNow;
        _dbSet.Add(budgetRatio);
        return budgetRatio.Id;
    }

    public async Task UpdateAsync(BudgetRatio budgetRatio)
    {
        var existing = await _context.BudgetRatio.FindAsync(budgetRatio.Id);
        if (existing == null)
            return;

        existing.Code = budgetRatio.Code;
        existing.Description = budgetRatio.Description;
        existing.Year = budgetRatio.Year;
        existing.Ratio = budgetRatio.Ratio;
        existing.TotalBugget = budgetRatio.TotalBugget;
        existing.BuggetRatioMounths = budgetRatio.BuggetRatioMounths;
        existing.BuggetType = budgetRatio.BuggetType;
        existing.Status = budgetRatio.Status;
        existing.LastUpdateTime = DateTime.UtcNow;

        _dbSet.Update(existing);
    }

    public async Task DeleteAsync(string id)
    {
        var budgetRatio = await _context.BudgetRatio.FindAsync(id);
        if (budgetRatio == null)
            return;

        _dbSet.Remove(budgetRatio);
    }

    public async Task<bool> ExistsAsync(string code, int year)
    {
        return await _dbSet
            .AnyAsync(b => b.Code == code && b.Year == year);
    }
}





