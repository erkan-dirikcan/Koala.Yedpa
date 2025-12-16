using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels.YourNamespace.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface IApiLogoSqlDataService
{
    Task<ResponseListDto<List<ClCardInfoViewModel>>> GetAllClCardInfoAsync(int perPage,int pageNo);
    Task<ResponseListDto<List<ClCardInfoViewModel>>> WhereClCardInfoAsync(ClCardInfoSearchViewModel searchModel, int perPage, int pageNo);
}