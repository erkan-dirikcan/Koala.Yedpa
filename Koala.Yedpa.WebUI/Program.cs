using Koala.Yedpa.Core.Configuration;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.Service.Services;
using Koala.Yedpa.WebUI.Authorization;
using Koala.Yedpa.WebUI.Extentions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Extensions.Logging;

namespace Koala.Yedpa.WebUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // RabbitMQ/PG gibi secret'lar user-secrets'tan gelsin — ortam Development olmasa da yükle.
            // (Sunucuda dosya yoksa optional:true no-op olur; orada env değişkeni kullanılır.)
            builder.Configuration.AddUserSecrets<Program>(optional: true);

            // NLog yapılandırması
            builder.Host.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddNLog();
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // ASP.NET Identity Cookie Authentication for WebUI
            builder.Services.AddIdentityWithExt();

            // Add Authorization
            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Bearer token'ı 'Authorize' butonuna yapıştırın. Identity Server'dan aldığınız token'ın tamamını 'Bearer ' kelimesi olmadan yazın.",

                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {jwtSecurityScheme, Array.Empty<string>()}
                });

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Sistem Koala Yedpa Api v01.00",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Email = "info@sistem-bilgi.com",
                        Name = "Sistem Bilgisayar",
                        Url = new Uri("https://www.sistem-bilgi.com")

                    },
                    Description = "Bu api Sistem Bilgisayar Tarafında Yedpa İçin İhtiyaçları Doğrultusunda Özel Olarak Hazırlanmıştır.",
                    License = new OpenApiLicense { Name = "Sistem Bilgisayar Tarafından Geliştirilmiştir", Url = new Uri("Https://sistem-bilgi.com") },

                });
                var filePath = Path.Combine(AppContext.BaseDirectory, "YedpaApi.xml");
                var filePathCore = Path.Combine(AppContext.BaseDirectory, "YedpaApiCore.xml");

                c.IncludeXmlComments(filePath);
                c.IncludeXmlComments(filePathCore);
            });




            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("YedpaYonetim"),
                    x => x.UseCompatibilityLevel(150));
            });

            // QR Code Settings
            builder.Services.Configure<QRCodeSettings>(
                builder.Configuration.GetSection(QRCodeSettings.SectionName));

            // RabbitMQ (N8N → tetik) ayarları
            builder.Services.Configure<RabbitMqSettings>(
                builder.Configuration.GetSection(RabbitMqSettings.SectionName));

            builder.Services.AddHttpClient();
            builder.Services.AddDataProtection();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMappingConfExt();
            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            builder.Services.AddApplicationServices();
            builder.Services.AddApplicationRepositories();
            builder.Services.AddApplicationProviders();
            builder.Services.AddHostedService<AuthorizationRulesInitializer>();

            // Crypto API + X-SKey header handler
            builder.Services.AddHttpClient("CryptoApi", client =>
            {
                client.BaseAddress = new Uri("https://GetDec.sistem-koala.com:44326");
            })
                .AddHttpMessageHandler<LicenseHeaderHandler>()
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
                });

            builder.Services.AddScoped<ICryptoService>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                var client = factory.CreateClient("CryptoApi");
                var configuration = sp.GetRequiredService<IConfiguration>();
                var licenseReader = sp.GetService<ILicenseReader>();
                var licenseValidator = sp.GetRequiredService<ILicenseValidator>();
                var logger = sp.GetRequiredService<ILogger<CryptoService>>();
                return new CryptoService(client, configuration, licenseReader!, licenseValidator, logger);
            });

            // Message34 Email API
            builder.Services.AddHttpClient<IMessage34EmailService, Message34EmailService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });



            // =================================================================

            var app = builder.Build();

            // LİSANS KONTROLÜ
            using (var scope = app.Services.CreateScope())
            {
                var validator = scope.ServiceProvider.GetRequiredService<ILicenseValidator>();
                if (!validator.IsLicenseValid())
                {
                    app.Logger.LogCritical("LİSANS GEÇERSİZ! Uygulama başlatılmıyor.");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("==========================================================");
                    Console.WriteLine(" LİSANS DOSYASI GEÇERSİZ VEYA EKSİK");
                    Console.WriteLine(" wwwroot/Licenses klasörünü kontrol edin");
                    Console.WriteLine("==========================================================");
                    Console.ResetColor();
                    return;
                }
            }

            // TRANSACTION TYPE ID GÜNCELLEME
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    var transactionsToUpdate = await context.Transaction
                        .Where(t => t.TransactionTypeId == "c570d72f-d9c8-11f0-9657-e848b8c82000")
                        .ToListAsync();

                    if (transactionsToUpdate.Any())
                    {
                        foreach (var transaction in transactionsToUpdate)
                        {
                            transaction.TransactionTypeId = "c570d72f-d9c8-11f0-9657-e848b8c82000";
                        }

                        await context.SaveChangesAsync();
                        logger.LogInformation($"{transactionsToUpdate.Count} transaction kaydının TransactionTypeId alanı güncellendi.");
                    }
                    else
                    {
                        logger.LogInformation("Güncellenecek transaction kaydı bulunamadı.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Transaction Type ID güncellemesi sırasında hata oluştu.");
                }
            }

            // YETKİ KATALOĞU SEED (idempotent)
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    var eklenenKayit = await PermissionSeeder.SeedModulesAndClaimsAsync(
                        context, logger, CancellationToken.None);
                    var eklenenYetki = await PermissionSeeder.GrantAllToSuperAdminAsync(roleManager, logger);

                    logger.LogInformation(
                        "Yetki kataloğu senkronize edildi. Yeni kayıt: {Kayit}, Süper Yönetici'ye eklenen yetki: {Yetki}",
                        eklenenKayit, eklenenYetki);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Yetki kataloğu seed edilirken hata oluştu");
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // ← BU SATIRI EKLE (EN ÖNEMLİ!)
            }
            else
            {
                //Development'ta da Swagger aktif olsun

            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "YEDPA API V1");
                c.RoutePrefix = "swagger";
                c.OAuthScopeSeparator(" ");
                c.DefaultModelsExpandDepth(0);
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            });
            
            
            app.UseExceptionHandler("/Dashboard/Error");
            app.UseHsts();

            //app.UseSwagger();
            //app.UseSwaggerUI();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cari API V1");
            //    c.RoutePrefix = "swagger";
            //});


            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(builder.Environment.ContentRootPath, "wwwroot\\assets\\media\\users")),
                RequestPath = "/avatars"
            });

            app.UseRouting();

            // Authentication & Authorization middleware (must be after Routing, before endpoint mapping)
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllers();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}