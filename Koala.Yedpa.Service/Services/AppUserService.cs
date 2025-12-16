using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services
{
    public class AppUserService : IAppUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<EmailService> _logger;
        public AppUserService(UserManager<AppUser> userManager, ILogger<EmailService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ResponseDto<AppUserInfoViewModels>> GetUserInfoById(string id)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return ResponseDto<AppUserInfoViewModels>.FailData(404, "Kullanıcı Bilgilerine Ulaşılamadı", "Kullanıcı bilgilerine ulaşılamadı", true);
                }
                var retVal = new AppUserInfoViewModels
                {
                    Id = user.Id,

                };
                return ResponseDto<AppUserInfoViewModels>.SuccessData(200, $"{retVal.FullName} İsimli Kullanıcı Bilgileri Başarıyla Alındı", retVal);
            }
            catch (Exception ex)
            {
                return ResponseDto<AppUserInfoViewModels>.FailData(400, "Kullanıcı Bilgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
            }
        }

        public async Task<ResponseDto<AppUserInfoViewModels>> GetUserInfoByEmail(string email)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return ResponseDto<AppUserInfoViewModels>.FailData(404, "Kullanıcı Bilgilerine Ulaşılamadı", "Kullanıcı bilgilerine ulaşılamadı", true);
                }
                var retVal = new AppUserInfoViewModels
                {
                    Id = user.Id,

                };
                return ResponseDto<AppUserInfoViewModels>.SuccessData(200, $"{retVal.FullName} İsimli Kullanıcı Bilgileri Başarıyla Alındı", retVal);
            }
            catch (Exception ex)
            {
                return ResponseDto<AppUserInfoViewModels>.FailData(400, "Kullanıcı Bilgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
            }

        }

        

    }
}