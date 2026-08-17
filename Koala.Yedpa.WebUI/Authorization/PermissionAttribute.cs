using Microsoft.AspNetCore.Authorization;

namespace Koala.Yedpa.WebUI.Authorization;

/// <summary>
/// [Permission(PermissionCatalog.ModuleManagement.Create)] şeklinde kullanılır.
/// Policy adı doğrudan izin adıdır; PermissionPolicyProvider bunu
/// RequireClaim("Permission", izinAdi) şartına çevirir.
///
/// Birden fazla izin verilirse şart VEYA (OR) olarak çalışır — kullanıcının
/// bunlardan HERHANGİ BİRİNE sahip olması yeterlidir:
/// [Permission(A, B)] → RequireClaim("Permission", A, B)
///
/// Bunu, birden fazla ekranın ortak kullandığı uçlar için kullan. Örnek:
/// aidat istatistiği uçlarını hem bütçe emri hem işyerleri ekranı çağırıyor,
/// bu yüzden iki izinden biri yeterli sayılıyor.
///
/// VE (AND) mantığı istiyorsan iki ayrı attribute kullanma — bu attribute
/// AllowMultiple=false. Bunun yerine sınıf seviyesinde bir, metot seviyesinde
/// başka bir izin koy; ASP.NET Core ikisini de arar.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class PermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Policy adı içinde birden fazla izni ayırmak için kullanılan karakter.
    /// İzin adları yalnızca harf, rakam ve nokta içerdiği için (PermissionCatalog
    /// testleri bunu doğruluyor) bu karakterin bir izin adının içinde geçmesi mümkün değil.
    /// </summary>
    public const char Separator = '|';

    public PermissionAttribute(params string[] permissions)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        if (permissions.Length == 0 || permissions.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("En az bir izin adı verilmeli ve hiçbiri boş olamaz.", nameof(permissions));
        }

        Policy = string.Join(Separator, permissions);
    }
}
