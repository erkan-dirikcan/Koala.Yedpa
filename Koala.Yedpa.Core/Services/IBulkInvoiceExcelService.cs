using Koala.Yedpa.Core.Dtos.BulkInvoice;

namespace Koala.Yedpa.Core.Services
{
    /// <summary>
    /// Bilgilendirme maili için önizleme Excel'i üretir (oluşturulacak AIDAT faturaları).
    /// </summary>
    public interface IBulkInvoiceExcelService
    {
        byte[] BuildPreviewExcel(IReadOnlyList<PendingInvoiceLineDto> lines);
    }
}
