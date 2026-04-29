namespace Koala.Yedpa.Core.Dtos.Yonetim
{
    /// <summary>
    /// Otopark listesi DTO'su
    /// </summary>
    public class OtoparkListDto
    {
        public int KayitID { get; set; }
        public string? Plaka { get; set; }
        public DateTime? GirisTarih { get; set; }
        public DateTime? CikisTarih { get; set; }
        public string? AboneAd { get; set; }
        public string? Telefon { get; set; }
        public bool Aktif => CikisTarih == null;
        public string? Durum => CikisTarih == null ? "İçeride" : "Çıktı";
        public double? KalanSaat => GirisTarih.HasValue && CikisTarih == null
            ? (DateTime.Now - GirisTarih.Value).TotalHours
            : null;
    }

    /// <summary>
    /// Otopark detay DTO'su
    /// </summary>
    public class OtoparkDto
    {
        public int KayitID { get; set; }
        public string? Plaka { get; set; }
        public DateTime? GirisTarih { get; set; }
        public DateTime? CikisTarih { get; set; }
        public string? AboneAd { get; set; }
        public string? Telefon { get; set; }
        public bool Aktif => CikisTarih == null;
        public double? KalanSaat => GirisTarih.HasValue && CikisTarih == null
            ? (DateTime.Now - GirisTarih.Value).TotalHours
            : null;
        public double? ToplamSaat => GirisTarih.HasValue && CikisTarih.HasValue
            ? (CikisTarih.Value - GirisTarih.Value).TotalHours
            : null;
    }

    /// <summary>
    /// Otopark giriş DTO'su
    /// </summary>
    public class OtoparkGirisDto
    {
        public string? Plaka { get; set; }
        public string? AboneAd { get; set; }
        public string? Telefon { get; set; }
    }

    /// <summary>
    /// Otopark çıkış DTO'su
    /// </summary>
    public class OtoparkCikisDto
    {
        public string? Plaka { get; set; }
    }

    /// <summary>
    /// Otopark abonelik DTO'su
    /// </summary>
    public class OtoparkAboneDto
    {
        public string? Plaka { get; set; }
        public string? AboneAd { get; set; }
        public string? Telefon { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }
    }

    /// <summary>
    /// Otopark abonelik listesi DTO'su
    /// </summary>
    public class OtoparkAboneListDto
    {
        public int KayitID { get; set; }
        public string? Plaka { get; set; }
        public string? AboneAd { get; set; }
        public string? Telefon { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }
        public int KalanGun => (Bitis - DateTime.Now).Days;
        public bool Aktif => Bitis > DateTime.Now;
    }
}
