---
name: Asp-Identity
description: ASP.NET Core Identity expert for setting up complete authentication/authorization systems. Use when implementing Identity in ASP.NET Core applications, including: (1) Installing Identity packages and configuring AppUser/AppRole entities, (2) Setting up JWT for API authentication, (3) Implementing role/claim-based authorization with module structure, (4) Creating Identity controllers, services, and repositories, (5) Managing API vs Web user separation, (6) Building complete Identity workflows (Register, Login, Forgot Password, 2FA, Profile management). Expert knows when to use UserManager, RoleManager, UserStore, RoleStore vs direct Entity operations.
---

# Asp-Identity Expert

## Quick Start

When triggered with "identity kur" or Identity setup request:

1. **Analyze project** - Check if API or MVC, existing packages, project structure
2. **Install packages** - Add required NuGet packages
3. **Create entities** - AppUser, AppRole, UserStatus enum, Module, Claim entities
4. **Setup services** - Configure Identity, JWT (if API), add custom stores
5. **Create repositories/services** - IdentityService, AccountService
6. **Build controllers** - All Identity workflow controllers
7. **Create view structure** - Empty pages and folder structure (no frontend)

## Key Design Rules

### User/Role Entities

**AppUser extends IdentityUser<Guid>**
- Required: Name, LastName, FullName, Avatar (string path)
- Required: IsApiUser (bool) - API users cannot access web UI
- Required: UserStatus (enum: Active=0x01, Passive=0x02, Deleted=0x03, Blocked=0x04, AwaitingApproval)
- Avatar storage: `wwwroot/Upload/Avatars/{Guid}.ext`

**AppRole extends IdentityRole<Guid>**
- Required: Description (string)
- Inherits all IdentityRole properties

### Authorization Architecture

**Role-Claim-Module Structure:**
- **Module**: Groups related functionality (e.g., "Customer", "Accounting")
- **Claim**: Specific actions within modules (e.g., "Create", "Update", "Delete")
- **Role**: Collection of claims (e.g., "Accounting" role has Customer CRUD claims)
- **User Claims**: Individual claim assignments override role claims

**Critical Rules:**
1. Claims belong to Modules (many-to-one)
2. Roles have claims (many-to-many via RoleClaim)
3. Users have claims (many-to-many via UserClaim)
4. User can have claims NOT in their role (individual permissions)
5. Claims editor shows: Role claims + Individual claims (Select2 UI)
6. Non-role claims NOT shown in claim picker unless already assigned
7. Role claim changes affect ALL users with that role

### API vs Web Users

**API User (IsApiUser = true):**
- Cannot login via web UI
- Can only access API endpoints
- Can be managed by web admin (Create, Update Role/Claim)
- Never bypasses auth rules (even public endpoints)

**Web User (IsApiUser = false):**
- Can login via web UI
- Can access API if authenticated
- Subject to all authorization rules

### UserStatus Enum

If UserStatus not defined in project, add to EnumHelpers:
```csharp
public enum UserStatus : byte
{
    Active = 0x01,
    Passive = 0x02,
    Deleted = 0x03,
    Blocked = 0x04,
    AwaitingApproval = 0x05
}
```

## Installation Workflow

**Required Packages (always):**
```
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.AspNetCore.Identity.UI
```

**For API projects add:**
```
Microsoft.AspNetCore.Authentication.JwtBearer
System.IdentityModel.Tokens.Jwt
```

**Service Registration (Program.cs):**
```csharp
builder.Services.AddIdentity<AppUser, AppRole>(options => {
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    // Lockout, User, SignIn settings as needed
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// For API projects
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        // Configure JWT token validation
    });
```

## Identity Workflows

Create these controllers in this order:

1. **AuthController** - Login, Logout, Register, Forgot Password, Reset Password
2. **AccountController** - Profile view/edit, Change Password, 2FA setup
3. **AdminUserController** - User management (Create, Update, Delete, Change Password)
4. **RoleController** - Role CRUD
5. **ClaimController** - Claim management
6. **ModuleController** - Module management

### Controller Actions

**AuthController:**
- `GET /Login` - Login page
- `POST /Login` - Login with email/password
- `GET /Register` - Register page (ask if registration allowed)
- `POST /Register` - Create user (Status = AwaitingApproval if public reg)
- `GET /ForgotPassword` - Forgot password page
- `POST /ForgotPassword` - Send reset email
- `GET /ResetPassword` - Reset password page
- `POST /ResetPassword` - Confirm password reset
- `POST /Logout` - Logout action

**AccountController:**
- `GET /Profile` - View current user profile
- `GET /Profile/Edit` - Edit profile form
- `POST /Profile/Edit` - Update profile
- `GET /ChangePassword` - Change password form
- `POST /ChangePassword` - Update password
- `GET /TwoFactorAuthentication` - 2FA management
- `POST /EnableTwoFactor` - Enable 2FA
- `POST /DisableTwoFactor` - Disable 2FA

**AdminUserController:**
- `GET /Index` - List all users
- `GET /Create` - Create user form
- `POST /Create` - Create new user
- `GET /Edit/{id}` - Edit user form
- `POST /Edit/{id}` - Update user
- `GET /Delete/{id}` - Confirm delete
- `POST /Delete/{id}` - Soft delete (set status)
- `GET /ChangePassword/{id}` - Change password form
- `POST /ChangePassword/{id}` - Update password
- `GET /Roles/{id}` - Manage user roles
- `POST /Roles/{id}` - Update user roles
- `GET /Claims/{id}` - Manage user claims
- `POST /Claims/{id}` - Update user claims

**RoleController:**
- `GET /Index` - List all roles
- `GET /Create` - Create role form
- `POST /Create` - Create role
- `GET /Edit/{id}` - Edit role
- `POST /Edit/{id}` - Update role
- `GET /Claims/{id}` - Manage role claims
- `POST /Claims/{id}` - Update role claims

**ClaimController:**
- `GET /Index` - List all claims by module
- `GET /Create` - Create claim form
- `POST /Create` - Create claim
- `GET /Edit/{id}` - Edit claim
- `POST /Edit/{id}` - Update claim

**ModuleController:**
- `GET /Index` - List all modules
- `GET /Create` - Create module form
- `POST /Create` - Create module
- `GET /Edit/{id}` - Edit module
- `POST /Edit/{id}` - Update module

## Partial Views Structure

Create these partial views (empty, JSON-ready):
- `_UserPartial.cshtml` - User summary info (Avatar, Id, Name)
- `_UserActivityPartial.cshtml` - User activity summary
- `_UserInfoPartial.cshtml` - Extended user info (modal-style)

## View Folder Structure

```
Views/
├── Auth/
│   ├── Login.cshtml (custom design, no layout)
│   ├── Register.cshtml (custom design, no layout)
│   ├── ForgotPassword.cshtml (custom design, no layout)
│   └── ResetPassword.cshtml (custom design, no layout)
├── Account/
│   ├── Profile.cshtml
│   ├── EditProfile.cshtml
│   ├── ChangePassword.cshtml
│   └── TwoFactorAuthentication.cshtml
├── AdminUser/
│   ├── Index.cshtml
│   ├── Create.cshtml
│   ├── Edit.cshtml
│   ├── Delete.cshtml
│   ├── ChangePassword.cshtml
│   ├── Roles.cshtml
│   └── Claims.cshtml
├── Role/
│   ├── Index.cshtml
│   ├── Create.cshtml
│   ├── Edit.cshtml
│   └── Claims.cshtml
├── Claim/
│   ├── Index.cshtml
│   ├── Create.cshtml
│   └── Edit.cshtml
└── Module/
    ├── Index.cshtml
    ├── Create.cshtml
    └── Edit.cshtml
```

## Entity/Repository Decision Guidelines

**Use UserManager/RoleManager when:**
- Creating users/roles
- Managing passwords
- Adding/removing claims
- Adding/removing roles
- Generating tokens (email confirmation, password reset)
- Validation logic required

**Use UserStore/RoleStore directly when:**
- Custom query operations not in UserManager
- Bulk operations
- Complex joins with other entities
- Performance-critical read operations

**Use Repository/DbContext when:**
- Querying related entities (Modules, extended user data)
- Business logic spanning multiple entities
- Custom reporting queries
- Operations not related to Identity internals

**Never mix:**
- Don't use UserManager for queries - use Repository/DbContext
- Don't use direct DbContext for password operations - use UserManager
- Don't bypass RoleManager for role creation - use RoleManager

## Extensible User Properties

User profile must support runtime-extensible properties. Use JSON column or key-value table:

**Option 1: JSON Column (recommended for EF Core 7+)**
```csharp
public class AppUser : IdentityUser<Guid>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }

    [NotMapped]
    public string FullName => $"{Name} {LastName}";

    public string? Avatar { get; set; }

    public bool IsApiUser { get; set; }

    public UserStatus UserStatus { get; set; } = UserStatus.AwaitingApproval;

    // Extensible properties
    public Dictionary<string, object> ExtendedProperties { get; set; } = new();
}
```

**Option 2: Key-Value Table**
```csharp
public class UserProperty
{
    public string Id { get; set; } = Tools.CreateGuidStr();
    public string UserId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public PropertyType Type { get; set; }  // String, Int, Bool, MultiSelect

    public AppUser User { get; set; }
}
```

## Module and Claim Entities

```csharp
public class Module
{
    public string Id { get; set; } = Tools.CreateGuidStr();
    public string Name { get; set; }
    public string? DisplayName { get; set; }
    public string Description { get; set; }

    public ICollection<ModuleClaim> Claims { get; set; }
    public ICollection<GeneratedId> GeneratedIds { get; set; }
}

public class ModuleClaim
{
    public string Id { get; set; } = Tools.CreateGuidStr();
    public string ModuleId { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }

    public Module Module { get; set; }
}
```

## Authorization Implementation

**Policy-based authorization using claims:**
```csharp
builder.Services.AddAuthorization(options => {
    options.AddPolicy("CustomerCreate", policy => {
        policy.RequireClaim("Customer", "Create");
    });
});
```

**Dynamic policy creation (for module claims):**
Create claims from modules at startup and build policies dynamically.

## References

For detailed entity schemas, see [references/entities.md](references/entities.md)

For complete workflow implementations, see [references/workflows.md](references/workflows.md)

For repository patterns and service layers, see [references/patterns.md](references/patterns.md)

For controller templates, see [assets/controllers/](assets/controllers/)

For view templates, see [assets/views/](assets/views/)
