using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models
{
    [Table("BulkInvoiceItems", Schema = "dbo")]
    public class BulkInvoiceItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SessionId { get; set; }

        [Required]
        public int OrficheRef { get; set; }

        [Required]
        public int Orflineref { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; }

        [Required]
        [StringLength(200)]
        public string ClientName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string MonthName { get; set; }

        [Required]
        public BulkInvoiceItemStatus Status { get; set; } = BulkInvoiceItemStatus.Pending;

        public int? LogoInvoiceRef { get; set; }

        [StringLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>Kaç kez aktarım denendi.</summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>"Tekrar dene" — token/geçici hatada true; kuyruk sonrası yeniden denenir.</summary>
        public bool CanRetry { get; set; } = false;

        /// <summary>Neden aktarılmadığına dair iş açıklaması.</summary>
        [StringLength(500)]
        public string? Note { get; set; }

        /// <summary>REST service'ten dönen ham hata.</summary>
        [StringLength(2000)]
        public string? RestError { get; set; }

        [ForeignKey("SessionId")]
        public virtual BulkInvoiceSession Session { get; set; }
    }
}
