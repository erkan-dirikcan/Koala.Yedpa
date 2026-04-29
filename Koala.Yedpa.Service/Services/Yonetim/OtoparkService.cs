using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services.Yonetim;

/// <summary>
/// Otopark Service Implementation
/// </summary>
public class OtoparkService : IOtoparkService
{
    private readonly IOtoparkRepository _otoparkRepository;
    private readonly ILogger<OtoparkService> _logger;

    public OtoparkService(
        IOtoparkRepository otoparkRepository,
        ILogger<OtoparkService> logger)
    {
        _otoparkRepository = otoparkRepository;
        _logger = logger;
    }

    public async Task<ResponseDto<List<OtoparkListDto>>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Getting tüm otopark kayıtları");

            var result = await _otoparkRepository.GetAllAsync();

            return ResponseDto<List<OtoparkListDto>>.SuccessData(
                200,
                "Otopark kayıtları başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting otopark kayıtları");
            return ResponseDto<List<OtoparkListDto>>.FailData(
                500,
                "Otopark kayıtları getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<List<OtoparkListDto>>> GetActiveAsync()
    {
        try
        {
            _logger.LogDebug("Getting aktif otopark kayıtları");

            var result = await _otoparkRepository.GetActiveSubscriptionsAsync();

            return ResponseDto<List<OtoparkListDto>>.SuccessData(
                200,
                "Aktif otopark kayıtları başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting aktif otopark kayıtları");
            return ResponseDto<List<OtoparkListDto>>.FailData(
                500,
                "Aktif otopark kayıtları getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<OtoparkDto>> GetByPlakaAsync(string plaka)
    {
        try
        {
            _logger.LogDebug("Getting otopark by plaka: {Plaka}", plaka);

            var kayit = await _otoparkRepository.GetByPlakaAsync(plaka);
            if (kayit == null)
            {
                return ResponseDto<OtoparkDto>.FailData(
                    404,
                    "Kayıt bulunamadı",
                    $"Plaka: {plaka}",
                    false
                );
            }

            var dto = MapToDto(kayit);

            return ResponseDto<OtoparkDto>.SuccessData(
                200,
                "Otopark kaydı başarıyla getirildi",
                dto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting otopark by plaka: {Plaka}", plaka);
            return ResponseDto<OtoparkDto>.FailData(
                500,
                "Otopark kaydı getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> GirisYapAsync(OtoparkGirisDto dto)
    {
        try
        {
            _logger.LogDebug("Otopark giriş: {Plaka}", dto.Plaka);

            // Aktif kayıt var mı kontrol et
            var mevcutKayit = await _otoparkRepository.GetByPlakaAsync(dto.Plaka);
            if (mevcutKayit != null && mevcutKayit.CikisTarih == null)
            {
                return ResponseDto.Fail(
                    400,
                    "Araç zaten içinde",
                    $"Plaka: {dto.Plaka}",
                    false
                );
            }

            var kayit = new OtoparkKayit
            {
                Plaka = dto.Plaka,
                AboneAd = dto.AboneAd,
                Telefon = dto.Telefon
            };

            await _otoparkRepository.GirisYapAsync(kayit);

            return ResponseDto.Success(200, "Giriş başarıyla kaydedildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error otopark giriş: {Plaka}", dto.Plaka);
            return ResponseDto.Fail(
                500,
                "Giriş kaydı başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> CikisYapAsync(OtoparkCikisDto dto)
    {
        try
        {
            _logger.LogDebug("Otopark çıkış: {Plaka}", dto.Plaka);

            var result = await _otoparkRepository.CikisYapAsync(dto.Plaka);
            if (!result)
            {
                return ResponseDto.Fail(
                    404,
                    "Aktif kayıt bulunamadı",
                    $"Plaka: {dto.Plaka}",
                    false
                );
            }

            return ResponseDto.Success(200, "Çıkış başarıyla kaydedildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error otopark çıkış: {Plaka}", dto.Plaka);
            return ResponseDto.Fail(
                500,
                "Çıkış kaydı başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> AboneEkleAsync(OtoparkAboneDto dto)
    {
        try
        {
            _logger.LogDebug("Adding otopark abone: {Plaka}", dto.Plaka);

            var kayit = new OtoparkKayit
            {
                Plaka = dto.Plaka,
                AboneAd = dto.AboneAd,
                Telefon = dto.Telefon,
                GirisTarih = dto.Baslangic
                // CikisTarih abonelik sonunda olabilir ama abonelik için null kalacak
            };

            await _otoparkRepository.AboneEkleAsync(kayit);

            return ResponseDto.Success(201, "Abonelik başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding abone: {Plaka}", dto.Plaka);
            return ResponseDto.Fail(
                500,
                "Abonelik oluşturma başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> AboneGuncelleAsync(OtoparkAboneDto dto)
    {
        try
        {
            _logger.LogDebug("Updating otopark abone");

            // Plaka ile aktif kaydı bul
            var kayit = await _otoparkRepository.GetByPlakaAsync(dto.Plaka);
            if (kayit == null)
            {
                return ResponseDto.Fail(
                    404,
                    "Abonelik bulunamadı",
                    $"Plaka: {dto.Plaka}",
                    false
                );
            }

            kayit.AboneAd = dto.AboneAd;
            kayit.Telefon = dto.Telefon;
            // Giriş tarihini güncelleme (abonelik yenileme)
            kayit.GirisTarih = dto.Baslangic;

            await _otoparkRepository.AboneGuncelleAsync(kayit);

            return ResponseDto.Success(200, "Abonelik başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating abone: {Plaka}", dto.Plaka);
            return ResponseDto.Fail(
                500,
                "Abonelik güncelleme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> AboneSilAsync(int id)
    {
        try
        {
            _logger.LogDebug("Deleting otopark abone: {Id}", id);

            var result = await _otoparkRepository.AboneSilAsync(id);
            if (!result)
            {
                return ResponseDto.Fail(
                    404,
                    "Abonelik bulunamadı",
                    $"KayitID: {id}",
                    false
                );
            }

            return ResponseDto.Success(200, "Abonelik başarıyla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting abone: {Id}", id);
            return ResponseDto.Fail(
                500,
                "Abonelik silme başarısız",
                ex.Message,
                true
            );
        }
    }

    private OtoparkDto MapToDto(OtoparkKayit kayit)
    {
        return new OtoparkDto
        {
            KayitID = kayit.KayitID,
            Plaka = kayit.Plaka,
            GirisTarih = kayit.GirisTarih,
            CikisTarih = kayit.CikisTarih,
            AboneAd = kayit.AboneAd,
            Telefon = kayit.Telefon
        };
    }
}
