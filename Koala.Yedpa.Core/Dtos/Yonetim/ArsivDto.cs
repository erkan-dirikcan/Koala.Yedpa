namespace Koala.Yedpa.Core.Dtos.Yonetim
{
    /// <summary>
    /// Arşiv listesi DTO'su
    /// </summary>
    public class ArsivDto
    {
        public int RafID { get; set; }
        public string? RafKod { get; set; }
        public int BolmeID { get; set; }
        public string? BolmeNo { get; set; }
        public int KoliID { get; set; }
        public string? KoliNo { get; set; }
        public string? Detay { get; set; }
    }

    /// <summary>
    /// Arşiv detay DTO'su
    /// </summary>
    public class ArsivDetayDto : ArsivDto
    {
        public string? Icerik { get; set; }
        public int ToplamEsya { get; set; }
    }

    /// <summary>
    /// Raf DTO'su
    /// </summary>
    public class RafDto
    {
        public int RafID { get; set; }
        public string? RafKod { get; set; }
        public int BolmeSayisi { get; set; }
        public int KoliSayisi { get; set; }
    }

    /// <summary>
    /// Yeni koli oluşturma DTO'su
    /// </summary>
    public class KoliCreateDto
    {
        public int BolmeID { get; set; }
        public string? KoliNo { get; set; }
        public string? Detay { get; set; }
    }
}
