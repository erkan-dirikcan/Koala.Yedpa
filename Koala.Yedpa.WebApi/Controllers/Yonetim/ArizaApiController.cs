using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebApi.Controllers.Yonetim;

/// <summary>
/// Arıza Yönetimi API Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CurrentAccuant")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ArizaApiController : ControllerBase
{
    private readonly IArizaService _arizaService;
    private readonly ILogger<ArizaApiController> _logger;

    public ArizaApiController(
        IArizaService arizaService,
        ILogger<ArizaApiController> logger)
    {
        _arizaService = arizaService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm arızaları getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _arizaService.GetAllAsync();
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Arıza detayını getirir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _arizaService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Birime göre arızaları getirir
    /// </summary>
    [HttpGet("birim/{birim}")]
    public async Task<IActionResult> GetByBirim(string birim)
    {
        var result = await _arizaService.GetByBirimAsync(birim);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Aktif arızaları getirir
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _arizaService.GetActiveFaultsAsync();
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Yeni arıza oluşturur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ArizaCreateDto dto)
    {
        var result = await _arizaService.CreateAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Arıza durumunu günceller
    /// </summary>
    [HttpPut("{id}/durum")]
    public async Task<IActionResult> UpdateDurum(int id, [FromBody] ArizaDurumUpdateDto dto)
    {
        dto.ArizaID = id;
        var result = await _arizaService.UpdateDurumAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Arıza hareketi ekler
    /// </summary>
    [HttpPost("{id}/hareket")]
    public async Task<IActionResult> AddHareket(int id, [FromBody] ArizaHareketEkleDto dto)
    {
        dto.ArizaID = id;
        var result = await _arizaService.AddHareketAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Arıza siler
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _arizaService.DeleteAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
}
