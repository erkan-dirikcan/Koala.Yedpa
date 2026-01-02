// Service/Jobs/LogoSyncJobService.cs
using Hangfire;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services.Jobs
{
    public class LogoSyncJobService : ILogoSyncJobService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LogoSyncJobService> _logger;
        private readonly ITransactionService _transactionService;
        private readonly ITransactionItemService _transactionItemService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        public LogoSyncJobService(IServiceProvider serviceProvider, ILogger<LogoSyncJobService> logger, ITransactionItemService transactionItemService, ITransactionService transactionService, IEmailService emailService, IEmailTemplateService emailTemplateService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _transactionItemService = transactionItemService;
            _transactionService = transactionService;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        [DisableConcurrentExecution(timeoutInSeconds: 600)]
        public async Task SyncFromLogoAsync(string? triggeredByUserId = null)
        {
            string? firmNumber = null;
            try
            {
                // Her job kendi scope'unu yaratsın → temiz ve güvenli
                using var scope = _serviceProvider.CreateScope();
                var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
                var logoSyncService = scope.ServiceProvider.GetRequiredService<ILogoSyncService>();

                // AYARLARI HER SEFERİNDE TAZE ÇEK (en güvenli yöntem)
                var settingsResponse = await settingsService.GetLogoSettingsAsync();

                if (settingsResponse == null || !settingsResponse.IsSuccess || settingsResponse.Data == null)
                {
                    _logger.LogWarning("Logo ayarları alınamadı veya eksik.");
                    return;
                }

                var logoSettings = settingsResponse.Data;
                firmNumber = logoSettings.Firm;

                if (string.IsNullOrWhiteSpace(logoSettings.Firm))
                {
                    _logger.LogWarning("Logo firma numarası boş. Senkronizasyon iptal edildi.");
                    return;
                }

                _logger.LogInformation("Logo firma {Firm} için senkronizasyon başlatılıyor... (Tetkikleyen: {User})",
                    logoSettings.Firm, triggeredByUserId ?? "Sistem");

                var result = await logoSyncService.SyncXt001211Async(
                    firm: logoSettings.Firm,
                    userId: triggeredByUserId ?? null);

                var template = await _emailTemplateService.GetByNameAsyc("Default");
                if (!template.IsSuccess || template.Data == null)
                {
                    _logger.LogWarning("E-Posta şablonu alınamadı. E-posta gönderimi atlanıyor. TransactionId: {TransactionId}", result.Data);

                    // Mail gönderimi olmadan işlemi bitir
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Logo senkronizasyonu başarıyla tamamlandı. TransactionId: {TransactionId}", result.Data);
                    }
                    else
                    {
                        var errorDetails = result.Errors != null && result.Errors.Errors.Any()
                            ? string.Join("; ", result.Errors.Errors)
                            : result.Message;
                        _logger.LogError("Logo senkronizasyonu başarısız: {Message}. Hata Detayları: {ErrorDetails}", result.Message, errorDetails);
                        throw new InvalidOperationException($"Logo sync başarısız: {result.Message}. Detaylar: {errorDetails}");
                    }
                    return;
                }

                var mailList = new List<CustomEmailDto>
                {
                    new CustomEmailDto
                    {
                        Name = "Erkan",
                        Lastname = "DİRİKCAN",
                        Email = "erkan@sistem-bilgisayar.com",
                        Content = "",
                        Title = "Logo Senkronizasyon Bilgilendirme"
                    }
                };

                if (result.IsSuccess)
                {

                    foreach (var item in mailList)
                    {
                        var mailContent = template.Data.Content;
                        mailContent = mailContent.Replace("[[Name]]", $"{item.Name} {item.Lastname}");
                        var content =
                            string.Format(
                                "<span style=\"color:#ff0000;\">{0}</span> Transaction ID'li Senkronizasyon işlemi başarıyla tamamlandı",
                                result.Data);
                        mailContent = mailContent.Replace("[[Body]]", content);
                        item.Content = mailContent;

                        try
                        {
                            await _emailService.SendCustomMail(item);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "E-posta gönderilirken hata oluştu. Email: {Email}", item.Email);
                        }
                    }
                    _logger.LogInformation(
                        "Logo senkronizasyonu başarıyla tamamlandı. TransactionId: {TransactionId}", result.Data);
                }
                else
                {
                    foreach (var item in mailList)
                    {
                        var mailContent = template.Data.Content;
                        mailContent = mailContent.Replace("[[Name]]", $"{item.Name} {item.Lastname}");

                        var errorDetails = result.Errors != null && result.Errors.Errors.Any()
                            ? string.Join("; ", result.Errors.Errors)
                            : result.Message;

                        var content =
                            string.Format(
                                "<span style=\"color:#ff0000;\">{0}</span> Transaction ID'li Senkronizasyon işlemi başarısız oldu. Hata: {1}",
                                result.Data,
                                errorDetails);
                        mailContent = mailContent.Replace("[[Body]]", content);
                        item.Content = mailContent;

                        try
                        {
                            await _emailService.SendCustomMail(item);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "E-posta gönderilirken hata oluştu. Email: {Email}", item.Email);
                        }
                    }
                    var errorDetailsLog = result.Errors != null && result.Errors.Errors.Any()
                        ? string.Join("; ", result.Errors.Errors)
                        : result.Message;

                    _logger.LogError("Logo senkronizasyonu başarısız: {Message}. Hata Detayları: {ErrorDetails}", result.Message, errorDetailsLog);
                    throw new InvalidOperationException($"Logo sync başarısız: {result.Message}. Detaylar: {errorDetailsLog}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logo senkronizasyonu sırasında beklenmeyen hata oluştu. Firm: {Firm}, TriggeredBy: {User}",
                    firmNumber ?? "Bilinmiyor", triggeredByUserId ?? "Sistem");
                throw;
            }
        }
    }
}
