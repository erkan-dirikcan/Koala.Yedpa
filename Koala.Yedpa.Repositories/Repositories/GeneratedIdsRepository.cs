using System.Linq.Expressions;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories;

public class GeneratedIdsRepository: IGeneratedIdsRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<GeneratedIds> _dbSet;

    public GeneratedIdsRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<GeneratedIds>();
    }

    public async Task<List<GeneratedIds>> GetAllGeneratedIdsAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public IQueryable<GeneratedIds> Where(Expression<Func<GeneratedIds, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }

    public async Task<GeneratedIds?> GetGeneratedIdByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddGeneratedIdAsync(GeneratedIds model)
    {
        await _dbSet.AddAsync(model);
    }

    public void UpdateGeneratedId(GeneratedIds model)
    {
        _dbSet.Entry(model).State=EntityState.Modified;
    }
}