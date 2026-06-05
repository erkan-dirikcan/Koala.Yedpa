using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers;

/// <summary>
/// Dashboard API Controller
/// </summary>
[Route("api/Dashboard")]
[ApiController]
[Authorize]
public class DashboardApiController : ControllerBase
{
    private readonly IApiLogoSqlDataService _logoService;
    private readonly ILogger<DashboardApiController> _logger;

    public DashboardApiController(IApiLogoSqlDataService logoService, ILogger<DashboardApiController> logger)
    {
        _logoService = logoService;
        _logger = logger;
    }

    /// <summary>
    /// Aidat tahsilat KPI verilerini getirir
    /// </summary>
    /// <param name="year">Yıl (opsiyonel, varsayılan: mevcut yıl)</param>
    /// <param name="month">Ay (opsiyonel, varsayılan: mevcut ay)</param>
    /// <returns>Aidat tahsilat KPI verileri (ToplamAlacak, Odenen, Bekleyen, Ay, Yil)</returns>
    [HttpGet("aidat-tahsilat")]
    public async Task<IActionResult> GetAidatTahsilatKpi([FromQuery] int? year, [FromQuery] int? month)
    {
        _logger.LogInformation("GetAidatTahsilatKpi called with Year={Year}, Month={Month}", year, month);

        // Varsayılan değerler: mevcut yıl ve ay
        var currentYear = DateTime.Now.Year;
        var currentMonth = DateTime.Now.Month;

        var targetYear = year ?? currentYear;
        var targetMonth = month ?? currentMonth;

        // Ay doğrulama
        if (targetMonth < 1 || targetMonth > 12)
        {
            _logger.LogWarning("GetAidatTahsilatKpi: Invalid month={Month}", targetMonth);
            return BadRequest(ResponseDto<AidatTahsilatKpiDto>.Fail(400, "Geçersiz ay değeri", "Month must be between 1 and 12", true));
        }

        // Yıl doğrulama
        if (targetYear < 2000 || targetYear > 2100)
        {
            _logger.LogWarning("GetAidatTahsilatKpi: Invalid year={Year}", targetYear);
            return BadRequest(ResponseDto<AidatTahsilatKpiDto>.Fail(400, "Geçersiz yıl değeri", "Year must be between 2000 and 2100", true));
        }

        var result = await _logoService.GetAidatTahsilatKpiAsync(targetYear, targetMonth);

        if (result.IsSuccess)
        {
            _logger.LogInformation("GetAidatTahsilatKpi: Successfully retrieved KPI data for Year={Year}, Month={Month}, ToplamAlacak={ToplamAlacak}, Odenen={Odenen}, Bekleyen={Bekleyen}",
                targetYear, targetMonth, result.Data?.ToplamAlacak, result.Data?.Odenen, result.Data?.Bekleyen);
        }
        else
        {
            _logger.LogWarning("GetAidatTahsilatKpi: Failed to retrieve KPI data for Year={Year}, Month={Month}, StatusCode={StatusCode}",
                targetYear, targetMonth, result.StatusCode);
        }

        return StatusCode(result.StatusCode, result);
    }
}
