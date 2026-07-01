using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Koala.Yedpa.Core.Providers;
using System.Diagnostics;

namespace Koala.Yedpa.WebApi.Controllers;

/// <summary>
/// API Sağlık Kontrolü - API'ye erişim testi için
/// </summary>
[Route("api/[controller]")]
[ApiController]
//[ApiExplorerSettings(IgnoreApi = true)]
public class HealthCheckApiController : ControllerBase
{
    private readonly ILogoRestServiceProvider _logoRestServiceProvider;

    public HealthCheckApiController(ILogoRestServiceProvider logoRestServiceProvider)
    {
        _logoRestServiceProvider = logoRestServiceProvider;
    }
    /// <summary>
    /// API Sağlık Kontrolü - Basit GET isteği (Authentication gerektirmez)
    /// </summary>
    /// <returns>API çalışıyorsa 200 OK döner</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            message = "Koala Yedpa Web API çalışıyor!",
            timestamp = DateTime.Now,
            version = "v1.0.0"
        });
    }

    /// <summary>
    /// Detaylı sağlık kontrolü (Authentication gerektirmez)
    /// </summary>
    /// <returns>Detaylı sistem bilgileri</returns>
    [HttpGet("detailed")]
    [AllowAnonymous]
    public IActionResult GetDetailed()
    {
        return Ok(new
        {
            status = "Healthy",
            message = "Koala Yedpa Web API detaylı sağlık kontrolü",
            timestamp = DateTime.Now,
            version = "v1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            machineName = Environment.MachineName,
            osVersion = Environment.OSVersion.ToString()
        });
    }

    /// <summary>
    /// API Sağlık Kontrolü - Authentication gerektirir
    /// </summary>
    /// <returns>API çalışıyorsa 200 OK döner</returns>
    [HttpGet("token")]
    [Authorize(Policy = "CurrentAccuant")]
    public IActionResult Get_Token()
    {
        return Ok(new
        {
            status = "Healthy",
            message = "Koala Yedpa Web API çalışıyor! (Authenticated)",
            timestamp = DateTime.Now,
            version = "v1.0.0"
        });
    }

    /// <summary>
    /// Detaylı sağlık kontrolü (Authentication gerektirir)
    /// </summary>
    /// <returns>Detaylı sistem bilgileri</returns>
    [HttpGet("detailed/token")]
    [Authorize(Policy = "CurrentAccuant")]
    public IActionResult GetDetailed_Token()
    {
        return Ok(new
        {
            status = "Healthy",
            message = "Koala Yedpa Web API detaylı sağlık kontrolü",
            timestamp = DateTime.Now,
            version = "v1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            machineName = Environment.MachineName,
            osVersion = Environment.OSVersion.ToString()
        });
    }

    /// <summary>
    /// Token claim'lerini debug için listeler (sadece authenticated kullanıcı)
    /// </summary>
    [HttpGet("debug/claims")]
    [Authorize]  // Herhangi bir policy gerektirmez, sadece authentication yeterli
    public IActionResult DebugClaims()
    {
        var claims = User.Claims.Select(c => new
        {
            type = c.Type,
            value = c.Value,
            issuer = c.Issuer
        }).ToList();

        return Ok(new
        {
            isAuthenticated = User.Identity?.IsAuthenticated,
            name = User.Identity?.Name,
            authenticationType = User.Identity?.AuthenticationType,
            claimsCount = claims.Count,
            claims = claims,
            // CurrentAccuant policy kontrolü
            hasScopeClaim = claims.Any(c => c.type == "scope"),
            scopeValues = claims.Where(c => c.type == "scope").Select(c => c.value).ToList(),
            hasScpClaim = claims.Any(c => c.type == "scp"),
            scpValues = claims.Where(c => c.type == "scp").Select(c => c.value).ToList(),
        });
    }

    /// <summary>
    /// Logo REST Servisi Sağlık Kontrolü - Logo REST servisinin erişilebilirliğini kontrol eder (Authentication gerektirmez)
    /// </summary>
    /// <returns>Logo REST servisinin durum bilgisi</returns>
    [HttpGet("restservice")]
    [AllowAnonymous]
    public async Task<IActionResult> RestServiceCheck()
    {
        var stopwatch = Stopwatch.StartNew();
        bool isHealthy = false;
        string errorMessage = string.Empty;

        try
        {
            // Logo REST servisine ping at (token gerektirmez)
            var response = await _logoRestServiceProvider.PingAsync();

            stopwatch.Stop();

            if (response.IsSuccess)
            {
                isHealthy = true;
                return Ok(new
                {
                    status = "Healthy",
                    service = "Logo REST Service",
                    message = "Servis erişilebilir",
                    timestamp = DateTime.Now,
                    responseTimeMs = stopwatch.ElapsedMilliseconds,
                    details = "Success",
                    httpStatusCode = response.StatusCode
                });
            }
            else
            {
                isHealthy = false;
                errorMessage = response.Message ?? "Bilinmeyen hata";
                return Ok(new
                {
                    status = "Unhealthy",
                    service = "Logo REST Service",
                    message = "Servis erişilemez veya hata döndürdü",
                    timestamp = DateTime.Now,
                    responseTimeMs = stopwatch.ElapsedMilliseconds,
                    details = errorMessage,
                    httpStatusCode = response.StatusCode
                });
            }
        }
        catch (HttpRequestException httpEx)
        {
            stopwatch.Stop();
            isHealthy = false;
            errorMessage = $"HTTP bağlantı hatası: {httpEx.Message}";
            return Ok(new
            {
                status = "Unhealthy",
                service = "Logo REST Service",
                message = "Servis erişilemez",
                timestamp = DateTime.Now,
                responseTimeMs = stopwatch.ElapsedMilliseconds,
                details = errorMessage,
                exceptionType = "HttpRequestException"
            });
        }
        catch (TaskCanceledException taskEx) when (taskEx.CancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            isHealthy = false;
            errorMessage = $"İstek zaman aşımına uğradı (Timeout)";
            return Ok(new
            {
                status = "Unhealthy",
                service = "Logo REST Service",
                message = "Servis erişilemez veya timeout",
                timestamp = DateTime.Now,
                responseTimeMs = stopwatch.ElapsedMilliseconds,
                details = errorMessage,
                exceptionType = "Timeout"
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            isHealthy = false;
            errorMessage = $"Genel hata: {ex.Message}";
            return Ok(new
            {
                status = "Unhealthy",
                service = "Logo REST Service",
                message = "Servis erişilemez",
                timestamp = DateTime.Now,
                responseTimeMs = stopwatch.ElapsedMilliseconds,
                details = errorMessage,
                exceptionType = ex.GetType().Name
            });
        }
    }
}
