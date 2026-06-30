using FluentAssertions;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Service.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Koala.Yedpa.Service.Tests.BulkInvoice
{
    public class TransferServiceTests
    {
        [Fact]
        public async Task TransferLine_PostsItemsWrapper_AndParsesRef()
        {
            var rest = new Mock<ILogoRestServiceProvider>();
            string? sent = null;
            rest.Setup(r => r.HttpPost("salesInvoices", It.IsAny<string>()))
                .Callback<string, string>((_, j) => sent = j)
                .ReturnsAsync(ResponseDto<string>.SuccessData(200, "ok",
                    "{\"INTERNAL_REFERENCE\":23828,\"NUMBER\":\"YED2026000014227\"}"));

            var svc = new BulkInvoiceTransferService(rest.Object, NullLogger<BulkInvoiceTransferService>.Instance);
            var line = new PendingInvoiceLineDto
            {
                Orflineref = 28868,
                ClientCode = "1.F000.090.00.11",
                Amount = 5016.7m,
                MonthName = "TEMMUZ"
            };

            var res = await svc.TransferLineAsync(line, new DateTime(2026, 7, 1));

            res.Success.Should().BeTrue();
            res.LogoInvoiceRef.Should().Be(23828);
            res.InvoiceNumber.Should().Be("YED2026000014227");

            // Gönderilen JSON, InjectDataObjectParameter ile indent'lenir → parse ederek doğrula.
            sent.Should().NotBeNull();
            var p = JObject.Parse(sent!);
            p["ARP_CODE"]!.Value<string>().Should().Be("1.F000.090.00.11");
            p["DATE"]!.Value<string>().Should().Be("2026-07-01");
            p["NOTES1"]!.Value<string>().Should().Be("Temmuz AIDAT TAHAKKUKU");
            p["PAYMENT_CODE"]!.Value<string>().Should().Be("10-3");
            p["TRANSACTIONS"]!["items"].Should().NotBeNull();              // düz dizi DEĞİL, items sarmalı
            p["TRANSACTIONS"]!["items"]![0]!["PRICE"]!.Value<string>().Should().Be("5016.7");
            p["TRANSACTIONS"]!["items"]![0]!["MASTER_CODE"]!.Value<string>().Should().Be("600.11.0001");
            p["DataObjectParameter"]!["FillAccCodesOnPreSave"]!.Value<bool>().Should().BeTrue();
        }

        [Fact]
        public async Task TransferLine_OnRestFailure_ReturnsError()
        {
            var rest = new Mock<ILogoRestServiceProvider>();
            rest.Setup(r => r.HttpPost("salesInvoices", It.IsAny<string>()))
                .ReturnsAsync(ResponseDto<string>.FailData(500, "Logo hata", "ARP_CODE bulunamadı", true));

            var svc = new BulkInvoiceTransferService(rest.Object, NullLogger<BulkInvoiceTransferService>.Instance);
            var res = await svc.TransferLineAsync(
                new PendingInvoiceLineDto { Orflineref = 1, ClientCode = "X", Amount = 1m, MonthName = "TEMMUZ" },
                new DateTime(2026, 7, 1));

            res.Success.Should().BeFalse();
            res.RestError.Should().Contain("ARP_CODE");
            res.IsTransient.Should().BeFalse(); // 500 iş hatası, token değil
        }
    }
}
