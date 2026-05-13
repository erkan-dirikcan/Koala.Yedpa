namespace Koala.Yedpa.Core.Dtos.BulkInvoice
{
    /// <summary>
    /// Faturalandırılmamış aidat sipariş satırı DTO'su
    /// </summary>
    public class PendingInvoiceLineDto
    {
        /// <summary>
        /// Logo ORFICHE LOGICALREF
        /// </summary>
        public int OrficheRef { get; set; }

        /// <summary>
        /// Logo ORFLINE LOGICALREF
        /// </summary>
        public int Orflineref { get; set; }

        /// <summary>
        /// Cari kod
        /// </summary>
        public string ClientCode { get; set; } = string.Empty;

        /// <summary>
        /// Cari adı
        /// </summary>
        public string ClientName { get; set; } = string.Empty;

        /// <summary>
        /// Tutar
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Ay adı (HAZIRAN gibi)
        /// </summary>
        public string MonthName { get; set; } = string.Empty;

        /// <summary>
        /// Kapanma durumu (CLOSSED alanı)
        /// </summary>
        public int ClosedStatus { get; set; }

        /// <summary>
        /// Satır no (LINENO_)
        /// </summary>
        public int LineNo { get; set; }

        /// <summary>
        /// Seçili durumu (UI için)
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
