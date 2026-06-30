using System.Globalization;
using System.Text.Json;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services
{
    /// <summary>
    /// Bekleyen AIDAT satırını kanıtlanmış Logo salesInvoices payload'ı ile faturaya çevirir.
    /// Toplamları/GL kodlarını REST hesaplar; biz sadece girdileri gönderiyoruz.
    /// </summary>
    public class BulkInvoiceTransferService : IBulkInvoiceTransferService
    {
        private readonly ILogoRestServiceProvider _rest;
        private readonly ILogger<BulkInvoiceTransferService> _logger;
        private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

        public BulkInvoiceTransferService(
            ILogoRestServiceProvider rest,
            ILogger<BulkInvoiceTransferService> logger)
        {
            _rest = rest;
            _logger = logger;
        }

        public async Task<TransferLineResult> TransferLineAsync(PendingInvoiceLineDto line, DateTime invoiceDate)
        {
            try
            {
                // "TEMMUZ" -> "Temmuz" (NOTES1 / DESCRIPTION için)
                var ayTitle = Tr.TextInfo.ToTitleCase(line.MonthName.ToLower(Tr));

                var payload = new AidatInvoicePayload
                {
                    ArpCode = line.ClientCode,
                    Date = invoiceDate.ToString("yyyy-MM-dd"),
                    Time = invoiceDate.ConvertToLogoTime(),
                    Notes1 = $"{ayTitle} AIDAT TAHAKKUKU",
                    Transactions = new AidatInvoiceTransactions
                    {
                        Items =
                        {
                            new AidatInvoiceTransaction
                            {
                                Price = line.Amount.ToString(CultureInfo.InvariantCulture),
                                Description = $"{ayTitle} AIDAT"
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                json = LogoJsonHelper.InjectDataObjectParameter(json);

                // Anlık token-retry: token hatasında ~3 sn bekle, POST'u tekrar dene (en fazla 2 retry).
                ResponseDto<string>? resp = null;
                for (var attempt = 0; attempt < 3; attempt++)
                {
                    resp = await _rest.HttpPost("salesInvoices", json);
                    if (resp.IsSuccess || !IsTokenError(resp)) break;
                    await Task.Delay(3000);
                }

                if (resp == null || !resp.IsSuccess)
                {
                    var transient = resp != null && IsTokenError(resp);
                    return new TransferLineResult(false, line.Orflineref, line.ClientCode, null, null,
                        transient ? "Token alınamadı (geçici)" : "REST iş hatası", RawError(resp), transient);
                }

                using var doc = JsonDocument.Parse(resp.Data);
                int? refId = doc.RootElement.TryGetProperty("INTERNAL_REFERENCE", out var r)
                    && r.TryGetInt32(out var rv) ? rv : null;
                string? no = doc.RootElement.TryGetProperty("NUMBER", out var n) ? n.GetString() : null;

                return new TransferLineResult(true, line.Orflineref, line.ClientCode, refId, no, null, null, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura aktarım hatası. Cari {Code}", line.ClientCode);
                return new TransferLineResult(false, line.Orflineref, line.ClientCode, null, null,
                    "İstisna", ex.Message, true);
            }
        }

        private static bool IsTokenError(ResponseDto<string> r)
            => r.StatusCode == 401
            || (r.Message?.Contains("token", StringComparison.OrdinalIgnoreCase) ?? false)
            || RawError(r).Contains("token", StringComparison.OrdinalIgnoreCase);

        private static string RawError(ResponseDto<string>? r)
            => r?.Errors?.Errors is { Count: > 0 } e ? string.Join("; ", e) : (r?.Message ?? "Bilinmeyen hata");
    }
}
