using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories;

public class ExtendedPropertiesRepository : IExtendedPropertiesRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<ExtendedProperties> _dbSet;

    public ExtendedPropertiesRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ExtendedProperties>();
    }

    public async Task<List<ExtendedProperties>> GetAllExtendedPropertiesAsync()
    {
        return await _dbSet.Include(x => x.RecordValues).Include(x => x.Values).ToListAsync();
    }

    public async Task<List<ExtendedProperties>> GetModuleExtendedPropertiesAsync(string moduleId)
    {
        return await _dbSet.Include(x => x.RecordValues).Include(x => x.Values).Where(x => x.ModuleId == moduleId).ToListAsync();
    }



    public IQueryable<ExtendedProperties> WhereExtendedPropertiesAsync(Expression<Func<ExtendedProperties, bool>> predicate)
    {
        return _dbSet.Include(x => x.RecordValues).Include(x => x.Values).Where(predicate);
    }

    public async Task CreateExtendedProperties(ExtendedProperties model)
    {
        await _dbSet.AddAsync(model);

    }

    public void UpdateExtendedProperties(ExtendedProperties model)
    {
        _dbSet.Entry(model).State = EntityState.Modified;
    }

    public async Task<ExtendedProperties?> GetExtendedPropertiesByIdAsync(string id)
    {
        return await _dbSet.Include(x => x.RecordValues).Include(x => x.Values).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ExtendedProperties?> GetExtendedPropertiesByNameAsync(string name)
    {
        return await _dbSet.Include(x => x.RecordValues).Include(x => x.Values).FirstOrDefaultAsync(x => x.Name == name);
    }
}