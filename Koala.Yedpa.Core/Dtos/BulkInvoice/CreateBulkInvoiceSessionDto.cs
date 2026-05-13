namespace Koala.Yedpa.Core.Dtos.BulkInvoice
{
    /// <summary>
    /// Toplu fatura oturumu oluşturma DTO'su
    /// </summary>
    public class CreateBulkInvoiceSessionDto
    {
        /// <summary>
        /// Fatura tarihi (hem işlem hem vade tarihi olarak kullanılır)
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Seçili satırlar
        /// </summary>
        public List<SelectedLineDto> SelectedLines { get; set; } = new List<SelectedLineDto>();
    }
}
