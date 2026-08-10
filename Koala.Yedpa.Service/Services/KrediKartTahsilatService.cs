using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.KrediKartTahsilat;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Koala.Yedpa.Service.Services;

/// <summary>
/// Kredi kartı tahsilatları servisi implementasyonu.
/// Input (Model 1) -> Logo ArpSlips payload (Model 2) mapleme + Logo REST gönderimi.
/// </summary>
public class KrediKartTahsilatService : IKrediKartTahsilatService
{
    private readonly ILogoRestServiceProvider _logoRestServiceProvider;
    private readonly ILogger<KrediKartTahsilatService> _logger;

    // Sabit (constant) değerler — Logo ArpSlips kredi kartı tahsilat fişi için
    private const int TypeCreditCard   = 70;
    private const int PrintCounter     = 3;
    private const int CurrSelTotals    = 1;
    private const int AffectRisk       = 1;
    private const int PaymentModulenr  = 5;
    private const int PaymentType      = 4;
    private const int Disctrdellist    = 4;
    private const int PaymentTrcode    = 70;  // referans XML TRCODE=70

    public KrediKartTahsilatService(
        ILogoRestServiceProvider logoRestServiceProvider,
        ILogger<KrediKartTahsilatService> logger)
    {
        _logoRestServiceProvider = logoRestServiceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Dışarıdan alınan input modelini Logo ArpSlips payload'ına mapleyip
    /// Logo Tiger REST API üzerinden kredi kartı tahsilat fişi oluşturur.
    /// </summary>
    public async Task<ResponseDto<string>> CreateKrediKartTahsilatAsync(CreateKrediKartTahsilatRequestDto request)
    {
        try
        {
            _logger.LogInformation("Kredi kartı tahsilat fişi oluşturma başladı. Tarih: {Date}",
                request.CollectionDate.ToShortDateString());

            // 1. Input (Model 1) -> Logo payload (Model 2) — sabit defaults uygulanır
            var logo = MapToLogo(request);

            // 2. Newtonsoft ile serialize (TRANSACTIONS/PAYMENT_LIST {items:[]} olarak çıkar)
            var json = JsonConvert.SerializeObject(logo, Formatting.Indented);
            _logger.LogDebug("ArpSlips'e gönderilen JSON: {Json}", json);

            // DataObjectParameter inject et
            json = LogoJsonHelper.InjectDataObjectParameter(json);
            _logger.LogDebug("DataObjectParameter ile enrich edilmiş JSON: {Json}", json);

            // 3. Logo REST ArpSlips endpoint'ine POST
            var response = await _logoRestServiceProvider.HttpPost("ArpSlips", json);

            // 4. Response kontrol
            if (!response.IsSuccess)
            {
                _logger.LogError("Kredi kartı tahsilat fişi oluşturulamadı. Logo HTTP={StatusCode}, Message: {Message}",
                    response.StatusCode, response.Message);

                // Logo'nun durum kodunu AYNEN döndürmeyiz. 401/403 çağıranın kendi token'ını
                // suçlamasına yol açıyor (01.08.2026'da tam olarak bu yaşandı: müşteri saatlerce
                // kendi kimlik doğrulamasını aradı, oysa reddedilen Logo token'ıydı).
                // Yukarı akış hatası bizim için bir ağ geçidi hatasıdır → 502.
                var httpStatus = response.StatusCode is 401 or 403 ? 502 : response.StatusCode;

                return ResponseDto<string>.FailData(
                    httpStatus,
                    $"Logo tarafından reddedildi (Logo HTTP {response.StatusCode})",
                    response.Message,
                    true);
            }

            // 5. Başarılı
            _logger.LogInformation("Kredi kartı tahsilat fişi başarıyla oluşturuldu");

            return ResponseDto<string>.SuccessData(200,
                "Kredi kartı tahsilat fişi başarıyla oluşturuldu",
                response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kredi kartı tahsilat fişi oluşturma işlemi genel hata");

            return ResponseDto<string>.FailData(500,
                "Kredi kartı tahsilat fişi oluşturulurken bir hata oluştu",
                ex.Message,
                true);
        }
    }

    /// <summary>
    /// Slim input modelini (Model 1) Logo payload'ına (Model 2) mapler.
    /// NUMBER: request.Number boşsa "~" kullanılır.
    /// GUID (header): request.Guid boşsa Guid.NewGuid() üretilir.
    /// Transaction GUID'leri her zaman Guid.NewGuid() ile üretilir.
    /// BankAccCode: header/transaction/payment'a request.BankAccCode yazılır.
    /// BANK_GL_CODE gönderilmez — Logo FillAccCodesOnPreSave ile otomatik doldurur.
    /// CARDREF gönderilmez (null bırakılır, NullValueHandling.Ignore ile serialize dışı kalır).
    /// Items, CustomerCode'a göre gruplanarak her grup bir Transaction + PaymentList kalemi oluşturur.
    /// </summary>
    private static KrediKartTahsilatLogoDto MapToLogo(CreateKrediKartTahsilatRequestDto request)
    {
        var date = request.CollectionDate;

        var logo = new KrediKartTahsilatLogoDto
        {
            Number      = string.IsNullOrWhiteSpace(request.Number) ? "~" : request.Number,
            Date        = date,
            PrintDate   = date,
            Type        = TypeCreditCard,                                        // sabit: 70
            TotalCredit = request.Items.Sum(x => x.Amount),
            ArpCode     = request.Items.Count == 1 ? request.Items[0].CustomerCode : null,
            Notes1      = request.Notes1 ?? (request.Items.Count == 1 ? request.Items[0].CustomerName : null),
            PrintCounter  = PrintCounter,                                        // sabit: 3
            CurrSelTotals = CurrSelTotals,                                       // sabit: 1
            BankAccCode   = request.BankAccCode,
            // Logo TIME paketlenmiş bir int'tir (HHmmss DEĞİL) — bkz. Tools.ConvertToLogoTime.
            Time   = date.ConvertToLogoTime(),
            Hour   = date.Hour,
            Minute = date.Minute,
            Guid   = string.IsNullOrWhiteSpace(request.Guid)
                        ? Guid.NewGuid().ToString().ToUpper()
                        : request.Guid,
            Transactions = new KrediKartTahsilatLogoTransactionsDto
            {
                Items = request.Items
                    .GroupBy(x => x.CustomerCode)
                    .Select(group =>
                    {
                        var groupTotal = group.Sum(x => x.Amount);
                        return new KrediKartTahsilatLogoTransactionDto
                        {
                            ArpCode      = group.Key,
                            Credit       = groupTotal,
                            TcAmount     = groupTotal,
                            BnlnTcAmount = groupTotal,
                            Month        = date.Month,
                            Year         = date.Year,
                            AffectRisk   = AffectRisk,                           // sabit: 1
                            BankAccCode  = request.BankAccCode,
                            // BANK_GL_CODE gönderilmez — Logo otomatik doldurur
                            Guid   = Guid.NewGuid().ToString().ToUpper(),
                            Tranno = null,
                            PaymentList = new KrediKartTahsilatLogoPaymentListDto
                            {
                                Items = new List<KrediKartTahsilatLogoPaymentDto>
                                {
                                    new KrediKartTahsilatLogoPaymentDto
                                    {
                                        // Cardref null — NullValueHandling.Ignore ile serialize dışı kalır
                                        Cardref      = null,
                                        Date         = date,
                                        Modulenr     = PaymentModulenr,          // sabit: 5
                                        Ficheref     = null,
                                        Trcode       = PaymentTrcode,            // sabit: 70
                                        Total        = groupTotal,
                                        Procdate     = date,
                                        BankAccCode  = request.BankAccCode,
                                        PaymentType  = PaymentType,              // sabit: 4
                                        Disctrdellist = Disctrdellist            // sabit: 4
                                    }
                                }
                            }
                        };
                    })
                    .ToList()
            }
        };

        return logo;
    }
}
