using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services.Yonetim;

/// <summary>
/// Arıza Service Implementation
/// </summary>
public class ArizaService : IArizaService
{
    private readonly IArizaRepository _arizaRepository;
    private readonly ILogger<ArizaService> _logger;

    public ArizaService(
        IArizaRepository arizaRepository,
        ILogger<ArizaService> logger)
    {
        _arizaRepository = arizaRepository;
        _logger = logger;
    }

    public async Task<ResponseDto<List<ArizaListDto>>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Getting tüm arızalar");

            var result = await _arizaRepository.GetAllAsync();

            return ResponseDto<List<ArizaListDto>>.SuccessData(
                200,
                "Arızalar başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting arızalar");
            return ResponseDto<List<ArizaListDto>>.FailData(
                500,
                "Arızalar getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<ArizaDto>> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("Getting arıza: {Id}", id);

            var ariza = await _arizaRepository.GetByIdAsync(id);
            if (ariza == null)
            {
                return ResponseDto<ArizaDto>.FailData(
                    404,
                    "Arıza bulunamadı",
                    $"ArizaID: {id}",
                    false
                );
            }

            var dto = MapToDto(ariza);

            return ResponseDto<ArizaDto>.SuccessData(
                200,
                "Arıza başarıyla getirildi",
                dto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting arıza: {Id}", id);
            return ResponseDto<ArizaDto>.FailData(
                500,
                "Arıza getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<List<ArizaListDto>>> GetByBirimAsync(string birim)
    {
        try
        {
            _logger.LogDebug("Getting arızalar by birim: {Birim}", birim);

            var result = await _arizaRepository.GetByBirimAsync(birim);

            return ResponseDto<List<ArizaListDto>>.SuccessData(
                200,
                $"Birim ({birim}) arızaları başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting arızalar by birim: {Birim}", birim);
            return ResponseDto<List<ArizaListDto>>.FailData(
                500,
                "Arızalar getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<List<ArizaListDto>>> GetActiveFaultsAsync()
    {
        try
        {
            _logger.LogDebug("Getting aktif arızalar");

            var result = await _arizaRepository.GetActiveFaultsAsync();

            return ResponseDto<List<ArizaListDto>>.SuccessData(
                200,
                "Aktif arızalar başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting aktif arızalar");
            return ResponseDto<List<ArizaListDto>>.FailData(
                500,
                "Aktif arızalar getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<ArizaDto>> CreateAsync(ArizaCreateDto dto)
    {
        try
        {
            _logger.LogDebug("Creating new arıza: {Konu}", dto.Konu);

            var ariza = new Ariza
            {
                FirmaAdres = dto.FirmaAdres,
                Konu = dto.Konu,
                Tarih = DateTime.Now,
                Birim = dto.Birim,
                Durum = "Beklemede"
            };

            await _arizaRepository.CreateAsync(ariza);

            // İlk hareketi ekle
            if (!string.IsNullOrEmpty(dto.Aciklama))
            {
                await _arizaRepository.AddHareketAsync(new ArizaHareket
                {
                    ArizaID = ariza.ArizaID,
                    Aciklama = dto.Aciklama,
                    Kisi = "Sistem",
                    Tarih = DateTime.Now
                });
            }

            // İlgili kişileri ekle
            foreach (var mailId in dto.IlgiliKisiIds)
            {
                await _arizaRepository.AddIlgiliKisiAsync(new ArizaKisi
                {
                    ArizaID = ariza.ArizaID,
                    MailID = mailId
                });
            }

            var resultDto = MapToDto(ariza);

            return ResponseDto<ArizaDto>.SuccessData(
                201,
                "Arıza başarıyla oluşturuldu",
                resultDto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating arıza");
            return ResponseDto<ArizaDto>.FailData(
                500,
                "Arıza oluşturma başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> UpdateDurumAsync(ArizaDurumUpdateDto dto)
    {
        try
        {
            _logger.LogDebug("Updating arıza durum: {Id}, Durum: {Durum}", dto.ArizaID, dto.Durum);

            var result = await _arizaRepository.UpdateDurumAsync(
                dto.ArizaID,
                dto.Durum,
                dto.SonKisi
            );

            if (!result)
            {
                return ResponseDto.Fail(
                    404,
                    "Arıza bulunamadı",
                    $"ArizaID: {dto.ArizaID}",
                    false
                );
            }

            // Durum değişikliğini harekete ekle
            await _arizaRepository.AddHareketAsync(new ArizaHareket
            {
                ArizaID = dto.ArizaID,
                Aciklama = $"Durum değişti: {dto.Durum}",
                Kisi = dto.SonKisi,
                Tarih = DateTime.Now
            });

            return ResponseDto.Success(200, "Arıza durumu başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating durum: {Id}", dto.ArizaID);
            return ResponseDto.Fail(
                500,
                "Durum güncelleme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> AddHareketAsync(ArizaHareketEkleDto dto)
    {
        try
        {
            _logger.LogDebug("Adding hareket to arıza: {Id}", dto.ArizaID);

            var hareket = new ArizaHareket
            {
                ArizaID = dto.ArizaID,
                Aciklama = dto.Aciklama,
                Kisi = dto.Kisi,
                Tarih = DateTime.Now
            };

            await _arizaRepository.AddHareketAsync(hareket);

            return ResponseDto.Success(200, "Hareket başarıyla eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding hareket: {Id}", dto.ArizaID);
            return ResponseDto.Fail(
                500,
                "Hareket ekleme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> DeleteAsync(int id)
    {
        try
        {
            _logger.LogDebug("Deleting arıza: {Id}", id);

            var result = await _arizaRepository.DeleteAsync(id);
            if (!result)
            {
                return ResponseDto.Fail(
                    404,
                    "Arıza bulunamadı",
                    $"ArizaID: {id}",
                    false
                );
            }

            return ResponseDto.Success(200, "Arıza başarıyla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting arıza: {Id}", id);
            return ResponseDto.Fail(
                500,
                "Arıza silme başarısız",
                ex.Message,
                true
            );
        }
    }

    private ArizaDto MapToDto(Ariza ariza)
    {
        return new ArizaDto
        {
            ArizaID = ariza.ArizaID,
            FirmaAdres = ariza.FirmaAdres,
            Konu = ariza.Konu,
            Tarih = ariza.Tarih,
            Birim = ariza.Birim,
            Durum = ariza.Durum,
            SonTarih = ariza.SonTarih,
            SonKisi = ariza.SonKisi,
            Gizli = ariza.Gizli,
            Hareketler = ariza.Hareketler?.Select(h => new ArizaHareketDto
            {
                HareketID = h.HareketID,
                ArizaID = h.ArizaID,
                Aciklama = h.Aciklama,
                Tarih = h.Tarih,
                Kisi = h.Kisi
            }).OrderByDescending(h => h.Tarih).ToList() ?? new List<ArizaHareketDto>(),
            IlgiliKisiler = ariza.IlgiliKisiler?.Select(k => new ArizaKisiDto
            {
                ArizaKisiID = k.ArizaKisiID,
                ArizaID = k.ArizaID,
                MailID = k.MailID,
                Ad = k.Mail?.Ad,
                Soyad = k.Mail?.Soyad,
                EPosta = k.Mail?.EPosta,
                GSM = k.Mail?.GSM
            }).ToList() ?? new List<ArizaKisiDto>()
        };
    }
}
