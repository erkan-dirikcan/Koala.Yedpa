using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface ISettingsService
{
    Task<ResponseDto> AddEmailSettings(List<AddSettingViewModel> model);
    Task<ResponseDto> AddLogoSettings(List<AddSettingViewModel> model);
    Task<ResponseDto> AddLogoSqlSettings(List<AddSettingViewModel> model);
    Task<ResponseDto> AddLogoRestServiceSettings(List<AddSettingViewModel> model);

    Task<ResponseDto> UpdateEmailSettingsAsync(EmailSettingViewModel model);
    Task<ResponseDto> UpdateLogoSettingsAsync(LogoSettingViewModel model);
    Task<ResponseDto> UpdateLogoSqlSettingsAsync(LogoSqlSettingViewModel model);
    Task<ResponseDto> UpdateLogoRestServiceSettingsAsync(LogoRestServiceSettingViewModel model);

    Task<ResponseDto<EmailSettingViewModel>> GetEmailSettingsAsync();
    Task<ResponseDto<LogoSettingViewModel>> GetLogoSettingsAsync();
    Task<ResponseDto<LogoSqlSettingViewModel>> GetLogoSqlSettingsAsync();
    Task<ResponseDto<LogoRestServiceSettingViewModel>> GetLogoRestServiceSettingsAsync();



}