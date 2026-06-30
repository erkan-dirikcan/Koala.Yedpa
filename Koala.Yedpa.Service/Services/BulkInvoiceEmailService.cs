using System.Text;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services
{
    /// <summary>
    /// Toplu faturalandırma bilgi + rapor e-postaları.
    /// Alıcılar ŞİMDİLİK sabit; ileride "bilgilendirme maili yetkisi" olan kullanıcılardan okunacak.
    /// </summary>
    public class BulkInvoiceEmailService : IBulkInvoiceEmailService
    {
        // TODO (ileride): ISettingsService / yetki bazlı alıcı listesine taşınacak.
        private static readonly string[] Recipients =
        {
            "erkan@sistem-bilgisayar.com.tr",
            "adegimli@yedpa.com.tr",
            "muhasebe@yedpa.com.tr"
        };

        private readonly AppDbContext _db;
        private readonly IBulkInvoiceService _bulk;
        private readonly IBulkInvoiceExcelService _excel;
        private readonly IEmailService _email;
        private readonly ILogger<BulkInvoiceEmailService> _logger;

        public BulkInvoiceEmailService(
            AppDbContext db,
            IBulkInvoiceService bulk,
            IBulkInvoiceExcelService excel,
            IEmailService email,
            ILogger<BulkInvoiceEmailService> logger)
        {
            _db = db;
            _bulk = bulk;
            _excel = excel;
            _email = email;
            _logger = logger;
        }

        public async Task SendInfoMailAsync(int sessionId)
        {
            var session = await _db.BulkInvoiceSessions.FindAsync(sessionId);
            if (session == null) { _logger.LogWarning("Bilgi maili: session yok {Id}", sessionId); return; }

            var monthName = BulkInvoiceMonths.ToLogoName(session.Month);
            var pending = await _bulk.GetPendingLinesAsync(monthName);
            var lines = pending.Data ?? new();
            var total = lines.Sum(l => l.Amount);

            var excelBytes = _excel.BuildPreviewExcel(lines);
            var attachment = new EmailAttachmentDto
            {
                FileName = $"AIDAT_Faturalar_{monthName}.xlsx",
                Content = excelBytes,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            var body =
                $"<p><b>{session.InvoiceDate:dd.MM.yyyy}</b> tarihinde <b>{lines.Count}</b> adet AIDAT faturası oluşturulacaktır.</p>" +
                $"<p>Toplam tutar: <b>{total:N2} ₺</b></p>" +
                "<p>Oluşturulacak faturaların listesi ektedir.</p>";

            await SendToAllAsync($"Toplu Faturalandırma Bilgilendirme - {monthName}", body, attachment);
        }

        public async Task SendReportMailAsync(int sessionId)
        {
            var items = await _db.BulkInvoiceItems
                .Where(i => i.SessionId == sessionId)
                .ToListAsync();

            var success = items.Count(i => i.Status == BulkInvoiceItemStatus.Transferred);
            var failedItems = items.Where(i => i.Status == BulkInvoiceItemStatus.Failed).ToList();

            var sb = new StringBuilder();
            sb.Append("<p>Toplu faturalandırma işlemi tamamlandı.</p>");
            sb.Append($"<p>Başarılı: <b>{success}</b> &nbsp; Başarısız: <b>{failedItems.Count}</b></p>");

            if (failedItems.Count > 0)
            {
                sb.Append("<table border='1' cellpadding='4' cellspacing='0'>");
                sb.Append("<tr><th>Cari Kod</th><th>Fiş No/Ref</th><th>Not</th><th>REST Hata</th></tr>");
                foreach (var f in failedItems)
                {
                    sb.Append("<tr>")
                      .Append($"<td>{System.Net.WebUtility.HtmlEncode(f.ClientCode)}</td>")
                      .Append($"<td>{f.LogoInvoiceRef}</td>")
                      .Append($"<td>{System.Net.WebUtility.HtmlEncode(f.Note)}</td>")
                      .Append($"<td>{System.Net.WebUtility.HtmlEncode(f.RestError)}</td>")
                      .Append("</tr>");
                }
                sb.Append("</table>");
            }

            await SendToAllAsync("Toplu Faturalandırma Raporu", sb.ToString(), null);
        }

        private async Task SendToAllAsync(string title, string content, EmailAttachmentDto? attachment)
        {
            foreach (var to in Recipients)
            {
                try
                {
                    var dto = new EmailDto
                    {
                        Email = to,
                        Title = title,
                        Content = content,
                        Attachments = attachment != null ? new List<EmailAttachmentDto> { attachment } : null
                    };
                    await _email.SendRawHtmlMail(dto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Toplu fatura maili gönderilemedi: {To}", to);
                }
            }
        }
    }
}
