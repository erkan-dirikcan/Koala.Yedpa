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

            var bulk = new Mock<IBulkInvoiceService>();
            bulk.Setup(b => b.GetPendingLinesAsync("TEMMUZ")).ReturnsAsync(
                ResponseDto<List<PendingInvoiceLineDto>>.SuccessData(200, "ok", new List<PendingInvoiceLineDto>
                {
                    new() { OrficheRef = 10, Orflineref = 100, ClientCode = "A", ClientName = "CA", Amount = 10m, MonthName = "TEMMUZ" },
                    new() { OrficheRef = 20, Orflineref = 200, ClientCode = "B", ClientName = "CB", Amount = 20m, MonthName = "TEMMUZ" }
                }));
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

            (await db.BulkInvoiceSessions.FindAsync(session.Id))!.Status
                .Should().Be(BulkInvoiceSessionStatus.Completed);
        }
    }
}
