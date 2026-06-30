namespace Koala.Yedpa.Core.Services
{
    /// <summary>
    /// Toplu faturalandırma e-postaları:
    /// - SendInfoMailAsync: T-1 gün, oluşturulacak faturaların Excel listesiyle bilgilendirme.
    /// - SendReportMailAsync: aktarım tamamlanınca başarılı/başarısız raporu.
    /// </summary>
    public interface IBulkInvoiceEmailService
    {
        Task SendInfoMailAsync(int sessionId);
        Task SendReportMailAsync(int sessionId);
    }
}
