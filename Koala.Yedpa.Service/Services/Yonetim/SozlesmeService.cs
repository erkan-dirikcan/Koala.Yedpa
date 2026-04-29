using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services.Yonetim;

/// <summary>
/// Sözleşme Service Implementation
/// </summary>
public class SozlesmeService : ISozlesmeService
{
    private readonly ISozlesmeRepository _sozlesmeRepository;
    private readonly IOrtakRepository _ortakRepository;
    private readonly ILogger<SozlesmeService> _logger;

    public SozlesmeService(
        ISozlesmeRepository sozlesmeRepository,
        IOrtakRepository ortakRepository,
        ILogger<SozlesmeService> logger)
    {
        _sozlesmeRepository = sozlesmeRepository;
        _ortakRepository = ortakRepository;
        _logger = logger;
    }

    public async Task<ResponseDto<List<SozlesmeListDto>>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Getting tüm sözleşmeler");

            var result = await _sozlesmeRepository.GetAllAsync();

            return ResponseDto<List<SozlesmeListDto>>.SuccessData(
                200,
                "Sözleşmeler başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sözleşmeler");
            return ResponseDto<List<SozlesmeListDto>>.FailData(
                500,
                "Sözleşmeler getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<SozlesmeDto>> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("Getting sözleşme: {Id}", id);

            var sozlesme = await _sozlesmeRepository.GetByIdAsync(id);
            if (sozlesme == null)
            {
                return ResponseDto<SozlesmeDto>.FailData(
                    404,
                    "Sözleşme bulunamadı",
                    $"SozlesmeID: {id}",
                    false
                );
            }

            var dto = MapToDto(sozlesme);

            return ResponseDto<SozlesmeDto>.SuccessData(
                200,
                "Sözleşme başarıyla getirildi",
                dto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sözleşme: {Id}", id);
            return ResponseDto<SozlesmeDto>.FailData(
                500,
                "Sözleşme getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<List<SozlesmeListDto>>> GetExpiringAsync(int gun)
    {
        try
        {
            _logger.LogDebug("Getting yaklaşan sözleşmeler: {Gun} gün", gun);

            var result = await _sozlesmeRepository.GetExpiringContractsAsync(gun);

            return ResponseDto<List<SozlesmeListDto>>.SuccessData(
                200,
                $"{gun} gün içinde bitecek sözleşmeler başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting yaklaşan sözleşmeler");
            return ResponseDto<List<SozlesmeListDto>>.FailData(
                500,
                "Yaklaşan sözleşmeler getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<SozlesmeDto>> CreateAsync(SozlesmeCreateDto dto)
    {
        try
        {
            _logger.LogDebug("Creating new sözleşme: {Firma}", dto.Firma);

            var sozlesme = new Sozlesme
            {
                Firma = dto.Firma,
                Konu = dto.Konu,
                Tur = dto.Tur,
                Baslangic = dto.Baslangic,
                Bitis = dto.Bitis,
                Birim = dto.Birim
            };

            await _sozlesmeRepository.CreateAsync(sozlesme);

            // İlgili kişileri ekle
            foreach (var mailId in dto.IlgiliKisiIds)
            {
                await _sozlesmeRepository.AddIlgiliKisiAsync(new SozlesmeKisi
                {
                    SozlesmeID = sozlesme.SozlesmeID,
                    MailID = mailId
                });
            }

            // PDF varsa kaydet
            if (dto.Pdf != null && dto.Pdf.Length > 0)
            {
                sozlesme.Pdf = dto.Pdf;
                await _sozlesmeRepository.UpdateAsync(sozlesme);
            }

            var resultDto = MapToDto(sozlesme);

            return ResponseDto<SozlesmeDto>.SuccessData(
                201,
                "Sözleşme başarıyla oluşturuldu",
                resultDto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sözleşme");
            return ResponseDto<SozlesmeDto>.FailData(
                500,
                "Sözleşme oluşturma başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<SozlesmeDto>> UpdateAsync(SozlesmeUpdateDto dto)
    {
        try
        {
            _logger.LogDebug("Updating sözleşme: {Id}", dto.SozlesmeID);

            var sozlesme = new Sozlesme
            {
                SozlesmeID = dto.SozlesmeID,
                Firma = dto.Firma,
                Konu = dto.Konu,
                Tur = dto.Tur,
                Baslangic = dto.Baslangic,
                Bitis = dto.Bitis,
                Birim = dto.Birim,
                Pdf = dto.Pdf
            };

            var result = await _sozlesmeRepository.UpdateAsync(sozlesme);
            if (!result)
            {
                return ResponseDto<SozlesmeDto>.FailData(
                    404,
                    "Sözleşme bulunamadı",
                    $"SozlesmeID: {dto.SozlesmeID}",
                    false
                );
            }

            var resultDto = MapToDto(sozlesme);

            return ResponseDto<SozlesmeDto>.SuccessData(
                200,
                "Sözleşme başarıyla güncellendi",
                resultDto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sözleşme: {Id}", dto.SozlesmeID);
            return ResponseDto<SozlesmeDto>.FailData(
                500,
                "Sözleşme güncelleme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> DeleteAsync(int id)
    {
        try
        {
            _logger.LogDebug("Deleting sözleşme: {Id}", id);

            var result = await _sozlesmeRepository.DeleteAsync(id);
            if (!result)
            {
                return ResponseDto.Fail(
                    404,
                    "Sözleşme bulunamadı",
                    $"SozlesmeID: {id}",
                    false
                );
            }

            return ResponseDto.Success(200, "Sözleşme başarıyla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sözleşme: {Id}", id);
            return ResponseDto.Fail(
                500,
                "Sözleşme silme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<byte[]>> GetPdfAsync(int id)
    {
        try
        {
            _logger.LogDebug("Getting sözleşme PDF: {Id}", id);

            var pdf = await _sozlesmeRepository.GetContractPdfAsync(id);
            if (pdf == null || pdf.Length == 0)
            {
                return ResponseDto<byte[]>.FailData(
                    404,
                    "PDF bulunamadı",
                    $"SozlesmeID: {id}",
                    false
                );
            }

            return ResponseDto<byte[]>.SuccessData(
                200,
                "PDF başarıyla getirildi",
                pdf
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PDF: {Id}", id);
            return ResponseDto<byte[]>.FailData(
                500,
                "PDF getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> UpdateDurumAsync(SozlesmeDurumUpdateDto dto)
    {
        try
        {
            _logger.LogDebug("Updating sözleşme durum: {Id}", dto.SozlesmeID);

            var result = await _sozlesmeRepository.UpdateContractStatusAsync(
                dto.SozlesmeID,
                dto.Bitti,
                dto.SonKisi
            );

            if (!result)
            {
                return ResponseDto.Fail(
                    404,
                    "Sözleşme bulunamadı",
                    $"SozlesmeID: {dto.SozlesmeID}",
                    false
                );
            }

            return ResponseDto.Success(200, "Sözleşme durumu başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating durum: {Id}", dto.SozlesmeID);
            return ResponseDto.Fail(
                500,
                "Durum güncelleme başarısız",
                ex.Message,
                true
            );
        }
    }

    private SozlesmeDto MapToDto(Sozlesme sozlesme)
    {
        return new SozlesmeDto
        {
            SozlesmeID = sozlesme.SozlesmeID,
            Firma = sozlesme.Firma,
            Konu = sozlesme.Konu,
            Tur = sozlesme.Tur,
            Baslangic = sozlesme.Baslangic,
            Bitis = sozlesme.Bitis,
            Birim = sozlesme.Birim,
            AzKalda = sozlesme.AzKalda,
            Bitti = sozlesme.Bitti,
            SonTarih = sozlesme.SonTarih,
            SonKisi = sozlesme.SonKisi,
            Gizli = sozlesme.Gizli,
            Arsiv = sozlesme.Arsiv,
            PdfBase64 = sozlesme.Pdf != null ? Convert.ToBase64String(sozlesme.Pdf) : null,
            IlgiliKisiler = sozlesme.IlgiliKisiler?.Select(k => new SozlesmeKisiDto
            {
                SozlesmeKisiID = k.SozlesmeKisiID,
                SozlesmeID = k.SozlesmeID,
                MailID = k.MailID,
                Ad = k.Mail?.Ad,
                Soyad = k.Mail?.Soyad,
                EPosta = k.Mail?.EPosta,
                GSM = k.Mail?.GSM
            }).ToList() ?? new List<SozlesmeKisiDto>()
        };
    }
}
