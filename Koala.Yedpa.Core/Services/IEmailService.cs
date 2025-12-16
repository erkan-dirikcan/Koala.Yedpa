using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Services
{
    public interface IEmailService
    {
        Task<bool> SendResetPasswordEmailAsync(ResetPasswordEmailDto model);
        Task<bool> SendChangePasswordEmailAsync(CustomEmailDto model);
    }
}
