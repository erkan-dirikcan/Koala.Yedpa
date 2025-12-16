using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Providers;

public interface IEmailProvider
{
    Task SendSmtpMailAsyn(SendMailViewModel model);
}