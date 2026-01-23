using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface IWorkplaceService
{
    Task<ResponseDto<List<WorkplaceListViewModel>>> GetAllAsync();
    Task<ResponseDto<WorkplaceDetailViewModel>> GetByIdAsync(string id);
    Task<ResponseDto<WorkplaceDetailViewModel>> GetByCodeAsync(string code);
    Task<ResponseListDto<List<WorkplaceListViewModel>>> GetPagedListAsync(int start, int length, string? searchValue = null, string? orderColumn = null, bool orderAscending = true);
    Task<ResponseDto<WorkplaceDetailViewModel>> UpdateAsync(WorkplaceDetailViewModel model);
}
