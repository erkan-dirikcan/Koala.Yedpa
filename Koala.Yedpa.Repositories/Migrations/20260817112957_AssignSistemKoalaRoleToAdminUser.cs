using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <summary>
    /// Hedef yönetici kullanıcıyı ("erkan@sistem-bilgisayar.com.tr") "SistemKoala" rolüne idempotent şekilde atar.
    ///
    /// Neden gerekli: 20260811134035_SistemKoalaFullAccessAndCleanup migration'ı "SuperAdmin" rolünü silerken
    /// AspNetUserRoles'taki kullanıcı-rol ilişki satırlarını da (FK ile birlikte) sildi, ancak kullanıcıyı yerine
    /// "SistemKoala" rolüne atayan bir INSERT eklemedi. Aynı zamanda Koala.Yedpa.Service/Services/SeedService.cs
    /// kullanıcı zaten mevcutsa SeedUsersAsync() içindeki rol atama adımını atlıyor, bu yüzden rol-kullanıcı bağı
    /// hiçbir kod yolunda otomatik kurulmuyor. Sonuç: kullanıcının hiçbir rolü kalmadı ve uygulamaya kilitlendi.
    /// Bu migration, 20260811084906_AddSuperAdminRole.cs'deki aynı INSERT deseniyle bu bağı yeniden kurar.
    /// Tüm SQL NOT EXISTS ile korunduğu için migration tekrar çalıştırılsa bile hata vermez; kullanıcı veya rol
    /// bulunamazsa hiçbir satır eklenmez/hata oluşmaz.
    /// </summary>
    public partial class AssignSistemKoalaRoleToAdminUser : Migration
    {
        private const string RoleName = "SistemKoala";
        private const string RoleDisplayName = "Sistem Koala";
        private const string TargetUserEmail = "erkan@sistem-bilgisayar.com.tr";
        private const string TargetUserNormalizedEmail = "ERKAN@SISTEM-BILGISAYAR.COM.TR";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hedef kullanıcıyı "SistemKoala" rolüne ata (User+Role ikilisi zaten varsa ekleme).
            // Kullanıcı veya rol yoksa INSERT hiç satır eklemez; idempotenttir.
            migrationBuilder.Sql($$"""
                INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
                SELECT u.[Id], r.[Id]
                FROM [AspNetUsers] u
                CROSS JOIN [AspNetRoles] r
                WHERE (u.[Email] = N'{{TargetUserEmail}}'
                        OR u.[NormalizedEmail] = N'{{TargetUserNormalizedEmail}}')
                  AND (r.[Name] = N'{{RoleName}}' OR r.[DisplayName] = N'{{RoleDisplayName}}')
                  AND NOT EXISTS (
                      SELECT 1 FROM [AspNetUserRoles] ur
                      WHERE ur.[UserId] = u.[Id] AND ur.[RoleId] = r.[Id]
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Bu migration'ın eklediği tek kullanıcı-rol ilişki satırını kaldır. Veri kaybı değil,
            // yalnızca bu migration'ın kurduğu ilişkiyi geri alır; güvenle Down edilebilir.
            migrationBuilder.Sql($$"""
                DELETE ur
                FROM [AspNetUserRoles] ur
                INNER JOIN [AspNetUsers] u ON ur.[UserId] = u.[Id]
                INNER JOIN [AspNetRoles] r ON ur.[RoleId] = r.[Id]
                WHERE (u.[Email] = N'{{TargetUserEmail}}'
                        OR u.[NormalizedEmail] = N'{{TargetUserNormalizedEmail}}')
                  AND (r.[Name] = N'{{RoleName}}' OR r.[DisplayName] = N'{{RoleDisplayName}}');
                """);
        }
    }
}
