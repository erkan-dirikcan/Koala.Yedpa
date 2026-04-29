using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koala.Yedpa.Core.Models.Yonetim
{
    /// <summary>
    /// OtoparkKayit - Otopark kayıt entity'si
    /// </summary>
    [Table("kayit", Schema = "dbo")]
    public class OtoparkKayit : CommonProperties
    {
        [Key]
        [Column("KAYITID")]
        public int KayitID { get; set; }

        [Column("PLAKA")]
        [MaxLength(20)]
        public string? Plaka { get; set; }

        [Column("GIRISTARIH", TypeName = "datetime")]
        public DateTime? GirisTarih { get; set; }

        [Column("CIKISTARIH", TypeName = "datetime")]
        public DateTime? CikisTarih { get; set; }

        [Column("ABONEAD")]
        [MaxLength(200)]
        public string? AboneAd { get; set; }

        [Column("TELEFON")]
        [MaxLength(20)]
        public string? Telefon { get; set; }

        [NotMapped]
        public bool Aktif => CikisTarih == null;
    }
}
