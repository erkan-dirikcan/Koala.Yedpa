using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.BulkInvoice;

namespace Koala.Yedpa.Core.Services
{
    /// <summary>
    /// Toplu faturalandırma servisi interface'i
    /// </summary>
    public interface IBulkInvoiceService
    {
        /// <summary>
        /// Dashboard alert kontrolü yapar
        /// Ayın 15'inden sonra ve o ay için session yoksa alert gösterilir
        /// </summary>
        /// <returns>Alert kontrol sonucu</returns>
        Task<ResponseDto<AlertCheckResultDto>> CheckAlertAsync();

        /// <summary>
        /// Faturalandırılmamış ORFLINE satırlarını getirir (parametresiz = gelecek ay)
        /// </summary>
        /// <returns>Faturalandırılmamış satırlar listesi</returns>
        Task<ResponseDto<List<PendingInvoiceLineDto>>> GetPendingLinesAsync();

        /// <summary>
        /// Verilen Logo ay adı (LINEEXP, örn. "TEMMUZ") için faturalandırılmamış satırları getirir
        /// </summary>
        /// <param name="logoMonthName">Büyük harf ASCII ay adı (BulkInvoiceMonths.ToLogoName)</param>
        Task<ResponseDto<List<PendingInvoiceLineDto>>> GetPendingLinesAsync(string logoMonthName);

        /// <summary>
        /// Yeni bir toplu fatura oturumu oluşturur
        /// </summary>
        /// <param name="dto">Oturum oluşturma DTO'su</param>
        /// <param name="username">Kullanıcı adı</param>
        /// <returns>Oluşturulan oturum ID'si</returns>
        Task<ResponseDto<int>> CreateSessionAsync(CreateBulkInvoiceSessionDto dto, string username);

        /// <summary>
        /// Oturum durumunu getirir
        /// </summary>
        /// <param name="sessionId">Oturum ID</param>
        /// <returns>Oturum detayları</returns>
        Task<ResponseDto<BulkInvoiceSessionDto>> GetSessionStatusAsync(int sessionId);

        /// <summary>
        /// Başarıyla faturalanan sipariş satırlarını Logo'da TRGFLAG=1 (faturalandı) yapar.
        /// Logo, REST ile bağımsız fatura kesince TRGFLAG'ı otomatik yapmaz (doğrulandı) — bu adım zorunlu.
        /// </summary>
        /// <param name="orflinerefs">Faturalanmış ORFLINE LOGICALREF listesi</param>
        /// <returns>Güncellenen satır sayısı</returns>
        Task<ResponseDto<int>> MarkLinesAsTransferredAsync(IReadOnlyList<int> orflinerefs);

        /// <summary>Yönetim sayfası: tüm oturumları (özet sayılarla) getirir.</summary>
        Task<ResponseDto<List<BulkInvoiceSessionDto>>> GetSessionsAsync();

        /// <summary>Yönetim sayfası: bir oturumun aktarım satırlarını getirir.</summary>
        Task<ResponseDto<List<BulkInvoiceItemDto>>> GetSessionItemsAsync(int sessionId);

        /// <summary>
        /// Oturumun "aktarılacak" satırlarını (BulkInvoiceItem, Pending) o anki bekleyen AIDAT
        /// satırlarına göre oluşturur/senkronize eder: yenileri ekler, mevcut Pending'leri günceller,
        /// artık beklemeyen Pending'leri kaldırır. Transferred/Failed satırlara DOKUNMAZ.
        /// </summary>
        /// <returns>Aktarılacak (Pending) satır sayısı</returns>
        Task<ResponseDto<int>> SyncSessionItemsAsync(int sessionId);
    }
}
