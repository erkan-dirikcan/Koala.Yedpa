using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Koala.Yedpa.WebUI.Authorization;

/// <summary>
/// Policy adını doğrudan bir "Permission" claim şartına çevirir.
/// Böylece her izin için elle policy tanımlamaya gerek kalmaz:
/// [Permission("ModuleManagement.Create")] → RequireClaim("Permission", "ModuleManagement.Create")
///
/// Policy adı '|' ile ayrılmış birden fazla izin içerebilir; bu durumda şart
/// VEYA olarak kurulur (RequireClaim birden fazla değerle zaten "herhangi biri"
/// anlamına gelir):
/// [Permission(A, B)] → RequireClaim("Permission", A, B)
///
/// Veritabanına gitmez. Claim'in DB'de tanımlı olup olmadığını kontrol etmek
/// güvenlik açısından gereksizdir: kullanıcıda o claim yoksa zaten reddedilir.
/// DB'de olmayan bir izin adı yazılırsa sonuç "herkes reddedilir" olur (fail-closed).
/// </summary>
public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    /// <summary>Rol/kullanıcı claim'lerinde kullanılan sabit claim tipi.</summary>
    public const string PermissionClaimType = "Permission";

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName))
        {
            return null;
        }

        // Elle AddPolicy ile tanımlanmış bir policy varsa o kazanır.
        var existing = await base.GetPolicyAsync(policyName);
        if (existing is not null)
        {
            return existing;
        }

        var izinAdlari = policyName
            .Split(PermissionAttribute.Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (izinAdlari.Length == 0)
        {
            return null;
        }

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(PermissionClaimType, izinAdlari)
            .Build();
    }
}
