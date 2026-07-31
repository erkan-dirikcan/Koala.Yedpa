using System.Text;
using System.Text.Json;
using Koala.Yedpa.Core.Configuration;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Koala.Yedpa.Service.Services
{
    /// <summary>
    /// N8N'in RabbitMQ'ya attığı toplu faturalandırma tetiklerini dinleyen outbound consumer.
    /// İki tetik türü vardır (mesajdaki "kind" alanı):
    /// - <c>info</c>  : aktarımdan bir gün önce 12:01 → bilgilendirme maili (SendInfoMailAsync).
    /// - <c>transfer</c> (varsayılan): aktarım günü 00:01 → faturalandırma (RunTransferAsync).
    /// Her iki mesajda da "date" AKTARIM tarihidir; oturum bu tarihe göre bulunur.
    /// Broker erişilemezse uygulamayı ÇÖKERTMEZ; 10 sn'de bir yeniden dener (+ otomatik recovery).
    /// </summary>
    public class BulkInvoiceTriggerConsumer : BackgroundService
    {
        private readonly RabbitMqSettings _cfg;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly IHostEnvironment _env;
        private readonly ILogger<BulkInvoiceTriggerConsumer> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        public BulkInvoiceTriggerConsumer(
            IOptions<RabbitMqSettings> cfg,
            IServiceScopeFactory scopeFactory,
            IConfiguration config,
            IHostEnvironment env,
            ILogger<BulkInvoiceTriggerConsumer> logger)
        {
            _cfg = cfg.Value;
            _scopeFactory = scopeFactory;
            _config = config;
            _env = env;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(_cfg.HostName))
            {
                // DİKKAT: Bu durumda toplu faturalandırma tetiği HİÇ ÇALIŞMAZ (sessiz arıza).
                // Sunucuda RabbitMq__HostName / ConnectionStrings__N8nScheduleDb env değişkenleri tanımlı olmalı.
                _logger.LogError("RabbitMq:HostName boş — toplu fatura tetik consumer BAŞLATILMADI, " +
                    "aktarım otomatik tetiklenmeyecek. Env={Env} rawHost='{Raw}' pgConn={Pg}",
                    _env.EnvironmentName,
                    _config["RabbitMq:HostName"] ?? "<null>",
                    string.IsNullOrEmpty(_config.GetConnectionString("N8nScheduleDb")) ? "BOŞ" : "tanımlı");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await StartConsumingAsync(stoppingToken);
                    await Task.Delay(Timeout.Infinite, stoppingToken); // auto-recovery devrede; iptale kadar bekle
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RabbitMQ tetik consumer bağlanamadı; 10 sn sonra tekrar denenecek.");
                    await SafeCloseAsync();
                    try { await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
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
                var (date, kind) = ParseTrigger(body);
                await ProcessAsync(date, kind);
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

        /// <summary>Tetik türü — N8N mesajındaki "kind" alanı.</summary>
        private enum TriggerKind
        {
            /// <summary>Aktarım günü 00:01 — faturaları oluştur.</summary>
            Transfer,
            /// <summary>Aktarımdan bir gün önce 12:01 — bilgilendirme maili + Excel.</summary>
            Info
        }

        /// <summary>
        /// JSON { "date": "yyyy-MM-dd", "kind": "transfer|info" } ya da düz "yyyy-MM-dd".
        /// "kind" yoksa geriye dönük uyumluluk için Transfer varsayılır.
        /// Tarih yoksa null (o an bekleyen son oturum işlenir).
        /// Her iki tetikte de "date" AKTARIM tarihidir (info tetiği bir gün önce gelir ama
        /// yine aktarım tarihini taşır) — oturum eşleştirmesi tek kurala göre yapılır.
        /// </summary>
        private static (DateOnly? Date, TriggerKind Kind) ParseTrigger(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return (null, TriggerKind.Transfer);

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    DateOnly? date = null;
                    if (doc.RootElement.TryGetProperty("date", out var d) &&
                        d.GetString() is { } s && DateOnly.TryParse(s, out var jd))
                        date = jd;

                    var kind = TriggerKind.Transfer;
                    if (doc.RootElement.TryGetProperty("kind", out var k) &&
                        string.Equals(k.GetString(), "info", StringComparison.OrdinalIgnoreCase))
                        kind = TriggerKind.Info;

                    return (date, kind);
                }
            }
            catch (JsonException)
            {
                /* düz metin dene */
            }

            return (DateOnly.TryParse(body.Trim().Trim('"'), out var raw) ? raw : null, TriggerKind.Transfer);
        }

        private async Task ProcessAsync(DateOnly? date, TriggerKind kind)
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
                _logger.LogWarning("Tetik geldi ama uygun bekleyen oturum yok. Tür={Kind} Tarih={Date}", kind, date);
                return;
            }

            var jobs = scope.ServiceProvider.GetRequiredService<BulkInvoiceJobs>();

            if (kind == TriggerKind.Info)
            {
                _logger.LogInformation("Bilgilendirme tetiği → T-1 maili gönderiliyor. Session {Id}, Aktarım {Date:dd.MM.yyyy}",
                    session.Id, session.InvoiceDate);
                await jobs.SendInfoMailAsync(session.Id);
                return;
            }

            _logger.LogInformation("Aktarım tetiği → aktarım başlıyor. Session {Id}, Tarih {Date:dd.MM.yyyy}",
                session.Id, session.InvoiceDate);
            await jobs.RunTransferAsync(session.Id);
        }

        private async Task SafeCloseAsync()
        {
            try { if (_channel is not null) await _channel.CloseAsync(); }
            catch
            {
                /* yut */
            }
            try { if (_connection is not null) await _connection.CloseAsync(); }
            catch
            {
                /* yut */
            }
            _channel = null;
            _connection = null;
        }
    }
}
