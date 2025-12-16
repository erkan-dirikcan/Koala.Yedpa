using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Core.Repositories;

public interface IAppRoleRepository
{
    Task<AppRole?> GetRoleById(string id);
    Task<AppRole?> GetRoleByName(string name);
    Task<List<AppRole>> GetAllRoles();
    Task<bool> IsRoleExistsById(string id);
    Task<bool> IsRoleExistsByName(string name);
    Task<Claims> GetRoleClaimById(string id);
    Task<Claims> GetRoleClaimByName(string name);
    Task <List<Claims>> GetAllRoleClaims();
    Task<List<AppUser>> GetUsersInRoleById(string id);
    Task<List<AppUser>> GetUsersInRoleByName(string name);
}