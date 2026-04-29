using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koala.Yedpa.Core.Models.Yonetim
{
    /// <summary>
    /// Mail - İletişim bilgileri entity'si
    /// </summary>
    [Table("mail", Schema = "dbo")]
    public class Mail
    {
        [Key]
        [Column("mailid")]
        public int MailID { get; set; }

        [Column("ad")]
        [MaxLength(100)]
        public string? Ad { get; set; }

        [Column("soyad")]
        [MaxLength(100)]
        public string? Soyad { get; set; }

        [Column("eposta")]
        [MaxLength(200)]
        public string? EPosta { get; set; }

        [Column("gsm")]
        [MaxLength(20)]
        public string? GSM { get; set; }

        [Column("telefon")]
        [MaxLength(20)]
        public string? Telefon { get; set; }
    }

    /// <summary>
    /// Birim - Departman/birim entity'si
    /// </summary>
    [Table("BIRIM", Schema = "dbo")]
    public class Birim
    {
        [Key]
        [Column("BIRIMID")]
        public int BirimID { get; set; }

        [Column("BIRIMADI")]
        [MaxLength(200)]
        public string? BirimAdi { get; set; }
    }

    /// <summary>
    /// Durum - Durum kodları entity'si
    /// </summary>
    [Table("DURUM", Schema = "dbo")]
    public class Durum
    {
        [Key]
        [Column("DURUMID")]
        public int DurumID { get; set; }

        [Column("DURUMADI")]
        [MaxLength(100)]
        public string? DurumAdi { get; set; }
    }
}
