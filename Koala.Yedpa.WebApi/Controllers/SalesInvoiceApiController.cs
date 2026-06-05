using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.SalesInvoice;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Koala.Yedpa.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[SwaggerTag("Satış Faturası API - Logo Tiger entegrasyonu")]
[Authorize(Policy = "CurrentAccuant")]
public class SalesInvoiceApiController : ControllerBase
{
    private readonly ISalesInvoiceService _salesInvoiceService;
    private readonly IApiLogoSqlDataService _logoSqlService;
    private readonly ILogger<SalesInvoiceApiController> _logger;

    public SalesInvoiceApiController(
        ISalesInvoiceService salesInvoiceService,
        IApiLogoSqlDataService logoSqlService,
        ILogger<SalesInvoiceApiController> logger)
    {
        _salesInvoiceService = salesInvoiceService;
        _logoSqlService = logoSqlService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanılabilir ödeme tiplerini döndürür
    /// </summary>
    [HttpGet("payment-types")]
    [SwaggerOperation(Summary = "Ödeme tipleri listesi",
        Description = "Satış faturası oluşturulurken kullanılabilecek ödeme tiplerini döndürür. " +
                      "Her ödeme tipi, fatura ile birlikte farklı bir fiş tipinin otomatik oluşturulmasını tetikler.")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<PaymentTypeViewModel>>))]
    public IActionResult GetPaymentTypes()
    {
        var paymentTypes = new List<PaymentTypeViewModel>
        {
            new() { Code = "Cash", Description = "Nakit ödeme — Fatura + Kasa fişi oluşturur" },
            new() { Code = "CreditCard", Description = "Kredi kartı — Fatura + Cari hesap fişi (ARP Slip) oluşturur" },
            new() { Code = "BankTransfer", Description = "Banka havalesi/EFT — Fatura + Banka fişi oluşturur" },
            new() { Code = "Check", Description = "Çek ödemesi" },
            new() { Code = "Bill", Description = "Senet ödemesi" },
        };

        return Ok(ResponseListDto<List<PaymentTypeViewModel>>.SuccessData(
            200, "Ödeme tipleri getirildi", paymentTypes,
            RecordsTotal: paymentTypes.Count,
            RecordsFiltered: paymentTypes.Count,
            RecordsShow: paymentTypes.Count
        ));
    }

    /// <summary>
    /// Logo'daki banka kartlarını döndürür
    /// </summary>
    [HttpGet("banks")]
    [SwaggerOperation(Summary = "Banka listesi",
        Description = "Logo Tiger'daki banka kartlarını (BANKCARD) döndürür. " +
                      "Kredi kartı veya banka havalesi ödemelerinde BANK_CODE alanı için kullanılır.")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<BankViewModel>>))]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> GetBanks()
    {
        _logger.LogInformation("GetBanks called");
        var result = await _logoSqlService.GetBanksAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("GetBanks: Failed, StatusCode: {StatusCode}", result.StatusCode);
        }

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Kredi kartı ödemelerinde kullanılan banka hesaplarını döndürür
    /// </summary>
    [HttpGet("bank-accounts")]
    [SwaggerOperation(Summary = "KK banka hesap listesi",
        Description = "Logo Tiger'daki kredi kartı kullanımına açık banka hesaplarını (BNCARD + BANKACC, KKUSAGE=1) döndürür. " +
                      "Kredi kartı ödemesinde BANK_ACCOUNT_CODE alanı için kullanılır.")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<BankAccountViewModel>>))]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> GetBankAccounts()
    {
        _logger.LogInformation("GetBankAccounts called");
        var result = await _logoSqlService.GetBankAccountsAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("GetBankAccounts: Failed, StatusCode: {StatusCode}", result.StatusCode);
        }

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Logo'daki hizmet kartlarını döndürür (CARDTYPE=2)
    /// </summary>
    [HttpGet("services")]
    [SwaggerOperation(Summary = "Hizmet listesi",
        Description = "Logo Tiger'daki hizmet kartlarını (SRVCARD, CARDTYPE=2) döndürür. " +
                      "Fatura kalemlerinde MASTER_CODE alanı için kullanılır.")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<ServiceListViewModel>>))]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> GetServices()
    {
        _logger.LogInformation("GetServices called");
        var result = await _logoSqlService.GetServicesAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("GetServices: Failed, StatusCode: {StatusCode}", result.StatusCode);
        }

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Logo Tiger'a satış faturası gönderir.
    /// PaymentType alanına göre otomatik olarak ilişkili fişi de oluşturur.
    /// </summary>
    [HttpPost("create")]
    [SwaggerOperation(
        Summary = "Satış faturası oluşturur",
        Description = "Logo Tiger REST API üzerinden satış faturası oluşturur. " +
                      "PaymentType alanına göre otomatik olarak ilişkili fiş de oluşturulur: " +
                      "- **Cash**: Fatura + Kasa fişi oluşturur " +
                      "- **CreditCard**: Fatura + Cari hesap fişi (ARP Slip) oluşturur " +
                      "- **BankTransfer**: Fatura + Banka fişi oluşturur " +
                      "- **null / diğer**: Sadece fatura oluşturur")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseDto<CreateSalesInvoiceResponseDto>))]
    [SwaggerResponse(400, "Geçersiz istek")]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> CreateSalesInvoice([FromBody] CreateSalesInvoiceRequestDto request)
    {
        _logger.LogInformation("CreateSalesInvoice called with ArpCode={ArpCode}, Date={Date}, PaymentType={PaymentType}",
            request.ArpCode, request.Date, request.PaymentType);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("CreateSalesInvoice: Invalid model state");
            return BadRequest(ResponseDto<CreateSalesInvoiceResponseDto>.FailData(
                400, "Geçersiz istek", "Model validation hatası", true));
        }

        var result = await _salesInvoiceService.CreateSalesInvoiceAsync(request);

        if (result.IsSuccess)
        {
            _logger.LogInformation("CreateSalesInvoice: Successfully created invoice {InvoiceNumber}",
                result.Data?.InvoiceNumber ?? "UNKNOWN");
        }
        else
        {
            _logger.LogWarning("CreateSalesInvoice: Failed to create invoice, StatusCode: {StatusCode}, Message: {Message}",
                result.StatusCode, result.Message);
        }

        return StatusCode(result.StatusCode, result);
    }
}
