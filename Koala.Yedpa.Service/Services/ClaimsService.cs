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

public class ClaimsService(IClaimsRepository repository, IMapper mapper, IUnitOfWork<AppDbContext> unitOfWork, ILogger<EmailService> logger)
    : IClaimsService
{
    private readonly IClaimsRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ILogger<EmailService> _logger = logger;

    public async Task<ResponseDto> CreateClaim(CreateClaimsViewModel model)
    {
        try
        {
            var entity = mapper.Map<Claims>(model);
            await _repository.AddClaimAsync(entity);
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Hak Tanımlaması Başarıyla Yapıldı");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Hak Tanımlaması Yapılırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateClaim(UpdateClaimsViewModel model)
    {
        try
        {
            var entity = await _repository.GetClaimByIdAsync(model.Id);
            if (entity == null)
            {
                return ResponseDto.Fail(404, "Hak Bilgilerine Ulaşılamadı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            entity.ModuleId = model.ModuleId;
            entity.Name = model.Name;
            entity.DisplayName = model.DisplayName;
            entity.Description = model.Description;
            _repository.UpdateClaim(entity);
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Hak Bilgileri Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Hak Bilgileri Güncellenirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<ClaimsListViewModel>>> GetClaims()
    {
        try
        {
            var res = await _repository.GetAllClaimsAsync();
            if (res == null)
            {
                return ResponseDto<List<ClaimsListViewModel>>.FailData(404, "Hak Listesi Alınırken Bir Sorunla Karşılaşıldı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            var retVal = mapper.Map<List<ClaimsListViewModel>>(res);
            return ResponseDto<List<ClaimsListViewModel>>.SuccessData(200, "Hak Listesi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<List<ClaimsListViewModel>>.FailData(400, "Hak Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<ClaimsListViewModel>>> GetModuleClaims(string moduleId)
    {
        try
        {
            if (string.IsNullOrEmpty(moduleId))
            {
                return ResponseDto<List<ClaimsListViewModel>>.FailData(400, "Modül Kimlik Bilgisi Boş Bırakılamaz", "Modül Kimlik Bilgisi Boş Bırakılamnaz", true);
            }
            var res = await _repository.GetClaimsByModuleIdAsync(moduleId);
            if (res == null)
            {
                return ResponseDto<List<ClaimsListViewModel>>.FailData(404, "Hak Listesi Alınırken Bir Sorunla Karşılaşıldı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            res = res.Where(x => x.ModuleId == moduleId).ToList();
            var retVal = mapper.Map<List<ClaimsListViewModel>>(res);
            return ResponseDto<List<ClaimsListViewModel>>.SuccessData(200, "Hak Listesi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<List<ClaimsListViewModel>>.FailData(400, "Hak Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<IEnumerable<ClaimListForRoleViewModels>>> GetClaimToRoleList()
    {
        try
        {
            var items = await _repository.GetAllClaimsAsync();
            return ResponseDto<IEnumerable<ClaimListForRoleViewModels>>.SuccessData(200, "Claim başarıyla alındı", mapper.Map<List<ClaimListForRoleViewModels>>(items));
        }
        catch (Exception ex)
        {

            return ResponseDto<IEnumerable<ClaimListForRoleViewModels>>.FailData(400, "Claim listesi alınamadı", ex.Message, false);
        }
    }

    public async Task<ResponseDto<List<ClaimsListViewModel>?>> FindClaims(SearchClaimViewModel model)
    {
        try
        {
            var res = await _repository.GetAllClaimsAsync();
            if (res == null || !res.Any())
            {
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
            return ResponseDto<List<ClaimsListViewModel>?>.SuccessData(200, "Aradığınız Özelliklere Göre Hak Listesi Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<List<ClaimsListViewModel>?>.FailData(400, "Hak Listesi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<UpdateClaimsViewModel?>> GetClaimById(string id)
    {
        try
        {
            var res = await _repository.GetClaimByIdAsync(id);
            if (res == null)
            {
                return ResponseDto<UpdateClaimsViewModel?>.FailData(404, "Hak Bilgilerine Ulaşılamadı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            var retVal = mapper.Map<UpdateClaimsViewModel>(res);
            return ResponseDto<UpdateClaimsViewModel?>.SuccessData(200, "Hak Bilgileri Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<UpdateClaimsViewModel?>.FailData(400, "Hak Bilgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<UpdateClaimsViewModel?>> GetClaimByName(string name)
    {
        try
        {
            var res = (await _repository.WhereClaimsAsync(x => x.Name == name) ?? null).FirstOrDefault();
            if (res == null)
            {
                return ResponseDto<UpdateClaimsViewModel?>.FailData(404, "Hak Bilgilerine Ulaşılamadı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            var retVal = mapper.Map<UpdateClaimsViewModel>(res);
            return ResponseDto<UpdateClaimsViewModel?>.SuccessData(200, "Hak Bilgileri Başarıyla Alındı", retVal);
        }
        catch (Exception ex)
        {
            return ResponseDto<UpdateClaimsViewModel?>.FailData(400, "Hak Bilgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }


    }

    public async Task<ResponseDto> DeleteClaim(string id)
    {
        try
        {
            var res = await _repository.GetClaimByIdAsync(id);
            if (res == null)
            {
                return ResponseDto.Fail(404, "Hak Bilgilerine Ulaşılamadı", "Hak Bilgilerine Ulaşılamadı", true);
            }
            _repository.DeleteClaimAsync(id);
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Hak Bilgileri Başarıyla Silindi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Hak Bilgileri Silinirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }
}