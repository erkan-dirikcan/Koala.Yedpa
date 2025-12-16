using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Providers;
using System.Net;
using System.Net.Mail;

namespace Koala.Yedpa.Service.Providers
{
    public class EmailProvider : IEmailProvider
    {
        public SmtpMailSettingsDto _setting { get; set; }
        public EmailProvider()
        {
            
        }
        public async Task SendSmtpMailAsyn(SendMailViewModel model)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_setting.UserName, "Sistem Koala"),
                Subject = model.Subject,
                IsBodyHtml = true,
                Body = model.HtmlContent
            };
            if (model.Attachement != null && model.Attachement.Any())
            {
                foreach (var item in model.Attachement)
                {
                    mailMessage.Attachments.Add(new Attachment(fileName: item));
                }
            }
            mailMessage.To.Add(new MailAddress(model.DestinationMail, model.DestinationName));
            //mailMessage.To.Add(new MailAddress("erkan@sistem-bilgisayar.com.tr", model.DestinationName));
            var client = new SmtpClient(_setting.MailServer, _setting.Port)
            {
                EnableSsl = _setting.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_setting.UserName, _setting.Password)
            };
            try
            {
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
    }
}
