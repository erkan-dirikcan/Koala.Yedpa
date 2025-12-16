using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Koala.Yedpa.Repositories.Repositories
{
    public class ClaimsRepository(AppDbContext context) : IClaimsRepository
    {
        public readonly DbSet<Claims> _dbSet = context.Set<Claims>();
        private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));


        public async Task<List<Claims>?> GetClaimsByModuleIdAsync(string moduleId)
        {
            try
            {
                var claims = _dbSet
                       .Where(c => c.ModuleId == moduleId)
                       .Include(c => c.Module);
                return await claims.ToListAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<Claims>?> GetAllClaimsAsync()
        {
            try
            {
                var claimsList = await _context.Claims.Include(x => x.Module).ToListAsync();
                foreach (var claim in claimsList)
                {
                    _context.Entry(claim).State = EntityState.Detached; // Her bir nesneyi detached yap
                }
                return claimsList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<Claims>?> WhereClaimsAsync(Expression<Func<Claims, bool>> predicate)
        {
            var claims = await _dbSet
                .Include(c => c.Module)
                .Where(predicate)
                .ToListAsync();
            _context.Entry(claims).State = EntityState.Detached;
            return claims;
        }

        public async Task<Claims?> GetClaimByIdAsync(string id)
        {
            var res = await _dbSet.Include(x => x.Module).FirstOrDefaultAsync(x => x.Id == id);
            if (res != null)
            {
                _dbSet.Entry(res).State = EntityState.Detached;
            }
            return res;
        }

        public async Task AddClaimAsync(Claims claim)
        {
            await _dbSet.AddAsync(claim);
        }

        public Claims UpdateClaim(Claims claim)
        {
            _dbSet.Entry(claim).State = EntityState.Modified;
            return claim;
        }


        public async Task DeleteClaimAsync(string id)
        {
            var claim = await _dbSet.FindAsync(id);
            if (claim != null)
            {
                _dbSet.Remove(claim);
            }
        }
    }
}
