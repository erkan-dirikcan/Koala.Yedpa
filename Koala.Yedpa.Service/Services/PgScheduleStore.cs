using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Koala.Yedpa.Service.Services
{
    /// <summary>
    /// Aktarım tarihini Coolify PostgreSQL'de tek satırda tutar (N8N zamanlayıcısı bunu okur).
    /// Bağlantı: ConnectionStrings:N8nScheduleDb (secret). Tablo yoksa otomatik oluşturulur.
    /// </summary>
    public class PgScheduleStore : IScheduleStore
    {
        private readonly string _connString;
        private readonly ILogger<PgScheduleStore> _logger;

        public PgScheduleStore(IConfiguration config, ILogger<PgScheduleStore> logger)
        {
            _connString = config.GetConnectionString("N8nScheduleDb") ?? string.Empty;
            _logger = logger;
        }

        public async Task UpsertTransferDateAsync(DateOnly transferDate, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_connString))
            {
                _logger.LogWarning("ConnectionStrings:N8nScheduleDb boş — aktarım tarihi PG'ye yazılmadı ({Date}).", transferDate);
                return;
            }

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync(ct);

            const string ddl = @"CREATE TABLE IF NOT EXISTS bulk_invoice_schedule (
                                     id            int PRIMARY KEY,
                                     transfer_date date NOT NULL,
                                     updated_at    timestamptz NOT NULL DEFAULT now());";
            await using (var cmd = new NpgsqlCommand(ddl, conn))
                await cmd.ExecuteNonQueryAsync(ct);

            const string upsert = @"INSERT INTO bulk_invoice_schedule (id, transfer_date, updated_at)
                                    VALUES (1, @d, now())
                                    ON CONFLICT (id) DO UPDATE
                                        SET transfer_date = EXCLUDED.transfer_date, updated_at = now();";
            await using (var cmd = new NpgsqlCommand(upsert, conn))
            {
                cmd.Parameters.AddWithValue("d", transferDate);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            _logger.LogInformation("Aktarım tarihi PG'ye yazıldı (N8N okuyacak): {Date}", transferDate);
        }
    }
}
