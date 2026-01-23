using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.LogoJsonModels;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace Koala.Yedpa.Service.Services
{
    /// <summary>
    /// DuesStatistic → Logo SalesOrder aktarım servisi
    /// </summary>
    public interface IBudgetOrderTransferService
    {
        Task<ResponseDto<List<OrderResultViewModel>>> TransferDuesStatisticsToLogoAsync(
            List<string> duesStatisticIds,
            string? userId = null,
            bool isDebugMode = false);
    }

    public class BudgetOrderTransferService : IBudgetOrderTransferService
    {
        private readonly IUnitOfWork<AppDbContext> _unitOfWork;
        private readonly ILogoRestServiceProvider _logoRestServiceProvider;
        private readonly ILogger<BudgetOrderTransferService> _logger;
        private readonly IEmailService _emailService;

        public BudgetOrderTransferService(
            IUnitOfWork<AppDbContext> unitOfWork,
            ILogoRestServiceProvider logoRestServiceProvider,
            ILogger<BudgetOrderTransferService> logger,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _logoRestServiceProvider = logoRestServiceProvider;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<ResponseDto<List<OrderResultViewModel>>> TransferDuesStatisticsToLogoAsync(
            List<string> duesStatisticIds,
            string? userId = null,
            bool isDebugMode = false)
        {
            var results = new List<OrderResultViewModel>();
            var successCount = 0;
            var failedCount = 0;

            try
            {
                _logger.LogInformation("Aktarım başladı. Kayıt sayısı: {Count}, Debug Mod: {IsDebugMode}",
                    duesStatisticIds.Count, isDebugMode);

                // DuesStatistic kayıtlarını tek seferde getir (N+1 problemi çözümü)
                var duesStatisticsList = await _unitOfWork.DuesStatisticRepository.GetByIdsAsync(duesStatisticIds);
                var duesStatistics = duesStatisticsList.ToList();

                if (!duesStatistics.Any())
                {
                    return ResponseDto<List<OrderResultViewModel>>.FailData(
                        404, "Kayıtlar bulunamadı", "DuesStatistic kayıtları bulunamadı", true);
                }

                // Debug modunda sadece ilk 3 kaydı al
                var recordsToTransfer = isDebugMode
                    ? duesStatistics.Take(3).ToList()
                    : duesStatistics;

                if (isDebugMode)
                {
                    _logger.LogWarning("DEBUG MODU: Sadece {Count} kayıt aktarılacak", recordsToTransfer.Count);
                }

                // Her bir DuesStatistic için SalesOrder oluştur
                foreach (var dues in recordsToTransfer)
                {
                    try
                    {
                        _logger.LogInformation("Aktarılıyor: {Code} - {DivName}", dues.Code, dues.DivName);

                        // SalesOrder oluştur
                        var salesOrder = CreateSalesOrderFromDuesStatistic(dues);

                        // Logo'ya gönder
                        var response = await _logoRestServiceProvider.PostSalesOrderAsync(salesOrder);

                        if (response.IsSuccess)
                        {
                            // Başarılı - DuesStatistic transfer status'unu güncelle
                            var oldStatus = dues.TransferStatus;
                            dues.TransferStatus = TransferStatusEnum.Completed;
                            dues.LastUpdateTime = DateTime.Now;
                            await _unitOfWork.DuesStatisticRepository.UpdateAsync(dues);

                            _logger.LogInformation("TransferStatus güncellendi: {Code}, Eski durum: {OldStatus}, Yeni durum: {NewStatus}, Sipariş no: {OrderNumber}",
                                dues.Code, oldStatus, dues.TransferStatus, response.Data);

                            successCount++;
                            results.Add(new OrderResultViewModel
                            {
                                ClientCode = dues.ClientCode ?? "",
                                ClientRef = dues.ClientRef.ToString(),
                                IsSuccess = true,
                                OrderNumber = response.Data,
                                ErrorMessage = null,
                                OrderAmount = dues.Total
                            });

                            _logger.LogInformation("Başarılı: {Code}", dues.Code);
                        }
                        else
                        {
                            // Başarısız - DuesStatistic transfer status'unu güncelle
                            var oldStatus = dues.TransferStatus;
                            dues.TransferStatus = TransferStatusEnum.Failed;
                            dues.LastUpdateTime = DateTime.Now;
                            await _unitOfWork.DuesStatisticRepository.UpdateAsync(dues);

                            _logger.LogWarning("TransferStatus güncellendi (BAŞARISIZ): {Code}, Eski durum: {OldStatus}, Yeni durum: {NewStatus}, Hata: {Error}",
                                dues.Code, oldStatus, dues.TransferStatus, response.Message);

                            // Hata mesajını detaylandır
                            var errorMessage = response.Message ?? "";
                            if (response.Errors != null && response.Errors.Errors.Any())
                            {
                                errorMessage += " | " + string.Join(", ", response.Errors.Errors);
                            }

                            failedCount++;
                            results.Add(new OrderResultViewModel
                            {
                                ClientCode = dues.ClientCode ?? "",
                                ClientRef = dues.ClientRef.ToString(),
                                IsSuccess = false,
                                OrderNumber = null,
                                ErrorMessage = errorMessage,
                                OrderAmount = dues.Total
                            });

                            _logger.LogError("Başarısız: {Code} - {Error}", dues.Code, errorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Exception - DuesStatistic transfer status'unu güncelle
                        var oldStatus = dues.TransferStatus;
                        dues.TransferStatus = TransferStatusEnum.Failed;
                        dues.LastUpdateTime = DateTime.Now;
                        await _unitOfWork.DuesStatisticRepository.UpdateAsync(dues);

                        _logger.LogError(ex, "TransferStatus güncellendi (EXCEPTION): {Code}, Eski durum: {OldStatus}, Yeni durum: {NewStatus}",
                            dues.Code, oldStatus, dues.TransferStatus);

                        // Hata mesajını detaylandır (InnerException'ı da dahil et)
                        var errorMessage = ex.Message;
                        if (ex.InnerException != null)
                        {
                            errorMessage += " | " + ex.InnerException.Message;
                        }

                        failedCount++;
                        results.Add(new OrderResultViewModel
                        {
                            ClientCode = dues.ClientCode ?? "",
                            ClientRef = dues.ClientRef.ToString(),
                            IsSuccess = false,
                            OrderNumber = null,
                            ErrorMessage = errorMessage,
                            OrderAmount = dues.Total
                        });

                        _logger.LogError(ex, "Aktarım hatası: {Code}", dues.Code);
                    }
                }

                // Değişiklikleri kaydet
                await _unitOfWork.CommitAsync();

                // Not: BudgetRatio zaten aktarım başladığında Locked yapıldı
                // Burada tekrar kilitlemeye gerek yok

                var message = $"Aktarım tamamlandı. Başarılı: {successCount}, Başarısız: {failedCount}";
                if (isDebugMode)
                {
                    message += $" (DEBUG MOD - Toplam: {duesStatistics.Count} kayıttan {recordsToTransfer.Count} tanesi aktarıldı)";
                }
                _logger.LogInformation(message);

                // Her durumda rapor maili gönder (debug veya normal mod)
                // E-posta gönderimi başarısız olsa bile aktarım başarılı sayılsın
                try
                {
                    // Bütçe türünü al (ilk kayıttan)
                    var buggetType = duesStatistics.FirstOrDefault()?.BudgetType ?? BuggetTypeEnum.Budget;

                    await SendTransferReportEmailAsync(results, duesStatistics.Count, recordsToTransfer.Count, successCount, failedCount, isDebugMode, userId, buggetType);
                    _logger.LogInformation("Aktarım rapor e-postası gönderildi");
                }
                catch (Exception emailEx)
                {
                    // E-posta gönderimi başarısız olsa bile aktarım başarılı sayılsın
                    _logger.LogError(emailEx, "Aktarım rapor e-postası gönderilemedi ancak aktarım başarılı");
                }

                return ResponseDto<List<OrderResultViewModel>>.SuccessData(
                    200, message, results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TransferDuesStatisticsToLogoAsync hatası");
                return ResponseDto<List<OrderResultViewModel>>.FailData(
                    500, "Aktarım hatası", ex.Message, true);
            }
        }

        /// <summary>
        /// Aktarım raporu maili gönder (hem debug hem normal mod için)
        /// </summary>
        private async Task SendTransferReportEmailAsync(
            List<OrderResultViewModel> results,
            int totalCount,
            int processedCount,
            int successCount,
            int failedCount,
            bool isDebugMode,
            string? userId,
            BuggetTypeEnum buggetType)
        {
            try
            {
                // 1. Kullanıcı bilgilerini al
                string toEmail = "admin@sistembilgisayar.app"; // Fallback email
                string userName = "Kullanıcı";

                _logger.LogInformation("Email gönderimi başlıyor. UserId: {UserId}", userId ?? "null");

                if (!string.IsNullOrEmpty(userId))
                {
                    try
                    {
                        var user = await _unitOfWork.Context.Set<Koala.Yedpa.Core.Models.AppUser>()
                            .FirstOrDefaultAsync(u => u.Id == userId);

                        if (user != null)
                        {
                            if (!string.IsNullOrEmpty(user.Email))
                            {
                                toEmail = user.Email;
                            }
                            userName = user.ToString() ?? "Kullanıcı";
                            _logger.LogInformation("Kullanıcı bilgileri bulundu: {Email}, {Name}", toEmail, userName);
                        }
                        else
                        {
                            _logger.LogWarning("Kullanıcı bulunamadı. UserId: {UserId}, Fallback email kullanılacak: {Email}", userId, toEmail);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Kullanıcı bilgileri alınırken hata oluştu. UserId: {UserId}", userId);
                    }
                }
                else
                {
                    _logger.LogWarning("UserId boş, Fallback email kullanılacak: {Email}", toEmail);
                }

                // 2. Bütçe türü metni
                var budgetTypeText = buggetType == BuggetTypeEnum.Budget ? "Bütçe" : "Ek Bütçe";

                // 3. Body içeriğini hazırla (sadece [[Body]] kısmına gelecek kısım)
                var modeText = isDebugMode ? "Debug Raporu" : "Tam Rapor";
                var bodyContent = new StringBuilder();

                // Rapor başlığı
                bodyContent.AppendLine($"<h2 style='color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px;'>📊 {budgetTypeText} Aktarım Raporu</h2>");
                bodyContent.AppendLine($"<p>{budgetTypeText} aktarım işlemi <strong>{modeText}</strong> olarak tamamlandı.</p>");

                // İstatistikler kutusu
                bodyContent.AppendLine("<div style='background: #f8f9fa; padding: 15px; margin: 15px 0; border-left: 4px solid #3498db; border-radius: 4px;'>");

                if (isDebugMode)
                {
                    bodyContent.AppendLine($"<p style='color: #f39c12; margin: 5px 0;'>⚠ <strong>DEBUG MOD:</strong> Toplam {totalCount} kayıttan {processedCount} tanesi işlendi</p>");
                }
                else
                {
                    bodyContent.AppendLine($"<p style='margin: 5px 0;'><strong>📦 Toplam Kayıt:</strong> {totalCount}</p>");
                }

                bodyContent.AppendLine($"<p style='margin: 5px 0;'><strong>✅ Başarılı:</strong> <span style='color: #27ae60; font-weight: bold;'>{successCount}</span></p>");
                bodyContent.AppendLine($"<p style='margin: 5px 0;'><strong>❌ Başarısız:</strong> <span style='color: #e74c3c; font-weight: bold;'>{failedCount}</span></p>");
                bodyContent.AppendLine("</div>");

                // Excel dosyası bildirimi
                bodyContent.AppendLine("<p style='margin-top: 15px;'><strong>📎 Dosya Eki:</strong> Aktarım detayları Excel dosyasında yer almaktadır.</p>");

                // 4. Excel dosyası oluştur - 2 sheet
                byte[] excelBytes;
                using (var workbook = new XLWorkbook())
                {
                    // Sheet 1: Aktarılanlar
                    var successfulSheet = workbook.Worksheets.Add("Aktarılanlar");
                    successfulSheet.Cell("A1").Value = "Cari Kodu";
                    successfulSheet.Cell("B1").Value = "Cari Ref";
                    successfulSheet.Cell("C1").Value = "Sipariş No";
                    successfulSheet.Cell("D1").Value = "Tutar";

                    int row = 2;
                    foreach (var result in results.Where(r => r.IsSuccess))
                    {
                        successfulSheet.Cell(row, 1).Value = result.ClientCode;
                        successfulSheet.Cell(row, 2).Value = result.ClientRef.ToString();
                        successfulSheet.Cell(row, 3).Value = result.OrderNumber ?? "-";
                        successfulSheet.Cell(row, 4).Value = result.OrderAmount;
                        row++;
                    }

                    // Sheet 2: Aktarılamayanlar
                    var failedSheet = workbook.Worksheets.Add("Aktarılamayanlar");
                    failedSheet.Cell("A1").Value = "Cari Kodu";
                    failedSheet.Cell("B1").Value = "Cari Ref";
                    failedSheet.Cell("C1").Value = "Hata Mesajı";
                    failedSheet.Cell("D1").Value = "Tutar";

                    row = 2;
                    foreach (var result in results.Where(r => !r.IsSuccess))
                    {
                        failedSheet.Cell(row, 1).Value = result.ClientCode;
                        failedSheet.Cell(row, 2).Value = result.ClientRef.ToString();
                        failedSheet.Cell(row, 3).Value = result.ErrorMessage ?? "-";
                        failedSheet.Cell(row, 4).Value = result.OrderAmount;
                        row++;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        excelBytes = stream.ToArray();
                    }
                }

                // 5. Email başlığı
                var subject = $"{budgetTypeText} Aktarım {modeText} - {DateTime.Now:yyyy-MM-dd HH:mm}";

                // 6. CustomEmailDto oluştur (template kullanarak)
                // Ad ve soyadı ayrı ayrı gönder (template [[Name]] placeholder'ını kullanacak)
                var nameParts = userName.Split(' ');
                var name = nameParts.Length > 0 ? nameParts[0] : userName;
                var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

                var emailDto = new CustomEmailDto
                {
                    Email = toEmail,
                    Content = bodyContent.ToString(),
                    Title = subject,
                    Name = name,
                    Lastname = lastName,
                    Attachments = new List<EmailAttachmentDto>
                    {
                        new EmailAttachmentDto
                        {
                            FileName = $"Aktarim_Raporu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                            Content = excelBytes,
                            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                        }
                    }
                };

                await _emailService.SendCustomMail(emailDto);

                _logger.LogInformation("Aktarım raporu maili gönderildi: {Email}, Mod: {Mode}, Excel eklendi", toEmail, isDebugMode ? "Debug" : "Normal");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aktarım raporu maili gönderilemedi");
                // Mail hatası aktarım başarısız sayılmamalı
            }
        }

        /// <summary>
        /// DuesStatistic'ten SalesOrder oluştur
        /// </summary>
        private SalesOrderJsonViewModel CreateSalesOrderFromDuesStatistic(DuesStatistic dues)
        {
            // Ayları map et
            var monthMapping = new Dictionary<string, decimal?>
            {
                { "OCAK", dues.January },
                { "ŞUBAT", dues.February },
                { "MART", dues.March },
                { "NİSAN", dues.April },
                { "MAYIS", dues.May },
                { "HAZİRAN", dues.June },
                { "TEMMUZ", dues.July },
                { "AĞUSTOS", dues.August },
                { "EYLÜL", dues.September },
                { "EKİM", dues.October },
                { "KASIM", dues.November },
                { "ARALIK", dues.December }
            };

            // SalesOrder oluştur
            var salesOrder = new SalesOrderJsonViewModel
            {
                DOC_TRACK_NR = dues.DocTrackingNr.ToString(),
                ARP_CODE = dues.ClientCode ?? "",
                DATE = new DateTime(int.Parse(dues.Year), 1, 1),
                NOTES1 = $"{dues.Year} yılı için bütçe aktarımı - {dues.DivName}",
                NOTES2 = "Bütçe Aktarımı",
                DOC_TRACKING_NR = dues.DocTrackingNr.ToString()
            };

            // Transaction items oluştur (sadece tutar > 0 olan aylar)
            var items = new List<SalesOrderTransactionItem>();
            var year = int.Parse(dues.Year);

            foreach (var month in monthMapping)
            {
                var amount = month.Value;
                if (amount.HasValue && amount.Value > 0)
                {
                    // Ay numarasını bul
                    var monthNumber = Array.IndexOf(monthMapping.Keys.ToArray(), month.Key) + 1;

                    items.Add(new SalesOrderTransactionItem
                    {
                        PRICE = amount.Value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                        TRANS_DESCRIPTION = month.Key,
                        DUE_DATE = new DateTime(year, monthNumber, 1)
                    });
                }
            }

            salesOrder.TRANSACTIONS = new SalesOrderTransactions { Items = items };

            return salesOrder;
        }
    }
}
