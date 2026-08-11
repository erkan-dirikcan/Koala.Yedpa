using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <summary>
    /// "Süper Yönetici" (SuperAdmin) rolünü oluşturur, hedef kullanıcıya atar ve
    /// katalogdaki tüm yetkileri (Permission claim'leri) role idempotent şekilde verir.
    /// Tüm SQL'ler NOT EXISTS ile korunduğu için migration tekrar çalıştırılsa bile hata vermez.
    /// </summary>
    public partial class AddSuperAdminRole : Migration
    {
        private const string RoleName = "SuperAdmin";

        /// <summary>Katalogdaki tüm izin adları (PermissionCatalog.Modules → AllPermissionNames ile birebir aynı).</summary>
        private static readonly string[] Tümİzinler =
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

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) "Süper Yönetici" rolünü oluştur (yoksa). PermissionSeeder DisplayName "Süper Yönetici" veya Name ile aradığı için
            //    hem Name/NormalizedName hem de DisplayName üzerinden kontrol ediyoruz.
            migrationBuilder.Sql($$"""
                IF NOT EXISTS (
                    SELECT 1 FROM [AspNetRoles]
                    WHERE [Name] = N'{{RoleName}}' OR [NormalizedName] = N'SUPERADMIN' OR [DisplayName] = N'Süper Yönetici'
                )
                BEGIN
                    INSERT INTO [AspNetRoles]
                        ([Id], [Name], [NormalizedName], [DisplayName], [Description], [StatusEnum], [ConcurrencyStamp])
                    VALUES
                        (NEWID(), N'{{RoleName}}', N'SUPERADMIN', N'Süper Yönetici', N'Tüm yetkilere sahip süper yönetici rolü', 0, NEWID());
                END
                """);

            // 2) Hedef kullanıcıyı role ata (User+Role ikilisi zaten varsa ekleme).
            //    Kullanıcı veya rol yoksa INSERT hiç satır eklemez; idempotenttir.
            migrationBuilder.Sql($$"""
                INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
                SELECT u.[Id], r.[Id]
                FROM [AspNetUsers] u
                CROSS JOIN [AspNetRoles] r
                WHERE (u.[Email] = N'erkan@sistem-bilgisayar.com.tr'
                        OR u.[NormalizedEmail] = N'ERKAN@SISTEM-BILGISAYAR.COM.TR')
                  AND r.[Name] = N'{{RoleName}}'
                  AND NOT EXISTS (
                      SELECT 1 FROM [AspNetUserRoles] ur
                      WHERE ur.[UserId] = u.[Id] AND ur.[RoleId] = r.[Id]
                  );
                """);

            // 3) Role tüm katalog yetkilerini Permission claim'i olarak ata. Her claim ayrı NOT EXISTS ile korunur.
            foreach (var izin in Tümİzinler)
            {
                migrationBuilder.Sql($$"""
                    IF NOT EXISTS (
                        SELECT 1 FROM [AspNetRoleClaims] rc
                        INNER JOIN [AspNetRoles] r ON rc.[RoleId] = r.[Id]
                        WHERE r.[Name] = N'{{RoleName}}'
                          AND rc.[ClaimType] = N'Permission'
                          AND rc.[ClaimValue] = N'{{izin}}'
                    )
                    BEGIN
                        INSERT INTO [AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue])
                        SELECT r.[Id], N'Permission', N'{{izin}}'
                        FROM [AspNetRoles] r
                        WHERE r.[Name] = N'{{RoleName}}';
                    END
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // SuperAdmin rolüne ait Permission claim'lerini kaldır
            migrationBuilder.Sql($$"""
                DELETE rc
                FROM [AspNetRoleClaims] rc
                INNER JOIN [AspNetRoles] r ON rc.[RoleId] = r.[Id]
                WHERE r.[Name] = N'{{RoleName}}' AND rc.[ClaimType] = N'Permission';
                """);

            // Kullanıcı-rol ilişkisini kaldır
            migrationBuilder.Sql($$"""
                DELETE ur
                FROM [AspNetUserRoles] ur
                INNER JOIN [AspNetRoles] r ON ur.[RoleId] = r.[Id]
                WHERE r.[Name] = N'{{RoleName}}';
                """);

            // Rolü kaldır
            migrationBuilder.Sql($$"""
                DELETE FROM [AspNetRoles] WHERE [Name] = N'{{RoleName}}';
                """);
        }
    }
}
