namespace Koala.Yedpa.Core.Services
{
    /// <summary>
    /// N8N'in okuyacağı "sıradaki aktarım tarihi"ni tutan dış depo (Coolify PostgreSQL).
    /// Uygulama, tarih seçildiğinde tek satırı upsert eder (yoksa insert, varsa update).
    /// Yalnızca kontrol verisi (tarih) taşınır — iş verisi VPS'e gitmez.
    /// </summary>
    public interface IScheduleStore
    {
        Task UpsertTransferDateAsync(DateOnly transferDate, CancellationToken ct = default);
    }
}
