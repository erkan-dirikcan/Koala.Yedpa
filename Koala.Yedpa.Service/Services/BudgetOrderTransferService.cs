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

                // DuesStatistic kayıtlarını tek tek getir
                var duesStatistics = new List<DuesStatistic>();
                foreach (var id in duesStatisticIds)
                {
                    var dues = await _unitOfWork.DuesStatisticRepository.GetByIdAsync(id);
                    if (dues != null)
                    {
                        duesStatistics.Add(dues);
                    }
                }

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
                            dues.TransferStatus = TransferStatusEnum.Completed;
                            dues.LastUpdateTime = DateTime.Now;
                            await _unitOfWork.DuesStatisticRepository.UpdateAsync(dues);

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
                            // Başarısız
                            failedCount++;
                            results.Add(new OrderResultViewModel
                            {
                                ClientCode = dues.ClientCode ?? "",
                                ClientRef = dues.ClientRef.ToString(),
                                IsSuccess = false,
                                OrderNumber = null,
                                ErrorMessage = response.Message,
                                OrderAmount = dues.Total
                            });

                            _logger.LogError("Başarısız: {Code} - {Error}", dues.Code, response.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        results.Add(new OrderResultViewModel
                        {
                            ClientCode = dues.ClientCode ?? "",
                            ClientRef = dues.ClientRef.ToString(),
                            IsSuccess = false,
                            OrderNumber = null,
                            ErrorMessage = ex.Message,
                            OrderAmount = dues.Total
                        });

                        _logger.LogError(ex, "Aktarım hatası: {Code}", dues.Code);
                    }
                }

                // Değişiklikleri kaydet
                await _unitOfWork.CommitAsync();

                var message = $"Aktarım tamamlandı. Başarılı: {successCount}, Başarısız: {failedCount}";
                if (isDebugMode)
                {
                    message += $" (DEBUG MOD - Toplam: {duesStatistics.Count} kayıttan {recordsToTransfer.Count} tanesi aktarıldı)";
                }
                _logger.LogInformation(message);

                // Her durumda rapor maili gönder (debug veya normal mod)
                await SendTransferReportEmailAsync(results, duesStatistics.Count, recordsToTransfer.Count, successCount, failedCount, isDebugMode, userId);

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
            string? userId)
        {
            try
            {
                var modeText = isDebugMode ? "Debug Raporu" : "Tam Rapor";
                var subject = $"BudgetOrder Aktarım {modeText} - {DateTime.Now:yyyy-MM-dd HH:mm}";
                var body = new StringBuilder();

                body.AppendLine($"<h2>BudgetOrder Aktarım {modeText}</h2>");
                body.AppendLine($"<p><strong>Tarih:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

                if (isDebugMode)
                {
                    body.AppendLine($"<p style='color: #ff9800;'><strong>⚠ DEBUG MOD:</strong> Toplam {totalCount} kayıttan {processedCount} tanesi işlendi</p>");
                }
                else
                {
                    body.AppendLine($"<p><strong>Toplam Kayıt:</strong> {totalCount}</p>");
                }

                body.AppendLine($"<p><strong>Başarılı:</strong> <span style='color: green; font-weight: bold;'>{successCount}</span></p>");
                body.AppendLine($"<p><strong>Başarısız:</strong> <span style='color: red; font-weight: bold;'>{failedCount}</span></p>");
                body.AppendLine("<hr/>");

                body.AppendLine("<h3>Aktarım Detayları</h3>");
                body.AppendLine("<table border='1' cellpadding='5' style='border-collapse: collapse; width: 100%;'>");
                body.AppendLine("<tr style='background-color: #f0f0f0;'>");
                body.AppendLine("<th>Cari Kodu</th>");
                body.AppendLine("<th>Cari Ref</th>");
                body.AppendLine("<th>Durum</th>");
                body.AppendLine("<th>Sipariş No</th>");
                body.AppendLine("<th>Tutar</th>");
                body.AppendLine("<th>Hata Mesajı</th>");
                body.AppendLine("</tr>");

                foreach (var result in results)
                {
                    var rowColor = result.IsSuccess ? "#d4edda" : "#f8d7da";
                    var statusIcon = result.IsSuccess ? "✓" : "✗";
                    var statusText = result.IsSuccess ? "Başarılı" : "Başarısız";

                    body.AppendLine($"<tr style='background-color: {rowColor};'>");
                    body.AppendLine($"<td>{result.ClientCode}</td>");
                    body.AppendLine($"<td>{result.ClientRef}</td>");
                    body.AppendLine($"<td><strong>{statusIcon} {statusText}</strong></td>");
                    body.AppendLine($"<td>{result.OrderNumber ?? "-"}</td>");
                    body.AppendLine($"<td>{result.OrderAmount:F2} TL</td>");
                    body.AppendLine($"<td style='color: red;'>{result.ErrorMessage ?? "-"}</td>");
                    body.AppendLine("</tr>");
                }

                body.AppendLine("</table>");
                body.AppendLine("<hr/>");
                body.AppendLine("<p style='color: #666; font-size: 12px;'><em>Bu otomatik olarak üretilen bir rapordur.</em></p>");

                // İşlemi yapan kullanıcının email adresini al
                string toEmail = "admin@sistembilgisayar.app"; // Fallback email

                if (!string.IsNullOrEmpty(userId))
                {
                    try
                    {
                        // DbContext üzerinden AppUser'ı al
                        var user = await _unitOfWork.Context.Set<Koala.Yedpa.Core.Models.AppUser>()
                            .FirstOrDefaultAsync(u => u.Id == userId);

                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            toEmail = user.Email;
                            _logger.LogInformation("Kullanıcı email adresi bulundu: {Email}", toEmail);
                        }
                        else
                        {
                            _logger.LogWarning("Kullanıcı veya email adresi bulunamadı. UserId: {UserId}", userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Kullanıcı email adresi alınırken hata oluştu. UserId: {UserId}", userId);
                    }
                }
                else
                {
                    _logger.LogWarning("UserId boş, fallback email kullanılacak: {Email}", toEmail);
                }

                var emailDto = new CustomEmailDto
                {
                    Email = toEmail,
                    Content = body.ToString(),
                    Title = subject
                };

                await _emailService.SendCustomMail(emailDto);

                _logger.LogInformation("Aktarım raporu maili gönderildi: {Email}, Mod: {Mode}", toEmail, isDebugMode ? "Debug" : "Normal");
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
