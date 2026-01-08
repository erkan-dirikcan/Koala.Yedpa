using System.Net;
using System.Net.Mail;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services;

public class EmailService: IEmailService
{
    private readonly ISettingsService _settingsService;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ISettingsService settingsService, IEmailTemplateService templateService, ILogger<EmailService> logger)
    {
        _settingsService = settingsService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<bool> SendResetPasswordEmailAsync(ResetPasswordEmailDto model)
    {
        var mailContent = "";
        var mailTemplateRes = await _templateService.GetByNameAsyc("Default");
        if (!mailTemplateRes.IsSuccess)
        {
            mailContent = $"Şifrenizi sıfırlamak için lütfen <a href=\"{model.ResetLink}\" target=\"_blank\">Buraya</a> tıklayınız";
        }
        else
        {
            mailContent = mailTemplateRes.Data.Content
                .Replace("[[Title]]", "Sistem Bilgisayar Koala")
                .Replace("[[Date]]", DateTime.Now.ToLongDateString())
                .Replace("[[Name]]", model.Name + " " + model.Lastname)
                .Replace("[[Body]]", $"Şifrenizi sıfırlamak için lütfen <a href=\"{model.ResetLink}\" target=\"_blank\">Buraya</a> tıklayınız");
        }

        return await SendEmailAsync(new EmailDto { Content = mailContent, Email = model.Email, Title = "Şifre Sıfırlama Talebi" });
    }

    public async Task<bool> SendChangePasswordEmailAsync(CustomEmailDto model)
    {
        var mailContent = "";
        var mailTemplateRes = await _templateService.GetByNameAsyc("Default");
        if (!mailTemplateRes.IsSuccess)
        {
            mailContent = $"Sistem bilgisayar Koala uygulaması üzerinden şifreniz başarıyla değiştirildi";
        }
        else
        {
            mailContent = mailTemplateRes.Data.Content
                .Replace("[[Title]]", "Yedpa Ticaret Merkezi Yönetim Uygulaması")
                .Replace("[[Date]]", DateTime.Now.ToLongDateString())
                .Replace("[[Name]]", model.Name + " " + model.Lastname)
                .Replace("[[Body]]", $"Yedpa Ticaret Merkezi Yönetim Uygulaması üzerinden şifreniz başarıyla değiştirildi.<br />" +
                                     $"Yeni şifreniz ile sisteme giriş yapabilirsiniz. Bu işlem bilginiz dahilinde gerçekleşmediyse lütfen <a href=\"mailto:adegimli@yedpa.com.tr\">Yönetim İle İletişime Geçiniz.</a>");
        }
        return await SendEmailAsync(new EmailDto { Content = mailContent, Email = model.Email, Title = "Şifreniz Değiştirildi" });

    }

    public async Task<bool> SendCustomMail(CustomEmailDto model)
    {
        var mailContent = "";
        var mailTemplateRes = await _templateService.GetByNameAsyc("Default");

        if (!mailTemplateRes.IsSuccess || string.IsNullOrWhiteSpace(model.Content))
        {
            mailContent = $"Sistem bilgisayar Koala uygulaması üzerinden bilgilendirme mesajı";
        }
        else
        {
            mailContent = mailTemplateRes.Data.Content
                .Replace("[[Title]]", model.Title ?? "Yedpa Ticaret Merkezi Yönetim Uygulaması")
                .Replace("[[Date]]", DateTime.Now.ToLongDateString())
                .Replace("[[Name]]", model.Name + " " + model.Lastname)
                .Replace("[[Body]]", model.Content);
        }

        return await SendEmailAsync(new EmailDto { Content = mailContent, Email = model.Email, Title = model.Title ?? "Bilgilendirme" });
    }

    private async Task<bool> SendEmailAsync(EmailDto model)
    {
        // Email ayarlarını settings'den al
        var emailSettingsResponse = await _settingsService.GetEmailSettingsAsync();
        if (!emailSettingsResponse.IsSuccess || emailSettingsResponse.Data == null)
        {
            _logger.LogError("E-posta ayarları alınamadı. E-posta gönderimi başarısız.");
            return false;
        }

        var emailSettings = emailSettingsResponse.Data;

        var smtpClient = new SmtpClient();
        smtpClient.Host = emailSettings.SmtpServer;
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Port = emailSettings.Port;
        smtpClient.Credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
        smtpClient.EnableSsl = emailSettings.EnableSsl;



        var message = new MailMessage();
        message.From = new MailAddress(emailSettings.SenderEmail, emailSettings.SenderName ?? "Sistem Koala");
        message.To.Add(model.Email!);
        message.Subject = model.Title;
        message.Body = model.Content;
        message.IsBodyHtml = true;

        // Attachment desteği
        if (model.Attachments != null && model.Attachments.Any())
        {
            foreach (var attachment in model.Attachments)
            {
                var attachmentStream = new MemoryStream(attachment.Content);
                message.Attachments.Add(new Attachment(attachmentStream, attachment.FileName, attachment.ContentType));
            }
        }

        try
        {
            await smtpClient.SendMailAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-Posta gönderilirken Bir Sorunla Karşılaşıldı. Email: {Email}, Hata: {Hata}", model.Email, ex.Message);
            return false;
        }
    }
}