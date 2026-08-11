using Koala.Yedpa.Core.Configuration;
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
using Koala.Yedpa.WebUI.Localizations;
using Microsoft.AspNetCore.Identity;
using Service.Services;
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
            services.AddScoped<IBudgetRatioRepository, BudgetRatioRepository>();
            services.AddScoped<IAppRoleRepository, AppRoleRepository>();
            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<IClaimsRepository, ClaimsRepository>();
            services.AddScoped<IDuesStatisticRepository,DuesStatisticRepository >();
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
            //services.AddScoped<, >();
            //services.AddScoped<, >();

            // DuesStatistic Transfer BackgroundService ve Queue (Singleton)
            services.AddSingleton<DuesStatisticTransferQueue>();
            services.AddHostedService<DuesStatisticTransferBackgroundService>();


        }
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IBudgetRatioService, BudgetRatioService>();
            services.AddScoped<IBudgetOrderService, BudgetOrderService>();
            // Toplu Faturalandırma (Bulk Invoice) — bu metot WebUI Program.cs'in ÇAĞIRDIĞI metottur.
            // Service projesindeki ServiceCollectionExtensions.AddApplicationServices WebUI tarafından kullanılmaz.
            services.AddScoped<IBulkInvoiceService, BulkInvoiceService>();
            services.AddScoped<IBulkInvoiceTransferService, BulkInvoiceTransferService>();
            services.AddScoped<IBulkInvoiceExcelService, BulkInvoiceExcelService>();
            services.AddScoped<IBulkInvoiceEmailService, BulkInvoiceEmailService>();
            services.AddScoped<BulkInvoiceJobs>();
            services.AddScoped<IScheduleStore, PgScheduleStore>();          // tarih → Coolify PostgreSQL (N8N okur)
            services.AddHostedService<BulkInvoiceTriggerConsumer>();        // N8N → RabbitMQ → uygulama tetik dinleyici
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
            services.AddScoped<IDapperProvider, DapperProvider>();
            services.AddScoped<IEmailProvider, EmailProvider>();
            services.AddScoped<ILogoRestServiceProvider, LogoRestServiceProvider>();
            services.AddScoped<IRestServiceProvider, RestServiceProvider>();
            services.AddScoped<ISqlProvider, SqlProvider>();
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
    }
}
