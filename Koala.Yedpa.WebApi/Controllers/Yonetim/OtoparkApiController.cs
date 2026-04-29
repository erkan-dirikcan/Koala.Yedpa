using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebApi.Controllers.Yonetim;

/// <summary>
/// Otopark Yönetimi API Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CurrentAccuant")]
[ApiExplorerSettings(IgnoreApi = true)]
public class OtoparkApiController : ControllerBase
{
    private readonly IOtoparkService _otoparkService;
    private readonly ILogger<OtoparkApiController> _logger;

    public OtoparkApiController(
        IOtoparkService otoparkService,
        ILogger<OtoparkApiController> logger)
    {
        _otoparkService = otoparkService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm otopark kayıtlarını getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _otoparkService.GetAllAsync();
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Aktif otopark kayıtlarını getirir
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _otoparkService.GetActiveAsync();
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Plakaya göre kayıt getirir
    /// </summary>
    [HttpGet("plaka/{plaka}")]
    public async Task<IActionResult> GetByPlaka(string plaka)
    {
        var result = await _otoparkService.GetByPlakaAsync(plaka);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Giriş yapar
    /// </summary>
    [HttpPost("giris")]
    public async Task<IActionResult> Giris([FromBody] OtoparkGirisDto dto)
    {
        var result = await _otoparkService.GirisYapAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Çıkış yapar
    /// </summary>
    [HttpPost("cikis")]
    public async Task<IActionResult> Cikis([FromBody] OtoparkCikisDto dto)
    {
        var result = await _otoparkService.CikisYapAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Abonelik ekler
    /// </summary>
    [HttpPost("abone")]
    public async Task<IActionResult> AboneEkle([FromBody] OtoparkAboneDto dto)
    {
        var result = await _otoparkService.AboneEkleAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Aboneliği günceller
    /// </summary>
    [HttpPut("abone")]
    public async Task<IActionResult> AboneGuncelle([FromBody] OtoparkAboneDto dto)
    {
        var result = await _otoparkService.AboneGuncelleAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Aboneliği siler
    /// </summary>
    [HttpDelete("abone/{id}")]
    public async Task<IActionResult> AboneSil(int id)
    {
        var result = await _otoparkService.AboneSilAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
}
