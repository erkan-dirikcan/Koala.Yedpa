using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface IAppUserService
{
    Task<ResponseDto<AppUserInfoViewModels>> GetUserInfoById(string id);
    Task<ResponseDto<AppUserInfoViewModels>> GetUserInfoByEmail(string email);
}