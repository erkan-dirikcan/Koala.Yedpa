using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koala.Yedpa.Core.Models.Yonetim
{
    /// <summary>
    /// Sozlesme - Sözleşme yönetimi entity'si
    /// </summary>
    [Table("SOZLESME", Schema = "dbo")]
    public class Sozlesme : CommonProperties
    {
        [Key]
        [Column("SOZLESMEID")]
        public int SozlesmeID { get; set; }

        [Column("FIRMA")]
        [MaxLength(200)]
        public string? Firma { get; set; }

        [Column("KONU")]
        [MaxLength(500)]
        public string? Konu { get; set; }

        [Column("TUR")]
        [MaxLength(100)]
        public string? Tur { get; set; }

        [Column("BASLANGIC", TypeName = "date")]
        public DateTime Baslangic { get; set; }

        [Column("BITIS", TypeName = "date")]
        public DateTime Bitis { get; set; }

        [Column("BIRIM")]
        [MaxLength(100)]
        public string? Birim { get; set; }

        [Column("AZKALDA")]
        public bool AzKalda { get; set; }

        [Column("BITTI")]
        public bool Bitti { get; set; }

        [Column("SONTARIH", TypeName = "date")]
        public DateTime? SonTarih { get; set; }

        [Column("SONKISI")]
        [MaxLength(100)]
        public string? SonKisi { get; set; }

        [Column("GIZLI")]
        public bool Gizli { get; set; }

        [Column("ARSIV")]
        public bool Arsiv { get; set; }

        [Column("PDF")]
        public byte[]? Pdf { get; set; }

        public virtual ICollection<SozlesmeKisi>? IlgiliKisiler { get; set; }
    }

    /// <summary>
    /// SozlesmeKisi - Sözleşme ilgili kişi entity'si
    /// </summary>
    [Table("sozlesmekisi", Schema = "dbo")]
    public class SozlesmeKisi
    {
        [Key]
        [Column("SOZLESMEKISIID")]
        public int SozlesmeKisiID { get; set; }

        [Column("SOZLESMEID")]
        public int SozlesmeID { get; set; }

        [Column("MAILID")]
        public int MailID { get; set; }

        [ForeignKey("SozlesmeID")]
        public virtual Sozlesme? Sozlesme { get; set; }

        [ForeignKey("MailID")]
        public virtual Mail? Mail { get; set; }
    }
}
