using AutoMapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;

namespace Koala.Yedpa.Service.Services;

public class ModuleService(IMapper mapper, IModuleRepository repository, IUnitOfWork<AppDbContext> unitOfWork)
    : IModuleService
{
    public async Task<ResponseDto<ModuleListViewModel>?> GetModuleByIdAsync(string id)
    {
        try
        {
            var res = await repository.GetModuleByIdAsync(id);
            var retVal = mapper.Map<ModuleListViewModel>(res);
            return ResponseDto<ModuleListViewModel>.SuccessData(200, "Modül Bilgisi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<ModuleListViewModel>.FailData(400, "Modül Bilgisi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<UpdateModuleViewModel>> GetUpdateModel(string id)
    {
        try
        {
            var res = await repository.GetModuleByIdAsync(id);
            var retVal = mapper.Map<UpdateModuleViewModel>(res);
            return ResponseDto<UpdateModuleViewModel>.SuccessData(200, "Modül Bilgisi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<UpdateModuleViewModel>.FailData(400, "Modül Bilgisi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<IEnumerable<ModuleListViewModel>>> GetAllModuleAsync()
    {
        try
        {
            var res = await repository.GetAllModuleAsync();
            var retVal = mapper.Map<IEnumerable<ModuleListViewModel>>(res);
            return ResponseDto<IEnumerable<ModuleListViewModel>>.SuccessData(200, "Modül listesi başarıyla alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<IEnumerable<ModuleListViewModel>>.FailData(400, "Modül Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<IEnumerable<ModuleListViewModel>>> SearchModuleWhereAsync(SearchModuleViewModel sModel)
    {
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
            return ResponseDto<IEnumerable<ModuleListViewModel>>.SuccessData(200, "Modül Listesi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<IEnumerable<ModuleListViewModel>>.FailData(400, "Modül listesinde Arama Yapılırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> CreateModuleAsync(CreateModuleViewModel dto)
    {
        try
        {
            var entity = mapper.Map<Module>(dto);
            await repository.AddModuleAsync(entity);
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(201, "Modül Başarıyla Oluşturuldu");
        }
        catch (Exception ex)
        {
            return ResponseDto<ModuleListViewModel>.FailData(400, "Modül Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> ModuleChangeStatus(ModuleChangeStatusViewModel model)
    {
        try
        {
            var module = await repository.GetModuleByIdAsync(model.Id);
            if (module == null)
            {
                return ResponseDto.Fail(404, "Modül Bulunamadı", new ErrorDto("Modül bulunamadı", true));
            }

            module.Status = model.Status;
            var res =repository.UpdateModule(module);
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Modül Durumu Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Modül Durumu Güncellenirken Bir Sorunla Karşılaşıldı", new ErrorDto(ex.Message, true));
        }
    }

    public async Task<ResponseDto> UpdateModule(UpdateModuleViewModel dto, string id)
    {
        try
        {
            var module = await repository.GetModuleByIdAsync(id);
            if (module == null)
            {
                return ResponseDto.Fail(404, "Modül Bulunamadı", new ErrorDto("Modül bulunamadı", true));
            }
            module.Name=dto.Name;
            module.DisplayName = dto.DisplayName;
            module.Description = dto.Description;
            module.Status = dto.Status;
            var res=repository.UpdateModule(module);
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Modül Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Modül Güncellenirken Bir Sorunla Karşılaşıldı", new ErrorDto(ex.Message, true));
        }
    }
}