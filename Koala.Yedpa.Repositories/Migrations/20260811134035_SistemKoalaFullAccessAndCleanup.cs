using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <summary>
    /// "SistemKoala" rolünü tek tam yetkili rol yapar: katalogdaki 24 izni idempotent şekilde verir.
    /// Ardından "SuperAdmin" (Süper Yönetici) rolünü kaldırır (kullanıcı-rol ilişkisi → rol claim'leri → rol kaydı).
    /// Son olarak kullanılmayan Module_Management/User_Management modüllerine ait ölü claim ve modül kayıtlarını temizler.
    /// Tüm SQL'ler NOT EXISTS/IF EXISTS ile korunduğu için migration tekrar çalıştırılsa bile hata vermez.
    /// </summary>
    public partial class SistemKoalaFullAccessAndCleanup : Migration
    {
        private const string FullAccessRoleName = "SistemKoala";
        private const string FullAccessRoleDisplayName = "Sistem Koala";

        private const string SuperAdminRoleName = "SuperAdmin";
        private const string SuperAdminNormalizedName = "SUPERADMIN";

        /// <summary>Katalogdaki tüm izin adları (PermissionCatalog.Modules → AllPermissionNames ile birebir aynı).</summary>
        private static readonly string[] TumIzinler =
        [
            "UserManagement.List",
            "UserManagement.Create",
            "UserManagement.Update",
            "UserManagement.ChangeStatus",
            "UserManagement.AssignRole",

            "RoleManagement.List",
            "RoleManagement.Create",
            "RoleManagement.Update",
            "RoleManagement.Delete",
            "RoleManagement.AssignClaim",

            "ModuleManagement.List",
            "ModuleManagement.Create",
            "ModuleManagement.Update",
            "ModuleManagement.ChangeStatus",
            "ModuleManagement.ClaimList",
            "ModuleManagement.ClaimCreate",
            "ModuleManagement.ClaimUpdate",
            "ModuleManagement.ClaimDelete",

            "Dashboard.View",

            "BulkInvoice.View",
            "BulkInvoice.Transfer",

            "BudgetOrder.View",

            "Settings.View",
            "Settings.Update"
        ];

        /// <summary>Hiçbir role atanmamış, hiçbir kod tarafından kullanılmayan ölü izin adları.</summary>
        private static readonly string[] OluIzinAdlari =
        [
            "Create_Module",
            "Update_Module",
            "Update_Claims",
            "View_List",
            "Create_User",
            "Update_User",
            "Change_Status"
        ];

        /// <summary>Ölü izinlerin altında bulunduğu, artık kullanılmayan modüller.</summary>
        private static readonly string[] OluModulAdlari =
        [
            "Module_Management",
            "User_Management"
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ADIM 1 — ÖNCE VER: "SistemKoala" rolüne katalogdaki 24 izni idempotent şekilde ver.
            // Rol Name = 'SistemKoala' VEYA DisplayName = 'Sistem Koala' ile bulunur. Rol yoksa hiçbir satır eklenmez.
            foreach (var izin in TumIzinler)
            {
                migrationBuilder.Sql($$"""
                    IF NOT EXISTS (
                        SELECT 1 FROM [AspNetRoleClaims] rc
                        INNER JOIN [AspNetRoles] r ON rc.[RoleId] = r.[Id]
                        WHERE (r.[Name] = N'{{FullAccessRoleName}}' OR r.[DisplayName] = N'{{FullAccessRoleDisplayName}}')
                          AND rc.[ClaimType] = N'Permission'
                          AND rc.[ClaimValue] = N'{{izin}}'
                    )
                    BEGIN
                        INSERT INTO [AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue])
                        SELECT r.[Id], N'Permission', N'{{izin}}'
                        FROM [AspNetRoles] r
                        WHERE (r.[Name] = N'{{FullAccessRoleName}}' OR r.[DisplayName] = N'{{FullAccessRoleDisplayName}}');
                    END
                    """);
            }

            // ADIM 2 — SONRA SİL: "SuperAdmin" (Süper Yönetici) rolünü tamamen kaldır.
            // Sıra: kullanıcı-rol ilişkisi → rol claim'leri → rol kaydı.
            migrationBuilder.Sql($$"""
                DELETE ur
                FROM [AspNetUserRoles] ur
                INNER JOIN [AspNetRoles] r ON ur.[RoleId] = r.[Id]
                WHERE r.[Name] = N'{{SuperAdminRoleName}}' OR r.[NormalizedName] = N'{{SuperAdminNormalizedName}}';
                """);

            migrationBuilder.Sql($$"""
                DELETE rc
                FROM [AspNetRoleClaims] rc
                INNER JOIN [AspNetRoles] r ON rc.[RoleId] = r.[Id]
                WHERE r.[Name] = N'{{SuperAdminRoleName}}' OR r.[NormalizedName] = N'{{SuperAdminNormalizedName}}';
                """);

            migrationBuilder.Sql($$"""
                DELETE FROM [AspNetRoles]
                WHERE [Name] = N'{{SuperAdminRoleName}}' OR [NormalizedName] = N'{{SuperAdminNormalizedName}}';
                """);

            // ADIM 3 — ÖLÜ VERİYİ TEMİZLE.
            // 3a) Hiçbir role atanmamış ölü Claims kayıtlarını sil.
            foreach (var izinAdi in OluIzinAdlari)
            {
                migrationBuilder.Sql($$"""
                    DELETE FROM [Claims] WHERE [Name] = N'{{izinAdi}}';
                    """);
            }

            // 3b) Savunmacı: aynı adlarla AspNetRoleClaims'de kalmış Permission claim'i varsa onları da sil.
            foreach (var izinAdi in OluIzinAdlari)
            {
                migrationBuilder.Sql($$"""
                    DELETE FROM [AspNetRoleClaims]
                    WHERE [ClaimType] = N'Permission' AND [ClaimValue] = N'{{izinAdi}}';
                    """);
            }

            // 3c) Altında hiç Claims/GeneratedIds/ExtendedProperties kalmamış ölü modülleri sil.
            foreach (var modulAdi in OluModulAdlari)
            {
                migrationBuilder.Sql($$"""
                    DELETE FROM [Module]
                    WHERE [Name] = N'{{modulAdi}}'
                      AND NOT EXISTS (SELECT 1 FROM [Claims] c WHERE c.[ModuleId] = [Module].[Id])
                      AND NOT EXISTS (SELECT 1 FROM [GeneratedIds] g WHERE g.[ModuleId] = [Module].[Id])
                      AND NOT EXISTS (SELECT 1 FROM [ExtendedProperties] e WHERE e.[ModuleId] = [Module].[Id]);
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Veri temizliği geri alınamaz.
        }
    }
}
