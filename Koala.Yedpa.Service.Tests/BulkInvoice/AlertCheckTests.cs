using FluentAssertions;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Koala.Yedpa.Service.Tests.BulkInvoice
{
    /// <summary>
    /// Dashboard akışı: ayın 15'inden sonra tarih seçilmemişse uyarı; tarih seçilince uyarı
    /// kalkar ve yerine "Aktarım Yapılacak Firmaları Görüntüle" paneli gelir.
    /// </summary>
    public class AlertCheckTests
    {
        private static AppDbContext NewDb() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static BulkInvoiceService NewService(AppDbContext db) =>
            new(db,
                Mock.Of<IApiLogoSqlDataService>(),
                Mock.Of<ISettingsService>(),
                Mock.Of<ISqlProvider>(),
                NullLogger<BulkInvoiceService>.Instance);

        [Fact]
        public async Task CheckAlert_NoSession_DoesNotShowPlannedPanel()
        {
            using var db = NewDb();

            var result = await NewService(db).CheckAlertAsync();

            result.IsSuccess.Should().BeTrue();
            result.Data!.ShowPlannedPanel.Should().BeFalse();
            result.Data.SessionId.Should().BeNull();
            result.Data.TransferDate.Should().BeNull();
        }

        [Fact]
        public async Task CheckAlert_NextMonthDateSelected_HidesAlert_ShowsPlannedPanel()
        {
            using var db = NewDb();

            // Gelecek ayın aktarım tarihi seçilmiş (henüz geçmemiş bir gün).
            var next = DateTime.Now.AddMonths(1);
            var transferDate = new DateTime(next.Year, next.Month, 3);
            var session = new BulkInvoiceSession
            {
                InvoiceDate = transferDate,
                Month = transferDate.Month,
                Year = transferDate.Year,
                Status = BulkInvoiceSessionStatus.Pending,
                CreatedBy = "test"
            };
            db.BulkInvoiceSessions.Add(session);
            await db.SaveChangesAsync();

            var result = await NewService(db).CheckAlertAsync();

            result.IsSuccess.Should().BeTrue();
            // Tarih seçildiği için uyarı çıkmaz — ayın kaçı olduğundan bağımsız.
            result.Data!.ShowAlert.Should().BeFalse();
            result.Data.ShowPlannedPanel.Should().BeTrue();
            result.Data.SessionId.Should().Be(session.Id);
            result.Data.TransferDate.Should().Be(transferDate);
        }

        [Fact]
        public async Task CheckAlert_OnlyPastSessions_DoesNotShowPlannedPanel()
        {
            using var db = NewDb();

            // Tarihi geçmiş (aktarımı yapılmış) oturum panelde gösterilmez.
            db.BulkInvoiceSessions.Add(new BulkInvoiceSession
            {
                InvoiceDate = DateTime.Now.Date.AddMonths(-2),
                Month = DateTime.Now.AddMonths(-2).Month,
                Year = DateTime.Now.AddMonths(-2).Year,
                Status = BulkInvoiceSessionStatus.Completed,
                CreatedBy = "test"
            });
            await db.SaveChangesAsync();

            var result = await NewService(db).CheckAlertAsync();

            result.Data!.ShowPlannedPanel.Should().BeFalse();
        }

        [Fact]
        public async Task CheckAlert_MultipleUpcoming_PicksNearestTransferDate()
        {
            using var db = NewDb();

            var near = DateTime.Now.Date.AddDays(3);
            var far = DateTime.Now.Date.AddDays(40);

            db.BulkInvoiceSessions.AddRange(
                new BulkInvoiceSession { InvoiceDate = far, Month = far.Month, Year = far.Year, Status = BulkInvoiceSessionStatus.Pending, CreatedBy = "test" },
                new BulkInvoiceSession { InvoiceDate = near, Month = near.Month, Year = near.Year, Status = BulkInvoiceSessionStatus.Pending, CreatedBy = "test" });
            await db.SaveChangesAsync();

            var result = await NewService(db).CheckAlertAsync();

            result.Data!.ShowPlannedPanel.Should().BeTrue();
            result.Data.TransferDate.Should().Be(near);
        }

        [Fact]
        public async Task CheckAlert_TransferDayItself_StillShowsPlannedPanel()
        {
            using var db = NewDb();

            // Aktarım günü boyunca panel açık kalır (kullanıcı sonucu görebilsin).
            var today = DateTime.Now.Date;
            db.BulkInvoiceSessions.Add(new BulkInvoiceSession
            {
                InvoiceDate = today,
                Month = today.Month,
                Year = today.Year,
                Status = BulkInvoiceSessionStatus.Pending,
                CreatedBy = "test"
            });
            await db.SaveChangesAsync();

            var result = await NewService(db).CheckAlertAsync();

            result.Data!.ShowPlannedPanel.Should().BeTrue();
            result.Data.TransferDate.Should().Be(today);
        }
    }
}
