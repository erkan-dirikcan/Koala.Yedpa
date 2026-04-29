using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories
{
    public class AppUserRepository : IAppUserRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<AppUser> _dbSet;

        public AppUserRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<AppUser>();
        }

        public async Task<AppUser?> GetUserInfoById(string id)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<AppUser?> GetUserInfoByEmail(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<AppUser?> GetUserInfoByUserName(string userName)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.UserName == userName);
        }

        public async Task<List<AppUser>> GetAllUsers()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<bool> IsUserExists(string userName)
        {
            return await _dbSet.AnyAsync(x => x.UserName == userName);
        }

        public async Task<bool> IsUserExistsByEmail(string email)
        {
            return await _dbSet.AnyAsync(x => x.Email == email);
        }

        public async Task<bool> IsUserExistsById(string id)
        {
            return await _dbSet.AnyAsync(x => x.Id == id);
        }

        public async Task<StatusEnum> GetUserStatusById(string id)
        {
            var retVal = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            if (retVal != null && retVal.Status != null)
            {
                return (StatusEnum)retVal.Status;
            }

            throw new Exception("Kullanıcı Durum Bilgilerine Ulaşılamadı");
        }

        public async Task<StatusEnum> GetUserStatusByEmail(string email)
        {
            var retVal = await _dbSet.FirstOrDefaultAsync(x => x.Email == email);
            if (retVal != null && retVal.Status != null)
            {
                return (StatusEnum)retVal.Status;
            }

            throw new Exception("Kullanıcı Durum Bilgilerine Ulaşılamadı");
        }

        public async Task<StatusEnum> GetUserStatusByUserName(string userName)
        {
            var retVal = await _dbSet.FirstOrDefaultAsync(x => x.UserName == userName);
            if (retVal != null && retVal.Status != null)
            {
                return (StatusEnum)retVal.Status;
            }

            throw new Exception("Kullanıcı Durum Bilgilerine Ulaşılamadı");
        }

        public async Task<List<Claims>> GetUserClaimsById(string id)
        {
            var userClaims = await _context.UserClaims
                .Where(x => x.UserId == id)
                .ToListAsync();

            return userClaims.Select(claim => new Claims
            {
                Name = claim.ClaimType.Substring(claim.ClaimType.IndexOf(".", StringComparison.Ordinal) + 1)
            }).ToList();
        }

        public async Task<List<Claims>> GetUserClaimsByEmail(string email)
        {
            var user = await _dbSet.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");

            var userClaims = await _context.UserClaims
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            return userClaims.Select(claim => new Claims
            {
                Name = claim.ClaimType.Substring(claim.ClaimType.IndexOf(".", StringComparison.Ordinal) + 1)
            }).ToList();
        }

        public async Task<List<Claims>> GetUserClaimsByUserName(string userName)
        {
            var user = await _dbSet.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");

            var userClaims = await _context.UserClaims
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            return userClaims.Select(claim => new Claims
            {
                Name = claim.ClaimType.Substring(claim.ClaimType.IndexOf(".", StringComparison.Ordinal) + 1)
            }).ToList();
        }

        public async Task<List<AppRole>> GetUserRolesById(string id)
        {
            var userRoles = await _context.UserRoles
                .Where(x => x.UserId == id)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r)
                .ToListAsync();

            return userRoles;
        }

        public async Task<List<AppRole>> GetUserRolesByEmail(string email)
        {
            var user = await _dbSet.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");

            var userRoles = await _context.UserRoles
                .Where(x => x.UserId == user.Id)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r)
                .ToListAsync();

            return userRoles;
        }

        public async Task<List<AppRole>> GetUserRolesByUserName(string userName)
        {
            var user = await _dbSet.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");

            var userRoles = await _context.UserRoles
                .Where(x => x.UserId == user.Id)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r)
                .ToListAsync();

            return userRoles;
        }

        public async Task<bool> UpdateUserStatus(string id, StatusEnum status)
        {
            var user = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            if (user != null)
            {
                user.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }

            throw new Exception("Kullanıcı Bulunamadı");
        }

        public async Task<bool> UpdateUserStatusByEmail(string email, StatusEnum status)
        {
            var user = await _dbSet.FirstOrDefaultAsync(x => x.Email == email);
            if (user != null)
            {
                user.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }

            throw new Exception("Kullanıcı Bulunamadı");
        }

        public async Task<bool> UpdateUserStatusByUserName(string userName, StatusEnum status)
        {
            var user = await _dbSet.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user != null)
            {
                user.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }

            throw new Exception("Kullanıcı Bulunamadı");
        }

        public async Task<bool> UpdateUser(AppUser user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUserLockout(string id)
        {
            var user = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            if (user != null)
            {
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();
                return true;
            }

            throw new Exception("Kullanıcı Bulunamadı");
        }
    }
}
