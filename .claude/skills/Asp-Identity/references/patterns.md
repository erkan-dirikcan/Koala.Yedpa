# Repository Patterns and Service Layers

## When to Use Which Approach

### UserManager vs UserStore vs Repository

| Scenario | Use This | Why |
|----------|----------|-----|
| Create user | `UserManager.CreateAsync()` | Validates, hashes password, updates Identity tables |
| Add role to user | `UserManager.AddToRoleAsync()` | Handles UserRole junction table correctly |
| Add claim to user | `UserManager.AddClaimAsync()` | Handles UserClaim junction table correctly |
| Generate password reset token | `UserManager.GeneratePasswordResetTokenAsync()` | Creates secure token with security stamp |
| Reset password | `UserManager.ResetPasswordAsync()` | Validates token, updates password hash |
| Get user with roles | `UserManager.GetRolesAsync()` | Efficient query with joins |
| Get user with claims | `UserManager.GetClaimsAsync()` | Efficient query with joins |
| Search users by custom criteria | Repository/DbContext | Custom LINQ queries |
| Bulk update user status | Repository/DbContext | Direct SQL update |
| Query users with related data | Repository/DbContext | Include() for navigation properties |
| Get all active users | Repository/DbContext | Efficient .Where() query |
| Get user by email | `UserManager.FindByEmailAsync()` | Handles case-insensitive search |
| Check if user in role | `UserManager.IsInRoleAsync()` | Validates against role store |
| Validate password | `UserManager.CheckPasswordAsync()` | Hashes input and compares |

### Critical Rules

**NEVER use UserManager for:**
- Complex LINQ queries with multiple includes
- Bulk operations on multiple users
- Joining with non-Identity entities (Modules, etc.)
- Custom reporting queries
- Performance-critical read operations

**NEVER use DbContext directly for:**
- Creating users (password won't be hashed)
- Adding roles/claims (junction tables won't be updated correctly)
- Password operations (security vulnerability)

**ALWAYS use UserManager for:**
- User CRUD operations
- Role/Claim assignment to users
- Password operations
- Token generation
- Validation

## Service Layer Pattern

### IIdentityService Interface

```csharp
public interface IIdentityService
{
    // User operations
    Task<(bool Succeeded, string[] Errors)> CreateUserAsync(AppUser user, string password);
    Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(AppUser user);
    Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(string userId);
    Task<AppUser?> GetUserByIdAsync(string userId);
    Task<AppUser?> GetUserByEmailAsync(string email);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<IEnumerable<AppUser>> GetActiveUsersAsync();
    Task<IEnumerable<AppUser>> GetApiUsersAsync();
    Task<IEnumerable<AppUser>> GetWebUsersAsync();

    // Role operations
    Task<(bool Succeeded, string[] Errors)> CreateRoleAsync(AppRole role);
    Task<(bool Succeeded, string[] Errors)> UpdateRoleAsync(AppRole role);
    Task<(bool Succeeded, string[] Errors)> DeleteRoleAsync(string roleId);
    Task<AppRole?> GetRoleByIdAsync(string roleId);
    Task<IEnumerable<AppRole>> GetRolesAsync();

    // Role assignment
    Task<(bool Succeeded, string[] Errors)> AddToRoleAsync(AppUser user, string roleName);
    Task<(bool Succeeded, string[] Errors)> RemoveFromRoleAsync(AppUser user, string roleName);
    Task<IList<string>> GetUserRolesAsync(AppUser user);

    // Claim operations
    Task<(bool Succeeded, string[] Errors)> AddClaimAsync(AppUser user, Claim claim);
    Task<(bool Succeeded, string[] Errors)> RemoveClaimAsync(AppUser user, Claim claim);
    Task<(bool Succeeded, string[] Errors)> AddClaimToRoleAsync(string roleName, Claim claim);
    Task<(bool Succeeded, string[] Errors)> RemoveClaimFromRoleAsync(string roleName, Claim claim);
    Task<IList<Claim>> GetUserClaimsAsync(AppUser user);
    Task<IList<Claim>> GetRoleClaimsAsync(string roleName);

    // Password operations
    Task<string> GeneratePasswordResetTokenAsync(AppUser user);
    Task<(bool Succeeded, string[] Errors)> ResetPasswordAsync(AppUser user, string token, string newPassword);
    Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword);
    Task<(bool Succeeded, string[] Errors)> AdminChangePasswordAsync(AppUser user, string newPassword);

    // User status
    Task<(bool Succeeded, string[] Errors)> SetUserStatusAsync(string userId, UserStatus status);

    // Avatar
    Task<(bool Succeeded, string[] Errors)> UpdateAvatarAsync(AppUser user, IFormFile avatarFile);
    Task<(bool Succeeded, string[] Errors)> RemoveAvatarAsync(AppUser user);

    // Two-factor
    Task<string> GenerateTwoFactorTokenAsync(AppUser user);
    Task<bool> VerifyTwoFactorTokenAsync(AppUser user, string token);
}
```

### IdentityService Implementation

```csharp
public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        ApplicationDbContext context,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Succeeded, string[] Errors)> CreateUserAsync(AppUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(AppUser user)
    {
        var result = await _userManager.UpdateAsync(user);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, new[] { "User not found" });

        // Soft delete - update status
        user.UserStatus = UserStatus.Deleted;
        return await UpdateUserAsync(user);
    }

    public Task<AppUser?> GetUserByIdAsync(string userId)
        => _userManager.FindByIdAsync(userId);

    public Task<AppUser?> GetUserByEmailAsync(string email)
        => _userManager.FindByEmailAsync(email);

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
        => await _context.Users
            .OrderBy(u => u.Name)
            .ThenBy(u => u.LastName)
            .ToListAsync();

    public async Task<IEnumerable<AppUser>> GetActiveUsersAsync()
        => await _context.Users
            .Where(u => u.UserStatus == UserStatus.Active)
            .OrderBy(u => u.Name)
            .ThenBy(u => u.LastName)
            .ToListAsync();

    public async Task<IEnumerable<AppUser>> GetApiUsersAsync()
        => await _context.Users
            .Where(u => u.IsApiUser)
            .OrderBy(u => u.Name)
            .ThenBy(u => u.LastName)
            .ToListAsync();

    public async Task<IEnumerable<AppUser>> GetWebUsersAsync()
        => await _context.Users
            .Where(u => !u.IsApiUser)
            .OrderBy(u => u.Name)
            .ThenBy(u => u.LastName)
            .ToListAsync();

    public async Task<(bool Succeeded, string[] Errors)> CreateRoleAsync(AppRole role)
    {
        var result = await _roleManager.CreateAsync(role);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateRoleAsync(AppRole role)
    {
        var result = await _roleManager.UpdateAsync(role);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> DeleteRoleAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return (false, new[] { "Role not found" });

        var result = await _roleManager.DeleteAsync(role);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public Task<AppRole?> GetRoleByIdAsync(string roleId)
        => _roleManager.FindByIdAsync(roleId);

    public async Task<IEnumerable<AppRole>> GetRolesAsync()
        => await _context.Roles
            .OrderBy(r => r.Name)
            .ToListAsync();

    public async Task<(bool Succeeded, string[] Errors)> AddToRoleAsync(AppUser user, string roleName)
    {
        var result = await _userManager.AddToRoleAsync(user, roleName);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> RemoveFromRoleAsync(AppUser user, string roleName)
    {
        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public Task<IList<string>> GetUserRolesAsync(AppUser user)
        => _userManager.GetRolesAsync(user);

    public async Task<(bool Succeeded, string[] Errors)> AddClaimAsync(AppUser user, Claim claim)
    {
        var result = await _userManager.AddClaimAsync(user, claim);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> RemoveClaimAsync(AppUser user, Claim claim)
    {
        var result = await _userManager.RemoveClaimAsync(user, claim);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> AddClaimToRoleAsync(string roleName, Claim claim)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
            return (false, new[] { "Role not found" });

        var result = await _roleManager.AddClaimAsync(role, claim);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> RemoveClaimFromRoleAsync(string roleName, Claim claim)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
            return (false, new[] { "Role not found" });

        var result = await _roleManager.RemoveClaimAsync(role, claim);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public Task<IList<Claim>> GetUserClaimsAsync(AppUser user)
        => _userManager.GetClaimsAsync(user);

    public async Task<IList<Claim>> GetRoleClaimsAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
            return new List<Claim>();

        return await _roleManager.GetClaimsAsync(role);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(AppUser user)
        => await _userManager.GeneratePasswordResetTokenAsync(user);

    public async Task<(bool Succeeded, string[] Errors)> ResetPasswordAsync(AppUser user, string token, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
    {
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> AdminChangePasswordAsync(AppUser user, string newPassword)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> SetUserStatusAsync(string userId, UserStatus status)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, new[] { "User not found" });

        user.UserStatus = status;
        return await UpdateUserAsync(user);
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateAvatarAsync(AppUser user, IFormFile avatarFile)
    {
        try
        {
            // Delete old avatar
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                AvatarHelper.DeleteAvatar(user.Avatar);
            }

            // Save new avatar
            var avatarPath = await AvatarHelper.SaveAvatarAsync(avatarFile);
            user.Avatar = avatarPath;

            return await UpdateUserAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating avatar for user {UserId}", user.Id);
            return (false, new[] { "Error updating avatar" });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> RemoveAvatarAsync(AppUser user)
    {
        if (!string.IsNullOrEmpty(user.Avatar))
        {
            AvatarHelper.DeleteAvatar(user.Avatar);
            user.Avatar = null;
        }

        return await UpdateUserAsync(user);
    }

    public async Task<string> GenerateTwoFactorTokenAsync(AppUser user)
        => await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);

    public async Task<bool> VerifyTwoFactorTokenAsync(AppUser user, string token)
        => await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, token);
}
```

## IAccountService Interface

```csharp
public interface IAccountService
{
    Task<(bool Succeeded, string[] Errors)> RegisterAsync(RegisterViewModel model, bool isApiUser = false);
    Task<(bool Succeeded, string[] Errors)> LoginAsync(LoginViewModel model, bool isApiLogin = false);
    Task LogoutAsync();
    Task<(bool Succeeded, string[] Errors)> ForgotPasswordAsync(string email);
    Task<(bool Succeeded, string[] Errors)> ResetPasswordAsync(ResetPasswordViewModel model);
    Task<ProfileViewModel> GetUserProfileAsync(string userId);
    Task<(bool Succeeded, string[] Errors)> UpdateProfileAsync(string userId, ProfileViewModel model);
    Task<(bool Succeeded, string[] Errors)> EnableTwoFactorAsync(string userId);
    Task<(bool Succeeded, string[] Errors)> DisableTwoFactorAsync(string userId);
}
```

## Repository Pattern for Complex Queries

```csharp
public interface IUserRepository
{
    Task<AppUser?> GetByIdWithDetailsAsync(string userId);
    Task<IEnumerable<AppUser>> GetUsersByRoleAsync(string roleName);
    Task<IEnumerable<AppUser>> SearchUsersAsync(string searchTerm);
    Task<IEnumerable<AppUser>> GetUsersByStatusAsync(UserStatus status);
    Task<UserDetailsDto?> GetUserDetailsAsync(string userId);
}

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByIdWithDetailsAsync(string userId)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserClaims)
            .Include(u => u.UserProperties)
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
    }

    public async Task<IEnumerable<AppUser>> GetUsersByRoleAsync(string roleName)
    {
        return await _context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
            .Include(u => u.UserRoles)
            .ToListAsync();
    }

    public async Task<IEnumerable<AppUser>> SearchUsersAsync(string searchTerm)
    {
        return await _context.Users
            .Where(u => EF.Functions.Like(u.Name, $"%{searchTerm}%") ||
                        EF.Functions.Like(u.LastName, $"%{searchTerm}%") ||
                        EF.Functions.Like(u.Email, $"%{searchTerm}%") ||
                        EF.Functions.Like(u.FullName, $"%{searchTerm}%"))
            .Include(u => u.UserRoles)
            .ToListAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersByStatusAsync(UserStatus status)
    {
        return await _context.Users
            .Where(u => u.UserStatus == status)
            .ToListAsync();
    }

    public async Task<UserDetailsDto?> GetUserDetailsAsync(string userId)
    {
        return await _context.Users
            .Where(u => u.Id == Guid.Parse(userId))
            .Select(u => new UserDetailsDto
            {
                Id = u.Id.ToString(),
                Email = u.Email,
                Name = u.Name,
                LastName = u.LastName,
                FullName = u.FullName,
                Avatar = u.Avatar,
                UserStatus = u.UserStatus,
                IsApiUser = u.IsApiUser,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Claims = u.UserClaims.Select(uc => new ClaimDto
                {
                    Type = uc.ClaimType,
                    Value = uc.ClaimValue
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }
}
```

## JWT Token Service for API Projects

```csharp
public interface IJwtTokenService
{
    string GenerateAccessToken(AppUser user, IList<string> roles, IList<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(AppUser user, IList<string> roles, IList<Claim> claims)
    {
        var claimsList = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("IsApiUser", user.IsApiUser.ToString())
        };

        // Add roles
        foreach (var role in roles)
        {
            claimsList.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add custom claims
        claimsList.AddRange(claims);

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(
            Convert.ToDouble(_configuration["Jwt:ExpirationDays"] ?? "7"));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claimsList,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? "");

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
```

## Claim Management Service

```csharp
public interface IClaimManagementService
{
    Task<IEnumerable<ClaimGroupDto>> GetUserClaimGroupsAsync(string userId);
    Task<IEnumerable<ClaimGroupDto>> GetRoleClaimGroupsAsync(string roleId);
    Task<(bool Succeeded, string[] Errors)> UpdateUserClaimsAsync(string userId, List<string> selectedClaims);
    Task<(bool Succeeded, string[] Errors)> UpdateRoleClaimsAsync(string roleId, List<string> selectedClaims);
}

public class ClaimManagementService : IClaimManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public ClaimManagementService(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<ClaimGroupDto>> GetUserClaimGroupsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Enumerable.Empty<ClaimGroupDto>();

        var userRoles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);

        var roleClaims = new List<Claim>();
        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                roleClaims.AddRange(claims);
            }
        }

        var modules = await _context.Modules
            .Include(m => m.Claims)
            .OrderBy(m => m.DisplayName ?? m.Name)
            .ToListAsync();

        var result = new List<ClaimGroupDto>();

        foreach (var module in modules)
        {
            var claimGroup = new ClaimGroupDto
            {
                ModuleId = module.Id,
                ModuleName = module.Name,
                ModuleDisplayName = module.DisplayName ?? module.Name,
                Claims = new List<ClaimItemDto>()
            };

            foreach (var claim in module.Claims)
            {
                var claimName = $"{module.Name}.{claim.Name}";
                var isInherited = roleClaims.Any(c => c.Type == claimName);
                var isAssigned = userClaims.Any(c => c.Type == claimName);

                claimGroup.Claims.Add(new ClaimItemDto
                {
                    ClaimId = claim.Id,
                    ClaimName = claimName,
                    ClaimDisplayName = claim.DisplayName ?? claim.Name,
                    IsAssigned = isAssigned,
                    IsInherited = isInherited,
                    Source = isInherited ? "Role" : "Individual"
                });
            }

            if (claimGroup.Claims.Any())
            {
                result.Add(claimGroup);
            }
        }

        return result;
    }

    public async Task<IEnumerable<ClaimGroupDto>> GetRoleClaimGroupsAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null) return Enumerable.Empty<ClaimGroupDto>();

        var roleClaims = await _roleManager.GetClaimsAsync(role);

        var modules = await _context.Modules
            .Include(m => m.Claims)
            .OrderBy(m => m.DisplayName ?? m.Name)
            .ToListAsync();

        var result = new List<ClaimGroupDto>();

        foreach (var module in modules)
        {
            var claimGroup = new ClaimGroupDto
            {
                ModuleId = module.Id,
                ModuleName = module.Name,
                ModuleDisplayName = module.DisplayName ?? module.Name,
                Claims = new List<ClaimItemDto>()
            };

            foreach (var claim in module.Claims)
            {
                var claimName = $"{module.Name}.{claim.Name}";
                var isAssigned = roleClaims.Any(c => c.Type == claimName);

                claimGroup.Claims.Add(new ClaimItemDto
                {
                    ClaimId = claim.Id,
                    ClaimName = claimName,
                    ClaimDisplayName = claim.DisplayName ?? claim.Name,
                    IsAssigned = isAssigned,
                    IsInherited = false,
                    Source = "Role"
                });
            }

            if (claimGroup.Claims.Any())
            {
                result.Add(claimGroup);
            }
        }

        return result;
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateUserClaimsAsync(string userId, List<string> selectedClaims)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, new[] { "User not found" });

        var userRoles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();

        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                roleClaims.AddRange(claims);
            }
        }

        // Get current individual claims (not from roles)
        var currentClaims = await _userManager.GetClaimsAsync(user);
        var individualClaims = currentClaims.Where(c => !roleClaims.Any(rc => rc.Type == c.Type)).ToList();

        // Remove individual claims not in selection
        foreach (var claim in individualClaims)
        {
            if (!selectedClaims.Contains(claim.Type))
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }
        }

        // Add new individual claims
        foreach (var claimName in selectedClaims)
        {
            if (!roleClaims.Any(c => c.Type == claimName) &&
                !individualClaims.Any(c => c.Type == claimName))
            {
                await _userManager.AddClaimAsync(user, new Claim(claimName, "true"));
            }
        }

        return (true, Array.Empty<string>());
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateRoleClaimsAsync(string roleId, List<string> selectedClaims)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return (false, new[] { "Role not found" });

        var currentClaims = await _roleManager.GetClaimsAsync(role);

        // Remove claims not in selection
        foreach (var claim in currentClaims)
        {
            if (!selectedClaims.Contains(claim.Type))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }
        }

        // Add new claims
        foreach (var claimName in selectedClaims)
        {
            if (!currentClaims.Any(c => c.Type == claimName))
            {
                await _roleManager.AddClaimAsync(role, new Claim(claimName, "true"));
            }
        }

        return (true, Array.Empty<string>());
    }
}
```

## DTOs

```csharp
public class ClaimGroupDto
{
    public string ModuleId { get; set; }
    public string ModuleName { get; set; }
    public string ModuleDisplayName { get; set; }
    public List<ClaimItemDto> Claims { get; set; }
}

public class ClaimItemDto
{
    public string ClaimId { get; set; }
    public string ClaimName { get; set; }
    public string ClaimDisplayName { get; set; }
    public bool IsAssigned { get; set; }
    public bool IsInherited { get; set; }
    public string Source { get; set; } // "Role" or "Individual"
}

public class UserDetailsDto
{
    public string Id { get; set; }
    public string? Email { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string? Avatar { get; set; }
    public UserStatus UserStatus { get; set; }
    public bool IsApiUser { get; set; }
    public List<string> Roles { get; set; }
    public List<ClaimDto> Claims { get; set; }
}

public class ClaimDto
{
    public string Type { get; set; }
    public string Value { get; set; }
}
```
