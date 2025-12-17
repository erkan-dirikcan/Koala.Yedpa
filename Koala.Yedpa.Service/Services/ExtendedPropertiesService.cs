using AutoMapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;

namespace Koala.Yedpa.Service.Services;

public class ExtendedPropertiesService : IExtendedPropertiesService
{
    private readonly IExtendedPropertiesRepository _repository;
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly ILogger<EmailService> _logger;
    private readonly IMapper _mapper;
    //TODO: Log Eklenecek
    public ExtendedPropertiesService(IExtendedPropertiesRepository repository, ILogger<EmailService> logger, IMapper mapper, IUnitOfWork<AppDbContext> unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseDto<List<ExtendedPropertiesListViewModel>>> GetAllExtendedPropertiesAsync()
    {
        try
        {
            
            var res = await _repository.GetAllExtendedPropertiesAsync();

            return ResponseDto<List<ExtendedPropertiesListViewModel>>.SuccessData(200,
                "Ek Özellik Listesi Başarıyla Alındı", _mapper.Map<List<ExtendedPropertiesListViewModel>>(res));
        }
        catch (Exception ex)
        {
            return ResponseDto<List<ExtendedPropertiesListViewModel>>.FailData(400, "Ek ÖZellik Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<ExtendedPropertiesListViewModel>>> GetModuleExtendedPropertiesAsync(string moduleId)
    {
        try
        {
            var res = await _repository.GetModuleExtendedPropertiesAsync(moduleId);

            return ResponseDto<List<ExtendedPropertiesListViewModel>>.SuccessData(200,
                "Modül Ek Özellik Listesi Başarıyla Alındı", _mapper.Map<List<ExtendedPropertiesListViewModel>>(res));
        }
        catch (Exception ex)
        {
            return ResponseDto<List<ExtendedPropertiesListViewModel>>.FailData(400, "Modül Ek ÖZellik Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<ExtendedPropertiesDetailViewModel?>> GetExtendedPropertiesByIdAsync(string id)
    {
        try
        {
            var res = await _repository.GetExtendedPropertiesByIdAsync(id);
            if (res == null)
            {
                return ResponseDto<ExtendedPropertiesDetailViewModel?>.FailData(404, "Ek Özellik Bilgileri Bulunamadı", $"{id} Id Bilgisine Sahip Ek Özelliklik Bilgilerine Ulaşılamadı", true);
            }
            return ResponseDto<ExtendedPropertiesDetailViewModel?>.SuccessData(200, "Ek Özellik Bilgileri Başarıyla Alındı", _mapper.Map<ExtendedPropertiesDetailViewModel>(res));
        }
        catch (Exception ex)
        {
            return ResponseDto<ExtendedPropertiesDetailViewModel>.FailData(400, "Ek ÖZelliklik Bilgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<ExtendedPropertiesDetailViewModel?>> GetExtendedPropertiesByNameAsync(string name)
    {
        try
        {
            var res = await _repository.GetExtendedPropertiesByNameAsync(name);
            if (res == null)
            {
                return ResponseDto<ExtendedPropertiesDetailViewModel?>.FailData(404, "Ek Özellik Bilgileri Bulunamadı", $"{name} İsimli Ek Özelliklik Bilgilerine Ulaşılamadı", true);
            }
            return ResponseDto<ExtendedPropertiesDetailViewModel?>.SuccessData(200, "Ek Özellik Bilgileri Başarıyla Alındı", _mapper.Map<ExtendedPropertiesDetailViewModel>(res));
        }
        catch (Exception ex)
        {
            return ResponseDto<ExtendedPropertiesDetailViewModel>.FailData(400, "Ek ÖZelliklik Bilgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> CreateExtendedProperties(CreateExtendedPropertiesViewModel model)
    {
        var entity = _mapper.Map<ExtendedProperties>(model);
        try
        {
            await _repository.CreateExtendedProperties(entity);
            await _unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Ek Özellik Başarıyla Oluşturuldu");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Ek Özellik Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateExtendedProperties(UpdateExtendedPropertiesViewModel model)
    {
        var entity = await _repository.GetExtendedPropertiesByIdAsync(model.Id);
        if (entity == null)
        {
            return ResponseDto.Fail(404, "Ek Özellik Bulunamadı", $"{model.Id} Id Bilgisine Sahip Ek Özelliklik Bilgilerine Ulaşılamadı", true);
        }
        entity.ModuleId=model.ModuleId;
        entity.Name=model.Name;
        entity.DisplayName=model.DisplayName;
        entity.Description=model.Description;
        entity.ShowOn=model.ShowOn;
        entity.InputType = model.InputType;
        try
        {
            _repository.UpdateExtendedProperties(entity);
            await _unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Ek Özellik Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Ek Özellik Güncellenirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> DeleteExtendedProperties(string id)
    {
        var entity = await _repository.GetExtendedPropertiesByIdAsync(id);
        if (entity == null)
        {
            return ResponseDto.Fail(404, "Ek Özellik Bulunamadı", $"{id} Id Bilgisine Sahip Ek Özelliklik Bilgilerine Ulaşılamadı", true);
        }

        entity.Status = StatusEnum.Deleted;
        _repository.UpdateExtendedProperties(entity);
        await _unitOfWork.CommitAsync();
        return ResponseDto.Success(200,"Ek Özellik Başarıyla Silindi");
    }
}