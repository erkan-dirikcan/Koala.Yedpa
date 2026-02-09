using Hangfire;
using Koala.Yedpa.Core.Configuration;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.Repositories.Repositories;
using Koala.Yedpa.Repositories.UnitOfWork;
using Koala.Yedpa.Service.Providers;
using Koala.Yedpa.Service.Services;
using Koala.Yedpa.Service.Services.BackgroundServices;
using Koala.Yedpa.Service.Services.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Service.Services;

namespace Koala.Yedpa.Service.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationRepositories(this IServiceCollection services)
        {
            services.AddScoped<IBudgetRatioRepository, BudgetRatioRepository>();
            services.AddScoped<IAppRoleRepository, AppRoleRepository>();
            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<IClaimsRepository, ClaimsRepository>();
            services.AddScoped<IDuesStatisticRepository, DuesStatisticRepository>();
            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
            services.AddScoped<IExtendedPropertiesRepository, ExtendedPropertiesRepository>();
            services.AddScoped<IExtendedPropertyRecordValuesRepository, ExtendedPropertyRecordValuesRepository>();
            services.AddScoped<IExtendedPropertyValuesRepository, ExtendedPropertyValuesRepository>();
            services.AddScoped<IGeneratedIdsRepository, GeneratedIdsRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ITransactionItemRepository, TransactionItemRepository>();
            services.AddScoped<ITransactionTypeRepository, TransactionTypeRepository>();
            services.AddScoped<IWorkplaceRepository, WorkplaceRepository>();
            services.AddScoped<IQRCodeRepository, QRCodeRepository>();
            services.AddScoped<IQRCodeBatchRepository, QRCodeBatchRepository>();

            // Hangfire kaldırıldı, BackgroundService kullanılıyor
            // DuesStatistic Transfer BackgroundService ve Queue (Singleton)
            services.AddSingleton<DuesStatisticTransferQueue>();
            services.AddHostedService<DuesStatisticTransferBackgroundService>();
        }

        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IBudgetRatioService, BudgetRatioService>();
            services.AddScoped<IBudgetOrderService, BudgetOrderService>();
            services.AddScoped<IApiLogoSqlDataService, ApiLogoSqlDataService>();
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<IBackgroundServices, BackgroundServices>();
            services.AddScoped<IClaimsService, ClaimsService>();
            services.AddScoped<ICryptoService, CryptoService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IDuesStatisticService, DuesStatisticService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IMessage34EmailService, Message34EmailService>();
            services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSenderAdapter>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IExtendedPropertiesService, ExtendedPropertiesService>();
            services.AddScoped<ILicenseReader, LicenseReader>();
            services.AddScoped<ILicenseValidator, LicenseValidator>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<ISeedService, SeedService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ITransactionItemService, TransactionItemService>();
            services.AddScoped<ITransactionTypeService, TransactionTypeService>();
            services.AddScoped<IWorkplaceService, WorkplaceService>();
            services.AddScoped<IQRCodeService, QRCodeService>();

            services.AddHostedService<SeedHostedService>();
        }

        public static void AddApplicationProviders(this IServiceCollection services)
        {
            services.AddScoped<IDapperProvider, DapperProvider>();
            services.AddScoped<IEmailProvider, EmailProvider>();
            services.AddScoped<ILogoRestServiceProvider, LogoRestServiceProvider>();
            services.AddScoped<IRestServiceProvider, RestServiceProvider>();
            services.AddScoped<ISqlProvider, SqlProvider>();
            services.AddScoped<IUnitOfWork<AppDbContext>, UnitOfWork<AppDbContext>>();
        }
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
