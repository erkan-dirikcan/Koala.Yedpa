using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Core.Services;

public interface IQRCodeService
{
    // QR Kod Oluşturma
    Task<ResponseDto<byte[]>> GenerateQRCodeAsync(QRCodeDto request);
    Task<ResponseDto<byte[]>> GenerateQRCodeWithLogoAsync(QRCodeDto request);
    Task<ResponseDto<string>> GenerateAndSaveQRCodeAsync(string workplaceCode, string partnerNo);
    Task<ResponseDto<string>> GenerateQRCodeUrlAsync(string workplaceCode, string partnerNo);
    Task<ResponseDto<List<object>>> GenerateBulkQRCodesAsync();
    Task<ResponseDto<List<object>>> GenerateBulkQRCodesWithParamsAsync(string qrCodeYear, string qrCodePreCode, string sqlQuery, string description);

    // QR Kod Listeleme
    Task<ResponseDto<List<QRCodeBatch>>> GetBatchesAsync();
    Task<ResponseDto<List<QRCode>>> GetQRCodesAsync();
    Task<ResponseDto<List<QRCode>>> GetQRCodesByBatchIdAsync(int batchId);
    Task<ResponseDto<List<QRCode>>> GetQRCodesByYearAsync(string year);
    Task<ResponseDto<QRCode>> GetQRCodeByPartnerNoAsync(string partnerNo);

    // QR Kod Güncelleme ve Silme
    Task<ResponseDto> DeleteQRCodesAsync(bool deleteFiles = true);
    Task<ResponseDto> DeleteBatchAsync(int batchId, bool deleteFiles = true);
    Task<ResponseDto> RefreshQRCodesAsync();
    Task<ResponseDto<byte[]>> GetQRCodeImageAsync(string filePath);
}
