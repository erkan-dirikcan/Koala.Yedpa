using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Models.Yonetim;

namespace Koala.Yedpa.Core.Repositories
{
    /// <summary>
    /// Arşiv Repository Interface
    /// </summary>
    public interface IArsivRepository
    {
        // Raf işlemleri
        Task<List<Raf>> GetAllRaflarAsync();
        Task<Raf?> GetRafByIdAsync(int id);
        Task<bool> AddRafAsync(Raf raf);
        Task<bool> UpdateRafAsync(Raf raf);
        Task<bool> DeleteRafAsync(int id);

        // Bölme işlemleri
        Task<List<Bolme>> GetBolumelerByRafAsync(int rafId);
        Task<Bolme?> GetBolmeByIdAsync(int id);
        Task<bool> AddBolmeAsync(Bolme bolme);
        Task<bool> UpdateBolmeAsync(Bolme bolme);
        Task<bool> DeleteBolmeAsync(int id);

        // Koli işlemleri
        Task<List<Koli>> GetKolilerByRafAsync(int rafId);
        Task<List<Koli>> GetKolilerByBolmeAsync(int bolmeId);
        Task<Koli?> GetKoliByIdAsync(int id);
        Task<bool> AddKoliAsync(Koli koli);
        Task<bool> UpdateKoliAsync(Koli koli);
        Task<bool> DeleteKoliAsync(int id);

        // Arşiv listesi
        Task<List<ArsivDto>> GetArsivListesiAsync();
        Task<ArsivDetayDto?> GetKoliDetayAsync(int koliId);
    }

    /// <summary>
    /// Sözleşme Repository Interface
    /// </summary>
    public interface ISozlesmeRepository
    {
        Task<List<SozlesmeListDto>> GetAllAsync();
        Task<Sozlesme?> GetByIdAsync(int id);
        Task<List<SozlesmeListDto>> GetExpiringContractsAsync(int days);
        Task<bool> CreateAsync(Sozlesme sozlesme);
        Task<bool> UpdateAsync(Sozlesme sozlesme);
        Task<bool> DeleteAsync(int id);
        Task<byte[]?> GetContractPdfAsync(int sozlesmeId);
        Task<bool> UpdateContractStatusAsync(int id, bool bitti, string? sonKisi);
        Task<bool> AddIlgiliKisiAsync(SozlesmeKisi kisi);
        Task<bool> RemoveIlgiliKisiAsync(int kisiId);
        Task<List<SozlesmeKisi>> GetIlgiliKisilerAsync(int sozlesmeId);
    }

    /// <summary>
    /// Arıza Repository Interface
    /// </summary>
    public interface IArizaRepository
    {
        Task<List<ArizaListDto>> GetAllAsync();
        Task<Ariza?> GetByIdAsync(int id);
        Task<List<ArizaListDto>> GetByBirimAsync(string birim);
        Task<List<ArizaListDto>> GetActiveFaultsAsync();
        Task<bool> CreateAsync(Ariza ariza);
        Task<bool> UpdateAsync(Ariza ariza);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateDurumAsync(int id, string durum, string? sonKisi);
        Task<bool> AddHareketAsync(ArizaHareket hareket);
        Task<List<ArizaHareket>> GetHareketlerAsync(int arizaId);
        Task<bool> AddIlgiliKisiAsync(ArizaKisi kisi);
        Task<bool> RemoveIlgiliKisiAsync(int kisiId);
        Task<List<ArizaKisi>> GetIlgiliKisilerAsync(int arizaId);
    }

    /// <summary>
    /// Otopark Repository Interface
    /// </summary>
    public interface IOtoparkRepository
    {
        Task<List<OtoparkListDto>> GetAllAsync();
        Task<OtoparkKayit?> GetByIdAsync(int id);
        Task<OtoparkKayit?> GetByPlakaAsync(string plaka);
        Task<List<OtoparkListDto>> GetActiveSubscriptionsAsync();
        Task<bool> GirisYapAsync(OtoparkKayit kayit);
        Task<bool> CikisYapAsync(string plaka);
        Task<bool> AboneEkleAsync(OtoparkKayit kayit);
        Task<bool> AboneGuncelleAsync(OtoparkKayit kayit);
        Task<bool> AboneSilAsync(int id);
    }

    /// <summary>
    /// Ortak Repository Interface
    /// </summary>
    public interface IOrtakRepository
    {
        Task<List<Birim>> GetAllBirimlerAsync();
        Task<List<Mail>> GetAllMailAdresleriAsync();
        Task<Mail?> GetMailByIdAsync(int id);
        Task<bool> AddMailAsync(Mail mail);
        Task<bool> UpdateMailAsync(Mail mail);
        Task<bool> DeleteMailAsync(int id);
        Task<List<Durum>> GetAllDurumlarAsync();
    }
}
