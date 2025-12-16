using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Koala.Yedpa.Repositories.Repositories
{
    public class SiteRepository: ISiteRepository
    {
        public readonly DbSet<LgXt001211> _dbSet;
        private readonly AppDbContext _context;

        public SiteRepository( AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<LgXt001211>();
        }

        public async Task<IEnumerable<LgXt001211>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public IQueryable<LgXt001211> Where(Expression<Func<LgXt001211, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public async Task<LgXt001211> GetByIdAsync(string id)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<LgXt001211> GetByLogRefAsyc(int logReg)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.LogRef == logReg);
        }

        public async Task<LgXt001211> GetByParLogRefAsyc(int parLogRef)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.ParLogRef == parLogRef);
        }

        public async Task<LgXt001211> GetByCodeAsyc(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.ClientCode == code);
        }

        public LgXt001211 Update(LgXt001211 entity)
        {
            _dbSet.Update(entity);
            return entity;
        }
    }
}
