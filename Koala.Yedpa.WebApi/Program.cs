using Hangfire;
using Koala.Yedpa.Core.Configuration;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.Service.Extentions;
using Koala.Yedpa.Service.HangfireDashboard;
using Koala.Yedpa.Service.Services;
using Koala.Yedpa.WebApi.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

namespace Koala.Yedpa.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers()
                .AddApplicationPart(typeof(KoalaApiController).Assembly);
            builder.Services.AddEndpointsApiExplorer();

            // Swagger Configuration
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Koala Yedpa Web API",
                    Version = "v1",
                    Description = "Koala Yedpa Uygulaması için RESTful API",
                    Contact = new OpenApiContact
                    {
                        Name = "Koala Yedpa Team"
                    }
                });

                // Include XML comments for Swagger documentation
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // Only include controllers from WebApi assembly, ignore WebUI
                options.DocInclusionPredicate((docName, description) =>
                {
                    if (description.ActionDescriptor is not Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controller)
                    {
                        return false;
                    }

                    // Explicitly exclude WebUI controllers
                    if (controller.ControllerTypeInfo.Assembly.GetName().Name == "Koala.Yedpa.WebUI")
                    {
                        return false;
                    }

                    // Only include WebApi controllers
                    return controller.ControllerTypeInfo.Assembly.GetName().Name == "Koala.Yedpa.WebApi";
                });
            });

            // Database
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("YedpaYonetim"),
                    x => x.UseCompatibilityLevel(150));
            });

            // Hangfire
            builder.Services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("YedpaYonetim")));

            builder.Services.AddHttpClient();
            builder.Services.AddDataProtection();

            // AutoMapper Configuration
            builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

            // Identity Configuration
            builder.Services.AddIdentity<AppUser, AppRole>(options =>
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
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(8);
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                var cookieBuilder = new CookieBuilder();
                cookieBuilder.Name = "KoalaYedpaApi";
                options.LoginPath = new PathString("/api/User/Login");
                options.LogoutPath = new PathString("/api/User/Logout");
                options.AccessDeniedPath = new PathString("/api/User/AccessDenied");
                options.Cookie = cookieBuilder;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
            });

            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromSeconds(120);
            });

            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            builder.Services.AddApplicationServices();
            builder.Services.AddApplicationRepositories();
            builder.Services.AddApplicationProviders();
            builder.Services.AddHostedService<AuthorizationRulesInitializer>();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Koala Yedpa Web API v1");
                });
            }

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
