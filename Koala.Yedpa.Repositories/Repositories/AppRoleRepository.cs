using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories;

public class AppRoleRepository : IAppRoleRepository
{
    private readonly AppDbContext _context;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IClaimsRepository _claimsRepository;

    public AppRoleRepository(AppDbContext context,  RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IClaimsRepository claimsRepository)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _signInManager = signInManager;
        _claimsRepository = claimsRepository;

    }

    public async Task<AppRole?> GetRoleById(string id)
    {
        var retVal = await _roleManager.FindByIdAsync(id);
        return retVal;
    }

    public async Task<AppRole?> GetRoleByName(string name)
    {
        var retVal = await _roleManager.FindByNameAsync(name);
        return retVal;
    }

    public async Task<List<AppRole>> GetAllRoles()
    {
        return await _roleManager.Roles.ToListAsync();
    }

    public async Task<bool> IsRoleExistsById(string id)
    {
        return await _roleManager.Roles.AnyAsync(x => x.Id == id);
    }

    public async Task<bool> IsRoleExistsByName(string name)
    {
        return await _roleManager.Roles.AnyAsync(x => x.Name == name);

    }

    public async Task<Claims> GetRoleClaimById(string id)
    {
        //var role =await _roleManager.FindByIdAsync(id);
        //if (role == null) throw new Exception("Rol Bulunamadı");
        //var roleClaims = (await _roleManager.GetClaimsAsync(role)).Select(x=>x.Value==);
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
        throw new NotImplementedException();
    }

    public async Task<List<AppUser>> GetUsersInRoleByName(string name)
    {
        throw new NotImplementedException();
    }
}