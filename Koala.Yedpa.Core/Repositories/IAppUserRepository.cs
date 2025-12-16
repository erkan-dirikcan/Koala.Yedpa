using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Core.Repositories;

public interface IAppUserRepository
{
    Task<AppUser?> GetUserInfoById(string id);
    Task<AppUser?> GetUserInfoByEmail(string email);
    Task<AppUser?> GetUserInfoByUserName(string userName);
    Task<List<AppUser>> GetAllUsers();
    
    Task<bool> IsUserExists(string userName);
    Task<bool> IsUserExistsByEmail(string email);
    Task<bool> IsUserExistsById(string id);

    Task<StatusEnum> GetUserStatusById(string id);
    Task<StatusEnum> GetUserStatusByEmail(string email);
    Task<StatusEnum> GetUserStatusByUserName(string userName);

    Task<List<Claims>> GetUserClaimsById(string id);
    Task<List<Claims>> GetUserClaimsByEmail(string email);
    Task<List<Claims>> GetUserClaimsByUserName(string userName);

    Task<List<AppRole>> GetUserRolesById(string id);
    Task<List<AppRole>> GetUserRolesByEmail(string email);
    Task<List<AppRole>> GetUserRolesByUserName(string userName);


    Task<bool> UpdateUserStatus(string id, StatusEnum status);
    Task<bool> UpdateUserStatusByEmail(string email, StatusEnum status);
    Task<bool> UpdateUserStatusByUserName(string userName, StatusEnum status);
    Task<bool> UpdateUser(AppUser user);
    Task<bool> RemoveUserLockout(string id);
}