namespace Koala.Yedpa.Core.Dtos.Yonetim
{
    /// <summary>
    /// Sözleşme listesi DTO'su
    /// </summary>
    public class SozlesmeListDto
    {
        public int SozlesmeID { get; set; }
        public string? Firma { get; set; }
        public string? Konu { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }
        public string? Durum { get; set; }
        public int KalanGun { get; set; }
        public bool Yaklasan => KalanGun <= 30 && KalanGun > 0;
        public bool SuresiDoldu => KalanGun <= 0;
    }

    /// <summary>
    /// Sözleşme detay DTO'su
    /// </summary>
    public class SozlesmeDto
    {
        public int SozlesmeID { get; set; }
        public string? Firma { get; set; }
        public string? Konu { get; set; }
        public string? Tur { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }
        public string? Birim { get; set; }
        public bool AzKalda { get; set; }
        public bool Bitti { get; set; }
        public DateTime? SonTarih { get; set; }
        public string? SonKisi { get; set; }
        public bool Gizli { get; set; }
        public bool Arsiv { get; set; }
        public string? PdfBase64 { get; set; }
        public List<SozlesmeKisiDto> IlgiliKisiler { get; set; } = new();
    }

    /// <summary>
    /// Sözleşme ilgili kişi DTO'su
    /// </summary>
    public class SozlesmeKisiDto
    {
        public int SozlesmeKisiID { get; set; }
        public int SozlesmeID { get; set; }
        public int MailID { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? EPosta { get; set; }
        public string? GSM { get; set; }
    }

    /// <summary>
    /// Yeni sözleşme oluşturma DTO'su
    /// </summary>
    public class SozlesmeCreateDto
    {
        public string? Firma { get; set; }
        public string? Konu { get; set; }
        public string? Tur { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }
        public string? Birim { get; set; }
        public List<int> IlgiliKisiIds { get; set; } = new();
        public byte[]? Pdf { get; set; }
    }

    /// <summary>
    /// Sözleşme güncelleme DTO'su
    /// </summary>
    public class SozlesmeUpdateDto : SozlesmeCreateDto
    {
        public int SozlesmeID { get; set; }
    }

    /// <summary>
    /// Sözleşme durum güncelleme DTO'su
    /// </summary>
    public class SozlesmeDurumUpdateDto
    {
        public int SozlesmeID { get; set; }
        public bool Bitti { get; set; }
        public string? SonKisi { get; set; }
    }
}
