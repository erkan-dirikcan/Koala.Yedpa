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
        _logger.LogInformation("Generate called with Text: {Text}, Size: {Width}x{Height}", request?.Text, request?.Width, request?.Height);
        try
        {
            var result = await _qrCodeService.GenerateQRCodeAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Generate: Successfully generated QR code");
                return File(result.Data, "image/png");
            }

            _logger.LogWarning("Generate: Failed with StatusCode: {StatusCode}", result.StatusCode);
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
        _logger.LogInformation("GenerateWithLogo called with Text: {Text}, Size: {Width}x{Height}", request?.Text, request?.Width, request?.Height);
        try
        {
            var result = await _qrCodeService.GenerateQRCodeWithLogoAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("GenerateWithLogo: Successfully generated QR code with logo");
                return File(result.Data, "image/png");
            }

            _logger.LogWarning("GenerateWithLogo: Failed with StatusCode: {StatusCode}", result.StatusCode);
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
        _logger.LogInformation("GenerateForWorkplace called with WorkplaceCode: {WorkplaceCode}, PartnerNo: {PartnerNo}", workplaceCode, partnerNo);
        try
        {
            var result = await _qrCodeService.GenerateAndSaveQRCodeAsync(workplaceCode, partnerNo);

            if (result.IsSuccess)
            {
                _logger.LogInformation("GenerateForWorkplace: Successfully generated QR code for workplace {WorkplaceCode}", workplaceCode);
                return Ok(result);
            }

            _logger.LogWarning("GenerateForWorkplace: Failed with StatusCode: {StatusCode}", result.StatusCode);
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
        _logger.LogInformation("GetImage called for FilePath: {FilePath}", filePath);
        try
        {
            var result = await _qrCodeService.GetQRCodeImageAsync(filePath);

            if (result.IsSuccess)
            {
                _logger.LogInformation("GetImage: Successfully retrieved QR code image");
                return File(result.Data, "image/jpeg");
            }

            _logger.LogWarning("GetImage: Failed with StatusCode: {StatusCode}", result.StatusCode);
            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting QR code image: {FilePath}", filePath);
            return StatusCode(500, ResponseDto<byte[]>.Fail(500, "QR kod resmi getirilirken hata oluştu", new List<string> { ex.Message }, true));
        }
    }
}
