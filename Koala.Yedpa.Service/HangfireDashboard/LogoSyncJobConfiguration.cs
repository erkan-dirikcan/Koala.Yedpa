// 2. LogoSyncJobConfiguration.cs → yeni dosya, bunu ekle!
using Hangfire;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Koala.Yedpa.Service.Jobs
{
    public class LogoSyncJobConfiguration : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        public LogoSyncJobConfiguration(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

            recurringJobManager.AddOrUpdate<ILogoSyncJobService>(
                "logo-nightly-sync",
                job => job.SyncFromLogoAsync(null),
                "0 23 * * *"); // her gece 23:00

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}