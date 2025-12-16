// Services/SeedHostedService.cs
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services
{
    public class SeedHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SeedHostedService> _logger;

        public SeedHostedService(IServiceProvider serviceProvider, ILogger<SeedHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Seed işlemi başlatılıyor...");

            using var scope = _serviceProvider.CreateScope();
            var seedService = scope.ServiceProvider.GetRequiredService<ISeedService>();

            try
            {
                await seedService.SeedAsync(); // ← sadece bu satırı değiştir, token’ı kaldır
                _logger.LogInformation("Seed işlemi başarıyla tamamlandı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seed işlemi sırasında hata oluştu!");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}