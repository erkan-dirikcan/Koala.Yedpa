using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebApi.Controllers.Yonetim;

/// <summary>
/// Sözleşme Yönetimi API Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CurrentAccuant")]
[ApiExplorerSettings(IgnoreApi = true)]
public class SozlesmeApiController : ControllerBase
{
    private readonly ISozlesmeService _sozlesmeService;
    private readonly ILogger<SozlesmeApiController> _logger;

    public SozlesmeApiController(
        ISozlesmeService sozlesmeService,
        ILogger<SozlesmeApiController> logger)
    {
        _sozlesmeService = sozlesmeService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm sözleşmeleri getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _sozlesmeService.GetAllAsync();
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Sözleşme detayını getirir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _sozlesmeService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Yaklaşan sözleşmeleri getirir
    /// </summary>
    [HttpGet("expiring/{days}")]
    public async Task<IActionResult> GetExpiring(int days)
    {
        var result = await _sozlesmeService.GetExpiringAsync(days);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Yeni sözleşme oluşturur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SozlesmeCreateDto dto)
    {
        var result = await _sozlesmeService.CreateAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Sözleşme günceller
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SozlesmeUpdateDto dto)
    {
        dto.SozlesmeID = id;
        var result = await _sozlesmeService.UpdateAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Sözleşme siler
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _sozlesmeService.DeleteAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Sözleşme PDF'ini getirir
    /// </summary>
    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var result = await _sozlesmeService.GetPdfAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return File(result.Data ?? Array.Empty<byte>(), "application/pdf", $"sozlesme_{id}.pdf");
    }

    /// <summary>
    /// Sözleşme durumunu günceller
    /// </summary>
    [HttpPut("{id}/durum")]
    public async Task<IActionResult> UpdateDurum(int id, [FromBody] SozlesmeDurumUpdateDto dto)
    {
        dto.SozlesmeID = id;
        var result = await _sozlesmeService.UpdateDurumAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
}
