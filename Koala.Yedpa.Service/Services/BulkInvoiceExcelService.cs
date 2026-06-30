using ClosedXML.Excel;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Koala.Yedpa.Core.Services;

namespace Koala.Yedpa.Service.Services
{
    public class BulkInvoiceExcelService : IBulkInvoiceExcelService
    {
        public byte[] BuildPreviewExcel(IReadOnlyList<PendingInvoiceLineDto> lines)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("AIDAT Faturalar");

            ws.Cell(1, 1).Value = "Cari Kod";
            ws.Cell(1, 2).Value = "Cari Ad";
            ws.Cell(1, 3).Value = "Ay";
            ws.Cell(1, 4).Value = "Tutar";
            ws.Range(1, 1, 1, 4).Style.Font.Bold = true;

            var row = 2;
            foreach (var l in lines)
            {
                ws.Cell(row, 1).Value = l.ClientCode;
                ws.Cell(row, 2).Value = l.ClientName;
                ws.Cell(row, 3).Value = l.MonthName;
                ws.Cell(row, 4).Value = l.Amount;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
