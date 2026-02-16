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
        private readonly ILogger<AppUserService> _logger;
        public AppUserService(UserManager<AppUser> userManager, ILogger<AppUserService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ResponseDto<AppUserInfoViewModels>> GetUserInfoById(string id)
        {
            _logger.LogInformation("GetUserInfoById called for user ID {UserId}", id);
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    _logger.LogWarning("GetUserInfoById: User not found for ID {UserId}", id);
                    return ResponseDto<AppUserInfoViewModels>.FailData(404, "Kullanıcı Bilgilerine Ulaşılamadı", "Kullanıcı bilgilerine ulaşılamadı", true);
                }
                var retVal = new AppUserInfoViewModels
                {
                    Id = user.Id,

                };
                _logger.LogInformation("GetUserInfoById: User info retrieved successfully for ID {UserId}", id);
                return ResponseDto<AppUserInfoViewModels>.SuccessData(200, $"{retVal.FullName} İsimli Kullanıcı Bilgileri Başarıyla Alındı", retVal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserInfoById: Error while getting user info for ID {UserId}", id);
                return ResponseDto<AppUserInfoViewModels>.FailData(400, "Kullanıcı Bilgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
            }
        }

        public async Task<ResponseDto<AppUserInfoViewModels>> GetUserInfoByEmail(string email)
        {
            _logger.LogInformation("GetUserInfoByEmail called for email {Email}", email);
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    _logger.LogWarning("GetUserInfoByEmail: User not found for email {Email}", email);
                    return ResponseDto<AppUserInfoViewModels>.FailData(404, "Kullanıcı Bilgilerine Ulaşılamadı", "Kullanıcı bilgilerine ulaşılamadı", true);
                }
                var retVal = new AppUserInfoViewModels
                {
                    Id = user.Id,

                };
                _logger.LogInformation("GetUserInfoByEmail: User info retrieved successfully for email {Email}", email);
                return ResponseDto<AppUserInfoViewModels>.SuccessData(200, $"{retVal.FullName} İsimli Kullanıcı Bilgileri Başarıyla Alındı", retVal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserInfoByEmail: Error while getting user info for email {Email}", email);
                return ResponseDto<AppUserInfoViewModels>.FailData(400, "Kullanıcı Bilgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
            }

        }



    }
}