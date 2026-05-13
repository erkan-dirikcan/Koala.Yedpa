namespace Koala.Yedpa.Core.Dtos.BulkInvoice
{
    /// <summary>
    /// Seçili fatura satırı DTO'su
    /// </summary>
    public class SelectedLineDto
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
    }
}
