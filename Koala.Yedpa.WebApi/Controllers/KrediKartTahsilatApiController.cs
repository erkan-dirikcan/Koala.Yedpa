using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.KrediKartTahsilat;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Koala.Yedpa.WebApi.Controllers;

[Route("api/KrediKartTahsilat")]
[ApiController]
[Produces("application/json")]
[SwaggerTag("Kredi Kartı Tahsilatlar API - Logo Tiger ArpSlips entegrasyonu")]
[Authorize(Policy = "CurrentAccuant")]
public class KrediKartTahsilatApiController : ControllerBase
{
    private readonly IKrediKartTahsilatService _krediKartTahsilatService;
    private readonly IApiLogoSqlDataService _logoSqlService;
    private readonly ILogger<KrediKartTahsilatApiController> _logger;

    public KrediKartTahsilatApiController(
        IKrediKartTahsilatService krediKartTahsilatService,
        IApiLogoSqlDataService logoSqlService,
        ILogger<KrediKartTahsilatApiController> logger)
    {
        _krediKartTahsilatService = krediKartTahsilatService;
        _logoSqlService = logoSqlService;
        _logger = logger;
    }

    /// <summary>
    /// Kredi karti tahsilatinda kullanilacak banka/POS kartlarini listeler.
    /// </summary>
    [HttpGet("cards")]
    [SwaggerOperation(
        Summary = "Kredi karti hesap listesi",
        Description = "Logo Tiger'da kredi karti tahsilatinda kullanilabilecek banka kartlarini (BNCARD, KKUSAGE=1) dondurur. " +
                      "Donen CardRef degeri, tahsilat fisinin PAYMENT.CARDREF alani icin kullanilir.")]
    [SwaggerResponse(200, "Basarili", typeof(ResponseListDto<List<CreditCardAccountViewModel>>))]
    [SwaggerResponse(401, "Yetkisiz erisim")]
    [SwaggerResponse(500, "Sunucu hatasi")]
    public async Task<IActionResult> GetCreditCardAccounts()
    {
        _logger.LogInformation("GetCreditCardAccounts called");
        var result = await _logoSqlService.GetCreditCardAccountsAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("GetCreditCardAccounts: Failed, StatusCode: {StatusCode}", result.StatusCode);
        }

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Kredi kartı tahsilat fişi oluşturur (Logo Tiger ArpSlips, TYPE=70).
    /// Dışarıdan alınan input modelini Logo ArpSlips payload'ına mapleyip gönderir.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Kredi kartı tahsilat fişi oluşturur",
        Description = "Input modelini (Model 1) Logo ArpSlips payload'ına (Model 2) mapleyip " +
                      "Logo Tiger REST API üzerinden kredi kartı tahsilat fişi (TYPE=70) oluşturur. " +
                      "Sabit alanlar (TYPE, PRINT_COUNTER, CURRSEL_TOTALS, BANKACC_CODE, BANK_GL_CODE, " +
                      "MODULENR, PAYMENT_TYPE, DISCTRDELLIST, AFFECT_RISK) servis tarafında sabitlenir.")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseDto<string>))]
    [SwaggerResponse(400, "Geçersiz istek")]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> CreateKrediKartTahsilat([FromBody] CreateKrediKartTahsilatRequestDto request)
    {
        _logger.LogInformation("CreateKrediKartTahsilat called. Date={Date}", request.CollectionDate);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("CreateKrediKartTahsilat: Invalid model state");
            return BadRequest(ResponseDto<string>.FailData(
                400, "Geçersiz istek", "Model validation hatası", true));
        }

        var result = await _krediKartTahsilatService.CreateKrediKartTahsilatAsync(request);

        if (result.IsSuccess)
        {
            _logger.LogInformation("CreateKrediKartTahsilat: Success");
        }
        else
        {
            _logger.LogWarning("CreateKrediKartTahsilat: Failed, StatusCode: {StatusCode}, Message: {Message}",
                result.StatusCode, result.Message);
        }

        return StatusCode(result.StatusCode, result);
    }
}
