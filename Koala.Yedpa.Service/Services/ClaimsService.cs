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

public class ClaimsService(IClaimsRepository repository, IMapper mapper, IUnitOfWork<AppDbContext> unitOfWork, ILogger<ClaimsService> logger)
    : IClaimsService
{
    private readonly IClaimsRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ILogger<ClaimsService> _logger = logger;

    public async Task<ResponseDto> CreateClaim(CreateClaimsViewModel model)
    {
        _logger.LogInformation("CreateClaim called for claim {ClaimName}", model?.Name);
        try
        {
            var entity = mapper.Map<Claims>(model);
            await _repository.AddClaimAsync(entity);
            await unitOfWork.CommitAsync();
            _logger.LogInformation("CreateClaim: Claim created successfully with ID {ClaimId}", entity.Id);
            return ResponseDto.Success(200, "Hak Tanımlaması Başarıyla Yapıldı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateClaim: Error while creating claim");
            return ResponseDto.Fail(400, "Hak Tanımlaması Yapılırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateClaim(UpdateClaimsViewModel model)
    {
        _logger.LogInformation("UpdateClaim called for claim ID {ClaimId}", model?.Id);
        try
        {
            var entity = await _repository.GetClaimByIdAsync(model.Id);
            if (entity == null)
            {
                _logger.LogWarning("UpdateClaim: Claim not found for ID {ClaimId}", model.Id);
                return ResponseDto.Fail(404, "Hak Bilgilerine Ulaşılamadı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            entity.ModuleId = model.ModuleId;
            entity.Name = model.Name;
            entity.DisplayName = model.DisplayName;
            entity.Description = model.Description;
            _repository.UpdateClaim(entity);
            await unitOfWork.CommitAsync();
            _logger.LogInformation("UpdateClaim: Claim updated successfully for ID {ClaimId}", model.Id);
            return ResponseDto.Success(200, "Hak Bilgileri Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateClaim: Error while updating claim");
            return ResponseDto.Fail(400, "Hak Bilgileri Güncellenirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<ClaimsListViewModel>>> GetClaims()
    {
        _logger.LogInformation("GetClaims called");
        try
        {
            var res = await _repository.GetAllClaimsAsync();
            if (res == null)
            {
                _logger.LogWarning("GetClaims: No claims found");
                return ResponseDto<List<ClaimsListViewModel>>.FailData(404, "Hak Listesi Alınırken Bir Sorunla Karşılaşıldı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            var retVal = mapper.Map<List<ClaimsListViewModel>>(res);
            _logger.LogInformation("GetClaims: Retrieved {Count} claims", retVal?.Count ?? 0);
            return ResponseDto<List<ClaimsListViewModel>>.SuccessData(200, "Hak Listesi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetClaims: Error while getting claims");
            return ResponseDto<List<ClaimsListViewModel>>.FailData(400, "Hak Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<ClaimsListViewModel>>> GetModuleClaims(string moduleId)
    {
        _logger.LogInformation("GetModuleClaims called for module ID {ModuleId}", moduleId);
        try
        {
            if (string.IsNullOrEmpty(moduleId))
            {
                _logger.LogWarning("GetModuleClaims: ModuleId is null or empty");
                return ResponseDto<List<ClaimsListViewModel>>.FailData(400, "Modül Kimlik Bilgisi Boş Bırakılamaz", "Modül Kimlik Bilgisi Boş Bırakılamnaz", true);
            }
            var res = await _repository.GetClaimsByModuleIdAsync(moduleId);
            if (res == null)
            {
                _logger.LogWarning("GetModuleClaims: No claims found for module ID {ModuleId}", moduleId);
                return ResponseDto<List<ClaimsListViewModel>>.FailData(404, "Hak Listesi Alınırken Bir Sorunla Karşılaşıldı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            res = res.Where(x => x.ModuleId == moduleId).ToList();
            var retVal = mapper.Map<List<ClaimsListViewModel>>(res);
            _logger.LogInformation("GetModuleClaims: Retrieved {Count} claims for module ID {ModuleId}", retVal?.Count ?? 0, moduleId);
            return ResponseDto<List<ClaimsListViewModel>>.SuccessData(200, "Hak Listesi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetModuleClaims: Error while getting claims for module ID {ModuleId}", moduleId);
            return ResponseDto<List<ClaimsListViewModel>>.FailData(400, "Hak Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<IEnumerable<ClaimListForRoleViewModels>>> GetClaimToRoleList()
    {
        _logger.LogInformation("GetClaimToRoleList called");
        try
        {
            var items = await _repository.GetAllClaimsAsync();
            _logger.LogInformation("GetClaimToRoleList: Retrieved {Count} claims", items?.Count() ?? 0);
            return ResponseDto<IEnumerable<ClaimListForRoleViewModels>>.SuccessData(200, "Claim başarıyla alındı", mapper.Map<List<ClaimListForRoleViewModels>>(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetClaimToRoleList: Error while getting claims for roles");
            return ResponseDto<IEnumerable<ClaimListForRoleViewModels>>.FailData(400, "Claim listesi alınamadı", ex.Message, false);
        }
    }

    public async Task<ResponseDto<List<ClaimsListViewModel>?>> FindClaims(SearchClaimViewModel model)
    {
        _logger.LogInformation("FindClaims called with Name={Name}, DisplayName={DisplayName}", model?.Name, model?.DisplayName);
        try
        {
            var res = await _repository.GetAllClaimsAsync();
            if (res == null || !res.Any())
            {
                _logger.LogWarning("FindClaims: No claims found");
                return ResponseDto<List<ClaimsListViewModel>?>.FailData(404, "Hak Listesi Alınırken Bir Sorunla Karşılaşıldı", "Hak Bilgilerine Ulaşılamadı", true);
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                res = res.Where(x => x.Name.Contains(model.Name)).ToList();
            }

            if (string.IsNullOrEmpty(model.Description))
            {
                res = res.Where(x => x.Description.Contains(model.Description)).ToList();
            }
            if (!string.IsNullOrEmpty(model.DisplayName))
            {
                res = res.Where(x => x.DisplayName.Contains(model.DisplayName)).ToList();
            }
            if (model.ModuleId != null)
            {
                res = res.Where(x => x.ModuleId == model.ModuleId).ToList();
            }
            if (model.PageIndex > 0 && model.PageSize > 0)
            {
                res = res.Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize).ToList();
            }
            var retVal = mapper.Map<List<ClaimsListViewModel>>(res);
            _logger.LogInformation("FindClaims: Found {Count} matching claims", retVal?.Count ?? 0);
            return ResponseDto<List<ClaimsListViewModel>?>.SuccessData(200, "Aradığınız Özelliklere Göre Hak Listesi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FindClaims: Error while searching claims");
            return ResponseDto<List<ClaimsListViewModel>?>.FailData(400, "Hak Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<UpdateClaimsViewModel?>> GetClaimById(string id)
    {
        _logger.LogInformation("GetClaimById called for claim ID {ClaimId}", id);
        try
        {
            var res = await _repository.GetClaimByIdAsync(id);
            if (res == null)
            {
                _logger.LogWarning("GetClaimById: Claim not found for ID {ClaimId}", id);
                return ResponseDto<UpdateClaimsViewModel?>.FailData(404, "Hak Bilgilerine Ulaşılamadı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            var retVal = mapper.Map<UpdateClaimsViewModel>(res);
            _logger.LogInformation("GetClaimById: Claim found for ID {ClaimId}", id);
            return ResponseDto<UpdateClaimsViewModel?>.SuccessData(200, "Hak Bilgileri Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetClaimById: Error while getting claim by ID {ClaimId}", id);
            return ResponseDto<UpdateClaimsViewModel?>.FailData(400, "Hak Bilgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<UpdateClaimsViewModel?>> GetClaimByName(string name)
    {
        _logger.LogInformation("GetClaimByName called for claim {ClaimName}", name);
        try
        {
            var res = (await _repository.WhereClaimsAsync(x => x.Name == name) ?? null).FirstOrDefault();
            if (res == null)
            {
                _logger.LogWarning("GetClaimByName: Claim not found for name {ClaimName}", name);
                return ResponseDto<UpdateClaimsViewModel?>.FailData(404, "Hak Bilgilerine Ulaşılamadı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            var retVal = mapper.Map<UpdateClaimsViewModel>(res);
            _logger.LogInformation("GetClaimByName: Claim found for name {ClaimName}", name);
            return ResponseDto<UpdateClaimsViewModel?>.SuccessData(200, "Hak Bilgileri Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetClaimByName: Error while getting claim by name {ClaimName}", name);
            return ResponseDto<UpdateClaimsViewModel?>.FailData(400, "Hak Bilgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> DeleteClaim(string id)
    {
        _logger.LogInformation("DeleteClaim called for claim ID {ClaimId}", id);
        try
        {
            var res = await _repository.GetClaimByIdAsync(id);
            if (res == null)
            {
                _logger.LogWarning("DeleteClaim: Claim not found for ID {ClaimId}", id);
                return ResponseDto.Fail(404, "Hak Bilgilerine Ulaşılamadı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            _repository.DeleteClaimAsync(id);
            await unitOfWork.CommitAsync();
            _logger.LogInformation("DeleteClaim: Claim deleted successfully for ID {ClaimId}", id);
            return ResponseDto.Success(200, "Hak Bilgileri Başarıyla Silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteClaim: Error while deleting claim for ID {ClaimId}", id);
            return ResponseDto.Fail(400, "Hak Bilgileri Silinirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }
}