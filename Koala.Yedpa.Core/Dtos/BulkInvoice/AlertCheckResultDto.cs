namespace Koala.Yedpa.Core.Dtos.BulkInvoice
{
    /// <summary>
    /// Alert kontrol sonucu DTO'su
    /// </summary>
    public class AlertCheckResultDto
    {
        /// <summary>
        /// Alert gösterilecek mi?
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
    }
}
