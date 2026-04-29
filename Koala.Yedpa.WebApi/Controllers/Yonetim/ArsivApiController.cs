using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebApi.Controllers.Yonetim;

/// <summary>
/// Arşiv Yönetimi API Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CurrentAccuant")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ArsivApiController : ControllerBase
{
    private readonly IArsivService _arsivService;
    private readonly ILogger<ArsivApiController> _logger;

    public ArsivApiController(
        IArsivService arsivService,
        ILogger<ArsivApiController> logger)
    {
        _arsivService = arsivService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm arşiv listesini getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var result = await _arsivService.GetArsivListesiAsync();
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Koli detayını getirir
    /// </summary>
    [HttpGet("{koliId}")]
    public async Task<IActionResult> GetDetail(int koliId)
    {
        var result = await _arsivService.GetKoliDetayAsync(koliId);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Yeni raf ekler
    /// </summary>
    [HttpPost("raf")]
    public async Task<IActionResult> AddRaf([FromBody] string rafKod)
    {
        var result = await _arsivService.AddRafAsync(rafKod);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Yeni bölme ekler
    /// </summary>
    [HttpPost("bolme")]
    public async Task<IActionResult> AddBolme([FromBody] AddBolmeRequest request)
    {
        var result = await _arsivService.AddBolmeAsync(request.RafId, request.BolmeNo);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Yeni koli ekler
    /// </summary>
    [HttpPost("koli")]
    public async Task<IActionResult> AddKoli([FromBody] AddKoliRequest request)
    {
        var result = await _arsivService.AddKoliAsync(request.BolmeId, request.KoliNo, request.Detay);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Koli günceller
    /// </summary>
    [HttpPut("koli/{koliId}")]
    public async Task<IActionResult> UpdateKoli(int koliId, [FromBody] UpdateKoliRequest request)
    {
        var result = await _arsivService.UpdateKoliAsync(koliId, request.KoliNo, request.Detay);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    /// <summary>
    /// Koli siler
    /// </summary>
    [HttpDelete("koli/{koliId}")]
    public async Task<IActionResult> DeleteKoli(int koliId)
    {
        var result = await _arsivService.DeleteKoliAsync(koliId);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    // Request DTO'ları
    public class AddBolmeRequest
    {
        public int RafId { get; set; }
        public string BolmeNo { get; set; } = string.Empty;
    }

    public class AddKoliRequest
    {
        public int BolmeId { get; set; }
        public string KoliNo { get; set; } = string.Empty;
        public string? Detay { get; set; }
    }

    public class UpdateKoliRequest
    {
        public string KoliNo { get; set; } = string.Empty;
        public string? Detay { get; set; }
    }
}
