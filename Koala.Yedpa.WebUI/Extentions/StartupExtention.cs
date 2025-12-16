using Hangfire;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.Repositories.Repositories;
using Koala.Yedpa.Repositories.UnitOfWork;
using Koala.Yedpa.Service.Jobs;
using Koala.Yedpa.Service.Providers;
using Koala.Yedpa.Service.Services;
using Koala.Yedpa.Service.Services.Jobs;
using Koala.Yedpa.WebUI.Localizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Service.Services;
using System.Reflection;
using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.WebUI.Extentions
{
    public static class StartupExtention
    {
        public static void AddIdentityConfExt(this IServiceCollection services, ConfigurationManager configuration)
        {
            //services.Configure<EmailSettingListViewModel>(configuration.GetSection("EmailSettings"));

        }
        public static void AddIdentityWithExt(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, AppRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;

                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredUniqueChars = 3;
                    options.Password.RequiredLength = 8;

                    options.Lockout.MaxFailedAccessAttempts = 3;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(2);
                }).AddEntityFrameworkStores<AppDbContext>()
                //.AddUserValidator<>()
                .AddErrorDescriber<LocalizationIdentityErrorDescriber>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(8);
            });

            services.ConfigureApplicationCookie(options =>
            {
                var cookieBuilder = new CookieBuilder();
                cookieBuilder.Name = "KoalaYedpa";
                options.LoginPath = new PathString("/User/Login");
                options.LogoutPath = new PathString("/User/Logout");
                options.AccessDeniedPath = new PathString("/User/AccessDenied");
                options.Cookie = cookieBuilder;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;

            });
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromSeconds(120);

            });

        }


        public static void AddApplicationRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAppRoleRepository, AppRoleRepository>();
            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<IClaimsRepository, ClaimsRepository>();
            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
            services.AddScoped<IExtendedPropertiesRepository, ExtendedPropertiesRepository>();
            services.AddScoped<IExtendedPropertyRecordValuesRepository, ExtendedPropertyRecordValuesRepository>();
            services.AddScoped<IExtendedPropertyValuesRepository, ExtendedPropertyValuesRepository>();
            services.AddScoped<IGeneratedIdsRepository, GeneratedIdsRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            services.AddHangfireServer();


        }
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IApiLogoSqlDataService, ApiLogoSqlDataService>();
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<IBackgroundServices, BackgroundServices>();
            services.AddScoped<IClaimsService, ClaimsService>();
            services.AddScoped<ICryptoService, CryptoService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IExtendedPropertiesService, ExtendedPropertiesService>();
            services.AddScoped<ILicenseReader, LicenseReader>();
            services.AddScoped<ILicenseValidator, LicenseValidator>();
            services.AddScoped<ILogoSyncJobService, LogoSyncJobService>();
            services.AddScoped<ILogoSyncService, LogoSyncService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<ISeedService, SeedService>();
            services.AddScoped<ISettingsService, SettingsService>();


            services.AddHostedService<SeedHostedService>();
            services.AddHostedService<LogoSyncJobConfiguration>();
            
            
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            services.AddTransient<LicenseHeaderHandler>();
        }

        public static void AddApplicationProviders(this IServiceCollection services)
        {
            services.AddScoped<IRestServiceProvider, RestServiceProvider>();
            services.AddScoped<ISqlProvider, SqlProvider>();
            services.AddScoped<IEmailProvider, EmailProvider>();
            services.AddScoped<IUnitOfWork<AppDbContext>, UnitOfWork<AppDbContext>>();
            



            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
            //services.AddScoped<, >();
        }
        //public static void AddAuthorizationRules(this IServiceCollection services, AppDbContext context)
        //{
        //    var claims = context.Claims.ToList();


        //    services.AddAuthorization(options =>
        //    {
        //        foreach (var claim in claims)
        //        {
        //            options.AddPolicy(claim.Name, policy =>
        //            {
        //                policy.RequireClaim("Permission", claim.Name);

        //            });
        //        }
        //    });
        //}
    }

    public class AuthorizationRulesInitializer(IServiceProvider serviceProvider) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var claims = context.Claims.ToList();

                var authService = scope.ServiceProvider.GetRequiredService<IAuthorizationPolicyProvider>();
                foreach (var claim in claims)
                {
                    // Politikaları dinamik olarak eklemek için farklı bir yaklaşım gerekebilir
                    // Bu örnekte, AuthorizationOptions doğrudan değiştirilemez
                    // Bunun yerine, özel bir IAuthorizationPolicyProvider kullanılabilir
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
    public static class AuthorizationRulesExtensions
    {
        public static IServiceCollection AddAuthorizationRules(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();
            return services;
        }
    }
    public class DynamicAuthorizationPolicyProvider(
        IOptions<AuthorizationOptions> options,
        IServiceProvider serviceProvider)
        : DefaultAuthorizationPolicyProvider(options)
    {
        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            var policy = await base.GetPolicyAsync(policyName);
            if (policy != null)
            {
                return policy;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var claim = context.Claims.FirstOrDefault(c => c.Name == policyName);
                if (claim != null)
                {
                    return new AuthorizationPolicyBuilder()
                        .RequireClaim("Permission", claim.Name)
                        .Build();
                }
            }

            return null;
        }
    }
}
