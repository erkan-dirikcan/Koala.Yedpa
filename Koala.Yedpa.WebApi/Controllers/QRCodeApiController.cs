using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebApi.Controllers;

/// <summary>
/// QR Code API Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class QRCodeApiController : ControllerBase
{
    private readonly IQRCodeService _qrCodeService;
    private readonly ILogger<QRCodeApiController> _logger;

    public QRCodeApiController(
        IQRCodeService qrCodeService,
        ILogger<QRCodeApiController> logger)
    {
        _qrCodeService = qrCodeService;
        _logger = logger;
    }

    /// <summary>
    /// Generate QR Code for workplace
    /// </summary>
    /// <param name="request">QR Code generation request</param>
    /// <returns>QR Code image bytes</returns>
    [HttpPost("Generate")]
    public async Task<IActionResult> Generate([FromBody] QRCodeDto request)
    {
        try
        {
            var result = await _qrCodeService.GenerateQRCodeAsync(request);

            if (result.IsSuccess)
            {
                return File(result.Data, "image/png");
            }

            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code");
            return StatusCode(500, ResponseDto<byte[]>.Fail(500, "QR kod oluşturulurken hata oluştu", new List<string> { ex.Message }, true));
        }
    }

    /// <summary>
    /// Generate QR Code with logo
    /// </summary>
    /// <param name="request">QR Code generation request</param>
    /// <returns>QR Code image bytes</returns>
    [HttpPost("GenerateWithLogo")]
    public async Task<IActionResult> GenerateWithLogo([FromBody] QRCodeDto request)
    {
        try
        {
            var result = await _qrCodeService.GenerateQRCodeWithLogoAsync(request);

            if (result.IsSuccess)
            {
                return File(result.Data, "image/png");
            }

            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code with logo");
            return StatusCode(500, ResponseDto<byte[]>.Fail(500, "QR kod oluşturulurken hata oluştu", new List<string> { ex.Message }, true));
        }
    }

    /// <summary>
    /// Generate and save QR code for workplace
    /// </summary>
    /// <param name="workplaceCode">Workplace code</param>
    /// <param name="partnerNo">Partner number</param>
    /// <returns>QR Code file path</returns>
    [HttpGet("GenerateForWorkplace")]
    public async Task<IActionResult> GenerateForWorkplace([FromQuery] string workplaceCode, [FromQuery] string partnerNo)
    {
        try
        {
            var result = await _qrCodeService.GenerateAndSaveQRCodeAsync(workplaceCode, partnerNo);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code for workplace: {WorkplaceCode}", workplaceCode);
            return StatusCode(500, ResponseDto<string>.Fail(500, "QR kod oluşturulurken hata oluştu", new List<string> { ex.Message }, true));
        }
    }

    /// <summary>
    /// Get QR code image
    /// </summary>
    /// <param name="filePath">QR code file path</param>
    /// <returns>QR Code image</returns>
    [HttpGet("GetImage")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage([FromQuery] string filePath)
    {
        try
        {
            var result = await _qrCodeService.GetQRCodeImageAsync(filePath);

            if (result.IsSuccess)
            {
                return File(result.Data, "image/jpeg");
            }

            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting QR code image: {FilePath}", filePath);
            return StatusCode(500, ResponseDto<byte[]>.Fail(500, "QR kod resmi getirilirken hata oluştu", new List<string> { ex.Message }, true));
        }
    }
}
