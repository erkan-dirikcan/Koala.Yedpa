using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface IModuleService 
{
    Task<ResponseDto<ModuleListViewModel>?> GetModuleByIdAsync(string id);
    Task<ResponseDto<UpdateModuleViewModel>?> GetUpdateModel(string id);
    Task<ResponseDto<IEnumerable<ModuleListViewModel>>> GetAllModuleAsync();
    Task<ResponseDto<IEnumerable<ModuleListViewModel>>> SearchModuleWhereAsync(SearchModuleViewModel sModel);
    Task<ResponseDto> CreateModuleAsync(CreateModuleViewModel dto);
    Task<ResponseDto> ModuleChangeStatus(ModuleChangeStatusViewModel model);
    Task<ResponseDto> UpdateModule(UpdateModuleViewModel dto, string id);
    
}