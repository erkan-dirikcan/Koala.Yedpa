using AutoMapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;

namespace Koala.Yedpa.Service.Services;

public class SiteService: ISiteService
{
    private readonly ISiteRepository _siteRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;

    public SiteService(
        ISiteRepository siteRepository,
        IMapper mapper,
        IUnitOfWork<AppDbContext> unitOfWork)
    {
        _siteRepository = siteRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseDto<List<LgXt001211ListViewModel>>> LgXt001211List()
    {
        try
        {
            var entities = await _siteRepository.GetAllAsync();
            var viewModels = _mapper.Map<List<LgXt001211ListViewModel>>(entities);
            
            return ResponseDto<List<LgXt001211ListViewModel>>.SuccessData(200, "LgXt001211 listesi başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            return ResponseDto<List<LgXt001211ListViewModel>>.FailData(500, "LgXt001211 listesi getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseListDto<List<LgXt001211ListViewModel>>> GetPagedListAsync(int start, int length, string? searchValue = null, string? orderColumn = null, bool orderAscending = true)
    {
        try
        {
            System.Linq.Expressions.Expression<Func<LgXt001211, bool>>? predicate = null;

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var search = searchValue.ToLower();
                predicate = x =>
                    (x.LogRef.ToString().Contains(search)) ||
                    (x.GroupCode != null && x.GroupCode.ToLower().Contains(search)) ||
                    (x.GroupName != null && x.GroupName.ToLower().Contains(search)) ||
                    (x.ClientCode != null && x.ClientCode.ToLower().Contains(search)) ||
                    (x.ClientName != null && x.ClientName.ToLower().Contains(search));
            }

            // Toplam kayıt sayısı
            var recordsTotal = await _siteRepository.CountAsync();

            // Filtrelenmiş kayıt sayısı
            var recordsFiltered = predicate != null 
                ? await _siteRepository.CountAsync(predicate) 
                : recordsTotal;

            // Sayfalı veri getir
            var entities = await _siteRepository.GetPagedAsync(start, length, predicate, orderColumn, orderAscending);
            var viewModels = _mapper.Map<List<LgXt001211ListViewModel>>(entities);

            return ResponseListDto<List<LgXt001211ListViewModel>>.SuccessData(
                200,
                "LgXt001211 listesi başarıyla getirildi.",
                viewModels,
                recordsTotal,
                recordsFiltered,
                viewModels.Count
            );
        }
        catch (Exception ex)
        {
            return ResponseListDto<List<LgXt001211ListViewModel>>.FailData(500, "LgXt001211 listesi getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<LgXt001211UpdateViewModel>> GetByIdAsync(string id)
    {
        try
        {
            var entity = await _siteRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return ResponseDto<LgXt001211UpdateViewModel>.FailData(404, "LgXt001211 kaydı bulunamadı.", $"ID: {id}", false);
            }

            var viewModel = _mapper.Map<LgXt001211UpdateViewModel>(entity);
            
            return ResponseDto<LgXt001211UpdateViewModel>.SuccessData(200, "LgXt001211 kaydı başarıyla getirildi.", viewModel);
        }
        catch (Exception ex)
        {
            return ResponseDto<LgXt001211UpdateViewModel>.FailData(500, "LgXt001211 kaydı getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<LgXt001211UpdateViewModel>> UpdateLgXt001211(LgXt001211UpdateViewModel model)
    {
        try
        {
            var entity = await _siteRepository.GetByIdAsync(model.Id);
            if (entity == null)
            {
                return ResponseDto<LgXt001211UpdateViewModel>.FailData(404, "LgXt001211 kaydı bulunamadı.", $"ID: {model.Id}", false);
            }

            // Update entity with model values
            _mapper.Map(model, entity);
            
            _siteRepository.Update(entity);
            await _unitOfWork.CommitAsync();

            var updatedViewModel = _mapper.Map<LgXt001211UpdateViewModel>(entity);
            
            return ResponseDto<LgXt001211UpdateViewModel>.SuccessData(200, "LgXt001211 kaydı başarıyla güncellendi.", updatedViewModel);
        }
        catch (Exception ex)
        {
            return ResponseDto<LgXt001211UpdateViewModel>.FailData(500, "LgXt001211 güncelleme başarısız.", ex.Message, true);
        }
    }
}