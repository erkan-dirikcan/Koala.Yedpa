// Service/Jobs/LogoSyncJobService.cs
using Hangfire;
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

        public LogoSyncJobService(IServiceProvider serviceProvider, ILogger<LogoSyncJobService> logger, ITransactionItemService transactionItemService, ITransactionService transactionService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _transactionItemService = transactionItemService;
            _transactionService = transactionService;
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
