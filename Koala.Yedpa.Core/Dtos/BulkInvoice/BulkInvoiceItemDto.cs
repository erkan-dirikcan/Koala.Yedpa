namespace Koala.Yedpa.Core.Dtos.BulkInvoice
{
    /// <summary>
    /// Yönetim sayfası için tek aktarım satırı (crosstable kaydı).
    /// </summary>
    public class BulkInvoiceItemDto
    {
        public int Id { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string MonthName { get; set; } = string.Empty;

        /// <summary>0=Bekliyor, 1=Aktarıldı, 2=Başarısız</summary>
        public int Status { get; set; }
        public int? LogoInvoiceRef { get; set; }
        public int RetryCount { get; set; }
        public bool CanRetry { get; set; }
        public string? Note { get; set; }
        public string? RestError { get; set; }
    }
}
