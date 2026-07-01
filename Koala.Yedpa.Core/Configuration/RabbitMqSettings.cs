namespace Koala.Yedpa.Core.Configuration
{
    /// <summary>
    /// RabbitMQ (Coolify VPS) bağlantı ayarları. Parola secret'ten gelir (appsettings'e gömülmez).
    /// Uygulama yalnızca outbound consumer olarak bağlanır; N8N'in attığı "aktarımı başlat" tetiğini dinler.
    /// </summary>
    public class RabbitMqSettings
    {
        public const string SectionName = "RabbitMq";

        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; } = 5671;            // amqps varsayılan; TLS yoksa 5672
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string VirtualHost { get; set; } = "/";
        public bool UseTls { get; set; } = true;
        public string TriggerQueue { get; set; } = "bulk_invoice.run";
    }
}
