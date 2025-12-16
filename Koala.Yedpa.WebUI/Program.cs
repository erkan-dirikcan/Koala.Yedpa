using Hangfire;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.Service.HangfireDashboard;
using Koala.Yedpa.Service.Services;
using Koala.Yedpa.WebUI.Extentions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
namespace Koala.Yedpa.WebUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
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
                    Description = "Sadece Identity Serverdan aldığınız token bilgisini Bearer Kullanmadan yapıştırın",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
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

                c.IncludeXmlComments(filePath);
            });




            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("YedpaYonetim"),
                    x => x.UseCompatibilityLevel(150));
            });

            builder.Services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("YedpaYonetim")));

            builder.Services.AddHttpClient();
            builder.Services.AddDataProtection();
            builder.Services.AddMappingConfExt();
            builder.Services.AddIdentityConfExt(builder.Configuration);
            builder.Services.AddIdentityWithExt();
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
                var licenseReader = sp.GetService<ILicenseReader>();
                return new CryptoService(client, licenseReader!);
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
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseKoalaHangfireDashboard();
            app.MapStaticAssets();
            //app.MapControllers();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}