using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models
{
    [Table("BulkInvoiceSessions", Schema = "dbo")]
    public class BulkInvoiceSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime InvoiceDate { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public BulkInvoiceSessionStatus Status { get; set; } = BulkInvoiceSessionStatus.Pending;

        [Required]
        [StringLength(200)]
        public string CreatedBy { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime? CompletedAt { get; set; }

        /// <summary>Hangfire bilgi-maili job id (T-1 08:00). İptal/güncelleme için saklanır.</summary>
        [StringLength(100)]
        public string? InfoJobId { get; set; }

        /// <summary>Hangfire aktarım job id (T 00:01).</summary>
        [StringLength(100)]
        public string? TransferJobId { get; set; }

        public virtual ICollection<BulkInvoiceItem> Items { get; set; } = new HashSet<BulkInvoiceItem>();
    }
}
