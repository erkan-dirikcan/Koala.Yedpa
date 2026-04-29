using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Koala.Yedpa.WebUI.Controllers;

/// <summary>
/// Koala API Ayarları Yönetimi
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class KoalaApiController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<KoalaApiController> _logger;

    public KoalaApiController(ISettingsService settingsService, ILogger<KoalaApiController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Koala API Ayarlarını Getir
    /// </summary>
    /// <returns>Koala API Ayarları (BaseUrl, UserName, Password - Password şifreli olarak döner)</returns>
    [HttpGet("settings")]
    [SwaggerOperation(Summary = "Koala API Ayarlarını Getir")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseDto<KoalaApiSettingsViewModel>))]
    [SwaggerResponse(404, "Ayarlar bulunamadı")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> GetSettings()
    {
        _logger.LogInformation("GetSettings called");
        var result = await _settingsService.GetKoalaApiSettingsAsync();
        if (result.IsSuccess)
        {
            _logger.LogInformation("GetSettings: Successfully retrieved Koala API settings");
            return Ok(result);
        }
        _logger.LogWarning("GetSettings: Failed to retrieve Koala API settings, StatusCode: {StatusCode}", result.StatusCode);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Koala API Base URL'ini Getir (Şifresiz)
    /// </summary>
    /// <returns>Base URL</returns>
    [HttpGet("baseurl")]
    [SwaggerOperation(Summary = "Koala API Base URL'ini Getir")]
    [SwaggerResponse(200, "Başarılı")]
    [SwaggerResponse(404, "Ayar bulunamadı")]
    public async Task<IActionResult> GetBaseUrl()
    {
        _logger.LogInformation("GetBaseUrl called");
        var result = await _settingsService.GetKoalaApiSettingsAsync();
        if (result.IsSuccess && result.Data != null)
        {
            _logger.LogInformation("GetBaseUrl: Successfully retrieved BaseUrl");
            return Ok(new { BaseUrl = result.Data.BaseUrl });
        }
        _logger.LogWarning("GetBaseUrl: Koala API settings not found");
        return NotFound("Koala API ayarları bulunamadı");
    }
}
