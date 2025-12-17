// Service/Services/SeedService.cs

using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Service.Services
{
    public class SeedService(
        AppDbContext context,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager) : ISeedService
    {
        private readonly AppDbContext _context = context;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly RoleManager<AppRole> _roleManager = roleManager;

        public async Task SeedAsync()
        {
            await SeedRolesAsync();

            string? adminUserId = null;

            // Kullanıcı varsa ID'yi al, yoksa oluştur
            var existingUser = await _userManager.FindByEmailAsync("erkan@sistem-bilgisayar.com.tr");
            if (existingUser != null)
            {
                adminUserId = existingUser.Id;
            }
            else
            {
                await SeedUsersAsync();
                var user = await _userManager.FindByEmailAsync("erkan@sistem-bilgisayar.com.tr");
                adminUserId = user?.Id;
            }

            if (adminUserId != null)
            {
                await SeedSettingsAsync(adminUserId);
            }
        }

        private async Task SeedRolesAsync()
        {
            if (await _roleManager.Roles.AnyAsync()) return;

            var role = new AppRole
            {
                Id = Tools.CreateGuidStr(),
                Name = "SistemKoala",
                NormalizedName = "SISTEMKOALA",
                Description = "Sistem Koala Rolü",
                DisplayName = "Sistem Koala",
                StatusEnum = StatusEnum.Active
            };

            await _roleManager.CreateAsync(role);
        }

        private async Task SeedUsersAsync()
        {
            if (await _userManager.Users.AnyAsync()) return;

            var role = await _roleManager.FindByNameAsync("SistemKoala");
            if (role == null) return;

            var user = new AppUser
            {
                Id = Tools.CreateGuidStr(),
                UserName = "erkan@sistem-bilgisayar.com.tr",
                Email = "erkan@sistem-bilgisayar.com.tr",
                EmailConfirmed = true,
                PhoneNumber = "2163477889",
                PhoneNumberConfirmed = true,
                FirstName = "Sistem",
                MiddleName = "",
                LastName = "Bilgisayar",
                Status = StatusEnum.Active,
                Avatar = "SistemKoala.jpg",
                LockoutEnabled = true
            };

            var result = await _userManager.CreateAsync(user, "As26560770!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "SistemKoala");
            }
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

            _context.Settings.AddRange(settings);
            await _context.SaveChangesAsync();
        }
    }
}