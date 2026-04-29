using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services.Yonetim;

/// <summary>
/// Arşiv Service Implementation
/// </summary>
public class ArsivService : IArsivService
{
    private readonly IArsivRepository _arsivRepository;
    private readonly ILogger<ArsivService> _logger;

    public ArsivService(
        IArsivRepository arsivRepository,
        ILogger<ArsivService> logger)
    {
        _arsivRepository = arsivRepository;
        _logger = logger;
    }

    public async Task<ResponseDto<List<ArsivDto>>> GetArsivListesiAsync()
    {
        try
        {
            _logger.LogDebug("Getting arşiv listesi");

            var result = await _arsivRepository.GetArsivListesiAsync();

            return ResponseDto<List<ArsivDto>>.SuccessData(
                200,
                "Arşiv listesi başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting arşiv listesi");
            return ResponseDto<List<ArsivDto>>.FailData(
                500,
                "Arşiv listesi getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<ArsivDetayDto>> GetKoliDetayAsync(int koliId)
    {
        try
        {
            _logger.LogDebug("Getting koli detay: {KoliId}", koliId);

            var result = await _arsivRepository.GetKoliDetayAsync(koliId);
            if (result == null)
            {
                return ResponseDto<ArsivDetayDto>.FailData(
                    404,
                    "Koli bulunamadı",
                    $"KoliID: {koliId}",
                    false
                );
            }

            return ResponseDto<ArsivDetayDto>.SuccessData(
                200,
                "Koli detayı başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting koli detay: {KoliId}", koliId);
            return ResponseDto<ArsivDetayDto>.FailData(
                500,
                "Koli detayı getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<bool>> AddRafAsync(string rafKod)
    {
        try
        {
            _logger.LogDebug("Adding new raf: {RafKod}", rafKod);

            var raf = new Raf { RafKod = rafKod };
            await _arsivRepository.AddRafAsync(raf);

            return ResponseDto<bool>.SuccessData(200, "Raf başarıyla eklendi", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding raf: {RafKod}", rafKod);
            return ResponseDto<bool>.FailData(
                500,
                "Raf ekleme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<bool>> AddBolmeAsync(int rafId, string bolmeNo)
    {
        try
        {
            _logger.LogDebug("Adding new bolme: RafId={RafId}, BolmeNo={BolmeNo}", rafId, bolmeNo);

            var bolme = new Bolme { RafID = rafId, BolmeNo = bolmeNo };
            await _arsivRepository.AddBolmeAsync(bolme);

            return ResponseDto<bool>.SuccessData(200, "Bölme başarıyla eklendi", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding bolme: RafId={RafId}", rafId);
            return ResponseDto<bool>.FailData(
                500,
                "Bölme ekleme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<bool>> AddKoliAsync(int bolmeId, string koliNo, string? detay)
    {
        try
        {
            _logger.LogDebug("Adding new koli: BolmeId={BolmeId}, KoliNo={KoliNo}", bolmeId, koliNo);

            var koli = new Koli { BolmeID = bolmeId, KoliNo = koliNo, Detay = detay };
            await _arsivRepository.AddKoliAsync(koli);

            return ResponseDto<bool>.SuccessData(200, "Koli başarıyla eklendi", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding koli: BolmeId={BolmeId}", bolmeId);
            return ResponseDto<bool>.FailData(
                500,
                "Koli ekleme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<bool>> UpdateKoliAsync(int koliId, string koliNo, string? detay)
    {
        try
        {
            _logger.LogDebug("Updating koli: {KoliId}", koliId);

            var koli = new Koli { KoliID = koliId, KoliNo = koliNo, Detay = detay };
            var result = await _arsivRepository.UpdateKoliAsync(koli);

            if (!result)
            {
                return ResponseDto<bool>.FailData(
                    404,
                    "Koli bulunamadı",
                    $"KoliID: {koliId}",
                    false
                );
            }

            return ResponseDto<bool>.SuccessData(200, "Koli başarıyla güncellendi", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating koli: {KoliId}", koliId);
            return ResponseDto<bool>.FailData(
                500,
                "Koli güncelleme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<bool>> DeleteKoliAsync(int koliId)
    {
        try
        {
            _logger.LogDebug("Deleting koli: {KoliId}", koliId);

            var result = await _arsivRepository.DeleteKoliAsync(koliId);

            if (!result)
            {
                return ResponseDto<bool>.FailData(
                    404,
                    "Koli bulunamadı",
                    $"KoliID: {koliId}",
                    false
                );
            }

            return ResponseDto<bool>.SuccessData(200, "Koli başarıyla silindi", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting koli: {KoliId}", koliId);
            return ResponseDto<bool>.FailData(
                500,
                "Koli silme başarısız",
                ex.Message,
                true
            );
        }
    }
}
