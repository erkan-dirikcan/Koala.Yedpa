namespace Koala.Yedpa.Core.Dtos.BulkInvoice
{
    /// <summary>
    /// Dashboard toplu faturalandırma durumu.
    /// İki durumdan biri gösterilir:
    /// - <see cref="ShowAlert"/>: tarih henüz seçilmedi → "tarih seçin" uyarısı.
    /// - <see cref="ShowPlannedPanel"/>: tarih seçildi → "aktarım yapılacak firmaları görüntüle" paneli.
    /// </summary>
    public class AlertCheckResultDto
    {
        /// <summary>
        /// Alert gösterilecek mi? (ayın 15'inden sonra ve gelecek ay için tarih seçilmemişse)
        /// </summary>
        public bool ShowAlert { get; set; }

        /// <summary>
        /// Alert mesajı
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Mevcut ay
        /// </summary>
        public int CurrentMonth { get; set; }

        /// <summary>
        /// Mevcut yıl
        /// </summary>
        public int CurrentYear { get; set; }

        /// <summary>
        /// Yaklaşan (henüz tarihi geçmemiş) bir aktarım oturumu var mı?
        /// Varsa dashboard'da alert yerine "Aktarım Yapılacak Firmaları Görüntüle" paneli çıkar.
        /// </summary>
        public bool ShowPlannedPanel { get; set; }

        /// <summary>
        /// Yaklaşan oturumun ID'si (panel butonu bu oturuma derin bağlantı verir).
        /// </summary>
        public int? SessionId { get; set; }

        /// <summary>
        /// Yaklaşan oturumun aktarım tarihi.
        /// </summary>
        public DateTime? TransferDate { get; set; }
    }
}
