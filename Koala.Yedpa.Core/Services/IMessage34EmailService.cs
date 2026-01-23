using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Message34;

namespace Koala.Yedpa.Core.Services
{
    public interface IMessage34EmailService
    {
        /// <summary>
        /// Tek veya çoklu alıcıya transactional email gönderir
        /// </summary>
        Task<ResponseDto<int?>> SendTransactionEmailAsync(EmailDto email, List<string>? toEmails = null);

        /// <summary>
        /// Belirtilen gruplara bulk email gönderir
        /// </summary>
        Task<ResponseDto<int?>> SendBulkEmailAsync(Message34SendBulkEmailRequest request);

        /// <summary>
        /// Yeni grup oluşturup email gönderir (Transfer & Send)
        /// </summary>
        Task<ResponseDto<int?>> TransferAndSendEmailAsync(Message34TransferAndSendRequest request);

        /// <summary>
        /// Kampanya detaylarını getirir
        /// </summary>
        Task<ResponseDto<Message34CampaignDetailResponse?>> GetCampaignDetailsAsync(int campaignId);

        /// <summary>
        /// Authentication - token yeniler
        /// </summary>
        Task<bool> AuthenticateAsync();
    }
}
