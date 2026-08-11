using System.Security.Claims;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.WebUI.Authorization;

/// <summary>
/// PermissionCatalog'u veritabanına yansıtır. Idempotent:
/// var olan modül/claim kayıtlarına dokunmaz, sadece eksikleri ekler.
/// Kullanıcının ekrandan girdiği DisplayName/Description değerleri korunur.
/// </summary>
public static class PermissionSeeder
{
    public const string FullAccessRoleName = "SistemKoala";

    public const string FullAccessRoleDisplayName = "Sistem Koala";

    /// <summary>Eksik modül ve claim kayıtlarını ekler. Eklenen toplam kayıt sayısını döner.</summary>
    public static async Task<int> SeedModulesAndClaimsAsync(
        AppDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var eklenen = 0;

        var mevcutModuller = await context.Module.ToListAsync(cancellationToken);
        var mevcutClaimAdlari = await context.Claims
            .Where(c => c.Name != null)
            .Select(c => c.Name!)
            .ToListAsync(cancellationToken);
        var claimAdSeti = new HashSet<string>(mevcutClaimAdlari, StringComparer.Ordinal);

        foreach (var katalogModul in PermissionCatalog.Modules)
        {
            var modul = mevcutModuller.FirstOrDefault(m =>
                string.Equals(m.Name, katalogModul.Name, StringComparison.Ordinal));

            if (modul is null)
            {
                modul = new Module
                {
                    Id = Tools.CreateGuidStr(),
                    Name = katalogModul.Name,
                    DisplayName = katalogModul.DisplayName,
                    Description = katalogModul.Description,
                    Status = StatusEnum.Active
                };
                context.Module.Add(modul);
                eklenen++;
                logger.LogInformation("PermissionSeeder: '{ModulAdi}' modülü eklendi", katalogModul.Name);
            }

            foreach (var izin in katalogModul.Permissions)
            {
                if (claimAdSeti.Contains(izin.Name))
                {
                    continue;
                }

                context.Claims.Add(new Claims
                {
                    Id = Tools.CreateGuidStr(),
                    ModuleId = modul.Id,
                    Name = izin.Name,
                    DisplayName = izin.DisplayName,
                    Description = izin.Description
                });
                claimAdSeti.Add(izin.Name);
                eklenen++;
                logger.LogInformation("PermissionSeeder: '{IzinAdi}' yetkisi eklendi", izin.Name);
            }
        }

        if (eklenen > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return eklenen;
    }

    /// <summary>
    /// "SistemKoala" (tam yetkili) rolüne katalogdaki tüm izinleri verir. Rol yoksa uyarı loglar ve 0 döner.
    /// Bu rolün yeni eklenen izinleri otomatik alması içindir.
    /// </summary>
    public static async Task<int> GrantAllToFullAccessRoleAsync(
        RoleManager<AppRole> roleManager,
        ILogger logger)
    {
        // Bilinçli olarak senkron: Moq ile mock'lanan RoleManager.Roles düz bir IQueryable döner,
        // EF'in IAsyncQueryProvider'ı yoktur ve FirstOrDefaultAsync InvalidOperationException atar.
        // Açılışta bir kez, küçük AspNetRoles tablosuna giden bir sorgu — senkron olması sorun değil.
        var rol = roleManager.Roles.FirstOrDefault(r =>
            r.Name == FullAccessRoleName || r.DisplayName == FullAccessRoleDisplayName);

        if (rol is null)
        {
            logger.LogWarning(
                "PermissionSeeder: '{RolAdi}' rolü bulunamadı, otomatik yetkilendirme atlandı",
                FullAccessRoleName);
            return 0;
        }

        var mevcutClaimler = await roleManager.GetClaimsAsync(rol);
        var mevcutIzinler = new HashSet<string>(
            mevcutClaimler
                .Where(c => c.Type == PermissionPolicyProvider.PermissionClaimType)
                .Select(c => c.Value),
            StringComparer.Ordinal);

        var eklenen = 0;
        foreach (var izinAdi in PermissionCatalog.AllPermissionNames)
        {
            if (mevcutIzinler.Contains(izinAdi))
            {
                continue;
            }

            var sonuc = await roleManager.AddClaimAsync(
                rol, new Claim(PermissionPolicyProvider.PermissionClaimType, izinAdi));

            if (sonuc.Succeeded)
            {
                eklenen++;
            }
            else
            {
                logger.LogError("PermissionSeeder: '{IzinAdi}' {RolAdi} rolüne eklenemedi: {Hatalar}",
                    izinAdi, FullAccessRoleName, string.Join(", ", sonuc.Errors.Select(e => e.Description)));
            }
        }

        if (eklenen > 0)
        {
            logger.LogInformation(
                "PermissionSeeder: {RolAdi} rolüne {Sayi} yeni yetki eklendi", FullAccessRoleName, eklenen);
        }

        return eklenen;
    }
}
