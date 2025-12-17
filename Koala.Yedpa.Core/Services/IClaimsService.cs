using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services;

public interface IClaimsService
{
    Task<ResponseDto> CreateClaim(CreateClaimsViewModel model);
    Task<ResponseDto> UpdateClaim(UpdateClaimsViewModel model);
    Task<ResponseDto<List<ClaimsListViewModel>>?> GetClaims();
    Task<ResponseDto<List<ClaimsListViewModel>>?> GetModuleClaims(string id);
    Task<ResponseDto<IEnumerable<ClaimListForRoleViewModels>>> GetClaimToRoleList();
    Task<ResponseDto<List<ClaimsListViewModel>?>> FindClaims(SearchClaimViewModel model);
    Task<ResponseDto<UpdateClaimsViewModel?>> GetClaimById(string moduleId);
    Task<ResponseDto<UpdateClaimsViewModel?>> GetClaimByName(string name);
    Task<ResponseDto> DeleteClaim(string id);

}