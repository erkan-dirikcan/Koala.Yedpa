using Koala.Yedpa.Core.Dtos.BulkInvoice;

namespace Koala.Yedpa.Core.Services
{
    /// <summary>
    /// Tek bir bekleyen AIDAT satırını Logo REST salesInvoices ile faturaya çevirir.
    /// </summary>
    public interface IBulkInvoiceTransferService
    {
        Task<TransferLineResult> TransferLineAsync(PendingInvoiceLineDto line, DateTime invoiceDate);
    }

    /// <summary>
    /// Tek satır aktarım sonucu.
    /// IsTransient=true → token/geçici hata (kuyruk sonrası tekrar denenebilir).
    /// IsTransient=false → kalıcı iş hatası (tekrar denemeye gerek yok).
    /// </summary>
    public record TransferLineResult(
        bool Success,
        int Orflineref,
        string ClientCode,
        int? LogoInvoiceRef,
        string? InvoiceNumber,
        string? Note,
        string? RestError,
        bool IsTransient);
}
