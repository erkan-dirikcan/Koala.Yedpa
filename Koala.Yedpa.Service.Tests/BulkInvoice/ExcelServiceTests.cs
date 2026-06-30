using ClosedXML.Excel;
using FluentAssertions;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Koala.Yedpa.Service.Services;
using Xunit;

namespace Koala.Yedpa.Service.Tests.BulkInvoice
{
    public class ExcelServiceTests
    {
        [Fact]
        public void BuildPreviewExcel_HasHeaderAndRows()
        {
            var svc = new BulkInvoiceExcelService();

            var bytes = svc.BuildPreviewExcel(new List<PendingInvoiceLineDto>
            {
                new() { ClientCode = "A", ClientName = "Cari A", MonthName = "TEMMUZ", Amount = 100m }
            });

            using var wb = new XLWorkbook(new MemoryStream(bytes));
            var ws = wb.Worksheet(1);

            ws.Cell(1, 1).GetString().Should().Be("Cari Kod");
            ws.Cell(2, 1).GetString().Should().Be("A");
            ws.Cell(2, 3).GetString().Should().Be("TEMMUZ");
            ws.Cell(2, 4).GetDouble().Should().Be(100d);
        }
    }
}
