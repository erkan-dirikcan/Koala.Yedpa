using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services
{
    /// <summary>
    /// Toplu faturalandırma servisi implementasyonu
    /// </summary>
    public class BulkInvoiceService : IBulkInvoiceService
    {
        private readonly AppDbContext _context;
        private readonly IApiLogoSqlDataService _apiLogoSqlDataService;
        private readonly ISettingsService _settingsService;
        private readonly ISqlProvider _sqlProvider;
        private readonly ILogger<BulkInvoiceService> _logger;

        public BulkInvoiceService(
            AppDbContext context,
            IApiLogoSqlDataService apiLogoSqlDataService,
            ISettingsService settingsService,
            ISqlProvider sqlProvider,
            ILogger<BulkInvoiceService> logger)
        {
            _context = context;
            _apiLogoSqlDataService = apiLogoSqlDataService;
            _settingsService = settingsService;
            _sqlProvider = sqlProvider;
            _logger = logger;
        }

        public async Task<ResponseDto<AlertCheckResultDto>> CheckAlertAsync()
        {
            try
            {
                var now = DateTime.Now;
                var currentDay = now.Day;
                var currentMonth = now.Month;
                var currentYear = now.Year;

                // Ayın 15'inden önceyse alert gösterme
                if (currentDay < 15)
                {
                    return ResponseDto<AlertCheckResultDto>.SuccessData(200, "Alert kontrolü başarılı",
                        new AlertCheckResultDto
                        {
                            ShowAlert = false,
                            Message = $"Ayın 15'inden önce ({currentDay}). Alert gösterilmiyor.",
                            CurrentMonth = currentMonth,
                            CurrentYear = currentYear
                        });
                }

                // Bu ay için session var mı kontrol et
                var existingSession = await _context.BulkInvoiceSessions
                    .FirstOrDefaultAsync(s => s.Month == currentMonth && s.Year == currentYear);

                if (existingSession != null)
                {
                    return ResponseDto<AlertCheckResultDto>.SuccessData(200, "Alert kontrolü başarılı",
                        new AlertCheckResultDto
                        {
                            ShowAlert = false,
                            Message = $"Bu ay ({currentMonth}/{currentYear}) için zaten faturalandırma oturumu mevcut.",
                            CurrentMonth = currentMonth,
                            CurrentYear = currentYear
                        });
                }

                // Alert göster
                return ResponseDto<AlertCheckResultDto>.SuccessData(200, "Alert gösterilecek",
                    new AlertCheckResultDto
                    {
                        ShowAlert = true,
                        Message = $"Faturalandırılmamış aidat siparişleri bulunuyor. Lütfen faturalandırma işlemini başlatın.",
                        CurrentMonth = currentMonth,
                        CurrentYear = currentYear
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alert kontrolü hatası");
                return ResponseDto<AlertCheckResultDto>.FailData(500, "Alert kontrolü sırasında hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<List<PendingInvoiceLineDto>>> GetPendingLinesAsync()
        {
            try
            {
                // Logo SQL ayarlarını al
                var logoSettingResult = await _settingsService.GetLogoSettingsAsync();
                if (!logoSettingResult.IsSuccess || logoSettingResult.Data == null)
                {
                    return ResponseDto<List<PendingInvoiceLineDto>>.FailData(500, "Logo ayarları alınamadı", "Logo ayarları bulunamadı", true);
                }

                var firm = logoSettingResult.Data.Firm;
                var period = logoSettingResult.Data.Period;

                // Şu anki ayı al
                var currentMonth = DateTime.Now.Month;

                // Design spec'teki SQL query'sini kullan
                var query = $@"
                    SELECT
                        ORF.LOGICALREF AS OrficheRef,
                        ORL.LOGICALREF AS Orflineref,
                        ORF.CODE AS ClientCode,
                        ORF.CLIENTREFNAME AS ClientName,
                        ORL.AMOUNT AS Amount,
                        ORL.LINEEXP AS MonthName,
                        ORL.CLOSED AS ClosedStatus,
                        ORL.LINENO_ AS LineNo
                    FROM LG_{firm}_{period}_ORFICHE ORF
                    INNER JOIN LG_{firm}_{period}_ORFLINE AS ORL ON ORL.ORDFICHEREF=ORF.LOGICALREF
                    WHERE ORF.DOCODE='AIDAT'
                      AND ORL.TRGFLAG=0
                      AND ORL.LINENO_ = {currentMonth}
                      AND ORF.CANCELLED=0
                    ORDER BY ORF.CODE, ORL.LOGICALREF";

                // Logo SQL servisi ile sorgu çalıştır
                var result = _sqlProvider.SqlReader(query);

                if (!result.IsSuccess)
                {
                    _logger.LogError("ORFLINE sorgusu hatası: {Message}", result.Message);
                    return ResponseDto<List<PendingInvoiceLineDto>>.FailData(500, "Faturalandırılmamış satırlar alınamadı", result.Message, true);
                }

                // Sonuçları DTO'ya çevir
                var pendingLines = new List<PendingInvoiceLineDto>();

                foreach (System.Data.DataRow row in result.Data.Rows)
                {
                    pendingLines.Add(new PendingInvoiceLineDto
                    {
                        OrficheRef = Convert.ToInt32(row["OrficheRef"]),
                        Orflineref = Convert.ToInt32(row["Orflineref"]),
                        ClientCode = row["ClientCode"]?.ToString() ?? string.Empty,
                        ClientName = row["ClientName"]?.ToString() ?? string.Empty,
                        Amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0,
                        MonthName = row["MonthName"]?.ToString() ?? string.Empty,
                        ClosedStatus = row["ClosedStatus"] != DBNull.Value ? Convert.ToInt32(row["ClosedStatus"]) : 0,
                        LineNo = row["LineNo"] != DBNull.Value ? Convert.ToInt32(row["LineNo"]) : 0,
                        IsSelected = false // Varsayılan seçili değil
                    });
                }

                _logger.LogInformation("Faturalandırılmamış satırlar başarıyla getirildi. Toplam: {Count}", pendingLines.Count);

                return ResponseDto<List<PendingInvoiceLineDto>>.SuccessData(200,
                    $"Faturalandırılmamış {pendingLines.Count} satır bulundu", pendingLines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Faturalandırılmamış satırları getirme hatası");
                return ResponseDto<List<PendingInvoiceLineDto>>.FailData(500,
                    "Faturalandırılmamış satırlar getirilirken hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<int>> CreateSessionAsync(CreateBulkInvoiceSessionDto dto, string username)
        {
            try
            {
                if (dto == null)
                {
                    return ResponseDto<int>.FailData(400, "DTO boş olamaz", "Geçersiz istek", true);
                }

                if (dto.SelectedLines == null || !dto.SelectedLines.Any())
                {
                    return ResponseDto<int>.FailData(400, "Seçili satır yok", "En az bir satır seçmelisiniz", true);
                }

                _logger.LogInformation("Toplu fatura oturumu oluşturuluyor. Kullanıcı: {Username}, Satır sayısı: {Count}",
                    username, dto.SelectedLines.Count);

                // Session oluştur
                var session = new BulkInvoiceSession
                {
                    InvoiceDate = dto.InvoiceDate,
                    Month = dto.InvoiceDate.Month,
                    Year = dto.InvoiceDate.Year,
                    Status = BulkInvoiceSessionStatus.Pending,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.BulkInvoiceSessions.AddAsync(session);
                await _context.SaveChangesAsync();

                // Items oluştur
                foreach (var selectedLine in dto.SelectedLines)
                {
                    var item = new BulkInvoiceItem
                    {
                        SessionId = session.Id,
                        OrficheRef = selectedLine.OrficheRef,
                        Orflineref = selectedLine.Orflineref,
                        ClientCode = selectedLine.ClientCode,
                        ClientName = selectedLine.ClientName,
                        Amount = selectedLine.Amount,
                        MonthName = selectedLine.MonthName,
                        Status = BulkInvoiceItemStatus.Pending
                    };

                    await _context.BulkInvoiceItems.AddAsync(item);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Toplu fatura oturumu başarıyla oluşturuldu. Session ID: {SessionId}, Item sayısı: {ItemCount}",
                    session.Id, dto.SelectedLines.Count);

                return ResponseDto<int>.SuccessData(200, $"Oturum başarıyla oluşturuldu. {dto.SelectedLines.Count} satır kuyruğa alındı.", session.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oturum oluşturma hatası");
                return ResponseDto<int>.FailData(500, "Oturum oluşturulurken hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<BulkInvoiceSessionDto>> GetSessionStatusAsync(int sessionId)
        {
            try
            {
                var session = await _context.BulkInvoiceSessions
                    .Include(s => s.Items)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);

                if (session == null)
                {
                    return ResponseDto<BulkInvoiceSessionDto>.FailData(404, "Oturum bulunamadı", $"Session ID: {sessionId}", true);
                }

                var sessionDto = new BulkInvoiceSessionDto
                {
                    Id = session.Id,
                    InvoiceDate = session.InvoiceDate,
                    Month = session.Month,
                    Year = session.Year,
                    Status = (int)session.Status,
                    CreatedBy = session.CreatedBy,
                    CreatedAt = session.CreatedAt,
                    CompletedAt = session.CompletedAt,
                    TotalItems = session.Items.Count,
                    CompletedItems = session.Items.Count(i => i.Status == BulkInvoiceItemStatus.Transferred),
                    FailedItems = session.Items.Count(i => i.Status == BulkInvoiceItemStatus.Failed)
                };

                return ResponseDto<BulkInvoiceSessionDto>.SuccessData(200, "Oturum durumu getirildi", sessionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oturum durumu getirme hatası. Session ID: {SessionId}", sessionId);
                return ResponseDto<BulkInvoiceSessionDto>.FailData(500, "Oturum durumu getirilirken hata oluştu", ex.Message, true);
            }
        }
    }
}
