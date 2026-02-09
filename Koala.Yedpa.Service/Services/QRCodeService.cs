using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using Koala.Yedpa.Core.Configuration;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using AppQRCode = Koala.Yedpa.Core.Models.QRCode;
using AppQRCodeBatch = Koala.Yedpa.Core.Models.QRCodeBatch;

namespace Koala.Yedpa.Service.Services;

public class QRCodeService : IQRCodeService
{
    private readonly QRCodeSettings _settings;
    private readonly ILogger<QRCodeService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly ISettingsService _settingsService;
    private readonly IDapperProvider _dapperProvider;
    private readonly IQRCodeRepository _qrCodeRepository;
    private readonly IQRCodeBatchRepository _qrCodeBatchRepository;

    public QRCodeService(
        IOptions<QRCodeSettings> settings,
        ILogger<QRCodeService> logger,
        IWebHostEnvironment environment,
        ISettingsService settingsService,
        IDapperProvider dapperProvider,
        IQRCodeRepository qrCodeRepository,
        IQRCodeBatchRepository qrCodeBatchRepository)
    {
        _settings = settings.Value;
        _logger = logger;
        _environment = environment;
        _settingsService = settingsService;
        _dapperProvider = dapperProvider;
        _qrCodeRepository = qrCodeRepository;
        _qrCodeBatchRepository = qrCodeBatchRepository;
    }

    public async Task<ResponseDto<byte[]>> GenerateQRCodeAsync(QRCodeDto request)
    {
        try
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(request.Content, QRCodeGenerator.ECCLevel.Q);

            using var qrCode = new QRCoder.QRCode(qrCodeData);
            using var bitmap = qrCode.GetGraphic(request.PixelSize);

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);

            return ResponseDto<byte[]>.SuccessData(200, "QR kod başarıyla oluşturuldu", stream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kod oluşturulurken hata oluştu");
            return ResponseDto<byte[]>.FailData(500, "QR kod oluşturma başarısız", ex.Message, true);
        }
    }

    public async Task<ResponseDto<byte[]>> GenerateQRCodeWithLogoAsync(QRCodeDto request)
    {
        try
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(request.Content, QRCodeGenerator.ECCLevel.H);

            using var qrCode = new QRCoder.QRCode(qrCodeData);

            // For now, generate QR code without logo
            // Logo integration requires more complex bitmap manipulation
            using var bitmap = qrCode.GetGraphic(request.PixelSize);

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);

            return ResponseDto<byte[]>.SuccessData(200, "QR kod başarıyla oluşturuldu", stream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kod (logo ile) oluşturulurken hata oluştu");
            return ResponseDto<byte[]>.FailData(500, "QR kod oluşturma başarısız", ex.Message, true);
        }
    }

    public async Task<ResponseDto<string>> GenerateAndSaveQRCodeAsync(string workplaceCode, string partnerNo)
    {
        try
        {
            var qrNumber = $"{_settings.QrCodePrefix}{partnerNo}";
            var fileName = $"{qrNumber}.jpg";
            var relativePath = Path.Combine(_settings.QrCodeStoragePath, fileName);
            var fullPath = Path.Combine(_environment.WebRootPath, _settings.QrCodeStoragePath, fileName);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var qrUrl = $"{_settings.QrCodeBaseUrl}/Workplace/Detail/{workplaceCode}";

            var request = new QRCodeDto
            {
                Content = qrUrl,
                PixelSize = _settings.DefaultPixelSize,
                IncludeLogo = !string.IsNullOrEmpty(_settings.LogoFilePath),
                LogoFilePath = _settings.LogoFilePath
            };

            var result = await GenerateQRCodeWithLogoAsync(request);

            if (!result.IsSuccess)
            {
                return ResponseDto<string>.FailData(500, "QR kod oluşturma başarısız", result.Message ?? "Bilinmeyen hata", true);
            }

            await File.WriteAllBytesAsync(fullPath, result.Data);

            _logger.LogInformation("QR kod oluşturuldu ve kaydedildi: {FilePath}", fullPath);

            return ResponseDto<string>.SuccessData(200, "QR kod başarıyla oluşturuldu ve kaydedildi", relativePath.Replace("\\", "/"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kod kaydedilirken hata oluştu. WorkplaceCode: {WorkplaceCode}", workplaceCode);
            return ResponseDto<string>.FailData(500, "QR kod kaydetme başarısız", ex.Message, true);
        }
    }

    public async Task<ResponseDto<string>> GenerateQRCodeUrlAsync(string workplaceCode, string partnerNo)
    {
        try
        {
            var qrNumber = $"{_settings.QrCodePrefix}{partnerNo}";
            var qrUrl = $"{_settings.QrCodeBaseUrl}/Workplace/Detail/{workplaceCode}";

            return ResponseDto<string>.SuccessData(200, "QR kod URL oluşturuldu", qrNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kod URL oluşturulurken hata oluştu");
            return ResponseDto<string>.FailData(500, "QR kod URL oluşturma başarısız", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<object>>> GenerateBulkQRCodesAsync()
    {
        try
        {
            // QR Code ayarlarını getir
            var settingsResult = await _settingsService.GetQRCodeSettingsAsync();
            if (!settingsResult.IsSuccess)
            {
                return ResponseDto<List<object>>.FailData(400, "QR Kod ayarları bulunamadı", "Lütfen önce QR kod ayarlarını yapılandırın", true);
            }

            var settings = settingsResult.Data;
            if (string.IsNullOrEmpty(settings.QrSqlQuery))
            {
                return ResponseDto<List<object>>.FailData(400, "SQL sorgusu boş", "Lütfen QR kod ayarlarından SQL sorgusunu girin", true);
            }

            // Logo veritabanında sorguyu çalıştır
            var partners = await ExecuteSqlQueryAsync(settings.QrSqlQuery);
            if (!partners.Any())
            {
                return ResponseDto<List<object>>.FailData(404, "Kayıt bulunamadı", "SQL sorgusu herhangi bir kayıt döndürmedi", true);
            }

            // 1. Yeni Batch kaydı oluştur
            var batch = new AppQRCodeBatch
            {
                SqlQuery = settings.QrSqlQuery,
                QrCodeYear = settings.QrCodeYear,
                QrCodePreCode = settings.QrCodePreCode,
                QRCodeCount = 0,
                Description = $"{DateTime.Now:dd.MM.yyyy HH:mm} tarihinde oluşturulan QR kodları",
                Status = StatusEnum.Active
            };
            batch = await _qrCodeBatchRepository.AddAsync(batch);
            _logger.LogInformation("Yeni QR kod batch oluşturuldu. BatchId: {BatchId}", batch.Id);

            // Klasör yapısını oluştur: wwwroot/Uploads/Qr/{Yıl}/
            var folderPath = Path.Combine(_environment.WebRootPath, "Uploads", "Qr", settings.QrCodeYear);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                _logger.LogInformation("QR kod klasörü oluşturuldu: {FolderPath}", folderPath);
            }

            var generatedQRCodes = new List<object>();
            var successCount = 0;
            var errorCount = 0;

            // Her partner için QR kod oluştur
            foreach (var partner in partners)
            {
                var partnerNo = partner?.ToString();
                try
                {
                    if (string.IsNullOrEmpty(partnerNo))
                    {
                        _logger.LogWarning("PartnerNo boş geldi, atlanıyor");
                        errorCount++;
                        continue;
                    }

                    var qrFileName = $"{settings.QrCodePreCode}-{partnerNo}.jpg";
                    var fullPath = Path.Combine(folderPath, qrFileName);
                    var relativePath = $"/Uploads/Qr/{settings.QrCodeYear}/{qrFileName}";
                    var folderRelativePath = $"Uploads/Qr/{settings.QrCodeYear}/";

                    // QR kod içeriğini oluştur
                    var qrContent = $"{partnerNo}";

                    // QR kod oluştur
                    using var qrGenerator = new QRCodeGenerator();
                    var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.M);
                    using var qrCode = new QRCoder.QRCode(qrCodeData);
                    using var bitmap = qrCode.GetGraphic(10); // 10px piksel boyutu

                    // JPEG formatında kaydet
                    using var stream = new MemoryStream();
                    bitmap.Save(stream, ImageFormat.Jpeg);
                    await File.WriteAllBytesAsync(fullPath, stream.ToArray());

                    // Veritabanına kaydet
                    var qrCodeEntity = new AppQRCode
                    {
                        BatchId = batch.Id,
                        PartnerNo = partnerNo,
                        QRCodeNumber = $"{settings.QrCodePreCode}-{partnerNo}",
                        QRImagePath = relativePath,
                        FolderPath = folderRelativePath,
                        QrCodeYear = settings.QrCodeYear,
                        Status = StatusEnum.Active
                    };
                    await _qrCodeRepository.AddAsync(qrCodeEntity);

                    generatedQRCodes.Add(new
                    {
                        partnerNo = partnerNo,
                        fileName = qrFileName,
                        qrPath = relativePath,
                        folder = folderRelativePath
                    });

                    successCount++;
                    _logger.LogInformation("QR kod oluşturuldu: {FileName} (PartnerNo: {PartnerNo})", qrFileName, (object)partnerNo);
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _logger.LogError(ex, "QR kod oluşturulurken hata oluştu. PartnerNo: {PartnerNo}", (object)(partnerNo ?? "unknown"));
                }
            }

            // Batch kaydını güncelle - QR kod sayısı
            batch.QRCodeCount = successCount;
            _qrCodeBatchRepository.Update(batch);
            _logger.LogInformation("Batch kaydı güncellendi. BatchId: {BatchId}, QR Kod Sayısı: {Count}", batch.Id, successCount);

            _logger.LogInformation("Toplu QR kod oluşturma tamamlandı. BatchId: {BatchId}, Başarılı: {Success}, Hatalı: {Error}", batch.Id, successCount, errorCount);

            return ResponseDto<List<object>>.SuccessData(
                200,
                $"{successCount} adet QR kod başarıyla oluşturuldu. Batch ID: {batch.Id}. {errorCount} adet hata oluştu.",
                generatedQRCodes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu QR kod oluşturulurken hata oluştu");
            return ResponseDto<List<object>>.FailData(500, "Toplu QR kod oluşturma başarısız", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<AppQRCodeBatch>>> GetBatchesAsync()
    {
        try
        {
            var batches = await _qrCodeBatchRepository.GetAllAsync();
            return ResponseDto<List<AppQRCodeBatch>>.SuccessData(200, "QR kod batch listesi başarıyla getirildi", batches.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kod batch listesi getirilirken hata oluştu");
            return ResponseDto<List<AppQRCodeBatch>>.FailData(500, "Batch listesi getirilemedi", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<AppQRCode>>> GetQRCodesByBatchIdAsync(int batchId)
    {
        try
        {
            var qrcodes = await _qrCodeRepository.GetByBatchIdAsync(batchId);
            return ResponseDto<List<AppQRCode>>.SuccessData(200, $"QR kodlar (Batch: {batchId}) başarıyla getirildi", qrcodes.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kodlar getirilirken hata oluştu. BatchId: {BatchId}", batchId);
            return ResponseDto<List<AppQRCode>>.FailData(500, "QR kodlar getirilemedi", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<AppQRCode>>> GetQRCodesAsync()
    {
        try
        {
            var qrcodes = await _qrCodeRepository.GetAllAsync();
            return ResponseDto<List<AppQRCode>>.SuccessData(200, "QR kodlar başarıyla getirildi", qrcodes.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kodlar getirilirken hata oluştu");
            return ResponseDto<List<AppQRCode>>.FailData(500, "QR kodlar getirilemedi", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<AppQRCode>>> GetQRCodesByYearAsync(string year)
    {
        try
        {
            var qrcodes = await _qrCodeRepository.GetByYearAsync(year);
            return ResponseDto<List<AppQRCode>>.SuccessData(200, $"QR kodlar ({year}) başarıyla getirildi", qrcodes.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kodlar getirilirken hata oluştu. Year: {Year}", year);
            return ResponseDto<List<AppQRCode>>.FailData(500, "QR kodlar getirilemedi", ex.Message, true);
        }
    }

    public async Task<ResponseDto<AppQRCode>> GetQRCodeByPartnerNoAsync(string partnerNo)
    {
        try
        {
            var qrCode = await _qrCodeRepository.GetByPartnerNoAsync(partnerNo);
            if (qrCode == null)
            {
                return ResponseDto<AppQRCode>.FailData(404, "QR kod bulunamadı", $"PartnerNo: {partnerNo}", false);
            }

            return ResponseDto<AppQRCode>.SuccessData(200, "QR kod başarıyla getirildi", qrCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kod getirilirken hata oluştu. PartnerNo: {PartnerNo}", partnerNo);
            return ResponseDto<AppQRCode>.FailData(500, "QR kod getirilemedi", ex.Message, true);
        }
    }

    public async Task<ResponseDto> DeleteBatchAsync(int batchId, bool deleteFiles = true)
    {
        try
        {
            var batch = await _qrCodeBatchRepository.GetByIdAsync(batchId);
            if (batch == null)
            {
                return ResponseDto.Fail(404, "Batch bulunamadı", $"Batch ID: {batchId}", false);
            }

            // Batch'e ait QR kodları getir
            var qrcodes = await _qrCodeRepository.GetByBatchIdAsync(batchId);
            var qrcodeList = qrcodes.ToList();

            foreach (var qrCode in qrcodeList)
            {
                // Dosyayı sil
                if (deleteFiles && !string.IsNullOrEmpty(qrCode.QRImagePath))
                {
                    var fullPath = Path.Combine(_environment.WebRootPath, qrCode.QRImagePath.TrimStart('/'));
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        _logger.LogInformation("QR kod dosyası silindi: {FilePath}", fullPath);
                    }
                }

                // Veritabanından sil (soft delete)
                qrCode.Status = StatusEnum.Deleted;
                qrCode.LastUpdateTime = DateTime.UtcNow;
                _qrCodeRepository.Update(qrCode);
            }

            // Batch'i de sil
            batch.Status = StatusEnum.Deleted;
            batch.LastUpdateTime = DateTime.UtcNow;
            _qrCodeBatchRepository.Update(batch);

            _logger.LogInformation("Batch silindi. BatchId: {BatchId}, QR Kod Sayısı: {Count}", batchId, qrcodeList.Count());

            return ResponseDto.Success(200, $"{qrcodeList.Count()} adet QR kod başarıyla silindi. Batch silindi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch silinirken hata oluştu. BatchId: {BatchId}", batchId);
            return ResponseDto.Fail(500, "Batch silme başarısız", ex.Message, true);
        }
    }

    public async Task<ResponseDto> DeleteQRCodesAsync(bool deleteFiles = true)
    {
        try
        {
            // Tüm QR kodlarını getir
            var qrcodes = await _qrCodeRepository.GetAllAsync();
            var qrcodeList = qrcodes.ToList();

            if (!qrcodeList.Any())
            {
                return ResponseDto.Success(200, "Silinecek QR kod bulunamadı");
            }

            var deletedCount = 0;

            foreach (var qrCode in qrcodeList)
            {
                try
                {
                    // Dosyayı sil
                    if (deleteFiles && !string.IsNullOrEmpty(qrCode.QRImagePath))
                    {
                        var fullPath = Path.Combine(_environment.WebRootPath, qrCode.QRImagePath.TrimStart('/'));
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            _logger.LogInformation("QR kod dosyası silindi: {FilePath}", fullPath);
                        }
                    }

                    // Veritabanından sil (soft delete)
                    qrCode.Status = StatusEnum.Deleted;
                    qrCode.LastUpdateTime = DateTime.UtcNow;
                    _qrCodeRepository.Update(qrCode);

                    deletedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "QR kod silinirken hata oluştu. PartnerNo: {PartnerNo}", qrCode.PartnerNo);
                }
            }

            _logger.LogInformation("{Count} adet QR kod başarıyla silindi", deletedCount);

            return ResponseDto.Success(200, $"{deletedCount} adet QR kod başarıyla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kodlar silinirken hata oluştu");
            return ResponseDto.Fail(500, "QR kodlar silinirken hata oluştu", ex.Message, true);
        }
    }

    public async Task<ResponseDto> RefreshQRCodesAsync()
    {
        try
        {
            _logger.LogInformation("QR kodlar yeniden oluşturuluyor...");

            // 1. Önce mevcut QR kodlarını sil (dosya + veritabanı)
            await DeleteQRCodesAsync(deleteFiles: true);

            // 2. Yeni QR kodları oluştur
            var result = await GenerateBulkQRCodesAsync();

            if (!result.IsSuccess)
            {
                return ResponseDto.Fail(500, "QR kodlar yeniden oluşturulurken hata oluştu", result.Message, true);
            }

            _logger.LogInformation("QR kodlar başarıyla yeniden oluşturuldu");

            return ResponseDto.Success(200, "QR kodlar başarıyla yeniden oluşturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kodlar yeniden oluşturulurken hata oluştu");
            return ResponseDto.Fail(500, "QR kodlar yeniden oluşturulurken hata oluştu", ex.Message, true);
        }
    }

    private async Task<List<dynamic>> ExecuteSqlQueryAsync(string sqlQuery)
    {
        try
        {
            var result = await _dapperProvider.QueryAsync<dynamic>(sqlQuery);

            if (!result.IsSuccess || result.Data == null)
            {
                _logger.LogWarning("SQL sorgusu sonuç döndürmedi");
                return new List<dynamic>();
            }

            return result.Data.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL sorgusu çalıştırılırken hata oluştu. Query: {Query}", sqlQuery);
            throw new Exception($"SQL sorgusu hatası: {ex.Message}", ex);
        }
    }

    public async Task<ResponseDto<byte[]>> GetQRCodeImageAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);

            if (!File.Exists(fullPath))
            {
                return ResponseDto<byte[]>.FailData(404, "QR kod dosyası bulunamadı", $"Dosya: {filePath}", false);
            }

            var imageBytes = await File.ReadAllBytesAsync(fullPath);

            return ResponseDto<byte[]>.SuccessData(200, "QR kod resmi başarıyla getirildi", imageBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kod resmi getirilirken hata oluştu. FilePath: {FilePath}", filePath);
            return ResponseDto<byte[]>.FailData(500, "QR kod resim getirme başarısız", ex.Message, true);
        }
    }
}
