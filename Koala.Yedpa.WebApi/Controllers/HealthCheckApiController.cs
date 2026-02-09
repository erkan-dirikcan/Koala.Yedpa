using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebApi.Controllers;

/// <summary>
/// API Sağlık Kontrolü - API'ye erişim testi için
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class HealthCheckApiController : ControllerBase
{
    /// <summary>
    /// API Sağlık Kontrolü - Basit GET isteği
    /// </summary>
    /// <returns>API çalışıyorsa 200 OK döner</returns>
    [HttpGet]
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
    /// Detaylı sağlık kontrolü
    /// </summary>
    /// <returns>Detaylı sistem bilgileri</returns>
    [HttpGet("detailed")]
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
}
