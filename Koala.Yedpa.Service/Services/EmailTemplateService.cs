using AutoMapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services;

public class EmailTemplateService(
    IEmailTemplateRepository repository,
    IUnitOfWork<AppDbContext> unitOfWork,
    IMapper mapper,
    ILogger<EmailTemplateService> logger)
    : IEmailTemplateService
{
    private readonly ILogger<EmailTemplateService> _logger = logger;

    public async Task<ResponseDto<List<EmailTemplateListViewModel>>> GetAllAsync()
    {
        _logger.LogInformation("GetAllAsync called");
        try
        {
            var res = await repository.GetAllAsync();
            var retVal = mapper.Map<List<EmailTemplateListViewModel>>(res);
            _logger.LogInformation("GetAllAsync: Retrieved {Count} email templates", retVal?.Count ?? 0);
            return ResponseDto<List<EmailTemplateListViewModel>>.SuccessData(200, "E-Posta Şablonu listesi başarıyla alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync: Error while getting email templates");
            return ResponseDto<List<EmailTemplateListViewModel>>.FailData(400, "E-Posta Şablonu Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<EmailTemplateDetailViewModel>> GetByNameAsyc(string name)
    {
        _logger.LogInformation("GetByNameAsyc called for template {TemplateName}", name);
        try
        {
            var res = await repository.GetByNameAsyc(name);
            if (res == null)
            {
                _logger.LogWarning("GetByNameAsyc: Template not found for name {TemplateName}", name);
                return ResponseDto<EmailTemplateDetailViewModel>.FailData(404, "E-Posta Şablonu Bulunamadı", $"'{name}' adında e-posta şablonu bulunamadı", true);
            }
            var retVal = mapper.Map<EmailTemplateDetailViewModel>(res);
            _logger.LogInformation("GetByNameAsyc: Template found for name {TemplateName}", name);
            return ResponseDto<EmailTemplateDetailViewModel>.SuccessData(200, "E-Posta Şablonu Bilgisi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByNameAsyc: Error while getting template by name {TemplateName}", name);
            return ResponseDto<EmailTemplateDetailViewModel>.FailData(400, "E-Posta Şablonu Bilgisi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<EmailTemplateDetailViewModel>> GetByIdAsync(string id)
    {
        _logger.LogInformation("GetByIdAsync called for template ID {TemplateId}", id);
        try
        {
            var res = await repository.GetByIdAsync(id);
            if (res == null)
            {
                _logger.LogWarning("GetByIdAsync: Template not found for ID {TemplateId}", id);
                return ResponseDto<EmailTemplateDetailViewModel>.FailData(404, "E-Posta Şablonu Bulunamadı", $"'{id}' ID'li e-posta şablonu bulunamadı", true);
            }
            var retVal = mapper.Map<EmailTemplateDetailViewModel>(res);
            _logger.LogInformation("GetByIdAsync: Template found for ID {TemplateId}", id);
            return ResponseDto<EmailTemplateDetailViewModel>.SuccessData(200, "E-Posta Şablonu Bilgisi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByIdAsync: Error while getting template by ID {TemplateId}", id);
            return ResponseDto<EmailTemplateDetailViewModel>.FailData(400, "E-Posta Şablonu Bilgisi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> CreateAsync(EmailTemplateCreateViewModel emailTemplate)
    {
        _logger.LogInformation("CreateAsync called for template {TemplateName}", emailTemplate?.Name);
        try
        {
            var entity = mapper.Map<EmailTemplate>(emailTemplate);
            await repository.AddAsync(entity);
            await unitOfWork.CommitAsync();
            _logger.LogInformation("CreateAsync: Template created successfully with ID {TemplateId}", entity.Id);
            return ResponseDto.Success(201, "E-Posta Şablonu Başarıyla Oluşturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateAsync: Error while creating template");
            return ResponseDto<ModuleListViewModel>.FailData(400, "E-Posta Şablonu Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<EmailTemplateDetailViewModel>> UpdateAsync(EmailTemplateUpdateViewModel emailTemplate)
    {
        _logger.LogInformation("UpdateAsync called for template ID {TemplateId}", emailTemplate?.Id);
        try
        {
            var template = await repository.GetByIdAsync(emailTemplate.Id);
            if (template == null)
            {
                _logger.LogWarning("UpdateAsync: Template not found for ID {TemplateId}", emailTemplate.Id);
                return ResponseDto<EmailTemplateDetailViewModel>.FailData(404, "E-Posta Şablonu Bulunamadı", "E-Posta Şablonu bulunamadı", true);
            }
            template.Name = emailTemplate.Name;
            template.Content = emailTemplate.Content;
            template.Description = emailTemplate.Description;
            repository.Update(template);
            await unitOfWork.CommitAsync();
            var retVal=mapper.Map<EmailTemplateDetailViewModel>(template);
            _logger.LogInformation("UpdateAsync: Template updated successfully for ID {TemplateId}", emailTemplate.Id);
            return ResponseDto<EmailTemplateDetailViewModel>.SuccessData(200, "E-Posta Şablonu Başarıyla Güncellendi", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAsync: Error while updating template");
            return ResponseDto<EmailTemplateDetailViewModel>.FailData(400, "E-Posta Şablonu Güncellenirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> ChangeStatus(EmailTemplateChangeStatusViewModel model)
    {
        _logger.LogInformation("ChangeStatus called for template ID {TemplateId}, new status: {Status}", model?.Id, model?.Status);
        try
        {
            var template = await repository.GetByIdAsync(model.Id);
            if (template == null)
            {
                _logger.LogWarning("ChangeStatus: Template not found for ID {TemplateId}", model.Id);
                return ResponseDto.Fail(404, "E-Posta Şablonu Bulunamadı", "E-Posta Şablonu bulunamadı", true);
            }
            template.Status = model.Status;
            repository.Update(template);
            await unitOfWork.CommitAsync();
            _logger.LogInformation("ChangeStatus: Template status updated successfully for ID {TemplateId}", model.Id);
            return ResponseDto.Success(200, "E-Posta Şablonu Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangeStatus: Error while changing template status");
            return ResponseDto.Fail(400, "E-Posta Şablonu Güncellenirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }
}