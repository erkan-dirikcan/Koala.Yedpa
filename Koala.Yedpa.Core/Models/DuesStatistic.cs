using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models
{
    [Table("DuesStatistics", Schema = "dbo")]
    public class DuesStatistic : CommonProperties
    {
        [Key]
        public string Id { get; set; }

        public string? BuggetRatioId { get; set; }
        [Required]
        [StringLength(50)]
        public string Code { get; set; } // KOD1 karşılığı

        [Required]
        [StringLength(10)]
        public string Year { get; set; }// İşlem Filtresi İçin Eklendi

        [Required]
        [StringLength(50)]
        public string DivCode { get; set; } // ISYERI karşılığı

        [StringLength(255)]
        public string DivName { get; set; } // ISYERIADI karşılığı

        [Required]
        public long DocTrackingNr { get; set; } // DOCTRACKINGNR karşılığı

        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } // UYEFIRMA karşılığı

        [Required]
        public long ClientRef { get; set; } // CLIENTREF karşılığı

        [Required]
        [StringLength(20)]
        public BuggetTypeEnum BudgetType { get; set; } // "Budget" veya "ExtraBudget"

        [Column(TypeName = "decimal(18,2)")]
        public decimal January { get; set; } // OCAK

        [Column(TypeName = "decimal(18,2)")]
        public decimal February { get; set; } // SUBAT

        [Column(TypeName = "decimal(18,2)")]
        public decimal March { get; set; } // MART

        [Column(TypeName = "decimal(18,2)")]
        public decimal April { get; set; } // NISAN

        [Column(TypeName = "decimal(18,2)")]
        public decimal May { get; set; } // MAYIS

        [Column(TypeName = "decimal(18,2)")]
        public decimal June { get; set; } // HAZIRAN

        [Column(TypeName = "decimal(18,2)")]
        public decimal July { get; set; } // TEMMUZ

        [Column(TypeName = "decimal(18,2)")]
        public decimal August { get; set; } // AGUSTOS

        [Column(TypeName = "decimal(18,2)")]
        public decimal September { get; set; } // EYLUL

        [Column(TypeName = "decimal(18,2)")]
        public decimal October { get; set; } // EKIM

        [Column(TypeName = "decimal(18,2)")]
        public decimal November { get; set; } // KASIM

        [Column(TypeName = "decimal(18,2)")]
        public decimal December { get; set; } // ARALIK

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; } // TOPLAM
        public TransferStatusEnum TransferStatus { get; set; } // AKTARIM DURUMU



        // Constructor
        public DuesStatistic()
        {
            Id = Tools.CreateGuidStr();
            BudgetType = BuggetTypeEnum.Budget; // Varsayılan değer
            Status = StatusEnum.Active;
        }
    }
}
