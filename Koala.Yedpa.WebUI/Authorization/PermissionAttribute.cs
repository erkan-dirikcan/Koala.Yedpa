using Microsoft.AspNetCore.Authorization;

namespace Koala.Yedpa.WebUI.Authorization;

/// <summary>
/// [Permission(PermissionCatalog.ModuleManagement.Create)] şeklinde kullanılır.
/// Policy adı doğrudan izin adıdır; PermissionPolicyProvider bunu
/// RequireClaim("Permission", izinAdi) şartına çevirir.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class PermissionAttribute : AuthorizeAttribute
{
    public PermissionAttribute(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        Policy = permission;
    }
}
