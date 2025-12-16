using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface IExtendedPropertiesService
{
    Task<ResponseDto<List<ExtendedPropertiesListViewModel>>> GetAllExtendedPropertiesAsync();
    Task<ResponseDto<List<ExtendedPropertiesListViewModel>>> GetModuleExtendedPropertiesAsync(string moduleId);
    Task<ResponseDto<ExtendedPropertiesDetailViewModel?>> GetExtendedPropertiesByIdAsync(string id);
    Task<ResponseDto<ExtendedPropertiesDetailViewModel?>> GetExtendedPropertiesByNameAsync(string name);
    Task<ResponseDto> CreateExtendedProperties(CreateExtendedPropertiesViewModel model);
    Task<ResponseDto> UpdateExtendedProperties(UpdateExtendedPropertiesViewModel model);
    Task<ResponseDto> DeleteExtendedProperties(string id);


}