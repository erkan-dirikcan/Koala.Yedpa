using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Koala.Yedpa.Repositories.Repositories;

public class WorkplaceRepository : IWorkplaceRepository
{
    private readonly DbSet<Workplace> _dbSet;
    private readonly AppDbContext _context;

    public WorkplaceRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Workplace>();
    }

    public async Task<IEnumerable<Workplace>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public IQueryable<Workplace> Where(Expression<Func<Workplace, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }

    public async Task<Workplace?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Workplace?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Code == code);
    }

    public async Task<Workplace?> GetByLogicalRefAsync(int logicalRef)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.LogicalRef == logicalRef);
    }

    public async Task<List<int>> Select(Expression<Func<Workplace, int>> selector)
    {
        return await _dbSet.Select(selector).ToListAsync();
    }

    public async Task<Workplace?> GetByLogRefAsync(int logRef)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.LogRef == logRef);
    }

    public Workplace Update(Workplace entity)
    {
        _dbSet.Update(entity);
        return entity;
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<int> CountAsync(Expression<Func<Workplace, bool>> predicate)
    {
        return await _dbSet.Where(predicate).CountAsync();
    }

    public async Task<IEnumerable<Workplace>> GetPagedAsync(int skip, int take, Expression<Func<Workplace, bool>>? predicate = null, string? orderBy = null, bool ascending = true)
    {
        var query = predicate != null ? _dbSet.Where(predicate) : _dbSet.AsQueryable();

        // Sıralama
        if (!string.IsNullOrEmpty(orderBy))
        {
            switch (orderBy.ToLower())
            {
                case "code":
                    query = ascending ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code);
                    break;
                case "definition":
                    query = ascending ? query.OrderBy(x => x.Definition) : query.OrderByDescending(x => x.Definition);
                    break;
                case "logicalref":
                    query = ascending ? query.OrderBy(x => x.LogicalRef) : query.OrderByDescending(x => x.LogicalRef);
                    break;
                default:
                    query = query.OrderBy(x => x.Code);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(x => x.Code);
        }

        return await query.Skip(skip).Take(take).ToListAsync();
    }
}
