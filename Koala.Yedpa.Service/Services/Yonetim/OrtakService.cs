using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services.Yonetim;

/// <summary>
/// Ortak Service Implementation
/// </summary>
public class OrtakService : IOrtakService
{
    private readonly IOrtakRepository _ortakRepository;
    private readonly ILogger<OrtakService> _logger;

    public OrtakService(
        IOrtakRepository ortakRepository,
        ILogger<OrtakService> logger)
    {
        _ortakRepository = ortakRepository;
        _logger = logger;
    }

    public async Task<ResponseDto<List<Birim>>> GetAllBirimlerAsync()
    {
        try
        {
            _logger.LogDebug("Getting tüm birimler");

            var result = await _ortakRepository.GetAllBirimlerAsync();

            return ResponseDto<List<Birim>>.SuccessData(
                200,
                "Birimler başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting birimler");
            return ResponseDto<List<Birim>>.FailData(
                500,
                "Birimler getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<List<Mail>>> GetAllMailAdresleriAsync()
    {
        try
        {
            _logger.LogDebug("Getting tüm mail adresleri");

            var result = await _ortakRepository.GetAllMailAdresleriAsync();

            return ResponseDto<List<Mail>>.SuccessData(
                200,
                "Mail adresleri başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mail adresleri");
            return ResponseDto<List<Mail>>.FailData(
                500,
                "Mail adresleri getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<Mail>> GetMailByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("Getting mail by ID: {Id}", id);

            var result = await _ortakRepository.GetMailByIdAsync(id);
            if (result == null)
            {
                return ResponseDto<Mail>.FailData(
                    404,
                    "Mail bulunamadı",
                    $"MailID: {id}",
                    false
                );
            }

            return ResponseDto<Mail>.SuccessData(
                200,
                "Mail başarıyla getirildi",
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mail by ID: {Id}", id);
            return ResponseDto<Mail>.FailData(
                500,
                "Mail getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> AddMailAsync(Mail mail)
    {
        try
        {
            _logger.LogDebug("Adding new mail: {Email}", mail.EPosta);

            await _ortakRepository.AddMailAsync(mail);

            return ResponseDto.Success(201, "Mail başarıyla eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding mail");
            return ResponseDto.Fail(
                500,
                "Mail ekleme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> UpdateMailAsync(Mail mail)
    {
        try
        {
            _logger.LogDebug("Updating mail: {Id}", mail.MailID);

            var result = await _ortakRepository.UpdateMailAsync(mail);
            if (!result)
            {
                return ResponseDto.Fail(
                    404,
                    "Mail bulunamadı",
                    $"MailID: {mail.MailID}",
                    false
                );
            }

            return ResponseDto.Success(200, "Mail başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mail: {Id}", mail.MailID);
            return ResponseDto.Fail(
                500,
                "Mail güncelleme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> DeleteMailAsync(int id)
    {
        try
        {
            _logger.LogDebug("Deleting mail: {Id}", id);

            var result = await _ortakRepository.DeleteMailAsync(id);
            if (!result)
            {
                return ResponseDto.Fail(
                    404,
                    "Mail bulunamadı",
                    $"MailID: {id}",
                    false
                );
            }

            return ResponseDto.Success(200, "Mail başarıyla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting mail: {Id}", id);
            return ResponseDto.Fail(
                500,
                "Mail silme başarısız",
                ex.Message,
                true
            );
        }
    }
}
