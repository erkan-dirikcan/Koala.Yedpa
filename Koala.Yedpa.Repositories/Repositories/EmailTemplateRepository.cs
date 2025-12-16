using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Koala.Yedpa.Repositories.Repositories;

public class EmailTemplateRepository(AppDbContext context) : IEmailTemplateRepository
{
    private readonly AppDbContext _context = context;
    private DbSet<EmailTemplate> _dbSet = context.Set<EmailTemplate>();

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync()
    {
        var all = await _dbSet.ToListAsync();
        return all;

    }

    public IQueryable<EmailTemplate> WhereAsync(Expression<Func<EmailTemplate, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }

    public async Task<EmailTemplate> GetByIdAsync(string id)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
        if (entity != null)
        {
            _dbSet.Entry(entity).State = EntityState.Detached;
        }
        return entity;
    }

    public async Task<EmailTemplate> GetByNameAsyc(string name)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(x => x.Name == name);
        if (entity != null)
        {
            _dbSet.Entry(entity).State = EntityState.Detached;
        }
        return entity;
    }

    public EmailTemplate Update(EmailTemplate entity)
    {
        _dbSet.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public async Task AddAsync(EmailTemplate entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void DeleteAsync(EmailTemplate entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<bool> IsExistAsync(string name)
    {
        return await _dbSet.AnyAsync(x => x.Name == name);
    }
}