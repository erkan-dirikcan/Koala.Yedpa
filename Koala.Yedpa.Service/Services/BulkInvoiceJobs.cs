using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services
{
    /// <summary>
    /// Hangfire ile zamanlanan toplu faturalandırma job'ları (bilgi maili + aktarım).
    /// Sonuçlar cache'te bekletilmez — her satır anında DB'ye yazılır (elektrik/sistem riski).
    /// </summary>
    public class BulkInvoiceJobs
    {
        private const int MaxRetryRounds = 3;

        private readonly AppDbContext _db;
        private readonly IBulkInvoiceService _bulk;
        private readonly IBulkInvoiceTransferService _transfer;
        private readonly IBulkInvoiceEmailService _email;
        private readonly ILogger<BulkInvoiceJobs> _logger;

        public BulkInvoiceJobs(
            AppDbContext db,
            IBulkInvoiceService bulk,
            IBulkInvoiceTransferService transfer,
            IBulkInvoiceEmailService email,
            ILogger<BulkInvoiceJobs> logger)
        {
            _db = db;
            _bulk = bulk;
            _transfer = transfer;
            _email = email;
            _logger = logger;
        }

        /// <summary>T-1 gün 08:00 — bilgilendirme maili + Excel.</summary>
        public Task SendInfoMailAsync(int sessionId) => _email.SendInfoMailAsync(sessionId);

        /// <summary>T günü 00:01 — bekleyen AIDAT satırlarından fatura oluşturur.</summary>
        public async Task RunTransferAsync(int sessionId)
        {
            var session = await _db.BulkInvoiceSessions.FindAsync(sessionId);
            if (session == null) { _logger.LogWarning("Aktarım: session yok {Id}", sessionId); return; }

            session.Status = BulkInvoiceSessionStatus.Processing;
            await _db.SaveChangesAsync();

            // 1+2) Aktarılacak satırları güncelle/oluştur — gün içi "Aktarılacak Verileri Oluştur"
            // ile AYNI snapshot. Mevcut Pending'ler korunur/güncellenir, çift kayıt olmaz.
            await _bulk.SyncSessionItemsAsync(sessionId);

            // Yalnızca "gönderilmedi" (Pending) satırları işle
            var items = await _db.BulkInvoiceItems
                .Where(i => i.SessionId == session.Id && i.Status == BulkInvoiceItemStatus.Pending)
                .ToListAsync();

            // 3) İlk geçiş — her sonucu ANINDA kaydet
            foreach (var item in items)
                await TryTransferItemAsync(item, session.InvoiceDate);

            // 4) Kuyruk sonrası retry — token/geçici hatalılar (CanRetry), 3 tura kadar
            for (var round = 0; round < MaxRetryRounds; round++)
            {
                var retryables = items
                    .Where(i => i.Status == BulkInvoiceItemStatus.Failed && i.CanRetry && i.RetryCount < MaxRetryRounds)
                    .ToList();
                if (retryables.Count == 0) break;

                await Task.Delay(5000);
                foreach (var item in retryables)
                    await TryTransferItemAsync(item, session.InvoiceDate);
            }

            // 3 denemeden sonra hâlâ başarısız → artık denenmez
            foreach (var item in items.Where(i => i.Status == BulkInvoiceItemStatus.Failed && i.RetryCount >= MaxRetryRounds))
            {
                item.CanRetry = false;
                item.Note = "3 deneme sonrası aktarılamadı";
            }
            await _db.SaveChangesAsync();

            // 5) Başarılı satırlar → Logo'da TRGFLAG=1
            var transferredRefs = items
                .Where(i => i.Status == BulkInvoiceItemStatus.Transferred)
                .Select(i => i.Orflineref)
                .ToList();
            await _bulk.MarkLinesAsTransferredAsync(transferredRefs);

            session.Status = BulkInvoiceSessionStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // 6) Görev komple bitti → rapor maili
            await _email.SendReportMailAsync(session.Id);
        }

        /// <summary>
        /// Yönetim sayfasından manuel tetiklenen yeniden aktarım: oturumun başarısız (Failed)
        /// satırlarını tekrar dener (3-deneme limitini gözetmeden, manuel istek olduğundan).
        /// </summary>
        public async Task RetryFailedAsync(int sessionId)
        {
            var session = await _db.BulkInvoiceSessions.FindAsync(sessionId);
            if (session == null) { _logger.LogWarning("Yeniden aktarım: session yok {Id}", sessionId); return; }

            var failed = await _db.BulkInvoiceItems
                .Where(i => i.SessionId == sessionId && i.Status == BulkInvoiceItemStatus.Failed)
                .ToListAsync();
            if (failed.Count == 0) { _logger.LogInformation("Yeniden aktarım: başarısız satır yok {Id}", sessionId); return; }

            foreach (var item in failed)
                await TryTransferItemAsync(item, session.InvoiceDate);

            var newlyTransferred = failed
                .Where(i => i.Status == BulkInvoiceItemStatus.Transferred)
                .Select(i => i.Orflineref)
                .ToList();
            await _bulk.MarkLinesAsTransferredAsync(newlyTransferred);

            await _email.SendReportMailAsync(sessionId);
        }

        /// <summary>Tek item'ı dener (satır verisini item'dan kurar); sonucu yazar ve ANINDA persist eder.</summary>
        private async Task TryTransferItemAsync(BulkInvoiceItem item, DateTime invoiceDate)
        {
            var line = new PendingInvoiceLineDto
            {
                OrficheRef = item.OrficheRef,
                Orflineref = item.Orflineref,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                Amount = item.Amount,
                MonthName = item.MonthName
            };

            var r = await _transfer.TransferLineAsync(line, invoiceDate);
            item.RetryCount++;

            if (r.Success)
            {
                item.Status = BulkInvoiceItemStatus.Transferred;
                item.LogoInvoiceRef = r.LogoInvoiceRef;
                item.Note = null;
                item.RestError = null;
                item.CanRetry = false;
            }
            else
            {
                item.Status = BulkInvoiceItemStatus.Failed;
                item.Note = r.Note;
                item.RestError = r.RestError;
                item.CanRetry = r.IsTransient;
            }

            await _db.SaveChangesAsync(); // per-item kalıcılık
        }
    }
}
