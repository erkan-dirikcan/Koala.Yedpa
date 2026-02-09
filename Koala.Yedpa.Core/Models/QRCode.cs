using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models
{
    public class QRCode : CommonProperties
    {
        public int Id { get; set; }

        /// <summary>
        /// Hangi QRCodeBatch işlemine ait olduğu
        /// </summary>
        public int BatchId { get; set; }

        /// <summary>
        /// Cari hesap partner numarası
        /// </summary>
        public string PartnerNo { get; set; } = string.Empty;

        /// <summary>
        /// QR kod numarası: {QrCodePreCode}-{PartnerNo}
        /// Örnek: G11522-Yd-12001
        /// </summary>
        public string QRCodeNumber { get; set; } = string.Empty;

        /// <summary>
        /// QR kod resim dosyasının yolu (relative path)
        /// Örnek: /Uploads/Qr/2025/G11522-Yd-12001.jpg
        /// </summary>
        public string? QRImagePath { get; set; }

        /// <summary>
        /// QR kodların bulunduğu klasör yolu
        /// Örnek: Uploads/Qr/2025/
        /// </summary>
        public string? FolderPath { get; set; }

        /// <summary>
        /// QR kod yılı
        /// </summary>
        public string QrCodeYear { get; set; } = string.Empty;

        /// <summary>
        /// QRCodeBatch navigation property
        /// </summary>
        public QRCodeBatch? Batch { get; set; }
    }
}
