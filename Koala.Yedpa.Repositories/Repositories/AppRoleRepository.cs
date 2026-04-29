using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories;

public class AppRoleRepository : IAppRoleRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<AppRole> _dbSet;
    private readonly IClaimsRepository _claimsRepository;

    public AppRoleRepository(AppDbContext context, IClaimsRepository claimsRepository)
    {
        _context = context;
        _dbSet = _context.Set<AppRole>();
        _claimsRepository = claimsRepository;
    }

    public async Task<AppRole?> GetRoleById(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<AppRole?> GetRoleByName(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<List<AppRole>> GetAllRoles()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<bool> IsRoleExistsById(string id)
    {
        return await _dbSet.AnyAsync(x => x.Id == id);
    }

    public async Task<bool> IsRoleExistsByName(string name)
    {
        return await _dbSet.AnyAsync(x => x.Name == name);
    }

    public async Task<Claims> GetRoleClaimById(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<Claims> GetRoleClaimByName(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Claims>> GetAllRoleClaims()
    {
        throw new NotImplementedException();
    }

    public async Task<List<AppUser>> GetUsersInRoleById(string id)
    {
        var userIds = await _context.UserRoles
            .Where(x => x.RoleId == id)
            .Select(x => x.UserId)
            .ToListAsync();

        return await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();
    }

    public async Task<List<AppUser>> GetUsersInRoleByName(string name)
    {
        var role = await _dbSet.FirstOrDefaultAsync(x => x.Name == name);
        if (role == null) throw new Exception("Rol Bulunamadı");

        var userIds = await _context.UserRoles
            .Where(x => x.RoleId == role.Id)
            .Select(x => x.UserId)
            .ToListAsync();

        return await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();
    }
}
