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
        /// Faturalandırılmamış ORFLINE satırlarını getirir
        /// </summary>
        /// <returns>Faturalandırılmamış satırlar listesi</returns>
        Task<ResponseDto<List<PendingInvoiceLineDto>>> GetPendingLinesAsync();

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
    }
}
