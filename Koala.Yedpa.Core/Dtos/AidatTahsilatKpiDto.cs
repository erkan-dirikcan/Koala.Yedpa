namespace Koala.Yedpa.Core.Dtos
{
    public class AidatTahsilatKpiDto
    {
        public decimal ToplamAlacak { get; set; }
        public decimal Odenen { get; set; }
        public decimal Bekleyen { get; set; }
        public string Ay { get; set; }     // "Ocak", "Şubat" etc.
        public int Yil { get; set; }
    }
}
