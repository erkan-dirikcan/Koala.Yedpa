using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koala.Yedpa.Core.Models.Yonetim
{
    /// <summary>
    /// Raf - Arşiv sistemi Raf entity'si
    /// </summary>
    [Table("RAF", Schema = "dbo")]
    public class Raf : CommonProperties
    {
        [Key]
        [Column("RAFID")]
        public int RafID { get; set; }

        [Column("RAFKOD")]
        [MaxLength(50)]
        public string? RafKod { get; set; }

        public virtual ICollection<Bolme>? Bolumeler { get; set; }
    }

    /// <summary>
    /// Bolme - Arşiv sistemi Bölme entity'si
    /// </summary>
    [Table("BOLME", Schema = "dbo")]
    public class Bolme : CommonProperties
    {
        [Key]
        [Column("BOLMEID")]
        public int BolmeID { get; set; }

        [Column("RAFID")]
        public int RafID { get; set; }

        [Column("BOLMENO")]
        [MaxLength(50)]
        public string? BolmeNo { get; set; }

        [ForeignKey("RafID")]
        public virtual Raf? Raf { get; set; }

        public virtual ICollection<Koli>? Koliler { get; set; }
    }

    /// <summary>
    /// Koli - Arşiv sistemi Koli entity'si
    /// </summary>
    [Table("KOLI", Schema = "dbo")]
    public class Koli : CommonProperties
    {
        [Key]
        [Column("KOLIID")]
        public int KoliID { get; set; }

        [Column("BOLMEID")]
        public int BolmeID { get; set; }

        [Column("KOLINO")]
        [MaxLength(50)]
        public string? KoliNo { get; set; }

        [Column("detay")]
        [MaxLength(500)]
        public string? Detay { get; set; }

        [ForeignKey("BolmeID")]
        public virtual Bolme? Bolme { get; set; }
    }
}
