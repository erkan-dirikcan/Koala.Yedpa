using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace Koala.Yedpa.Service.Services.BackgroundServices
{
    /// <summary>
    /// DuesStatistic aktarım arkaplan servisi
    /// Hangfire yerine daha basit bir arkaplan işlemi için
    /// </summary>
    public class DuesStatisticTransferBackgroundService : BackgroundService
    {
        private readonly ILogger<DuesStatisticTransferBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly DuesStatisticTransferQueue _queue;

        public DuesStatisticTransferBackgroundService(
            ILogger<DuesStatisticTransferBackgroundService> logger,
            IServiceProvider serviceProvider,
            DuesStatisticTransferQueue queue)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DuesStatisticTransferBackgroundService başladı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Queue'dan iş al (awaitable, blocking)
                    var workItem = await _queue.DequeueAsync(stoppingToken);

                    if (workItem != null)
                    {
                        _logger.LogInformation("Aktarım job'ı başladı: {JobId}, Kayıt sayısı: {Count}",
                            workItem.JobId, workItem.DuesStatisticIds.Count);

                        // Scope oluştur (scoped services için)
                        using var scope = _serviceProvider.CreateScope();

                        var budgetOrderService = scope.ServiceProvider.GetRequiredService<IBudgetOrderService>();

                        // Aktarımı yap
                        var result = await budgetOrderService.TransferDuesStatisticsToLogoAsync(
                            workItem.DuesStatisticIds,
                            workItem.UserId,
                            workItem.IsDebugMode);

                        if (result.IsSuccess)
                        {
                            _logger.LogInformation("Aktarım job'ı tamamlandı: {JobId}, Sonuç: {Message}",
                                workItem.JobId, result.Message);
                        }
                        else
                        {
                            _logger.LogError("Aktarım job'ı başarısız: {JobId}, Hata: {Message}",
                                workItem.JobId, result.Message);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Service durduruluyor
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Aktarım job'ı işlenirken hata oluştu");
                }
            }

            _logger.LogInformation("DuesStatisticTransferBackgroundService durduruldu.");
        }
    }

    /// <summary>
    /// DuesStatistic aktarım iş öğesi
    /// </summary>
    public class DuesStatisticTransferWorkItem
    {
        public string JobId { get; set; } = Guid.NewGuid().ToString();
        public List<string> DuesStatisticIds { get; set; } = new();
        public string? UserId { get; set; }
        public bool IsDebugMode { get; set; }
    }

    /// <summary>
    /// DuesStatistic aktarım kuyruğu
    /// Thread-safe, singleton queue
    /// </summary>
    public class DuesStatisticTransferQueue
    {
        private readonly Channel<DuesStatisticTransferWorkItem> _queue;
        private readonly ILogger<DuesStatisticTransferQueue> _logger;
        private int _count = 0;

        public DuesStatisticTransferQueue(ILogger<DuesStatisticTransferQueue> logger)
        {
            var options = new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            };
            _queue = Channel.CreateUnbounded<DuesStatisticTransferWorkItem>(options);
            _logger = logger;
        }

        /// <summary>
        /// Queue'ya iş ekle
        /// </summary>
        public async Task EnqueueAsync(DuesStatisticTransferWorkItem workItem)
        {
            await _queue.Writer.WriteAsync(workItem);
            Interlocked.Increment(ref _count);
            _logger.LogInformation("Job kuyruğa eklendi: {JobId}, Kayıt sayısı: {Count}",
                workItem.JobId, workItem.DuesStatisticIds.Count);
        }

        /// <summary>
        /// Queue'dan iş al (blocking)
        /// </summary>
        public async Task<DuesStatisticTransferWorkItem?> DequeueAsync(CancellationToken cancellationToken)
        {
            var item = await _queue.Reader.ReadAsync(cancellationToken);
            if (item != null)
            {
                Interlocked.Decrement(ref _count);
            }
            return item;
        }

        /// <summary>
        /// Queue'daki iş sayısı
        /// </summary>
        public int Count => Volatile.Read(ref _count);
    }
}
