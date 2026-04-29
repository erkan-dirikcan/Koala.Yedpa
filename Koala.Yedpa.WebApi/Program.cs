using Hangfire;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.Service.Extentions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;

namespace Koala.Yedpa.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // NLog yapılandırması
            builder.Host.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddNLog();
            });

            // Add services to the container
            builder.Services.AddControllers();
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
                    Description = "Sistem Bilgisayar Tarafından Yedpa Websitesi tarafından gerekli verilerin Logo Tiger Uygulamasına Aktarmak, ve Okumak İçin Geliştirilmiştir ",
                    License = new OpenApiLicense { Name = "Sistem Bilgisayar Tarafından Geliştirilmiştir", Url = new Uri("Https://sistem-bilgi.com") },

                });
                var filePath = Path.Combine(AppContext.BaseDirectory, "YedpaApiCore.xml");

                c.IncludeXmlComments(filePath);
            });

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
       {
           opts.Authority = "https://identity.sistem-koala.com:44982/";
           opts.Audience = "Rs-19001";
           opts.RequireHttpsMetadata = false;
       });

            builder.Services.AddAuthorization(opts =>
            {
                opts.AddPolicy("CurrentAccuant", policy => { policy.RequireClaim("scope", "sc-190101"); });
                opts.AddPolicy("Sistem", policy => { policy.RequireClaim("scope", "sc-030100"); });
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
            builder.Services.AddHttpContextAccessor();

            // AutoMapper Configuration
            builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

            // JWT Bearer Authentication for External IdentityServer
            //var identityServerAuthority = builder.Configuration["IdentityServer:Authority"] ?? "https://identity.sistem-koala.com:44982";
            //var apiName = builder.Configuration["IdentityServer:ApiName"] ?? "Rs-19001";
            //var requireHttpsMetadata = bool.Parse(builder.Configuration["IdentityServer:RequireHttpsMetadata"] ?? "false");

            //builder.Services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            //{
            //    options.Authority = identityServerAuthority;
            //    options.RequireHttpsMetadata = requireHttpsMetadata;
            //    options.Audience = apiName;

            //    // Token validation parameters
            //    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            //    {
            //        ValidateAudience = true,
            //        ValidAudience = apiName,
            //        ValidateIssuer = true,
            //        ValidIssuers = new[] { identityServerAuthority },
            //        ValidateIssuerSigningKey = true,
            //        ValidateLifetime = true,
            //        ClockSkew = TimeSpan.FromSeconds(30)
            //    };

            //    // Events for debugging
            //    options.Events = new JwtBearerEvents
            //    {
            //        OnAuthenticationFailed = context =>
            //        {
            //            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            //            logger.LogError($"Authentication failed: {context.Exception.Message}");
            //            return Task.CompletedTask;
            //        },
            //        OnTokenValidated = context =>
            //        {
            //            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            //            logger.LogInformation($"Token validated for: {context.Principal?.Identity?.Name}");
            //            return Task.CompletedTask;
            //        },
            //        OnChallenge = context =>
            //        {
            //            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            //            logger.LogWarning($"Authorization challenge: {context.Error}, {context.ErrorDescription}");
            //            return Task.CompletedTask;
            //        }
            //    };
            //});

            //// Add Authorization
            //builder.Services.AddAuthorization(options =>
            //{
            //    // Default policy - require authenticated user
            //    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            //        .RequireAuthenticatedUser()
            //        .Build();
            //});

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
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Koala Yedpa Web API v1");
                c.OAuthScopeSeparator(" ");

                // Bearer Token Configuration
                c.DefaultModelsExpandDepth(0);
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            });

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            // Authentication & Authorization middleware order is important
            app.UseAuthentication();
            app.UseAuthorization();

            // Redirect root to Swagger
            app.MapGet("/", context =>
            {
                context.Response.Redirect("/swagger/index.html", permanent: false);
                return Task.CompletedTask;
            });

            app.MapControllers();

            app.Run();
        }
    }
}
