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

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<LgXt001211, bool>> predicate)
        {
            return await _dbSet.Where(predicate).CountAsync();
        }

        public async Task<IEnumerable<LgXt001211>> GetPagedAsync(int skip, int take, Expression<Func<LgXt001211, bool>>? predicate = null, string? orderBy = null, bool ascending = true)
        {
            var query = predicate != null ? _dbSet.Where(predicate) : _dbSet.AsQueryable();

            // Sıralama
            if (!string.IsNullOrEmpty(orderBy))
            {
                switch (orderBy.ToLower())
                {
                    case "logref":
                        query = ascending ? query.OrderBy(x => x.LogRef) : query.OrderByDescending(x => x.LogRef);
                        break;
                    case "groupcode":
                        query = ascending ? query.OrderBy(x => x.GroupCode) : query.OrderByDescending(x => x.GroupCode);
                        break;
                    case "groupname":
                        query = ascending ? query.OrderBy(x => x.GroupName) : query.OrderByDescending(x => x.GroupName);
                        break;
                    case "clientcode":
                        query = ascending ? query.OrderBy(x => x.ClientCode) : query.OrderByDescending(x => x.ClientCode);
                        break;
                    case "clientname":
                        query = ascending ? query.OrderBy(x => x.ClientName) : query.OrderByDescending(x => x.ClientName);
                        break;
                    case "begdate":
                        query = ascending ? query.OrderBy(x => x.BegDate) : query.OrderByDescending(x => x.BegDate);
                        break;
                    case "enddate":
                        query = ascending ? query.OrderBy(x => x.EndDate) : query.OrderByDescending(x => x.EndDate);
                        break;
                    default:
                        query = query.OrderBy(x => x.LogRef);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.LogRef);
            }

            return await query.Skip(skip).Take(take).ToListAsync();
        }
    }
}
