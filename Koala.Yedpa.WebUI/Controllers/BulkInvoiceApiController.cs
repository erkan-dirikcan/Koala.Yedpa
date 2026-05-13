using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Koala.Yedpa.WebUI.Controllers;

/// <summary>
/// Toplu Faturalandırma API Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class BulkInvoiceApiController : ControllerBase
{
    private readonly IBulkInvoiceService _service;
    private readonly ILogger<BulkInvoiceApiController> _logger;

    public BulkInvoiceApiController(IBulkInvoiceService service, ILogger<BulkInvoiceApiController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Dashboard alert kontrolü - Ayın 15'inden sonra ve o ay için session var mı kontrol eder
    /// </summary>
    /// <returns>Alert gösterilmeli mi?</returns>
    [HttpGet("check-alert")]
    public async Task<IActionResult> CheckAlert()
    {
        _logger.LogInformation("CheckAlert called");

        var result = await _service.CheckAlertAsync();

        if (result.IsSuccess)
        {
            _logger.LogInformation("CheckAlert: Successfully retrieved alert status, ShowAlert={ShowAlert}", result.Data?.ShowAlert);
        }
        else
        {
            _logger.LogWarning("CheckAlert: Failed to retrieve alert status, StatusCode: {StatusCode}", result.StatusCode);
        }

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Faturalandırılmamış ORFLINE satırlarını çeker
    /// </summary>
    /// <returns>Faturalandırılmamış satırlar listesi</returns>
    [HttpGet("pending-lines")]
    public async Task<IActionResult> GetPendingLines()
    {
        _logger.LogInformation("GetPendingLines called");

        var result = await _service.GetPendingLinesAsync();

        if (result.IsSuccess)
        {
            _logger.LogInformation("GetPendingLines: Successfully retrieved {Count} pending lines", result.Data?.Count ?? 0);
        }
        else
        {
            _logger.LogWarning("GetPendingLines: Failed to retrieve pending lines, StatusCode: {StatusCode}", result.StatusCode);
        }

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Yeni toplu faturalandırma oturumu oluşturur
    /// </summary>
    /// <param name="model">Oturum oluşturma modeli</param>
    /// <returns>Oluşturulan oturum ID'si</returns>
    [HttpPost("create-session")]
    public async Task<IActionResult> CreateSession([FromBody] CreateBulkInvoiceSessionDto model)
    {
        _logger.LogInformation("CreateSession called with InvoiceDate={InvoiceDate}, SelectedLinesCount={SelectedLinesCount}",
            model.InvoiceDate, model.SelectedLines?.Count ?? 0);

        if (model == null)
        {
            _logger.LogWarning("CreateSession: Model is null");
            return BadRequest(ResponseDto<int>.Fail(400, "Oturum oluşturma modeli boş olamaz", "Model null", true));
        }

        if (model.SelectedLines == null || !model.SelectedLines.Any())
        {
            _logger.LogWarning("CreateSession: No selected lines provided");
            return BadRequest(ResponseDto<int>.Fail(400, "En az bir satır seçilmelidir", "SelectedLines empty", true));
        }

        var username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
        var result = await _service.CreateSessionAsync(model, username);

        if (result.IsSuccess)
        {
            _logger.LogInformation("CreateSession: Successfully created session with ID={SessionId}", result.Data);
        }
        else
        {
            _logger.LogWarning("CreateSession: Failed to create session, StatusCode: {StatusCode}, Message={Message}",
                result.StatusCode, result.Message);
        }

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Oturum durumunu getirir (progress tracking için)
    /// </summary>
    /// <param name="id">Oturum ID</param>
    /// <returns>Oturum detayları ve durumu</returns>
    [HttpGet("session-status/{id}")]
    public async Task<IActionResult> GetSessionStatus(int id)
    {
        _logger.LogInformation("GetSessionStatus called with SessionId={SessionId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("GetSessionStatus: Invalid SessionId={SessionId}", id);
            return BadRequest(ResponseDto<BulkInvoiceSessionDto>.Fail(400, "Geçersiz oturum ID", "SessionId must be positive", true));
        }

        var result = await _service.GetSessionStatusAsync(id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("GetSessionStatus: Successfully retrieved session status, Status={Status}, Progress={Progress}%",
                result.Data?.Status, result.Data?.ProgressPercentage);
        }
        else
        {
            _logger.LogWarning("GetSessionStatus: Failed to retrieve session status, StatusCode: {StatusCode}", result.StatusCode);
        }

        return StatusCode(result.StatusCode, result);
    }
}
