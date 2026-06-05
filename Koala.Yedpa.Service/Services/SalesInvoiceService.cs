using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.SalesInvoice;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Koala.Yedpa.Service.Services;

/// <summary>
/// Satış faturası servisi implementasyonu
/// </summary>
public class SalesInvoiceService : ISalesInvoiceService
{
    private readonly ILogoRestServiceProvider _logoRestServiceProvider;
    private readonly ILogger<SalesInvoiceService> _logger;

    public SalesInvoiceService(
        ILogoRestServiceProvider logoRestServiceProvider,
        ILogger<SalesInvoiceService> logger)
    {
        _logoRestServiceProvider = logoRestServiceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Logo Tiger REST API üzerinden satış faturası oluşturur
    /// </summary>
    public async Task<ResponseDto<CreateSalesInvoiceResponseDto>> CreateSalesInvoiceAsync(CreateSalesInvoiceRequestDto request)
    {
        try
        {
            _logger.LogInformation("Satış faturası oluşturma işlemi başladı. Cari: {ArpCode}, Tarih: {Date}",
                request.ArpCode, request.Date.ToShortDateString());

            // 1. DTO'yu JSON'a serialize et
            var json = JsonConvert.SerializeObject(request, Formatting.Indented);
            _logger.LogDebug("Gönderilen JSON: {Json}", json);

            // 2. Logo Tiger REST API'ye gönder
            // salesInvoices endpoint'ine POST isteği atıyoruz
            var response = await _logoRestServiceProvider.HttpPost("salesInvoices", json);

            // 3. Response'u kontrol et
            if (!response.IsSuccess)
            {
                _logger.LogError("Satış faturası oluşturulamadı. StatusCode: {StatusCode}, Message: {Message}",
                    response.StatusCode, response.Message);

                return ResponseDto<CreateSalesInvoiceResponseDto>.FailData(
                    response.StatusCode,
                    "Satış faturası oluşturulamadı",
                    response.Message,
                    true);
            }

            // 4. Başarılı response'u parse et
            try
            {
                // Logo'dan dönen JSON'ı parse et
                var logoResponse = JsonConvert.DeserializeObject<LogoSalesInvoiceResponse>(response.Data);

                if (logoResponse == null)
                {
                    _logger.LogWarning("Logo response parse edilemedi, ham response kullanılıyor");

                    return ResponseDto<CreateSalesInvoiceResponseDto>.SuccessData(200,
                        "Satış faturası oluşturuldu (response parse edilemedi)",
                        new CreateSalesInvoiceResponseDto
                        {
                            IsSuccess = true,
                            Message = "Fatura başarıyla oluşturuldu ancak detaylar alınamadı",
                            RawResponse = response.Data,
                            ProcessedAt = DateTime.UtcNow
                        });
                }

                // 5. DTO'ya map et
                var result = new CreateSalesInvoiceResponseDto
                {
                    InvoiceRef = logoResponse.INTERNAL_REFERENCE,
                    InvoiceNumber = logoResponse.NUMBER ?? "Otomatik Atanan",
                    IsSuccess = true,
                    Message = "Satış faturası başarıyla oluşturuldu",
                    RawResponse = response.Data,
                    ProcessedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Satış faturası başarıyla oluşturuldu. LogicalRef: {LogicalRef}, Fatura No: {InvoiceNumber}",
                    result.InvoiceRef, result.InvoiceNumber);

                // 6. Ödeme tipine göre fiş oluştur
                if (!string.IsNullOrEmpty(request.PaymentType))
                {
                    ResponseDto<string> slipResponse;

                    if (request.PaymentType.Equals("CreditCard", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Kredi kartı ödemesi tespit edildi, ARP Slip oluşturuluyor");

                        // Kredi kartı için ARP Slip oluştur
                        // NOT: Burada ARP Slip DTO'su oluşturulmalı ve CreateArpSlipAsync çağrılmalı
                        // Şu anda sadece log atıyoruz, çünkü ARP Slip verileri request'te yok
                        _logger.LogWarning("ARP Slip oluşturma işlevi henüz tamamlanmadı - PaymentType: CreditCard tespit edildi ancak gerekli veriler eksik");
                    }
                    else if (request.PaymentType.Equals("Cash", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Nakit ödeme tespit edildi, Kasa fişi oluşturuluyor");

                        // Nakit için Kasa fişi oluştur
                        // NOT: Burada Kasa fişi DTO'su oluşturulmalı ve CreateSafeDepositSlipAsync çağrılmalı
                        // Şu anda sadece log atıyoruz, çünkü Kasa fişi verileri request'te yok
                        _logger.LogWarning("Kasa fişi oluşturma işlevi henüz tamamlanmadı - PaymentType: Cash tespit edildi ancak gerekli veriler eksik");
                    }
                    else
                    {
                        _logger.LogInformation("Desteklenmeyen ödeme tipi: {PaymentType}", request.PaymentType);
                    }
                }

                return ResponseDto<CreateSalesInvoiceResponseDto>.SuccessData(200,
                    "Satış faturası başarıyla oluşturuldu", result);
            }
            catch (JsonException parseEx)
            {
                _logger.LogError(parseEx, "Logo response JSON parse hatası");

                return ResponseDto<CreateSalesInvoiceResponseDto>.SuccessData(200,
                    "Satış faturası oluşturuldu (response parse edilemedi)",
                    new CreateSalesInvoiceResponseDto
                    {
                        IsSuccess = true,
                        Message = "Fatura başarıyla oluşturuldu ancak detaylar alınamadı",
                        RawResponse = response.Data,
                        ErrorDetail = parseEx.Message,
                        ProcessedAt = DateTime.UtcNow
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Satış faturası oluşturma işlemi genel hata");

            return ResponseDto<CreateSalesInvoiceResponseDto>.FailData(500,
                "Satış faturası oluşturulurken bir hata oluştu",
                ex.Message,
                true);
        }
    }

    /// <summary>
    /// Logo Tiger REST API üzerinden kredi kartı cari hesap fişi (ARP Slip) oluşturur
    /// </summary>
    public async Task<ResponseDto<string>> CreateArpSlipAsync(CreateArpSlipRequestDto request)
    {
        try
        {
            _logger.LogInformation("Kredi kartı cari hesap fişi oluşturma işlemi başladı. Tarih: {Date}",
                request.Date.ToShortDateString());

            // 1. DTO'yu JSON'a serialize et
            var json = JsonConvert.SerializeObject(request, Formatting.Indented);
            _logger.LogDebug("Gönderilen JSON: {Json}", json);

            // 2. Logo Tiger REST API'ye gönder
            // ArpSlips endpoint'ine POST isteği atıyoruz
            var response = await _logoRestServiceProvider.HttpPost("ArpSlips", json);

            // 3. Response'u kontrol et
            if (!response.IsSuccess)
            {
                _logger.LogError("Kredi kartı cari hesap fişi oluşturulamadı. StatusCode: {StatusCode}, Message: {Message}",
                    response.StatusCode, response.Message);

                return ResponseDto<string>.FailData(
                    response.StatusCode,
                    "Kredi kartı cari hesap fişi oluşturulamadı",
                    response.Message,
                    true);
            }

            // 4. Başarılı response'u dön
            _logger.LogInformation("Kredi kartı cari hesap fişi başarıyla oluşturuldu");

            return ResponseDto<string>.SuccessData(200,
                "Kredi kartı cari hesap fişi başarıyla oluşturuldu",
                response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kredi kartı cari hesap fişi oluşturma işlemi genel hata");

            return ResponseDto<string>.FailData(500,
                "Kredi kartı cari hesap fişi oluşturulurken bir hata oluştu",
                ex.Message,
                true);
        }
    }

    /// <summary>
    /// Logo Tiger REST API üzerinden nakit/online kasa fişi (Safe Deposit Slip) oluşturur
    /// </summary>
    public async Task<ResponseDto<string>> CreateSafeDepositSlipAsync(CreateSafeDepositSlipRequestDto request)
    {
        try
        {
            _logger.LogInformation("Kasa fişi oluşturma işlemi başladı. Tarih: {Date}, Kasa: {SdCode}",
                request.Date, request.SdCode);

            // 1. DTO'yu JSON'a serialize et
            var json = JsonConvert.SerializeObject(request, Formatting.Indented);
            _logger.LogDebug("Gönderilen JSON: {Json}", json);

            // 2. Logo Tiger REST API'ye gönder
            // safeDepositSlips endpoint'ine POST isteği atıyoruz
            var response = await _logoRestServiceProvider.HttpPost("safeDepositSlips", json);

            // 3. Response'u kontrol et
            if (!response.IsSuccess)
            {
                _logger.LogError("Kasa fişi oluşturulamadı. StatusCode: {StatusCode}, Message: {Message}",
                    response.StatusCode, response.Message);

                return ResponseDto<string>.FailData(
                    response.StatusCode,
                    "Kasa fişi oluşturulamadı",
                    response.Message,
                    true);
            }

            // 4. Başarılı response'u dön
            _logger.LogInformation("Kasa fişi başarıyla oluşturuldu");

            return ResponseDto<string>.SuccessData(200,
                "Kasa fişi başarıyla oluşturuldu",
                response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kasa fişi oluşturma işlemi genel hata");

            return ResponseDto<string>.FailData(500,
                "Kasa fişi oluşturulurken bir hata oluştu",
                ex.Message,
                true);
        }
    }

    /// <summary>
    /// Logo Tiger REST API'den dönen yanıt modeli
    /// </summary>
    private class LogoSalesInvoiceResponse
    {
        [JsonProperty("INTERNAL_REFERENCE")]
        public int INTERNAL_REFERENCE { get; set; }

        [JsonProperty("NUMBER")]
        public string? NUMBER { get; set; }
    }
}
