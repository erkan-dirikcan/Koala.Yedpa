using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface IEmailTemplateService
{
    Task<ResponseDto<List<EmailTemplateListViewModel>>> GetAllAsync();
    Task<ResponseDto<EmailTemplateDetailViewModel>> GetByNameAsyc(string name);
    Task<ResponseDto<EmailTemplateDetailViewModel>> GetByIdAsync(string id);
    Task<ResponseDto> CreateAsync(EmailTemplateCreateViewModel emailTemplate);
    Task<ResponseDto<EmailTemplateDetailViewModel>> UpdateAsync(EmailTemplateUpdateViewModel emailTemplate);
    Task<ResponseDto> ChangeStatus(EmailTemplateChangeStatusViewModel model);


}    
