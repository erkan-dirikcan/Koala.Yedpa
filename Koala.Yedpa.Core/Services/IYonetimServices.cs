using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Yonetim;

namespace Koala.Yedpa.Core.Services
{
    /// <summary>
    /// Arşiv Service Interface
    /// </summary>
    public interface IArsivService
    {
        Task<ResponseDto<List<ArsivDto>>> GetArsivListesiAsync();
        Task<ResponseDto<ArsivDetayDto>> GetKoliDetayAsync(int koliId);
        Task<ResponseDto<bool>> AddRafAsync(string rafKod);
        Task<ResponseDto<bool>> AddBolmeAsync(int rafId, string bolmeNo);
        Task<ResponseDto<bool>> AddKoliAsync(int bolmeId, string koliNo, string? detay);
        Task<ResponseDto<bool>> UpdateKoliAsync(int koliId, string koliNo, string? detay);
        Task<ResponseDto<bool>> DeleteKoliAsync(int koliId);
    }

    /// <summary>
    /// Sözleşme Service Interface
    /// </summary>
    public interface ISozlesmeService
    {
        Task<ResponseDto<List<SozlesmeListDto>>> GetAllAsync();
        Task<ResponseDto<SozlesmeDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<SozlesmeListDto>>> GetExpiringAsync(int gun);
        Task<ResponseDto<SozlesmeDto>> CreateAsync(SozlesmeCreateDto dto);
        Task<ResponseDto<SozlesmeDto>> UpdateAsync(SozlesmeUpdateDto dto);
        Task<ResponseDto> DeleteAsync(int id);
        Task<ResponseDto<byte[]>> GetPdfAsync(int id);
        Task<ResponseDto> UpdateDurumAsync(SozlesmeDurumUpdateDto dto);
    }

    /// <summary>
    /// Arıza Service Interface
    /// </summary>
    public interface IArizaService
    {
        Task<ResponseDto<List<ArizaListDto>>> GetAllAsync();
        Task<ResponseDto<ArizaDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<ArizaListDto>>> GetByBirimAsync(string birim);
        Task<ResponseDto<List<ArizaListDto>>> GetActiveFaultsAsync();
        Task<ResponseDto<ArizaDto>> CreateAsync(ArizaCreateDto dto);
        Task<ResponseDto> UpdateDurumAsync(ArizaDurumUpdateDto dto);
        Task<ResponseDto> AddHareketAsync(ArizaHareketEkleDto dto);
        Task<ResponseDto> DeleteAsync(int id);
    }

    /// <summary>
    /// Otopark Service Interface
    /// </summary>
    public interface IOtoparkService
    {
        Task<ResponseDto<List<OtoparkListDto>>> GetAllAsync();
        Task<ResponseDto<List<OtoparkListDto>>> GetActiveAsync();
        Task<ResponseDto<OtoparkDto>> GetByPlakaAsync(string plaka);
        Task<ResponseDto> GirisYapAsync(OtoparkGirisDto dto);
        Task<ResponseDto> CikisYapAsync(OtoparkCikisDto dto);
        Task<ResponseDto> AboneEkleAsync(OtoparkAboneDto dto);
        Task<ResponseDto> AboneGuncelleAsync(OtoparkAboneDto dto);
        Task<ResponseDto> AboneSilAsync(int id);
    }

    /// <summary>
    /// Ortak Service Interface
    /// </summary>
    public interface IOrtakService
    {
        Task<ResponseDto<List<Core.Models.Yonetim.Birim>>> GetAllBirimlerAsync();
        Task<ResponseDto<List<Core.Models.Yonetim.Mail>>> GetAllMailAdresleriAsync();
        Task<ResponseDto<Core.Models.Yonetim.Mail>> GetMailByIdAsync(int id);
        Task<ResponseDto> AddMailAsync(Core.Models.Yonetim.Mail mail);
        Task<ResponseDto> UpdateMailAsync(Core.Models.Yonetim.Mail mail);
        Task<ResponseDto> DeleteMailAsync(int id);
    }
}
