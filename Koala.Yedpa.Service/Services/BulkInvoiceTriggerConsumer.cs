using System.Text;
using System.Text.Json;
using Koala.Yedpa.Core.Configuration;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Koala.Yedpa.Service.Services
{
    /// <summary>
    /// N8N'in RabbitMQ'ya attığı "aktarımı başlat" tetiğini dinleyen outbound consumer.
    /// Mesajdaki tarihe ait bekleyen oturumu bulur ve BulkInvoiceJobs.RunTransferAsync'i çalıştırır.
    /// Broker erişilemezse uygulamayı ÇÖKERTMEZ; 10 sn'de bir yeniden dener (+ otomatik recovery).
    /// </summary>
    public class BulkInvoiceTriggerConsumer : BackgroundService
    {
        private readonly RabbitMqSettings _cfg;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BulkInvoiceTriggerConsumer> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        public BulkInvoiceTriggerConsumer(
            IOptions<RabbitMqSettings> cfg,
            IServiceScopeFactory scopeFactory,
            ILogger<BulkInvoiceTriggerConsumer> logger)
        {
            _cfg = cfg.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(_cfg.HostName))
            {
                _logger.LogWarning("RabbitMq:HostName boş — toplu fatura tetik consumer başlatılmadı.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await StartConsumingAsync(stoppingToken);
                    await Task.Delay(Timeout.Infinite, stoppingToken); // auto-recovery devrede; iptale kadar bekle
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RabbitMQ tetik consumer bağlanamadı; 10 sn sonra tekrar denenecek.");
                    await SafeCloseAsync();
                    try { await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); }
                    catch (OperationCanceledException) { break; }
                }
            }

            await SafeCloseAsync();
        }

        private async Task StartConsumingAsync(CancellationToken ct)
        {
            var factory = new ConnectionFactory
            {
                HostName = _cfg.HostName,
                Port = _cfg.Port,
                UserName = _cfg.UserName,
                Password = _cfg.Password,
                VirtualHost = string.IsNullOrWhiteSpace(_cfg.VirtualHost) ? "/" : _cfg.VirtualHost,
                AutomaticRecoveryEnabled = true
            };
            if (_cfg.UseTls)
                factory.Ssl = new SslOption { Enabled = true, ServerName = _cfg.HostName };

            _connection = await factory.CreateConnectionAsync(ct);
            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
            await _channel.QueueDeclareAsync(_cfg.TriggerQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageAsync;
            await _channel.BasicConsumeAsync(_cfg.TriggerQueue, autoAck: false, consumer: consumer, cancellationToken: ct);

            _logger.LogInformation("RabbitMQ tetik consumer dinliyor: {Queue}@{Host}", _cfg.TriggerQueue, _cfg.HostName);
        }

        private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            try
            {
                await ProcessAsync(ParseDate(body));
                if (_channel is not null)
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toplu fatura tetiği işlenemedi: {Body}", body);
                // Döngüye girmesin diye requeue etmiyoruz; eksikler Manage sayfasından yeniden aktarılır.
                if (_channel is not null)
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        }

        /// <summary>JSON { "date": "yyyy-MM-dd" } ya da düz "yyyy-MM-dd"; tarih yoksa null (o an bekleyen son oturum işlenir).</summary>
        private static DateOnly? ParseDate(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("date", out var d) &&
                    d.GetString() is { } s && DateOnly.TryParse(s, out var jd))
                    return jd;
            }
            catch (JsonException) { /* düz metin dene */ }

            return DateOnly.TryParse(body.Trim().Trim('"'), out var raw) ? raw : null;
        }

        private async Task ProcessAsync(DateOnly? date)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var query = db.BulkInvoiceSessions.Where(s => s.Status == BulkInvoiceSessionStatus.Pending);
            if (date is { } d)
            {
                var dt = d.ToDateTime(TimeOnly.MinValue);
                query = query.Where(s => s.InvoiceDate.Date == dt);
            }

            var session = await query.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
            if (session is null)
            {
                _logger.LogWarning("Tetik geldi ama uygun bekleyen oturum yok. Tarih={Date}", date);
                return;
            }

            _logger.LogInformation("Tetik alındı → aktarım başlıyor. Session {Id}, Tarih {Date:dd.MM.yyyy}", session.Id, session.InvoiceDate);
            var jobs = scope.ServiceProvider.GetRequiredService<BulkInvoiceJobs>();
            await jobs.RunTransferAsync(session.Id);
        }

        private async Task SafeCloseAsync()
        {
            try { if (_channel is not null) await _channel.CloseAsync(); } catch { /* yut */ }
            try { if (_connection is not null) await _connection.CloseAsync(); } catch { /* yut */ }
            _channel = null;
            _connection = null;
        }
    }
}
