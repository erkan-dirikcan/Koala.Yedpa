using FluentAssertions;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Service.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Koala.Yedpa.Service.Tests.BulkInvoice
{
    /// <summary>
    /// TRGFLAG toplu güncelleme — Logo REST fatura kesince TRGFLAG'ı otomatik yapmadığı
    /// (canlı veriyle doğrulandı) için zorunlu idempotency adımı.
    /// </summary>
    public class MarkTransferredTests
    {
        private static (BulkInvoiceService svc, Mock<ISqlProvider> sql) Create()
        {
            var settings = new Mock<ISettingsService>();
            settings.Setup(s => s.GetLogoSettingsAsync()).ReturnsAsync(
                ResponseDto<LogoSettingViewModel>.SuccessData(200, "ok",
                    new LogoSettingViewModel { Firm = "211", Period = "16" }));

            var sql = new Mock<ISqlProvider>();
            // context MarkLinesAsTransferredAsync'te kullanılmadığı için null! geçilebilir.
            var svc = new BulkInvoiceService(null!, Mock.Of<IApiLogoSqlDataService>(),
                settings.Object, sql.Object, NullLogger<BulkInvoiceService>.Instance);
            return (svc, sql);
        }

        [Fact]
        public async Task EmptyList_ReturnsZero_NoSql()
        {
            var (svc, sql) = Create();

            var res = await svc.MarkLinesAsTransferredAsync(new List<int>());

            res.IsSuccess.Should().BeTrue();
            res.Data.Should().Be(0);
            sql.Verify(p => p.WriteToSql(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task BuildsInClause_FromLogoSettings_AndReturnsAffected()
        {
            var (svc, sql) = Create();
            sql.Setup(p => p.WriteToSql(It.IsAny<string>()))
               .Returns(ResponseDto<string>.SuccessData(200, "ok", "2"));

            var res = await svc.MarkLinesAsTransferredAsync(new List<int> { 100, 200 });

            res.Data.Should().Be(2);
            sql.Verify(p => p.WriteToSql(It.Is<string>(s =>
                    s.Contains("LG_211_16_ORFLINE")
                 && s.Contains("SET TRGFLAG=1")
                 && s.Contains("100,200"))),
                Times.Once);
        }
    }
}
