using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models
{
    public class QRCodeBatch : CommonProperties
    {
        public int Id { get; set; }

        /// <summary>
        /// Kullanılan SQL sorgusu
        /// </summary>
        public string? SqlQuery { get; set; }

        /// <summary>
        /// QR kod yılı
        /// </summary>
        public string QrCodeYear { get; set; } = string.Empty;

        /// <summary>
        /// QR kod ön kodu
        /// </summary>
        public string QrCodePreCode { get; set; } = string.Empty;

        /// <summary>
        /// Oluşturulan QR kod sayısı
        /// </summary>
        public int QRCodeCount { get; set; }

        /// <summary>
        /// İşlem açıklaması
        /// </summary>
        public string? Description { get; set; }
    }
}
