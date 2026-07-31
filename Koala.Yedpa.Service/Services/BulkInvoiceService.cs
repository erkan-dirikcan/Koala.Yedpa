using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Koala.Yedpa.Core.Helpers;
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
                var today = now.Date;
                var next = now.AddMonths(1);

                // Yaklaşan aktarım: tarihi henüz geçmemiş (bugün dahil) en yakın oturum.
                // Bu varsa dashboard'da alert yerine "aktarım yapılacak firmaları görüntüle" paneli çıkar.
                var upcoming = await _context.BulkInvoiceSessions
                    .Where(s => s.InvoiceDate >= today)
                    .OrderBy(s => s.InvoiceDate)
                    .FirstOrDefaultAsync();

                // GELECEK ayın aktarımı için oturum seçilmiş mi? Aktarım tarihi gelecek ayın ilk iş
                // günüdür; oturum o ayın (N+1) Month/Year'ına yazılır → MEVCUT ay değil, GELECEK ay
                // için ararız. Böylece tarih seçilince alert kalkar ve o ay 15'ine dek çıkmaz.
                var nextMonthSession = await _context.BulkInvoiceSessions
                    .FirstOrDefaultAsync(s => s.Month == next.Month && s.Year == next.Year);

                // Alert yalnızca ayın 15'inden sonra VE gelecek ay için tarih seçilmemişse.
                var showAlert = now.Day >= 15 && nextMonthSession == null;

                var message = showAlert
                    ? $"Gelecek ayın ({next.Month}/{next.Year}) aktarım gününü seçin."
                    : upcoming != null
                        ? $"Aktarım {upcoming.InvoiceDate:dd.MM.yyyy} tarihinde yapılacak."
                        : $"Bekleyen aktarım yok. Alert ayın 15'inden sonra çıkar.";

                return ResponseDto<AlertCheckResultDto>.SuccessData(200, "Alert kontrolü başarılı",
                    new AlertCheckResultDto
                    {
                        ShowAlert = showAlert,
                        Message = message,
                        CurrentMonth = next.Month,
                        CurrentYear = next.Year,
                        ShowPlannedPanel = upcoming != null,
                        SessionId = upcoming?.Id,
                        TransferDate = upcoming?.InvoiceDate
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alert kontrolü hatası");
                return ResponseDto<AlertCheckResultDto>.FailData(500, "Alert kontrolü sırasında hata oluştu", ex.Message, true);
            }
        }

        public Task<ResponseDto<List<PendingInvoiceLineDto>>> GetPendingLinesAsync()
            => GetPendingLinesAsync(BulkInvoiceMonths.ToLogoName(DateTime.Now.AddMonths(1).Month));

        public async Task<ResponseDto<List<PendingInvoiceLineDto>>> GetPendingLinesAsync(string logoMonthName)
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

                // Ay eşleşmesi LINEEXP (ay adı) ile yapılır; çağıran tarafın verdiği ay kullanılır.
                // Parametresiz overload gelecek ayı verir (modülün amacı: bir sonraki ayı faturalamak).
                var hedefAyAdi = logoMonthName;

                // Cari kod/ad CLCARD'dan join ile gelir (ORF.CLIENTREF -> LG_{firm}_CLCARD).
                // ORFICHE'de CODE/CLIENTREFNAME kolonları YOKTUR (canlı şemada doğrulandı) — kullanılmaz.
                // Tutar: ORL.AMOUNT = miktar (1), ORL.TOTAL = satır tutarı (KDV dahil) → Amount = ORL.TOTAL.
                // Ay eşleşmesi: ORL.LINENO_ takvim ayıyla GÜVENİLİR DEĞİL (kiracı değişiminde kayıyor);
                // gerçek ay ORL.LINEEXP'tedir (canlı veride doğrulandı) → LINEEXP ile filtreleniyor.
                var query = $@"
                    SELECT
                        ORF.LOGICALREF AS OrficheRef,
                        ORL.LOGICALREF AS Orflineref,
                        CLC.CODE AS ClientCode,
                        CLC.DEFINITION_ AS ClientName,
                        ORL.TOTAL AS Amount,
                        ORL.LINEEXP AS MonthName,
                        ORL.CLOSED AS ClosedStatus,
                        ORL.LINENO_ AS [LineNo]
                    FROM LG_{firm}_{period}_ORFICHE ORF
                    INNER JOIN LG_{firm}_{period}_ORFLINE AS ORL ON ORL.ORDFICHEREF=ORF.LOGICALREF
                    LEFT JOIN LG_{firm}_CLCARD CLC ON ORF.CLIENTREF=CLC.LOGICALREF
                    WHERE ORF.DOCODE='AIDAT'
                      AND ORL.TRGFLAG=0
                      AND ORL.LINEEXP = '{hedefAyAdi}'
                      AND ORF.CANCELLED=0
                      -- Cari kirmizi/pasif (Kullanimda Degil = CLC.ACTIVE=1) olmamali; pasif cariye fatura kesilmez.
                      -- ISNULL: cari bulunamayan satir (LEFT JOIN NULL) onizlemede kaybolmasin diye aktif sayilir.
                      AND ISNULL(CLC.ACTIVE, 0) = 0
                    ORDER BY CLC.CODE, ORL.LOGICALREF";

                // Logo SQL servisi ile sorgu çalıştır
                var result = _sqlProvider.SqlReader(query);

                if (!result.IsSuccess)
                {
                    // result.Message = genel metin; result.Errors.Errors[0] = SqlProvider'ın yakaladığı GERÇEK SQL hatası.
                    var sqlError = result.Errors?.Errors?.FirstOrDefault() ?? result.Message;
                    _logger.LogError("ORFLINE sorgusu hatası. Firm=[{Firm}] Period=[{Period}] GERÇEK HATA={Error} | Sorgu={Query}",
                        firm, period, sqlError, query);
                    return ResponseDto<List<PendingInvoiceLineDto>>.FailData(500, "Faturalandırılmamış satırlar alınamadı", sqlError, true);
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

        public async Task<ResponseDto<int>> MarkLinesAsTransferredAsync(IReadOnlyList<int> orflinerefs)
        {
            if (orflinerefs == null || orflinerefs.Count == 0)
                return ResponseDto<int>.SuccessData(200, "Güncellenecek satır yok", 0);

            var logo = await _settingsService.GetLogoSettingsAsync();
            if (!logo.IsSuccess || logo.Data == null)
                return ResponseDto<int>.FailData(500, "Logo ayarları alınamadı", "Logo ayarları bulunamadı", true);

            // orflinerefs int listesi → IN(...) doğrudan güvenli (SQL injection riski yok).
            var inClause = string.Join(",", orflinerefs);
            var sql = $"UPDATE LG_{logo.Data.Firm}_{logo.Data.Period}_ORFLINE SET TRGFLAG=1 WHERE LOGICALREF IN ({inClause})";

            var res = _sqlProvider.WriteToSql(sql);
            if (!res.IsSuccess)
            {
                _logger.LogError("TRGFLAG güncelleme hatası: {Message}", res.Message);
                return ResponseDto<int>.FailData(500, "TRGFLAG güncellenemedi", res.Message, true);
            }

            int.TryParse(res.Data, out var affected);
            _logger.LogInformation("{Count} sipariş satırı TRGFLAG=1 yapıldı", affected);
            return ResponseDto<int>.SuccessData(200, $"{affected} satır transferli işaretlendi", affected);
        }

        public async Task<ResponseDto<List<BulkInvoiceSessionDto>>> GetSessionsAsync()
        {
            try
            {
                var sessions = await _context.BulkInvoiceSessions
                    .Include(s => s.Items)
                    .OrderByDescending(s => s.Id)
                    .ToListAsync();

                var dtos = sessions.Select(s => new BulkInvoiceSessionDto
                {
                    Id = s.Id,
                    InvoiceDate = s.InvoiceDate,
                    Month = s.Month,
                    Year = s.Year,
                    Status = (int)s.Status,
                    CreatedBy = s.CreatedBy,
                    CreatedAt = s.CreatedAt,
                    CompletedAt = s.CompletedAt,
                    TotalItems = s.Items.Count,
                    CompletedItems = s.Items.Count(i => i.Status == BulkInvoiceItemStatus.Transferred),
                    FailedItems = s.Items.Count(i => i.Status == BulkInvoiceItemStatus.Failed)
                }).ToList();

                return ResponseDto<List<BulkInvoiceSessionDto>>.SuccessData(200, $"{dtos.Count} oturum", dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oturum listesi getirme hatası");
                return ResponseDto<List<BulkInvoiceSessionDto>>.FailData(500, "Oturumlar getirilemedi", ex.Message, true);
            }
        }

        public async Task<ResponseDto<List<BulkInvoiceItemDto>>> GetSessionItemsAsync(int sessionId)
        {
            try
            {
                var items = await _context.BulkInvoiceItems
                    .Where(i => i.SessionId == sessionId)
                    .OrderBy(i => i.ClientCode)
                    .Select(i => new BulkInvoiceItemDto
                    {
                        Id = i.Id,
                        ClientCode = i.ClientCode,
                        ClientName = i.ClientName,
                        Amount = i.Amount,
                        MonthName = i.MonthName,
                        Status = (int)i.Status,
                        LogoInvoiceRef = i.LogoInvoiceRef,
                        RetryCount = i.RetryCount,
                        CanRetry = i.CanRetry,
                        Note = i.Note,
                        RestError = i.RestError
                    })
                    .ToListAsync();

                return ResponseDto<List<BulkInvoiceItemDto>>.SuccessData(200, $"{items.Count} satır", items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oturum satırları getirme hatası. Session {Id}", sessionId);
                return ResponseDto<List<BulkInvoiceItemDto>>.FailData(500, "Satırlar getirilemedi", ex.Message, true);
            }
        }

        public async Task<ResponseDto<int>> SyncSessionItemsAsync(int sessionId)
        {
            try
            {
                var session = await _context.BulkInvoiceSessions.FindAsync(sessionId);
                if (session == null)
                    return ResponseDto<int>.FailData(404, "Oturum bulunamadı", $"Session {sessionId}", true);

                var monthName = BulkInvoiceMonths.ToLogoName(session.Month);
                var pending = await GetPendingLinesAsync(monthName);
                if (!pending.IsSuccess)
                    return ResponseDto<int>.FailData(500, "Bekleyen satırlar alınamadı", pending.Message, true);

                var lines = pending.Data ?? new List<PendingInvoiceLineDto>();
                var pendingRefs = lines.Select(l => l.Orflineref).ToHashSet();

                var existing = await _context.BulkInvoiceItems
                    .Where(i => i.SessionId == sessionId)
                    .ToListAsync();
                var existingByRef = existing.ToDictionary(i => i.Orflineref);

                // Yeni bekleyenleri ekle / mevcut Pending'leri güncelle
                foreach (var line in lines)
                {
                    if (existingByRef.TryGetValue(line.Orflineref, out var ex))
                    {
                        if (ex.Status == BulkInvoiceItemStatus.Pending)
                        {
                            ex.OrficheRef = line.OrficheRef;
                            ex.ClientCode = line.ClientCode;
                            ex.ClientName = line.ClientName;
                            ex.Amount = line.Amount;
                            ex.MonthName = line.MonthName;
                        }
                    }
                    else
                    {
                        _context.BulkInvoiceItems.Add(new BulkInvoiceItem
                        {
                            SessionId = sessionId,
                            OrficheRef = line.OrficheRef,
                            Orflineref = line.Orflineref,
                            ClientCode = line.ClientCode,
                            ClientName = line.ClientName,
                            Amount = line.Amount,
                            MonthName = line.MonthName,
                            Status = BulkInvoiceItemStatus.Pending
                        });
                    }
                }

                // Artık beklemeyen (örn. elle faturalanmış/iptal) Pending satırları kaldır
                foreach (var ex in existing.Where(i => i.Status == BulkInvoiceItemStatus.Pending && !pendingRefs.Contains(i.Orflineref)))
                    _context.BulkInvoiceItems.Remove(ex);

                await _context.SaveChangesAsync();

                var count = await _context.BulkInvoiceItems
                    .CountAsync(i => i.SessionId == sessionId && i.Status == BulkInvoiceItemStatus.Pending);

                return ResponseDto<int>.SuccessData(200, $"{count} satır aktarıma hazır", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aktarılacak satır senkronizasyon hatası. Session {Id}", sessionId);
                return ResponseDto<int>.FailData(500, "Aktarılacak satırlar oluşturulamadı", ex.Message, true);
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

                _logger.LogInformation("Toplu fatura oturumu oluşturuluyor. Kullanıcı: {Username}, Tarih: {Date}",
                    username, dto.InvoiceDate.ToShortDateString());

                // Aynı ay için henüz aktarılmamış bir oturum varsa YENİSİ AÇILMAZ, tarihi güncellenir.
                // (Aksi halde çift oturum oluşur ve hangisinin tetikleneceği belirsizleşir.)
                var existing = await _context.BulkInvoiceSessions
                    .FirstOrDefaultAsync(s => s.Month == dto.InvoiceDate.Month
                                           && s.Year == dto.InvoiceDate.Year
                                           && s.Status == BulkInvoiceSessionStatus.Pending);

                if (existing != null)
                {
                    existing.InvoiceDate = dto.InvoiceDate;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Mevcut oturumun aktarım tarihi güncellendi. Session ID: {SessionId}, Tarih: {Date:dd.MM.yyyy}",
                        existing.Id, existing.InvoiceDate);

                    return ResponseDto<int>.SuccessData(200,
                        $"Aktarım tarihi güncellendi: {existing.InvoiceDate:dd.MM.yyyy}.",
                        existing.Id);
                }

                // Session oluştur. Aktarılacak satırlar AKTARIM ANINDA (o ayın tüm bekleyen AIDAT
                // satırları) çekilir — bu nedenle burada item oluşturulmaz, job oluşturur.
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

                // Zamanlama/tetik artık uygulama içinde DEĞİL (Hangfire kaldırıldı). Seçilen tarih
                // N8N'e bildirilecek; aktarım ve bilgi maili N8N'in RabbitMQ'ya attığı mesajla tetiklenir.
                // Burada yalnızca oturum (tarih) DB'ye kaydedilir; alert mantığı bu kaydı kullanır.
                _logger.LogInformation("Oturum kaydedildi. Session ID: {SessionId}, Fatura tarihi: {Date:dd.MM.yyyy}",
                    session.Id, session.InvoiceDate);

                return ResponseDto<int>.SuccessData(200,
                    $"Oturum kaydedildi. Aktarım tarihi: {session.InvoiceDate:dd.MM.yyyy}.",
                    session.Id);
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
