namespace Koala.Yedpa.Core.Dtos.Yonetim
{
    /// <summary>
    /// Arıza listesi DTO'su
    /// </summary>
    public class ArizaListDto
    {
        public int ArizaID { get; set; }
        public string? Konu { get; set; }
        public DateTime Tarih { get; set; }
        public string? Birim { get; set; }
        public string? Durum { get; set; }
        public string? SonKisi { get; set; }
        public bool Gizli { get; set; }
        public int HareketSayisi { get; set; }
    }

    /// <summary>
    /// Arıza detay DTO'su
    /// </summary>
    public class ArizaDto
    {
        public int ArizaID { get; set; }
        public string? FirmaAdres { get; set; }
        public string? Konu { get; set; }
        public DateTime Tarih { get; set; }
        public string? Birim { get; set; }
        public string? Durum { get; set; }
        public DateTime? SonTarih { get; set; }
        public string? SonKisi { get; set; }
        public bool Gizli { get; set; }
        public List<ArizaHareketDto> Hareketler { get; set; } = new();
        public List<ArizaKisiDto> IlgiliKisiler { get; set; } = new();
    }

    /// <summary>
    /// Arıza hareket DTO'su
    /// </summary>
    public class ArizaHareketDto
    {
        public int HareketID { get; set; }
        public int ArizaID { get; set; }
        public string? Aciklama { get; set; }
        public DateTime Tarih { get; set; }
        public string? Kisi { get; set; }
    }

    /// <summary>
    /// Arıza ilgili kişi DTO'su
    /// </summary>
    public class ArizaKisiDto
    {
        public int ArizaKisiID { get; set; }
        public int ArizaID { get; set; }
        public int MailID { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? EPosta { get; set; }
        public string? GSM { get; set; }
    }

    /// <summary>
    /// Yeni arıza oluşturma DTO'su
    /// </summary>
    public class ArizaCreateDto
    {
        public string? FirmaAdres { get; set; }
        public string? Konu { get; set; }
        public string? Birim { get; set; }
        public string? Aciklama { get; set; }
        public List<int> IlgiliKisiIds { get; set; } = new();
    }

    /// <summary>
    /// Arıza durum güncelleme DTO'su
    /// </summary>
    public class ArizaDurumUpdateDto
    {
        public int ArizaID { get; set; }
        public string? Durum { get; set; }
        public string? SonKisi { get; set; }
    }

    /// <summary>
    /// Arıza hareket ekleme DTO'su
    /// </summary>
    public class ArizaHareketEkleDto
    {
        public int ArizaID { get; set; }
        public string? Aciklama { get; set; }
        public string? Kisi { get; set; }
    }
}
