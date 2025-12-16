using AutoMapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;

namespace Koala.Yedpa.Service.Services;

public class EmailTemplateService(
    IEmailTemplateRepository repository,
    IUnitOfWork<AppDbContext> unitOfWork,
    IMapper mapper)
    : IEmailTemplateService
{
    public async Task<ResponseDto<List<EmailTemplateListViewModel>>> GetAllAsync()
    {
        try
        {
            var res = await repository.GetAllAsync();
            var retVal = mapper.Map<List<EmailTemplateListViewModel>>(res);
            return ResponseDto<List<EmailTemplateListViewModel>>.SuccessData(200, "E-Posta Şablonu listesi başarıyla alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<List<EmailTemplateListViewModel>>.FailData(400, "E-Posta Şablonu Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<EmailTemplateDetailViewModel>> GetByNameAsyc(string name)
    {
        try
        {
            var res = await repository.GetByNameAsyc(name);
            var retVal = mapper.Map<EmailTemplateDetailViewModel>(res);
            return ResponseDto<EmailTemplateDetailViewModel>.SuccessData(200, "E-Posta Şablonu Bilgisi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<EmailTemplateDetailViewModel>.FailData(400, "E-Posta Şablonu Bilgisi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<EmailTemplateDetailViewModel>> GetByIdAsync(string id)
    {
        try
        {
            var res = await repository.GetByIdAsync(id);
            var retVal = mapper.Map<EmailTemplateDetailViewModel>(res);
            return ResponseDto<EmailTemplateDetailViewModel>.SuccessData(200, "E-Posta Şablonu Bilgisi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<EmailTemplateDetailViewModel>.FailData(400, "E-Posta Şablonu Bilgisi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> CreateAsync(EmailTemplateCreateViewModel emailTemplate)
    {
        try
        {
            var entity = mapper.Map<EmailTemplate>(emailTemplate);
            await repository.AddAsync(entity);
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(201, "E-Posta Şablonu Başarıyla Oluşturuldu");
        }
        catch (Exception ex)
        {
            return ResponseDto<ModuleListViewModel>.FailData(400, "E-Posta Şablonu Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<EmailTemplateDetailViewModel>> UpdateAsync(EmailTemplateUpdateViewModel emailTemplate)
    {
        try
        {
            var template = await repository.GetByIdAsync(emailTemplate.Id);
            if (template == null)
            {
                return ResponseDto<EmailTemplateDetailViewModel>.FailData(404, "E-Posta Şablonu Bulunamadı", "E-Posta Şablonu bulunamadı", true);
            }
            template.Name = emailTemplate.Name;
            template.Content = emailTemplate.Content;
            template.Description = emailTemplate.Description;
            repository.Update(template);
            await unitOfWork.CommitAsync();
            var retVal=mapper.Map<EmailTemplateDetailViewModel>(template);
            return ResponseDto<EmailTemplateDetailViewModel>.SuccessData(200, "E-Posta Şablonu Başarıyla Güncellendi", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<EmailTemplateDetailViewModel>.FailData(400, "E-Posta Şablonu Güncellenirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> ChangeStatus(EmailTemplateChangeStatusViewModel model)
    {
        try
        {
            var template = await repository.GetByIdAsync(model.Id);
            if (template == null)
            {
                return ResponseDto.Fail(404, "E-Posta Şablonu Bulunamadı", "E-Posta Şablonu bulunamadı", true);
            }
            template.Status = model.Status;
            repository.Update(template);
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "E-Posta Şablonu Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "E-Posta Şablonu Güncellenirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }
}