using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Koala.Yedpa.WebApi.Controllers;

/// <summary>
/// Koala API Ayarları Yönetimi
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class KoalaApiController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public KoalaApiController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
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
        var result = await _settingsService.GetKoalaApiSettingsAsync();
        if (result.IsSuccess)
        {
            return Ok(result);
        }
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
        var result = await _settingsService.GetKoalaApiSettingsAsync();
        if (result.IsSuccess && result.Data != null)
        {
            return Ok(new { BaseUrl = result.Data.BaseUrl });
        }
        return NotFound("Koala API ayarları bulunamadı");
    }
}
