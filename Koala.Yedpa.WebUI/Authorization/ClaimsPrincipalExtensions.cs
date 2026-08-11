using System.Security.Claims;

namespace Koala.Yedpa.WebUI.Authorization;

/// <summary>
/// View'larda ve controller'larda izin kontrolü için.
/// Örnek: @if (User.HasPermission(PermissionCatalog.ModuleManagement.List)) { ... }
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal? user, string permission)
    {
        if (user?.Identity is null || !user.Identity.IsAuthenticated)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        return user.HasClaim(PermissionPolicyProvider.PermissionClaimType, permission);
    }

    /// <summary>Verilen izinlerden en az biri varsa true. Menü başlıklarını gizlemek için.</summary>
    public static bool HasAnyPermission(this ClaimsPrincipal? user, params string[] permissions)
    {
        if (permissions is null || permissions.Length == 0)
        {
            return false;
        }

        return permissions.Any(p => user.HasPermission(p));
    }
}
