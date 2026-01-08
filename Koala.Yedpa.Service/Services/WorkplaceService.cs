using AutoMapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Koala.Yedpa.Service.Services;

public class WorkplaceService : IWorkplaceService
{
    private readonly IWorkplaceRepository _workplaceRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly ISqlProvider _sqlProvider;
    private readonly AppDbContext _context;
    private readonly ILogger<WorkplaceService> _logger;
    private readonly string? _currentUserId;

    public WorkplaceService(
        IWorkplaceRepository workplaceRepository,
        IMapper mapper,
        IUnitOfWork<AppDbContext> unitOfWork,
        ISqlProvider sqlProvider,
        ILogger<WorkplaceService> logger,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _workplaceRepository = workplaceRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _sqlProvider = sqlProvider;
        _context = unitOfWork.Context;
        _logger = logger;
        _currentUserId = httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? null;
    }

    public async Task<ResponseDto<List<WorkplaceListViewModel>>> GetAllAsync()
    {
        try
        {
            // 1. Workplace kayıtlarını kendi veritabanından çek
            var workplaces = await _workplaceRepository.GetAllAsync();
            var workplaceList = workplaces.ToList();

            // 2. Cari bilgilerini Logo veritabanından çek
            var cariQuery = "SELECT ICODE, CCODE, CDEF FROM IS_YERI_MEVCUT_CARI";
            var cariResult = _sqlProvider.SqlReader(cariQuery);

            var cariDict = new Dictionary<string, (string ClCode, string ClDefinition)>();
            if (cariResult.IsSuccess && cariResult.Data != null)
            {
                foreach (DataRow row in cariResult.Data.Rows)
                {
                    var icode = GetValueOrDefault<string>(row["ICODE"]) ?? string.Empty;
                    var ccode = GetValueOrDefault<string>(row["CCODE"]) ?? string.Empty;
                    var cdef = GetValueOrDefault<string>(row["CDEF"]) ?? string.Empty;

                    if (!string.IsNullOrEmpty(icode))
                    {
                        cariDict[icode] = (ccode, cdef);
                    }
                }
            }

            // 3. Verileri birleştir
            var viewModels = workplaceList.Select(w => new WorkplaceListViewModel
            {
                Id = w.Id,
                Code = w.Code,
                Definition = w.Definition,
                ClCode = cariDict.TryGetValue(w.Code, out var cariInfo) ? cariInfo.ClCode : string.Empty,
                ClDefinition = cariDict.TryGetValue(w.Code, out var cariInfo2) ? cariInfo2.ClDefinition : string.Empty,
                LogicalRef = w.LogicalRef,
                LogRef = w.LogRef,
                CustomerType = w.CustomerType,
                BegDate = w.BegDate,
                EndDate = w.EndDate,
                TotalNetMetre = w.TotalNetMetre,
                TotalFuelMetre = w.TotalFuelMetre
            }).ToList();

            return ResponseDto<List<WorkplaceListViewModel>>.SuccessData(200, "İşyerleri listesi başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşyerleri listesi getirilirken hata oluştu");
            return ResponseDto<List<WorkplaceListViewModel>>.FailData(500, "İşyerleri listesi getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<WorkplaceDetailViewModel>> GetByIdAsync(string id)
    {
        try
        {
            // 1. Workplace kaydını kendi veritabanından çek
            var workplace = await _workplaceRepository.GetByIdAsync(id);
            if (workplace == null)
            {
                return ResponseDto<WorkplaceDetailViewModel>.FailData(404, "İşyeri kaydı bulunamadı.", $"ID: {id}", false);
            }

            // 2. Cari bilgisini Logo veritabanından çek
            string cariCode = string.Empty;
            string cariDefinition = string.Empty;

            var cariQuery = $"SELECT CCODE, CDEF FROM IS_YERI_MEVCUT_CARI WHERE ICODE = '{workplace.Code}'";
            var cariResult = _sqlProvider.SqlReader(cariQuery);

            if (cariResult.IsSuccess && cariResult.Data != null && cariResult.Data.Rows.Count > 0)
            {
                var row = cariResult.Data.Rows[0];
                cariCode = GetValueOrDefault<string>(row["CCODE"]) ?? string.Empty;
                cariDefinition = GetValueOrDefault<string>(row["CDEF"]) ?? string.Empty;
            }

            // 3. ViewModel'i oluştur
            var viewModel = new WorkplaceDetailViewModel
            {
                Id = workplace.Id,
                Code = workplace.Code,
                Definition = workplace.Definition,
                ClCode = cariCode,
                ClDefinition = cariDefinition,
                LogicalRef = workplace.LogicalRef,
                LogRef = workplace.LogRef,
                CustomerType = workplace.CustomerType,
                ResidenceTypeRef = workplace.ResidenceTypeRef,
                ResidenceGroupRef = workplace.ResidenceGroupRef,
                ParcelRef = workplace.ParcelRef,
                PhaseRef = workplace.PhaseRef,
                CauldronRef = workplace.CauldronRef,
                ShareNo = workplace.ShareNo,
                BegDate = workplace.BegDate,
                EndDate = workplace.EndDate,
                BlockRef = workplace.BlockRef,
                IndDivNo = workplace.IndDivNo,
                ResidenceNo = workplace.ResidenceNo,
                DimGross = workplace.DimGross,
                DimField = workplace.DimField,
                PersonCount = workplace.PersonCount,
                WaterMeterNo = workplace.WaterMeterNo,
                CalMeterNo = workplace.CalMeterNo,
                HotWaterMeterNo = workplace.HotWaterMeterNo,
                ChiefReg = workplace.ChiefReg,
                TaxPayer = workplace.TaxPayer,
                IdentityNr = workplace.IdentityNr,
                DeedInfo = workplace.DeedInfo,
                ProfitingOwner = workplace.ProfitingOwner,
                GasCoefficient = workplace.GasCoefficient,
                ActiveResDate = workplace.ActiveResDate,
                BudgetDepotMetre1 = workplace.BudgetDepotMetre1,
                BudgetDepotMetre2 = workplace.BudgetDepotMetre2,
                BudgetGroundMetre = workplace.BudgetGroundMetre,
                BudgetHungMetre = workplace.BudgetHungMetre,
                BudgetFloorMetre = workplace.BudgetFloorMetre,
                BudgetPassageMetre1 = workplace.BudgetPassageMetre1,
                BudgetPassageMetre2 = workplace.BudgetPassageMetre2,
                BudgetDepotCoefficient1 = workplace.BudgetDepotCoefficient1,
                BudgetDepotCoefficient2 = workplace.BudgetDepotCoefficient2,
                BudgetGroundCoefficient = workplace.BudgetGroundCoefficient,
                BudgetHungCoefficient = workplace.BudgetHungCoefficient,
                BudgetFloorCoefficient = workplace.BudgetFloorCoefficient,
                BudgetPassageCoefficient1 = workplace.BudgetPassageCoefficient1,
                BudgetPassageCoefficient2 = workplace.BudgetPassageCoefficient2,
                FuelDepotMetre1 = workplace.FuelDepotMetre1,
                FuelDepotMetre2 = workplace.FuelDepotMetre2,
                FuelGroundMetre = workplace.FuelGroundMetre,
                FuelHungMetre = workplace.FuelHungMetre,
                FuelFloorMetre = workplace.FuelFloorMetre,
                FuelPassageMetre1 = workplace.FuelPassageMetre1,
                FuelPassageMetre2 = workplace.FuelPassageMetre2,
                FuelDepotCoefficient1 = workplace.FuelDepotCoefficient1,
                FuelDepotCoefficient2 = workplace.FuelDepotCoefficient2,
                FuelGroundCoefficient = workplace.FuelGroundCoefficient,
                FuelHungCoefficient = workplace.FuelHungCoefficient,
                FuelFloorCoefficient = workplace.FuelFloorCoefficient,
                FuelPassageCoefficient1 = workplace.FuelPassageCoefficient1,
                FuelPassageCoefficient2 = workplace.FuelPassageCoefficient2,
                TotalBrutCoefficientMetre = workplace.TotalBrutCoefficientMetre,
                TotalNetMetre = workplace.TotalNetMetre,
                TotalFuelMetre = workplace.TotalFuelMetre,
                CariCode = cariCode,
                CariDefinition = cariDefinition
            };

            return ResponseDto<WorkplaceDetailViewModel>.SuccessData(200, "İşyeri kaydı başarıyla getirildi.", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşyeri kaydı getirilirken hata oluştu. ID: {Id}", id);
            return ResponseDto<WorkplaceDetailViewModel>.FailData(500, "İşyeri kaydı getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<WorkplaceDetailViewModel>> GetByCodeAsync(string code)
    {
        try
        {
            // 1. Workplace kaydını kendi veritabanından çek
            var workplace = await _workplaceRepository.GetByCodeAsync(code);
            if (workplace == null)
            {
                return ResponseDto<WorkplaceDetailViewModel>.FailData(404, "İşyeri kaydı bulunamadı.", $"Kod: {code}", false);
            }

            // 2. Cari bilgisini Logo veritabanından çek
            string cariCode = string.Empty;
            string cariDefinition = string.Empty;

            var cariQuery = $"SELECT CCODE, CDEF FROM IS_YERI_MEVCUT_CARI WHERE ICODE = '{workplace.Code}'";
            var cariResult = _sqlProvider.SqlReader(cariQuery);

            if (cariResult.IsSuccess && cariResult.Data != null && cariResult.Data.Rows.Count > 0)
            {
                var row = cariResult.Data.Rows[0];
                cariCode = GetValueOrDefault<string>(row["CCODE"]) ?? string.Empty;
                cariDefinition = GetValueOrDefault<string>(row["CDEF"]) ?? string.Empty;
            }

            // 3. ViewModel'i oluştur
            var viewModel = new WorkplaceDetailViewModel
            {
                Id = workplace.Id,
                Code = workplace.Code,
                Definition = workplace.Definition,
                ClCode = cariCode,
                ClDefinition = cariDefinition,
                LogicalRef = workplace.LogicalRef,
                LogRef = workplace.LogRef,
                CustomerType = workplace.CustomerType,
                ResidenceTypeRef = workplace.ResidenceTypeRef,
                ResidenceGroupRef = workplace.ResidenceGroupRef,
                ParcelRef = workplace.ParcelRef,
                PhaseRef = workplace.PhaseRef,
                CauldronRef = workplace.CauldronRef,
                ShareNo = workplace.ShareNo,
                BegDate = workplace.BegDate,
                EndDate = workplace.EndDate,
                BlockRef = workplace.BlockRef,
                IndDivNo = workplace.IndDivNo,
                ResidenceNo = workplace.ResidenceNo,
                DimGross = workplace.DimGross,
                DimField = workplace.DimField,
                PersonCount = workplace.PersonCount,
                WaterMeterNo = workplace.WaterMeterNo,
                CalMeterNo = workplace.CalMeterNo,
                HotWaterMeterNo = workplace.HotWaterMeterNo,
                ChiefReg = workplace.ChiefReg,
                TaxPayer = workplace.TaxPayer,
                IdentityNr = workplace.IdentityNr,
                DeedInfo = workplace.DeedInfo,
                ProfitingOwner = workplace.ProfitingOwner,
                GasCoefficient = workplace.GasCoefficient,
                ActiveResDate = workplace.ActiveResDate,
                BudgetDepotMetre1 = workplace.BudgetDepotMetre1,
                BudgetDepotMetre2 = workplace.BudgetDepotMetre2,
                BudgetGroundMetre = workplace.BudgetGroundMetre,
                BudgetHungMetre = workplace.BudgetHungMetre,
                BudgetFloorMetre = workplace.BudgetFloorMetre,
                BudgetPassageMetre1 = workplace.BudgetPassageMetre1,
                BudgetPassageMetre2 = workplace.BudgetPassageMetre2,
                BudgetDepotCoefficient1 = workplace.BudgetDepotCoefficient1,
                BudgetDepotCoefficient2 = workplace.BudgetDepotCoefficient2,
                BudgetGroundCoefficient = workplace.BudgetGroundCoefficient,
                BudgetHungCoefficient = workplace.BudgetHungCoefficient,
                BudgetFloorCoefficient = workplace.BudgetFloorCoefficient,
                BudgetPassageCoefficient1 = workplace.BudgetPassageCoefficient1,
                BudgetPassageCoefficient2 = workplace.BudgetPassageCoefficient2,
                FuelDepotMetre1 = workplace.FuelDepotMetre1,
                FuelDepotMetre2 = workplace.FuelDepotMetre2,
                FuelGroundMetre = workplace.FuelGroundMetre,
                FuelHungMetre = workplace.FuelHungMetre,
                FuelFloorMetre = workplace.FuelFloorMetre,
                FuelPassageMetre1 = workplace.FuelPassageMetre1,
                FuelPassageMetre2 = workplace.FuelPassageMetre2,
                FuelDepotCoefficient1 = workplace.FuelDepotCoefficient1,
                FuelDepotCoefficient2 = workplace.FuelDepotCoefficient2,
                FuelGroundCoefficient = workplace.FuelGroundCoefficient,
                FuelHungCoefficient = workplace.FuelHungCoefficient,
                FuelFloorCoefficient = workplace.FuelFloorCoefficient,
                FuelPassageCoefficient1 = workplace.FuelPassageCoefficient1,
                FuelPassageCoefficient2 = workplace.FuelPassageCoefficient2,
                TotalBrutCoefficientMetre = workplace.TotalBrutCoefficientMetre,
                TotalNetMetre = workplace.TotalNetMetre,
                TotalFuelMetre = workplace.TotalFuelMetre,
                CariCode = cariCode,
                CariDefinition = cariDefinition
            };

            return ResponseDto<WorkplaceDetailViewModel>.SuccessData(200, "İşyeri kaydı başarıyla getirildi.", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşyeri kaydı getirilirken hata oluştu. Kod: {Code}", code);
            return ResponseDto<WorkplaceDetailViewModel>.FailData(500, "İşyeri kaydı getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseListDto<List<WorkplaceListViewModel>>> GetPagedListAsync(int start, int length, string? searchValue = null, string? orderColumn = null, bool orderAscending = true)
    {
        try
        {
            // 1. Workplace kayıtlarını kendi veritabanından çek
            var workplaces = await _workplaceRepository.GetAllAsync();
            var workplaceList = workplaces.ToList();

            // 2. Cari bilgilerini Logo veritabanından çek
            var cariQuery = "SELECT ICODE, CCODE, CDEF FROM IS_YERI_MEVCUT_CARI";
            var cariResult = _sqlProvider.SqlReader(cariQuery);

            var cariDict = new Dictionary<string, (string ClCode, string ClDefinition)>();
            if (cariResult.IsSuccess && cariResult.Data != null)
            {
                foreach (DataRow row in cariResult.Data.Rows)
                {
                    var icode = GetValueOrDefault<string>(row["ICODE"]) ?? string.Empty;
                    var ccode = GetValueOrDefault<string>(row["CCODE"]) ?? string.Empty;
                    var cdef = GetValueOrDefault<string>(row["CDEF"]) ?? string.Empty;

                    if (!string.IsNullOrEmpty(icode))
                    {
                        cariDict[icode] = (ccode, cdef);
                    }
                }
            }

            // 3. Verileri birleştir
            var viewModels = workplaceList.Select(w => new WorkplaceListViewModel
            {
                Id = w.Id,
                Code = w.Code,
                Definition = w.Definition,
                ClCode = cariDict.TryGetValue(w.Code, out var cariInfo) ? cariInfo.ClCode : string.Empty,
                ClDefinition = cariDict.TryGetValue(w.Code, out var cariInfo2) ? cariInfo2.ClDefinition : string.Empty,
                LogicalRef = w.LogicalRef,
                LogRef = w.LogRef,
                CustomerType = w.CustomerType,
                BegDate = w.BegDate,
                EndDate = w.EndDate,
                TotalNetMetre = w.TotalNetMetre,
                TotalFuelMetre = w.TotalFuelMetre
            }).ToList();

            // 4. Client-side filtering
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var search = searchValue.ToLower();
                viewModels = viewModels
                    .Where(w => w.Code.ToLower().Contains(search)
                              || (w.Definition != null && w.Definition.ToLower().Contains(search))
                              || (w.ClCode != null && w.ClCode.ToLower().Contains(search))
                              || (w.ClDefinition != null && w.ClDefinition.ToLower().Contains(search)))
                    .ToList();
            }

            // 5. Client-side sorting
            if (!string.IsNullOrEmpty(orderColumn))
            {
                switch (orderColumn.ToLower())
                {
                    case "code":
                        viewModels = orderAscending
                            ? viewModels.OrderBy(w => w.Code).ToList()
                            : viewModels.OrderByDescending(w => w.Code).ToList();
                        break;
                    case "definition":
                        viewModels = orderAscending
                            ? viewModels.OrderBy(w => w.Definition).ToList()
                            : viewModels.OrderByDescending(w => w.Definition).ToList();
                        break;
                    case "clcode":
                        viewModels = orderAscending
                            ? viewModels.OrderBy(w => w.ClCode).ToList()
                            : viewModels.OrderByDescending(w => w.ClCode).ToList();
                        break;
                    case "cldefinition":
                        viewModels = orderAscending
                            ? viewModels.OrderBy(w => w.ClDefinition).ToList()
                            : viewModels.OrderByDescending(w => w.ClDefinition).ToList();
                        break;
                    default:
                        viewModels = viewModels.OrderBy(w => w.Code).ToList();
                        break;
                }
            }

            var recordsTotal = workplaceList.Count;
            var recordsFiltered = viewModels.Count;

            // 6. Client-side paging
            var pagedViewModels = viewModels.Skip(start).Take(length).ToList();

            return ResponseListDto<List<WorkplaceListViewModel>>.SuccessData(
                200,
                "İşyerleri listesi başarıyla getirildi.",
                pagedViewModels,
                recordsTotal,
                recordsFiltered,
                pagedViewModels.Count
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşyerleri listesi getirilirken hata oluştu");
            return ResponseListDto<List<WorkplaceListViewModel>>.FailData(500, "İşyerleri listesi getirme başarısız.", ex.Message, true);
        }
    }

    private static T GetValueOrDefault<T>(object value)
    {
        if (value == DBNull.Value || value == null)
            return default(T)!;

        try
        {
            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return (T)Convert.ChangeType(value, underlyingType!);
            }

            return (T)Convert.ChangeType(value, type);
        }
        catch
        {
            return default(T)!;
        }
    }
}
