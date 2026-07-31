using FluentAssertions;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Koala.Yedpa.Service.Tests.BulkInvoice
{
    public class TransferJobTests
    {
        private static AppDbContext NewDb() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        [Fact]
        public async Task RunTransfer_PersistsResults_UpdatesTrgflag_SendsReport()
        {
            using var db = NewDb();
            var session = new BulkInvoiceSession
            {
                Month = 7,
                Year = 2026,
                InvoiceDate = new DateTime(2026, 7, 1),
                CreatedBy = "test",
                Status = BulkInvoiceSessionStatus.Pending
            };
            db.BulkInvoiceSessions.Add(session);
            await db.SaveChangesAsync();

            // Aktarılacak satırlar önceden hazır (gün içi "Aktarılacak Verileri Oluştur" / Sync sonucu)
            db.BulkInvoiceItems.AddRange(
                new BulkInvoiceItem { SessionId = session.Id, OrficheRef = 10, Orflineref = 100, ClientCode = "A", ClientName = "CA", Amount = 10m, MonthName = "TEMMUZ", Status = BulkInvoiceItemStatus.Pending },
                new BulkInvoiceItem { SessionId = session.Id, OrficheRef = 20, Orflineref = 200, ClientCode = "B", ClientName = "CB", Amount = 20m, MonthName = "TEMMUZ", Status = BulkInvoiceItemStatus.Pending });
            await db.SaveChangesAsync();

            var bulk = new Mock<IBulkInvoiceService>();
            // RunTransferAsync önce Sync çağırır (burada item'lar zaten hazır → no-op success)
            bulk.Setup(b => b.SyncSessionItemsAsync(It.IsAny<int>()))
                .ReturnsAsync(ResponseDto<int>.SuccessData(200, "ok", 2));
            bulk.Setup(b => b.MarkLinesAsTransferredAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(ResponseDto<int>.SuccessData(200, "ok", 1));

            var transfer = new Mock<IBulkInvoiceTransferService>();
            transfer.Setup(t => t.TransferLineAsync(It.Is<PendingInvoiceLineDto>(l => l.Orflineref == 100), It.IsAny<DateTime>()))
                    .ReturnsAsync(new TransferLineResult(true, 100, "A", 999, "YED1", null, null, false));
            transfer.Setup(t => t.TransferLineAsync(It.Is<PendingInvoiceLineDto>(l => l.Orflineref == 200), It.IsAny<DateTime>()))
                    .ReturnsAsync(new TransferLineResult(false, 200, "B", null, null, "REST iş hatası", "Logo hata", false));

            var email = new Mock<IBulkInvoiceEmailService>();

            var jobs = new BulkInvoiceJobs(db, bulk.Object, transfer.Object, email.Object,
                NullLogger<BulkInvoiceJobs>.Instance);

            await jobs.RunTransferAsync(session.Id);

            var items = db.BulkInvoiceItems.Where(i => i.SessionId == session.Id).ToList();
            items.Should().HaveCount(2);

            var ok = items.Single(i => i.Orflineref == 100);
            ok.Status.Should().Be(BulkInvoiceItemStatus.Transferred);
            ok.LogoInvoiceRef.Should().Be(999);

            var fail = items.Single(i => i.Orflineref == 200);
            fail.Status.Should().Be(BulkInvoiceItemStatus.Failed);
            fail.RestError.Should().Be("Logo hata");
            fail.CanRetry.Should().BeFalse();

            // Sadece başarılı satır TRGFLAG=1 listesine girer
            bulk.Verify(b => b.MarkLinesAsTransferredAsync(
                It.Is<IReadOnlyList<int>>(ids => ids.Count == 1 && ids[0] == 100)), Times.Once);

            // Görev bitince rapor maili
            email.Verify(e => e.SendReportMailAsync(session.Id), Times.Once);

            // Eksik kalan satır varsa oturum "Hatalı" işaretlenir → Yönetim sayfasında görünür,
            // "Eksik Kalanları Yeniden Aktar" ile tamamlanabilir.
            (await db.BulkInvoiceSessions.FindAsync(session.Id))!.Status
                .Should().Be(BulkInvoiceSessionStatus.Failed);
        }

        [Fact]
        public async Task RunTransfer_AllLinesSucceed_MarksSessionCompleted()
        {
            using var db = NewDb();
            var session = new BulkInvoiceSession
            {
                Month = 8,
                Year = 2026,
                InvoiceDate = new DateTime(2026, 8, 3),
                CreatedBy = "test",
                Status = BulkInvoiceSessionStatus.Pending
            };
            db.BulkInvoiceSessions.Add(session);
            await db.SaveChangesAsync();

            db.BulkInvoiceItems.Add(new BulkInvoiceItem
            {
                SessionId = session.Id,
                OrficheRef = 10,
                Orflineref = 100,
                ClientCode = "A",
                ClientName = "CA",
                Amount = 10m,
                MonthName = "AGUSTOS",
                Status = BulkInvoiceItemStatus.Pending
            });
            await db.SaveChangesAsync();

            var bulk = new Mock<IBulkInvoiceService>();
            bulk.Setup(b => b.SyncSessionItemsAsync(It.IsAny<int>()))
                .ReturnsAsync(ResponseDto<int>.SuccessData(200, "ok", 1));
            bulk.Setup(b => b.MarkLinesAsTransferredAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(ResponseDto<int>.SuccessData(200, "ok", 1));

            var transfer = new Mock<IBulkInvoiceTransferService>();
            transfer.Setup(t => t.TransferLineAsync(It.IsAny<PendingInvoiceLineDto>(), It.IsAny<DateTime>()))
                    .ReturnsAsync(new TransferLineResult(true, 100, "A", 999, "YED1", null, null, false));

            var email = new Mock<IBulkInvoiceEmailService>();

            var jobs = new BulkInvoiceJobs(db, bulk.Object, transfer.Object, email.Object,
                NullLogger<BulkInvoiceJobs>.Instance);

            await jobs.RunTransferAsync(session.Id);

            (await db.BulkInvoiceSessions.FindAsync(session.Id))!.Status
                .Should().Be(BulkInvoiceSessionStatus.Completed);
            email.Verify(e => e.SendReportMailAsync(session.Id), Times.Once);
        }

        [Fact]
        public async Task RetryFailed_CompletesRemainingLines_AndMarksSessionCompleted()
        {
            using var db = NewDb();
            var session = new BulkInvoiceSession
            {
                Month = 8,
                Year = 2026,
                InvoiceDate = new DateTime(2026, 8, 3),
                CreatedBy = "test",
                Status = BulkInvoiceSessionStatus.Failed
            };
            db.BulkInvoiceSessions.Add(session);
            await db.SaveChangesAsync();

            // Yarım kalmış koşu: biri aktarıldı, biri başarısız, biri hiç denenmedi.
            db.BulkInvoiceItems.AddRange(
                new BulkInvoiceItem { SessionId = session.Id, OrficheRef = 10, Orflineref = 100, ClientCode = "A", ClientName = "CA", Amount = 10m, MonthName = "AGUSTOS", Status = BulkInvoiceItemStatus.Transferred, LogoInvoiceRef = 999 },
                new BulkInvoiceItem { SessionId = session.Id, OrficheRef = 20, Orflineref = 200, ClientCode = "B", ClientName = "CB", Amount = 20m, MonthName = "AGUSTOS", Status = BulkInvoiceItemStatus.Failed, CanRetry = true },
                new BulkInvoiceItem { SessionId = session.Id, OrficheRef = 30, Orflineref = 300, ClientCode = "C", ClientName = "CC", Amount = 30m, MonthName = "AGUSTOS", Status = BulkInvoiceItemStatus.Pending });
            await db.SaveChangesAsync();

            var bulk = new Mock<IBulkInvoiceService>();
            bulk.Setup(b => b.MarkLinesAsTransferredAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(ResponseDto<int>.SuccessData(200, "ok", 2));

            var transfer = new Mock<IBulkInvoiceTransferService>();
            transfer.Setup(t => t.TransferLineAsync(It.IsAny<PendingInvoiceLineDto>(), It.IsAny<DateTime>()))
                    .ReturnsAsync((PendingInvoiceLineDto l, DateTime _) =>
                        new TransferLineResult(true, l.Orflineref, l.ClientCode, 1000 + l.Orflineref, "YED", null, null, false));

            var email = new Mock<IBulkInvoiceEmailService>();

            var jobs = new BulkInvoiceJobs(db, bulk.Object, transfer.Object, email.Object,
                NullLogger<BulkInvoiceJobs>.Instance);

            await jobs.RetryFailedAsync(session.Id);

            // Zaten aktarılmış satır tekrar denenmez: yalnızca 200 ve 300.
            transfer.Verify(t => t.TransferLineAsync(It.IsAny<PendingInvoiceLineDto>(), It.IsAny<DateTime>()), Times.Exactly(2));
            bulk.Verify(b => b.MarkLinesAsTransferredAsync(
                It.Is<IReadOnlyList<int>>(ids => ids.Count == 2 && ids.Contains(200) && ids.Contains(300))), Times.Once);

            db.BulkInvoiceItems.Count(i => i.SessionId == session.Id && i.Status != BulkInvoiceItemStatus.Transferred)
                .Should().Be(0);
            (await db.BulkInvoiceSessions.FindAsync(session.Id))!.Status
                .Should().Be(BulkInvoiceSessionStatus.Completed);
            email.Verify(e => e.SendReportMailAsync(session.Id), Times.Once);
        }

        [Fact]
        public async Task SendInfoMail_SyncsItemsFirst_SoManagePageMatchesMailedList()
        {
            using var db = NewDb();
            var session = new BulkInvoiceSession
            {
                Month = 8,
                Year = 2026,
                InvoiceDate = new DateTime(2026, 8, 3),
                CreatedBy = "test",
                Status = BulkInvoiceSessionStatus.Pending
            };
            db.BulkInvoiceSessions.Add(session);
            await db.SaveChangesAsync();

            var bulk = new Mock<IBulkInvoiceService>();
            bulk.Setup(b => b.SyncSessionItemsAsync(session.Id))
                .ReturnsAsync(ResponseDto<int>.SuccessData(200, "ok", 5));

            var email = new Mock<IBulkInvoiceEmailService>();

            var jobs = new BulkInvoiceJobs(db, bulk.Object, Mock.Of<IBulkInvoiceTransferService>(), email.Object,
                NullLogger<BulkInvoiceJobs>.Instance);

            await jobs.SendInfoMailAsync(session.Id);

            bulk.Verify(b => b.SyncSessionItemsAsync(session.Id), Times.Once);
            email.Verify(e => e.SendInfoMailAsync(session.Id), Times.Once);
        }
    }
}
