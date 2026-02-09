using Microsoft.AspNetCore.Mvc;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Services;

namespace Koala.Yedpa.WebUI.Controllers
{
    /// <summary>
    /// Test Email API - Message34 üzerinden test maili göndermek için kullanılır
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestEmailController : ControllerBase
    {
        private readonly IMessage34EmailService _message34EmailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<TestEmailController> _logger;

        public TestEmailController(
            IMessage34EmailService message34EmailService,
            IEmailTemplateService emailTemplateService,
            ILogger<TestEmailController> logger)
        {
            _message34EmailService = message34EmailService;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        /// <summary>
        /// Test maili gönderir
        /// </summary>
        /// <param name="request">Test mail isteği</param>
        /// <returns></returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                // Email şablonunu al
                var templateResult = await _emailTemplateService.GetByNameAsyc(request.TemplateName ?? "BuggetOrder");
                if (!templateResult.IsSuccess || templateResult.Data == null)
                {
                    return BadRequest(new { error = $"Email template '{request.TemplateName ?? "BuggetOrder"}' not found" });
                }

                // Template değişkenlerini replace et
                var content = templateResult.Data.Content;

                if (!string.IsNullOrEmpty(request.Name))
                {
                    content = content.Replace("[[Name]]", request.Name);
                }

                if (!string.IsNullOrEmpty(request.Body))
                {
                    content = content.Replace("[[Body]]", request.Body);
                }

                // EmailDto oluştur
                var emailDto = new EmailDto
                {
                    Email = request.ToEmail,
                    Title = request.Subject,
                    Content = content,
                    FromName = request.FromName,
                    FromEmail = request.FromEmail,
                    ReplyEmail = request.ReplyEmail
                };

                // Mail gönder
                var result = await _message34EmailService.SendTransactionEmailAsync(
                    emailDto,
                    new List<string> { request.ToEmail });

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Test email sent successfully to {Email}, CampaignId: {CampaignId}",
                        request.ToEmail, result.Data);
                    return Ok(new
                    {
                        success = true,
                        message = "Test maili başarıyla gönderildi",
                        campaignId = result.Data,
                        toEmail = request.ToEmail
                    });
                }
                else
                {
                    _logger.LogError("Failed to send test email: {Error}", result.Message);
                    return StatusCode(500, new { success = false, error = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Önceden tanımlı ödeme planı test maili gönderir
        /// </summary>
        /// <param name="toEmail">Alıcı e-posta adresi (varsayılan: erkan@sistem-bilgisayar.com.tr)</param>
        /// <returns></returns>
        [HttpPost("send-payment-plan")]
        public async Task<IActionResult> SendPaymentPlanTestEmail([FromQuery] string toEmail = "erkan@sistem-bilgisayar.com.tr")
        {
            try
            {
                // HTML tablo formatında ödeme planı oluştur
                var bodyHtml = $@"
<p>A CADDESİ NO:1 (YENİ NO:1) adresinde bulunan dükkan bütçe ödeme planı aşağıdadır.</p>

<table border=""1"" cellpadding=""3"" cellspacing=""0"" style=""border-collapse: collapse; width: 100%; max-width: 500px; margin-top: 10px; font-size: 13px;"">
    <thead>
        <tr style=""background-color: #011560; color: white;"">
            <th style=""padding: 4px 6px; text-align: left; border: 1px solid #011560;"">Ay</th>
            <th style=""padding: 4px 6px; text-align: right; border: 1px solid #011560;"">Tutar</th>
        </tr>
    </thead>
    <tbody>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Ocak</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">9303.00 TL</td>
        </tr>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Şubat</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">9303.00 TL</td>
        </tr>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Mart</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">9303.00 TL</td>
        </tr>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Nisan</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">9303.00 TL</td>
        </tr>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Mayıs</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">6010.00 TL</td>
        </tr>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Haziran</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">6010.00 TL</td>
        </tr>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Temmuz</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">6010.00 TL</td>
        </tr>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Ağustos</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">6010.00 TL</td>
        </tr>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Eylül</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">6010.00 TL</td>
        </tr>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Ekim</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">6010.00 TL</td>
        </tr>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Kasım</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">9303.00 TL</td>
        </tr>
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">Aralık</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">9303.00 TL</td>
        </tr>
    </tbody>
    <tfoot>
        <tr style=""background-color: #f0f0f0; font-weight: bold;"">
            <td style=""padding: 4px 6px; border: 1px solid #ddd;"">Toplam</td>
            <td style=""padding: 4px 6px; text-align: right; border: 1px solid #ddd;"">91878.00 TL</td>
        </tr>
    </tfoot>
</table>";

                var request = new TestEmailRequest
                {
                    ToEmail = toEmail,
                    TemplateName = "BuggetOrder",
                    Subject = "A CADDESİ NO:1 (YENİ NO:1) Ödeme Planı",
                    Name = "Ahmet DEĞİMLİ",
                    FromName = "Yedpa Ticaret Merkezi",
                    FromEmail = "tahsilat@e.yedpa.com.tr",
                    Body = bodyHtml
                };

                return await SendTestEmail(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment plan test email");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Test mail isteği modeli
    /// </summary>
    public class TestEmailRequest
    {
        /// <summary>
        /// Alıcı e-posta adresi
        /// </summary>
        public string ToEmail { get; set; } = string.Empty;

        /// <summary>
        /// Email şablonu adı (varsayılan: Default)
        /// </summary>
        public string? TemplateName { get; set; }

        /// <summary>
        /// Mail başlığı
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// [[Name]] placeholder'ı için değer
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// [[Body]] placeholder'ı için değer
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// Gönderen adı (opsiyonel)
        /// </summary>
        public string? FromName { get; set; }

        /// <summary>
        /// Gönderen e-posta adresi (opsiyonel)
        /// </summary>
        public string? FromEmail { get; set; }

        /// <summary>
        /// Yanıt e-posta adresi (opsiyonel)
        /// </summary>
        public string? ReplyEmail { get; set; }
    }
}
