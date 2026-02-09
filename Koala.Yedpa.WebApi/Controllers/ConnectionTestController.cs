using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Koala.Yedpa.WebApi.Controllers;

/// <summary>
/// Bağlantı Testi
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ConnectionTestController : ControllerBase
{
    /// <summary>
    /// Bağlantı testi - API'ye erişim kontrolü
    /// </summary>
    /// <returns>Başarılı cevap</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Bağlantı Testi",
        Description = "Müşteri bağlantı kurup kuramadığını test etmek için kullanılır"
    )]
    [SwaggerResponse(200, "Bağlantı başarılı", typeof(object))]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "OK",
            message = "Bağlantı başarılı",
            timestamp = DateTime.UtcNow,
            server = "Koala Yedpa Web API"
        });
    }

    /// <summary>
    /// Bağlantı testi - Detaylı bilgi
    /// </summary>
    /// <returns>Detaylı sistem bilgisi</returns>
    [HttpGet("details")]
    [SwaggerOperation(
        Summary = "Detaylı Bağlantı Testi",
        Description = "Sistem durumunu ve bağlantı bilgilerini döndürür"
    )]
    [SwaggerResponse(200, "Bağlantı başarılı", typeof(object))]
    public IActionResult GetDetails()
    {
        return Ok(new
        {
            status = "OK",
            message = "Bağlantı başarılı - Detaylı mod",
            timestamp = DateTime.UtcNow,
            server = "Koala Yedpa Web API",
            version = "v1.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }
}
