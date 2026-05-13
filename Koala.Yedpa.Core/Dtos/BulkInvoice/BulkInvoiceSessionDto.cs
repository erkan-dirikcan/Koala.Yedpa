namespace Koala.Yedpa.Core.Dtos.BulkInvoice
{
    /// <summary>
    /// Toplu fatura oturumu DTO'su
    /// </summary>
    public class BulkInvoiceSessionDto
    {
        /// <summary>
        /// Oturum ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fatura tarihi
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Ay (1-12)
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Yıl
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Durum: 0=Pending, 1=Processing, 2=Completed, 3=Failed
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Durum metni
        /// </summary>
        public string StatusText => Status switch
        {
            0 => "Bekliyor",
            1 => "İşleniyor",
            2 => "Tamamlandı",
            3 => "Başarısız",
            _ => "Bilinmiyor"
        };

        /// <summary>
        /// Oluşturan kullanıcı
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Oluşturma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Tamamlanma tarihi
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Toplam kayıt sayısı
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Tamamlanan kayıt sayısı
        /// </summary>
        public int CompletedItems { get; set; }

        /// <summary>
        /// Başarısız kayıt sayısı
        /// </summary>
        public int FailedItems { get; set; }

        /// <summary>
        /// İlerleme yüzdesi
        /// </summary>
        public decimal ProgressPercentage => TotalItems > 0 ? (decimal)CompletedItems / TotalItems * 100 : 0;
    }
}
