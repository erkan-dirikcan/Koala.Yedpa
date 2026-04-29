using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koala.Yedpa.Core.Models.Yonetim
{
    /// <summary>
    /// Ariza - Arıza/fatura takip entity'si
    /// </summary>
    [Table("ARIZA", Schema = "dbo")]
    public class Ariza : CommonProperties
    {
        [Key]
        [Column("ARIZAID")]
        public int ArizaID { get; set; }

        [Column("FIRMA-ADRES")]
        [MaxLength(500)]
        public string? FirmaAdres { get; set; }

        [Column("KONU")]
        [MaxLength(500)]
        public string? Konu { get; set; }

        [Column("TARIH", TypeName = "datetime")]
        public DateTime Tarih { get; set; }

        [Column("BIRIM")]
        [MaxLength(100)]
        public string? Birim { get; set; }

        [Column("DURUM")]
        [MaxLength(100)]
        public string? Durum { get; set; }

        [Column("SONTARIH", TypeName = "date")]
        public DateTime? SonTarih { get; set; }

        [Column("SONKISI")]
        [MaxLength(100)]
        public string? SonKisi { get; set; }

        [Column("GIZLI")]
        public bool Gizli { get; set; }

        [NotMapped]
        public bool Bitti => Durum == "Tamamlandı";

        public virtual ICollection<ArizaHareket>? Hareketler { get; set; }
        public virtual ICollection<ArizaKisi>? IlgiliKisiler { get; set; }
    }

    /// <summary>
    /// ArizaHareket - Arıza hareket/fiş entity'si
    /// </summary>
    [Table("ARIZAHAREKET", Schema = "dbo")]
    public class ArizaHareket
    {
        [Key]
        [Column("HAREKETID")]
        public int HareketID { get; set; }

        [Column("ARIZAID")]
        public int ArizaID { get; set; }

        [Column("ACIKLAMA")]
        [MaxLength(1000)]
        public string? Aciklama { get; set; }

        [Column("TARIH", TypeName = "datetime")]
        public DateTime Tarih { get; set; }

        [Column("KISI")]
        [MaxLength(100)]
        public string? Kisi { get; set; }

        [ForeignKey("ArizaID")]
        public virtual Ariza? Ariza { get; set; }
    }

    /// <summary>
    /// ArizaKisi - Arıza ilgili kişi entity'si
    /// </summary>
    [Table("arizakisi", Schema = "dbo")]
    public class ArizaKisi
    {
        [Key]
        [Column("ARIZAKISIID")]
        public int ArizaKisiID { get; set; }

        [Column("ARIZAID")]
        public int ArizaID { get; set; }

        [Column("MAILID")]
        public int MailID { get; set; }

        [ForeignKey("ArizaID")]
        public virtual Ariza? Ariza { get; set; }

        [ForeignKey("MailID")]
        public virtual Mail? Mail { get; set; }
    }
}
