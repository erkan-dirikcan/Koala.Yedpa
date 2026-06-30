using System.Text.Json;
using FluentAssertions;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Xunit;

namespace Koala.Yedpa.Service.Tests.BulkInvoice
{
    /// <summary>
    /// AidatInvoicePayload'ın Logo REST salesInvoices yapısına (kanıtlanmış
    /// test-aidat-fatura-temmuz.json) uygun serialize olduğunu doğrular.
    /// </summary>
    public class AidatInvoicePayloadTests
    {
        [Fact]
        public void Serialize_ProducesProvenLogoStructure()
        {
            // Arrange
            var payload = new AidatInvoicePayload
            {
                ArpCode = "1.F000.090.00.11",
                Date = "2026-07-01",
                Time = 66048,
                Notes1 = "Temmuz AIDAT TAHAKKUKU",
                Transactions = new AidatInvoiceTransactions
                {
                    Items =
                    {
                        new AidatInvoiceTransaction { Price = "5016.7", Description = "Temmuz AIDAT" }
                    }
                }
            };

            // Act
            var json = JsonSerializer.Serialize(payload);

            // Assert — sabit/girdi alanları + kritik items sarmalı
            json.Should().Contain("\"ARP_CODE\":\"1.F000.090.00.11\"");
            json.Should().Contain("\"PAYMENT_CODE\":\"10-3\"");      // tüm AIDAT için sabit
            json.Should().Contain("\"TYPE\":7");                       // satış faturası
            json.Should().Contain("\"DOC_NUMBER\":\"AIDAT\"");
            json.Should().Contain("\"TRANSACTIONS\":{\"items\":[");   // düz dizi DEĞİL, items sarmalı
            json.Should().Contain("\"MASTER_CODE\":\"600.11.0001\""); // sabit AIDAT hizmet kartı
            json.Should().Contain("\"VAT_INCLUDED\":1");
            json.Should().Contain("\"PRICE\":\"5016.7\"");

            // Toplamlar/VAT kırılımı GÖNDERİLMEZ — REST hesaplar
            json.Should().NotContain("VAT_AMOUNT");
            json.Should().NotContain("VAT_BASE");
            json.Should().NotContain("TOTAL_NET");
        }
    }
}
