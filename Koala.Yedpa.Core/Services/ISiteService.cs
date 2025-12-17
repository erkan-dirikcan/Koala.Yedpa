using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface ISiteService
{
    Task<ResponseDto<List<LgXt001211ListViewModel>>> LgXt001211List();
    Task<ResponseListDto<List<LgXt001211ListViewModel>>> GetPagedListAsync(int start, int length, string? searchValue = null, string? orderColumn = null, bool orderAscending = true);
    Task<ResponseDto<LgXt001211UpdateViewModel>> GetByIdAsync(string id);
    Task<ResponseDto<LgXt001211UpdateViewModel>> UpdateLgXt001211 (LgXt001211UpdateViewModel model);
}