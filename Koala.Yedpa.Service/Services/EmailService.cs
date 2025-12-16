using System.Net;
using System.Net.Mail;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services;

public class EmailService: IEmailService
{
    private readonly EmailSettingViewModel _emailOptions;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<EmailService> _logger;
    public async Task<bool> SendResetPasswordEmailAsync(ResetPasswordEmailDto model)
    {
        var mailContent = "";
        var mailTemplateRes = await _templateService.GetByNameAsyc("DefaultTemplate");
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
        var mailTemplateRes = await _templateService.GetByNameAsyc("DefaultTemplate");
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

    private async Task<bool> SendEmailAsync(EmailDto model)
    {
        var smtpClient = new SmtpClient();
        smtpClient.Host = _emailOptions.SmtpServer;
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Port = 587;
        smtpClient.Credentials = new NetworkCredential(_emailOptions.UserName, _emailOptions.Password);
        smtpClient.EnableSsl = false;



        var message = new MailMessage();
        message.From = new MailAddress(_emailOptions.SenderEmail);
        message.To.Add(model.Email!);
        message.Subject = model.Title;
        message.Body = model.Content;
        message.IsBodyHtml = true;


        try
        {
            await smtpClient.SendMailAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-Posta gönderilirken Bir Sorunla Karşılaşıldı", new { Hata = ex });
            return false;
        }
    }
}