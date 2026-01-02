using Microsoft.AspNetCore.Identity.UI.Services;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services;

/// <summary>
/// ASP.NET Core Identity IEmailSender interface'i için adapter
/// Mevcut IEmailService'i kullanarak Identity'nin email gönderme ihtiyacını karşılar
/// </summary>
public class EmailSenderAdapter : IEmailSender
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailSenderAdapter> _logger;

    public EmailSenderAdapter(IEmailService emailService, ILogger<EmailSenderAdapter> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// ASP.NET Core Identity tarafından çağrılan email gönderme metodu
    /// </summary>
    /// <param name="email">Alıcı email adresi</param>
    /// <param name="subject">Email konusu</param>
    /// <param name="htmlMessage">HTML formatında email içeriği</param>
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            // IEmailService'in SendCustomMail metodunu kullanarak email gönder
            // CustomEmailDto EmailDto'dan türüyor, Title property'si var
            var result = await _emailService.SendCustomMail(new Koala.Yedpa.Core.Dtos.CustomEmailDto
            {
                Email = email,
                Title = subject, // EmailDto'da Title var, Subject yok
                Content = htmlMessage, // EmailDto'da Content var, HtmlContent yok
                Name = "", // Identity'den gelen email'de isim bilgisi yok
                Lastname = ""
            });

            if (!result)
            {
                _logger.LogWarning("Email gönderilemedi: {Email}, {Subject}", email, subject);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email gönderilirken hata oluştu: {Email}, {Subject}", email, subject);
            throw;
        }
    }
}

