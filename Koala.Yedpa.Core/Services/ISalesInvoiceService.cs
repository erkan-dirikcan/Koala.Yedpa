using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.SalesInvoice;

namespace Koala.Yedpa.Core.Services;

/// <summary>
/// Satış faturası servisi arayüzü
/// </summary>
public interface ISalesInvoiceService
{
    /// <summary>
    /// Logo Tiger REST API üzerinden satış faturası oluşturur
    /// </summary>
    /// <param name="request">Fatura oluşturma isteği</param>
    /// <returns>Fatura oluşturma yanıtı</returns>
    Task<ResponseDto<CreateSalesInvoiceResponseDto>> CreateSalesInvoiceAsync(CreateSalesInvoiceRequestDto request);

    /// <summary>
    /// Logo Tiger REST API üzerinden kredi kartı cari hesap fişi (ARP Slip) oluşturur
    /// </summary>
    /// <param name="request">ARP Slip oluşturma isteği</param>
    /// <returns>ARP Slip oluşturma yanıtı (fiş referansı)</returns>
    Task<ResponseDto<string>> CreateArpSlipAsync(CreateArpSlipRequestDto request);

    /// <summary>
    /// Logo Tiger REST API üzerinden nakit/online kasa fişi (Safe Deposit Slip) oluşturur
    /// </summary>
    /// <param name="request">Kasa fişi oluşturma isteği</param>
    /// <returns>Kasa fişi oluşturma yanıtı (fiş referansı)</returns>
    Task<ResponseDto<string>> CreateSafeDepositSlipAsync(CreateSafeDepositSlipRequestDto request);
}
