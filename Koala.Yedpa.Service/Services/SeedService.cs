// Service/Services/SeedService.cs

using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Service.Services
{
    public class SeedService(AppDbContext context) : ISeedService
    {
        private readonly AppDbContext _context = context;

        public async Task SeedAsync()
        {
            await SeedRolesAsync();

            string? adminUserId = null;

            // Kullanıcı varsa ID'yi al, yoksa oluştur
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "erkan@sistem-bilgisayar.com.tr");
            if (existingUser != null)
            {
                adminUserId = existingUser.Id;
            }
            else
            {
                await SeedUsersAsync();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "erkan@sistem-bilgisayar.com.tr");
                adminUserId = user?.Id;
            }

            if (adminUserId != null)
            {
                await SeedSettingsAsync(adminUserId);
            }
        }

        private async Task SeedRolesAsync()
        {
            if (await _context.Roles.AnyAsync()) return;

            var role = new AppRole
            {
                Id = Tools.CreateGuidStr(),
                Name = "SistemKoala",
                NormalizedName = "SISTEMKOALA",
                Description = "Sistem Koala Rolü",
                DisplayName = "Sistem Koala",
                StatusEnum = StatusEnum.Active
            };

            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUsersAsync()
        {
            if (await _context.Users.AnyAsync()) return;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SistemKoala");
            if (role == null) return;

            var user = new AppUser
            {
                Id = Tools.CreateGuidStr(),
                UserName = "erkan@sistem-bilgisayar.com.tr",
                NormalizedUserName = "ERKAN@SISTEM-BILGISAYAR.COM.TR",
                Email = "erkan@sistem-bilgisayar.com.tr",
                NormalizedEmail = "ERKAN@SISTEM-BILGISAYAR.COM.TR",
                EmailConfirmed = true,
                PhoneNumber = "2163477889",
                PhoneNumberConfirmed = true,
                FirstName = "Sistem",
                MiddleName = "",
                LastName = "Bilgisayar",
                Status = StatusEnum.Active,
                Avatar = "SistemKoala.jpg",
                LockoutEnabled = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // Password hash - Production'da proper hash kullanılmalı
            // Bu örnek için Identity'in PasswordHasher'ı kullanılmıyor
            // External IdentityServer kullanacağımız için burada şifre hashlemeye gerek yok
            user.PasswordHash = "HASH_PLACEHOLDER"; // External IdentityServer will handle auth

            await _context.Users.AddAsync(user);

            // User role relationship
            var userRole = new Microsoft.AspNetCore.Identity.IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = role.Id
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }

        private async Task SeedSettingsAsync(string adminUserId)
        {
            if (await _context.Settings.AnyAsync()) return;

            var now = DateTime.UtcNow;

            var settings = new List<Settings>
    {
        #region Varsayılan E-Posta Ayarları
        new Settings
        {
            Name = "SmtpServer",
            Description = "SMTP Sunucusu",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.EmailSettings,

            // CommonProperties
            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "UserName",
            Description = "E-Posta Kullanıcı Adı",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.EmailSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "Password",
            Description = "E-Posta Kullanıcı Parolası",
            Value = "", // Şifre boş olacak, admin dolduracak
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.EmailSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "Port",
            Description = "SMTP Sunucusu port Numarası",
            Value = "587",
            SettingValueType = SettingValueTypeEnum.Int,
            SettingType = SettingsTypeEnum.EmailSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "SenderEmail",
            Description = "Gönderici/Cevap Email Adresi",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.EmailSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "SenderName",
            Description = "Gönderen Adı",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.EmailSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "EnableSsl",
            Description = "SSL Etkinleştir",
            Value = "true",
            SettingValueType = SettingValueTypeEnum.Bool,
            SettingType = SettingsTypeEnum.EmailSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        #endregion

        #region Logo RestService Ayarları
        new Settings
        {
            Name = "Server",
            Description = "Logo Rest Service Sunucusu",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoRestServiceSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "UserName",
            Description = "Logo Rest Service Kullanıcısı Adı",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoRestServiceSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "Password",
            Description = "Logo Rest Service Kullanıcı Şifresi",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoRestServiceSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "Port",
            Description = "Logo Rest Service Port Numarası",
            Value = "8080",
            SettingValueType = SettingValueTypeEnum.Int,
            SettingType = SettingsTypeEnum.LogoRestServiceSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "Firm",
            Description = "Logo Aktif Firma Bilgisi",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoRestServiceSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "Period",
            Description = "Logo Aktif Dönem Bilgisi",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoRestServiceSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        #endregion

        #region Logo SQL Server Ayarları
        new Settings
        {
            Name = "Server",
            Description = "Logo Veri Tabanı",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoSqlSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "UserName",
            Description = "Logo Veri Tabanı Kullanıcısı Adı",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoSqlSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "Password",
            Description = "Logo Veri Tabanı Kullanıcı Şifresi",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoSqlSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "Database",
            Description = "Logo Veri Tabanı Adı",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoSqlSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        #endregion

        #region Logo Kullanıcı Ayarları
        new Settings
        {
            Name = "UserName",
            Description = "Logo Kullanıcısı Adı",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoUserSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "Password",
            Description = "Logo Kullanıcı Şifresi",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoUserSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "Firm",
            Description = "Logo Aktif Firma Bilgisi",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoUserSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "Period",
            Description = "Logo Aktif Dönem Bilgisi",
            Value = "",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.LogoUserSettings,

            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        },
        new Settings
        {
            Name = "LogoFirmSync",
            Description = "Logo gece senkronizasyonu aktif mi?",
            Value = "false",
            SettingValueType = SettingValueTypeEnum.String,
            SettingType = SettingsTypeEnum.HangfireSettings,
            CreateUserId = adminUserId,
            CreateTime = now,
            Status = StatusEnum.Active
        }
        #endregion
    };

            await _context.Settings.AddRangeAsync(settings);
            await _context.SaveChangesAsync();
        }
    }
}
