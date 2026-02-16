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

public class ModuleService(IMapper mapper, IModuleRepository repository, IUnitOfWork<AppDbContext> unitOfWork, ILogger<ModuleService> logger)
    : IModuleService
{
    private readonly ILogger<ModuleService> _logger = logger;

    public async Task<ResponseDto<ModuleListViewModel>?> GetModuleByIdAsync(string id)
    {
        _logger.LogInformation("GetModuleByIdAsync called for module ID {ModuleId}", id);
        try
        {
            var res = await repository.GetModuleByIdAsync(id);
            var retVal = mapper.Map<ModuleListViewModel>(res);
            _logger.LogInformation("GetModuleByIdAsync: Module found for ID {ModuleId}", id);
            return ResponseDto<ModuleListViewModel>.SuccessData(200, "Modül Bilgisi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetModuleByIdAsync: Error while getting module by ID {ModuleId}", id);
            return ResponseDto<ModuleListViewModel>.FailData(400, "Modül Bilgisi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<UpdateModuleViewModel>> GetUpdateModel(string id)
    {
        _logger.LogInformation("GetUpdateModel called for module ID {ModuleId}", id);
        try
        {
            var res = await repository.GetModuleByIdAsync(id);
            var retVal = mapper.Map<UpdateModuleViewModel>(res);
            _logger.LogInformation("GetUpdateModel: Update model retrieved for module ID {ModuleId}", id);
            return ResponseDto<UpdateModuleViewModel>.SuccessData(200, "Modül Bilgisi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUpdateModel: Error while getting update model for module ID {ModuleId}", id);
            return ResponseDto<UpdateModuleViewModel>.FailData(400, "Modül Bilgisi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<IEnumerable<ModuleListViewModel>>> GetAllModuleAsync()
    {
        _logger.LogInformation("GetAllModuleAsync called");
        try
        {
            var res = await repository.GetAllModuleAsync();
            var retVal = mapper.Map<IEnumerable<ModuleListViewModel>>(res);
            var count = retVal?.Count() ?? 0;
            _logger.LogInformation("GetAllModuleAsync: Retrieved {Count} modules", count);
            return ResponseDto<IEnumerable<ModuleListViewModel>>.SuccessData(200, "Modül listesi başarıyla alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllModuleAsync: Error while getting all modules");
            return ResponseDto<IEnumerable<ModuleListViewModel>>.FailData(400, "Modül Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<IEnumerable<ModuleListViewModel>>> SearchModuleWhereAsync(SearchModuleViewModel sModel)
    {
        _logger.LogInformation("SearchModuleWhereAsync called with Name={Name}, DisplayName={DisplayName}, Status={Status}",
            sModel.Name, sModel.DisplayName, sModel.Status);
        try
        {
            var res = await repository.GetAllModuleAsync();
            if (sModel.Name != null)
            {
                res = res.Where(x => x.Name.Contains(sModel.Name));
            }
            if (sModel.DisplayName != null)
            {
                res = res.Where(x => x.DisplayName.Contains(sModel.DisplayName));
            }
            if (sModel.Status != null)
            {
                res = res.Where(x => x.Status == sModel.Status);
            }
            if (sModel.PageNumber != null && sModel.PageSize != null)
            {
                res = res.Skip((sModel.PageNumber.Value - 1) * sModel.PageSize.Value).Take(sModel.PageSize.Value);
            }
            var retVal = mapper.Map<IEnumerable<ModuleListViewModel>>(res);
            var count = retVal?.Count() ?? 0;
            _logger.LogInformation("SearchModuleWhereAsync: Found {Count} modules", count);
            return ResponseDto<IEnumerable<ModuleListViewModel>>.SuccessData(200, "Modül Listesi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SearchModuleWhereAsync: Error while searching modules");
            return ResponseDto<IEnumerable<ModuleListViewModel>>.FailData(400, "Modül listesinde Arama Yapılırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> CreateModuleAsync(CreateModuleViewModel dto)
    {
        _logger.LogInformation("CreateModuleAsync called for module {ModuleName}", dto?.Name);
        try
        {
            var entity = mapper.Map<Module>(dto);
            await repository.AddModuleAsync(entity);
            await unitOfWork.CommitAsync();
            _logger.LogInformation("CreateModuleAsync: Module created successfully with ID {ModuleId}", entity.Id);
            return ResponseDto.Success(201, "Modül Başarıyla Oluşturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateModuleAsync: Error while creating module");
            return ResponseDto<ModuleListViewModel>.FailData(400, "Modül Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> ModuleChangeStatus(ModuleChangeStatusViewModel model)
    {
        _logger.LogInformation("ModuleChangeStatus called for module ID {ModuleId}, new status: {Status}", model?.Id, model?.Status);
        try
        {
            var module = await repository.GetModuleByIdAsync(model.Id);
            if (module == null)
            {
                _logger.LogWarning("ModuleChangeStatus: Module not found for ID {ModuleId}", model.Id);
                return ResponseDto.Fail(404, "Modül Bulunamadı", new ErrorDto("Modül bulunamadı", true));
            }

            module.Status = model.Status;
            var res =repository.UpdateModule(module);
            await unitOfWork.CommitAsync();
            _logger.LogInformation("ModuleChangeStatus: Module status updated successfully for ID {ModuleId}", model.Id);
            return ResponseDto.Success(200, "Modül Durumu Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ModuleChangeStatus: Error while changing module status");
            return ResponseDto.Fail(400, "Modül Durumu Güncellenirken Bir Sorunla Karşılaşıldı", new ErrorDto(ex.Message, true));
        }
    }

    public async Task<ResponseDto> UpdateModule(UpdateModuleViewModel dto, string id)
    {
        _logger.LogInformation("UpdateModule called for module ID {ModuleId}, name: {ModuleName}", id, dto?.Name);
        try
        {
            var module = await repository.GetModuleByIdAsync(id);
            if (module == null)
            {
                _logger.LogWarning("UpdateModule: Module not found for ID {ModuleId}", id);
                return ResponseDto.Fail(404, "Modül Bulunamadı", new ErrorDto("Modül bulunamadı", true));
            }
            module.Name=dto.Name;
            module.DisplayName = dto.DisplayName;
            module.Description = dto.Description;
            module.Status = dto.Status;
            var res=repository.UpdateModule(module);
            await unitOfWork.CommitAsync();
            _logger.LogInformation("UpdateModule: Module updated successfully for ID {ModuleId}", id);
            return ResponseDto.Success(200, "Modül Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateModule: Error while updating module");
            return ResponseDto.Fail(400, "Modül Güncellenirken Bir Sorunla Karşılaşıldı", new ErrorDto(ex.Message, true));
        }
    }
}