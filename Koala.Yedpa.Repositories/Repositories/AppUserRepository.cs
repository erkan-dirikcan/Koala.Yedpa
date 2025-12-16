using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Koala.Yedpa.Repositories.Repositories
{
    public class AppUserRepository : IAppUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly DbSet<AppUser> _dbSet;

        public AppUserRepository(UserManager<AppUser> userManager, AppDbContext context, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _dbSet = _context.Set<AppUser>();
        }

        public async Task<AppUser?> GetUserInfoById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user;
        }

        public async Task<AppUser?> GetUserInfoByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user;
        }

        public async Task<AppUser?> GetUserInfoByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return user;
        }

        public async Task<List<AppUser>> GetAllUsers()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<bool> IsUserExists(string userName)
        {
            return await _userManager.FindByNameAsync(userName) != null;
        }

        public async Task<bool> IsUserExistsByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        public async Task<bool> IsUserExistsById(string id)
        {
            return await _userManager.FindByIdAsync(id) != null;
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
            var user = await _userManager.FindByEmailAsync(id);
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.Select(claim => claim.Value).Select(claimName => new Claims
            {
                Name = claimName.Substring(claimName.IndexOf(".", StringComparison.Ordinal))
            }).ToList();
        }

        public async Task<List<Claims>> GetUserClaimsByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.Select(claim => claim.Value).Select(claimName => new Claims
            {
                Name = claimName.Substring(claimName.IndexOf(".", StringComparison.Ordinal))
            }).ToList();
        }

        public async Task<List<Claims>> GetUserClaimsByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) throw new Exception("Kullanıcı Bulunamadı");

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.Select(claim => claim.Value).Select(claimName => new Claims
            {
                Name = claimName.Substring(claimName.IndexOf(".", StringComparison.Ordinal))
            }).ToList();

        }

        public async Task<List<AppRole>> GetUserRolesById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var appRoles = new List<AppRole>();
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        appRoles.Add(role);
                    }
                }
                return appRoles;
            }

            throw new Exception("Kullanıcı Bulunamadı");
        }

        public async Task<List<AppRole>> GetUserRolesByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var appRoles = new List<AppRole>();
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        appRoles.Add(role);
                    }
                }
                return appRoles;
            }

            throw new Exception("Kullanıcı Bulunamadı");
        }

        public async Task<List<AppRole>> GetUserRolesByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var appRoles = new List<AppRole>();
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        appRoles.Add(role);
                    }
                }
                return appRoles;
            }

            throw new Exception("Kullanıcı Bulunamadı");
        }

        public async Task<bool> UpdateUserStatus(string id, StatusEnum status)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Status = status;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }

            throw new Exception("Kullanıcı Bulunamadı");
        }

        public async Task<bool> UpdateUserStatusByEmail(string email, StatusEnum status)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                user.Status = status;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }

            throw new Exception("Kullanıcı Bulunamadı");
        }

        public async Task<bool> UpdateUserStatusByUserName(string userName, StatusEnum status)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                user.Status = status;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }

            throw new Exception("Kullanıcı Bulunamadı");
        }

        public async Task<bool> UpdateUser(AppUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> RemoveUserLockout(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            user.LockoutEnd = null;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
