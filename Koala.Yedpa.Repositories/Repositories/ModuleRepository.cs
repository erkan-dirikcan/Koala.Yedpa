
using System.Linq.Expressions;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories;

public class ModuleRepository(AppDbContext context) : IModuleRepository
{
    private readonly AppDbContext _context = context;
    private DbSet<Module> _dbSet = context.Set<Module>();

    public async Task<Module> GetModuleByIdAsync(string id)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(x=>x.Id==id);
        if (entity != null)
        {
            _dbSet.Entry(entity).State = EntityState.Detached;
        }
        return entity;
    }

    public async Task<IEnumerable<Module>> GetAllModuleAsync()
    {
        return await _dbSet.Include(x=>x.Claims).ToListAsync();
    }

    public IQueryable<Module> WhereModule(Expression<Func<Module, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }

    public async Task AddModuleAsync(Module entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void DeleteModule(Module entity)
    {
        _dbSet.Remove(entity);
    }

    public Module UpdateModule(Module entity)
    {
        _dbSet.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public async Task<List<Module>> GetModuleWithExtentedProperty()
    {
        var all =await _dbSet.Include(x => x.ExtendedProperties).ToListAsync();
        return all;
    }
}