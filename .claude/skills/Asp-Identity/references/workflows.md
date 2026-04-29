# Identity Workflows

## Complete Implementation Sequence

### Phase 1: Project Analysis

Before installing anything, analyze the project:

1. **Determine project type:**
   - API only? → Install JWT packages
   - MVC/WebApp? → Install Identity UI packages
   - Both? → Install both sets of packages

2. **Check existing configuration:**
   - Does `Program.cs` have Identity setup?
   - Does `ApplicationDbContext` exist?
   - What Identity classes are already defined?

3. **Ask user about registration:**
   - Will public registration be allowed?
   - Or only admin-created users?

### Phase 2: Package Installation

**Essential packages (all projects):**
```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Identity.UI
```

**API projects only:**
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
```

### Phase 3: Entity Creation

1. Create `UserStatus` enum in `EnumHelpers` if not exists
2. Create `AppUser` entity (extends `IdentityUser<Guid>`)
3. Create `AppRole` entity (extends `IdentityRole<Guid>`)
4. Create `Module` and `ModuleClaim` entities
5. Optionally create `UserProperty` entity for extensibility
6. Update `ApplicationDbContext` with all `DbSet` declarations

### Phase 4: Service Configuration

**In Program.cs:**

```csharp
// Identity configuration
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // SignIn settings
    options.SignIn.RequireConfirmedEmail = false; // Set to true for email confirmation
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// For API projects - JWT configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")))
    };
});

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IAccountService, AccountService>();
```

### Phase 5: Create Services

**IIdentityService interface:**
```csharp
public interface IIdentityService
{
    Task<(bool Succeeded, string[] Errors)> CreateUserAsync(AppUser user, string password);
    Task<(bool Succeeded, string[] Errors)> CreateRoleAsync(AppRole role);
    Task<AppUser?> GetUserByIdAsync(string userId);
    Task<AppUser?> GetUserByEmailAsync(string email);
    Task<AppRole?> GetRoleByIdAsync(string roleId);
    Task<(bool Succeeded, string[] Errors)> AddToRoleAsync(AppUser user, string roleName);
    Task<(bool Succeeded, string[] Errors)> AddClaimAsync(AppUser user, Claim claim);
    Task<(bool Succeeded, string[] Errors)> AddClaimToRoleAsync(string roleName, Claim claim);
    Task<IList<string>> GetUserRolesAsync(AppUser user);
    Task<IList<Claim>> GetUserClaimsAsync(AppUser user);
    Task<string> GeneratePasswordResetTokenAsync(AppUser user);
    Task<(bool Succeeded, string[] Errors)> ResetPasswordAsync(AppUser user, string token, string newPassword);
}
```

**IdentityService implementation:**
- Uses `UserManager` for user operations
- Uses `RoleManager` for role operations
- Handles business logic between Identity and other entities
- Returns consistent result tuples

### Phase 6: Create Controllers

Create controllers in this specific order:

#### 1. AuthController
Handles authentication flows (login, register, forgot password)

#### 2. AccountController
Handles logged-in user account management (profile, password, 2FA)

#### 3. AdminUserController
Handles user administration by authorized users

#### 4. RoleController
Handles role management

#### 5. ClaimController
Handles claim management

#### 6. ModuleController
Handles module management

### Phase 7: Create View Structure

Create folders and empty views (no HTML content, just placeholder views):

```
Views/
├── Auth/ (custom design, separate from main layout)
├── Account/
├── AdminUser/
├── Role/
├── Claim/
└── Module/
```

## Specific Workflow Implementations

### User Registration Flow

**Public Registration (if allowed):**
1. User fills registration form
2. Create AppUser with `UserStatus = AwaitingApproval`
3. Send confirmation email (optional)
4. Admin approves user (sets status to Active)
5. User can login

**Admin-Created User:**
1. Admin fills user creation form
2. Create AppUser with `UserStatus = Active` (or AwaitingApproval)
3. Assign initial roles
4. Send welcome email with temporary password (optional)
5. User changes password on first login

### Login Flow

**Web User:**
1. User submits email/password
2. Validate user exists and `IsApiUser = false`
3. Validate `UserStatus = Active`
4. Check password with `UserManager`
5. If 2FA enabled, require code
6. Create auth cookie and sign in

**API User:**
1. Client sends credentials to `/api/auth/login`
2. Validate user exists and `IsApiUser = true`
3. Validate `UserStatus = Active`
4. Check password with `UserManager`
5. Generate JWT token
6. Return token to client

### Claim Assignment Flow

**Role-based claims:**
1. Navigate to Role Claims page
2. Select Module
3. Check/uncheck claims for role
4. Save updates all users with this role

**Individual user claims:**
1. Navigate to User Claims page
2. Display:
   - Claims inherited from roles (read-only, show source role)
   - Individual claims assigned directly (editable)
3. Add/remove individual claims
4. Non-role claims NOT shown in picker unless already assigned

### Password Reset Flow

1. User requests password reset (provides email)
2. Generate reset token with `UserManager.GeneratePasswordResetTokenAsync()`
3. Send email with reset link containing token
4. User clicks link, enters new password
5. Validate token with `UserManager.ResetPasswordAsync()`
6. Password updated, user can login

### 2FA Setup Flow

1. User navigates to 2FA setup page
2. Generate shared secret key
4. Display QR code for authenticator app
3. User scans QR code with app
4. User enters verification code from app
5. Verify code with `UserManager.VerifyTwoFactorTokenAsync()`
6. Enable 2FA for user
7. Show backup recovery codes

## Authorization Implementation

### Claim-Based Authorization

**In Controller/Actions:**
```csharp
[Authorize]
public class CustomerController : Controller
{
    [Authorize(Policy = "Customer.Create")]
    public IActionResult Create() { }

    [Authorize(Policy = "Customer.Update")]
    public IActionResult Edit(string id) { }

    [Authorize(Policy = "Customer.Delete")]
    public IActionResult Delete(string id) { }
}
```

### Dynamic Policy Creation

**In Program.cs (after Identity setup):**
```csharp
// Create policies from ModuleClaim entities
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var claims = await dbContext.ModuleClaims.Include(c => c.Module).ToListAsync();

    foreach (var claim in claims)
    {
        var policyName = $"{claim.Module.Name}.{claim.Name}";
        options.AddPolicy(policyName, policy =>
        {
            policy.RequireClaim("Module", claim.Module.Name);
            policy.RequireClaim("Action", claim.Name ?? "");
        });
    }
}
```

## User Status Management

**Status transitions:**
- `AwaitingApproval` → `Active`: Admin approval
- `Active` ↔ `Passive`: User/admin toggle
- `Any` → `Blocked`: Admin action (user cannot login)
- `Any` → `Deleted`: Soft delete (never show, cannot login)

**Login validation:**
```csharp
if (user.UserStatus != UserStatus.Active)
{
    return "User account is not active";
}

if (user.IsApiUser && isWebLogin)
{
    return "API users cannot login via web interface";
}
```

## Avatar Management

**Upload flow:**
1. Receive file in controller
2. Validate file type (image only)
3. Validate file size (max 2MB)
4. Generate new GUID filename
5. Save to `wwwroot/Upload/Avatars/`
6. Update `AppUser.Avatar` with relative path
7. Delete old avatar if exists

**Delete flow:**
1. Get current avatar path from user
2. Delete physical file
3. Set `AppUser.Avatar = null`
