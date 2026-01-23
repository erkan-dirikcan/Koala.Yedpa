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
    private readonly IApiLogoSqlDataService _apiLogoSqlDataService;
    private readonly string? _currentUserId;
    public LogoSettingViewModel LogoSetting { get; set; }
    public LogoSqlSettingViewModel LogoSqlSetting { get; set; }
    public WorkplaceService(
        IWorkplaceRepository workplaceRepository,
        IMapper mapper,
        IUnitOfWork<AppDbContext> unitOfWork,
        ISqlProvider sqlProvider,
        ILogger<WorkplaceService> logger,
        IApiLogoSqlDataService apiLogoSqlDataService,
        ISettingsService settingsService,
        IHttpContextAccessor? httpContextAccessor = null  )
    {
        _workplaceRepository = workplaceRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _sqlProvider = sqlProvider;
        _context = unitOfWork.Context;
        _logger = logger;
        _apiLogoSqlDataService = apiLogoSqlDataService;
        _currentUserId = httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? null;

        var logoSettingResp = settingsService.GetLogoSettingsAsync().Result;
        if (logoSettingResp.IsSuccess)
        {
            LogoSetting = logoSettingResp.Data;
        }
        var logoSqlSettingResp = settingsService.GetLogoSqlSettingsAsync().Result;
        if (logoSqlSettingResp.IsSuccess)
        {
            LogoSqlSetting = logoSqlSettingResp.Data;
        }
    }

    public async Task<ResponseDto<List<WorkplaceListViewModel>>> GetAllAsync()
    {
        try
        {
            // 1. Workplace kayıtlarını kendi veritabanından çek
            var workplaces = await _workplaceRepository.GetAllAsync();
            var workplaceList = workplaces.ToList();

            // 2. Cari bilgilerini Logo veritabanından çek
            var cariQuery = $@"SELECT WP.CODE AS WPCODE,WP.DEFINITION_ AS WPADDRESS,CL.LOGICALREF AS CLREFFERANCE ,CL.CODE AS CLCODE,CL.DEFINITION_ AS CLCTITLE,CL.EMAILADDR AS EMAILADDR
                        FROM LG_{LogoSetting.Firm}_CLCARD AS CL
                        INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS WP ON WP.LOGICALREF=CL.PARENTCLREF
                        WHERE CL.SPECODE NOT IN ('KIRMIZI','MAVİ','YEŞİL')
                          AND LEFT(TRIM(CL.CODE),1)='1' AND CL.ACTIVE=0
                        ORDER BY WP.CODE";

            var cariResult = _sqlProvider.SqlReader(cariQuery);

            // Her işyeri kodu için birden fazla cari hesap olabilir
            var cariDict = new Dictionary<string, List<(int LogicalRef, string Code, string Definition, string EmailAddress)>>();
            if (cariResult.IsSuccess && cariResult.Data != null)
            {
                foreach (DataRow row in cariResult.Data.Rows)
                {
                    var logicalRef = row["CLREFFERANCE"] != DBNull.Value ? Convert.ToInt32(row["CLREFFERANCE"]) : 0;
                    var wpCode = row["WPCODE"]?.ToString() ?? string.Empty;
                    var code = row["CLCODE"]?.ToString() ?? string.Empty;
                    var definition = row["CLCTITLE"]?.ToString() ?? string.Empty;
                    var emailAddress = row["EMAILADDR"]?.ToString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(wpCode))
                    {
                        if (!cariDict.ContainsKey(wpCode))
                        {
                            cariDict[wpCode] = new List<(int LogicalRef, string Code, string Definition, string EmailAddress)>();
                        }
                        cariDict[wpCode].Add((logicalRef, code, definition, emailAddress));
                    }
                }
            }

            // 3. Current Accounts bilgilerini Logo'dan çek
            var currentAccountsResult = await _apiLogoSqlDataService.GetWorkplaceCurrentAccountsAsync();
            var currentAccountsDict = currentAccountsResult.IsSuccess && currentAccountsResult.Data != null
                ? currentAccountsResult.Data
                : new Dictionary<string, List<WorkplaceCurrentAccounts>>();

            // 4. Verileri birleştir
            var viewModels = workplaceList.Select(w =>
            {
                // İlk cari hesabı al (varsa)
                var firstCari = cariDict.TryGetValue(w.Code, out var cariList) && cariList.Any()
                    ? cariList.First()
                    : (LogicalRef: 0, Code: string.Empty, Definition: string.Empty, EmailAddress: string.Empty);

                return new WorkplaceListViewModel
                {
                    Id = w.Id,
                    Code = w.Code,
                    Definition = w.Definition,
                    ClCode = firstCari.Code,
                    ClDefinition = firstCari.Definition,
                    LogicalRef = w.LogicalRef,
                    LogRef = w.LogRef,
                    CustomerType = w.CustomerType,
                    BegDate = w.BegDate,
                    EndDate = w.EndDate,
                    TotalNetMetre = w.TotalNetMetre,
                    TotalFuelMetre = w.TotalFuelMetre,
                    CurrentAccounts = currentAccountsDict.TryGetValue(w.Code, out var accounts) ? accounts : new List<WorkplaceCurrentAccounts>()
                };
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

            var cariQuery = $@"SELECT TOP 1 WP.CODE AS WPCODE,WP.DEFINITION_ AS WPADDRESS,CL.LOGICALREF AS CLREFFERANCE ,CL.CODE AS CLCODE,CL.DEFINITION_ AS CLCTITLE,CL.EMAILADDR AS EMAILADDR
                        FROM LG_{LogoSetting.Firm}_CLCARD AS CL
                        INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS WP ON WP.LOGICALREF=CL.PARENTCLREF
                        WHERE WP.CODE = '{workplace.Code.Replace("'", "''")}'
                          AND CL.SPECODE NOT IN ('KIRMIZI','MAVİ','YEŞİL')
                          AND LEFT(TRIM(CL.CODE),1)='1'
                          AND CL.ACTIVE=0
                        ORDER BY WP.CODE";

            var cariResult = _sqlProvider.SqlReader(cariQuery);

            if (cariResult.IsSuccess && cariResult.Data != null && cariResult.Data.Rows.Count > 0)
            {
                var row = cariResult.Data.Rows[0];
                cariCode = row["CLCODE"]?.ToString() ?? string.Empty;
                cariDefinition = row["CLCTITLE"]?.ToString() ?? string.Empty;
            }

            // 3. Current Accounts bilgilerini Logo'dan çek
            var currentAccountsResult = await _apiLogoSqlDataService.GetWorkplaceCurrentAccountsAsync();
            var currentAccountsList = currentAccountsResult.IsSuccess && currentAccountsResult.Data != null &&
                                      currentAccountsResult.Data.TryGetValue(workplace.Code, out var accounts)
                ? accounts
                : new List<WorkplaceCurrentAccounts>();

            // 4. ViewModel'i oluştur
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
                CariDefinition = cariDefinition,
                CurrentAccounts = currentAccountsList
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

            var cariQuery = $@"SELECT TOP 1 WP.CODE AS WPCODE,WP.DEFINITION_ AS WPADDRESS,CL.LOGICALREF AS CLREFFERANCE ,CL.CODE AS CLCODE,CL.DEFINITION_ AS CLCTITLE,CL.EMAILADDR AS EMAILADDR
                        FROM LG_{LogoSetting.Firm}_CLCARD AS CL
                        INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS WP ON WP.LOGICALREF=CL.PARENTCLREF
                        WHERE WP.CODE = '{workplace.Code.Replace("'", "''")}'
                          AND CL.SPECODE NOT IN ('KIRMIZI','MAVİ','YEŞİL')
                          AND LEFT(TRIM(CL.CODE),1)='1'
                          AND CL.ACTIVE=0
                        ORDER BY WP.CODE";

            var cariResult = _sqlProvider.SqlReader(cariQuery);

            if (cariResult.IsSuccess && cariResult.Data != null && cariResult.Data.Rows.Count > 0)
            {
                var row = cariResult.Data.Rows[0];
                cariCode = row["CLCODE"]?.ToString() ?? string.Empty;
                cariDefinition = row["CLCTITLE"]?.ToString() ?? string.Empty;
            }

            // 3. Current Accounts bilgilerini Logo'dan çek
            var currentAccountsResult = await _apiLogoSqlDataService.GetWorkplaceCurrentAccountsAsync();
            var currentAccountsList = currentAccountsResult.IsSuccess && currentAccountsResult.Data != null &&
                                      currentAccountsResult.Data.TryGetValue(workplace.Code, out var accounts)
                ? accounts
                : new List<WorkplaceCurrentAccounts>();

            // 4. ViewModel'i oluştur
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
                CariDefinition = cariDefinition,
                CurrentAccounts = currentAccountsList
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
            var cariQuery = $@"SELECT WP.CODE AS WPCODE,WP.DEFINITION_ AS WPADDRESS,CL.LOGICALREF AS CLREFFERANCE ,CL.CODE AS CLCODE,CL.DEFINITION_ AS CLCTITLE,CL.EMAILADDR AS EMAILADDR
                        FROM LG_{LogoSetting.Firm}_CLCARD AS CL
                        INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS WP ON WP.LOGICALREF=CL.PARENTCLREF
                        WHERE CL.SPECODE NOT IN ('KIRMIZI','MAVİ','YEŞİL')
                          AND LEFT(TRIM(CL.CODE),1)='1'
                          AND CL.ACTIVE=0
                        ORDER BY WP.CODE";

            var cariResult = _sqlProvider.SqlReader(cariQuery);

            // Her işyeri kodu için birden fazla cari hesap olabilir
            var cariDict = new Dictionary<string, List<(int LogicalRef, string Code, string Definition, string EmailAddress)>>();
            if (cariResult.IsSuccess && cariResult.Data != null)
            {
                foreach (DataRow row in cariResult.Data.Rows)
                {
                    var logicalRef = row["CLREFFERANCE"] != DBNull.Value ? Convert.ToInt32(row["CLREFFERANCE"]) : 0;
                    var wpCode = row["WPCODE"]?.ToString() ?? string.Empty;
                    var code = row["CLCODE"]?.ToString() ?? string.Empty;
                    var definition = row["CLCTITLE"]?.ToString() ?? string.Empty;
                    var emailAddress = row["EMAILADDR"]?.ToString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(wpCode))
                    {
                        if (!cariDict.ContainsKey(wpCode))
                        {
                            cariDict[wpCode] = new List<(int LogicalRef, string Code, string Definition, string EmailAddress)>();
                        }
                        cariDict[wpCode].Add((logicalRef, code, definition, emailAddress));
                    }
                }
            }

            // 3. Current Accounts bilgilerini Logo'dan çek
            var currentAccountsResult = await _apiLogoSqlDataService.GetWorkplaceCurrentAccountsAsync();
            var currentAccountsDict = currentAccountsResult.IsSuccess && currentAccountsResult.Data != null
                ? currentAccountsResult.Data
                : new Dictionary<string, List<WorkplaceCurrentAccounts>>();

            // 4. Verileri birleştir
            var viewModels = workplaceList.Select(w =>
            {
                // İlk cari hesabı al (varsa)
                var firstCari = cariDict.TryGetValue(w.Code, out var cariList) && cariList.Any()
                    ? cariList.First()
                    : (LogicalRef: 0, Code: string.Empty, Definition: string.Empty, EmailAddress: string.Empty);

                return new WorkplaceListViewModel
                {
                    Id = w.Id,
                    Code = w.Code,
                    Definition = w.Definition,
                    ClCode = firstCari.Code,
                    ClDefinition = firstCari.Definition,
                    LogicalRef = w.LogicalRef,
                    LogRef = w.LogRef,
                    CustomerType = w.CustomerType,
                    BegDate = w.BegDate,
                    EndDate = w.EndDate,
                    TotalNetMetre = w.TotalNetMetre,
                    TotalFuelMetre = w.TotalFuelMetre,
                    CurrentAccounts = currentAccountsDict.TryGetValue(w.Code, out var accounts) ? accounts : new List<WorkplaceCurrentAccounts>()
                };
            }).ToList();

            // 5. Client-side filtering
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

            // 6. Client-side sorting
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

            // 7. Client-side paging
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

    public async Task<ResponseDto<WorkplaceDetailViewModel>> UpdateAsync(WorkplaceDetailViewModel model)
    {
        try
        {
            _logger.LogDebug("Updating Workplace: {Id}", model.Id);

            var workplace = await _workplaceRepository.GetByIdAsync(model.Id);
            if (workplace == null)
            {
                _logger.LogWarning("Workplace not found for update: {Id}", model.Id);
                return ResponseDto<WorkplaceDetailViewModel>.FailData(
                    404,
                    "İşyeri bulunamadı",
                    $"ID: {model.Id}",
                    false
                );
            }

            // Update entity from ViewModel
            workplace.CustomerType = model.CustomerType;
            workplace.ResidenceTypeRef = model.ResidenceTypeRef;
            workplace.ResidenceGroupRef = model.ResidenceGroupRef;
            workplace.ParcelRef = model.ParcelRef;
            workplace.PhaseRef = model.PhaseRef;
            workplace.CauldronRef = model.CauldronRef;
            workplace.ShareNo = model.ShareNo;
            workplace.BegDate = model.BegDate;
            workplace.EndDate = model.EndDate;
            workplace.BlockRef = model.BlockRef;
            workplace.IndDivNo = model.IndDivNo;
            workplace.ResidenceNo = model.ResidenceNo;
            workplace.DimGross = model.DimGross;
            workplace.DimField = model.DimField;
            workplace.PersonCount = model.PersonCount;
            workplace.WaterMeterNo = model.WaterMeterNo;
            workplace.CalMeterNo = model.CalMeterNo;
            workplace.HotWaterMeterNo = model.HotWaterMeterNo;
            workplace.ChiefReg = model.ChiefReg;
            workplace.TaxPayer = model.TaxPayer;
            workplace.IdentityNr = model.IdentityNr;
            workplace.DeedInfo = model.DeedInfo;
            workplace.ProfitingOwner = model.ProfitingOwner;
            workplace.GasCoefficient = model.GasCoefficient;
            workplace.ActiveResDate = model.ActiveResDate;
            workplace.BudgetDepotMetre1 = model.BudgetDepotMetre1;
            workplace.BudgetDepotMetre2 = model.BudgetDepotMetre2;
            workplace.BudgetGroundMetre = model.BudgetGroundMetre;
            workplace.BudgetHungMetre = model.BudgetHungMetre;
            workplace.BudgetFloorMetre = model.BudgetFloorMetre;
            workplace.BudgetPassageMetre1 = model.BudgetPassageMetre1;
            workplace.BudgetPassageMetre2 = model.BudgetPassageMetre2;
            workplace.BudgetDepotCoefficient1 = model.BudgetDepotCoefficient1;
            workplace.BudgetDepotCoefficient2 = model.BudgetDepotCoefficient2;
            workplace.BudgetGroundCoefficient = model.BudgetGroundCoefficient;
            workplace.BudgetHungCoefficient = model.BudgetHungCoefficient;
            workplace.BudgetFloorCoefficient = model.BudgetFloorCoefficient;
            workplace.BudgetPassageCoefficient1 = model.BudgetPassageCoefficient1;
            workplace.BudgetPassageCoefficient2 = model.BudgetPassageCoefficient2;
            workplace.FuelDepotMetre1 = model.FuelDepotMetre1;
            workplace.FuelDepotMetre2 = model.FuelDepotMetre2;
            workplace.FuelGroundMetre = model.FuelGroundMetre;
            workplace.FuelHungMetre = model.FuelHungMetre;
            workplace.FuelFloorMetre = model.FuelFloorMetre;
            workplace.FuelPassageMetre1 = model.FuelPassageMetre1;
            workplace.FuelPassageMetre2 = model.FuelPassageMetre2;
            workplace.FuelDepotCoefficient1 = model.FuelDepotCoefficient1;
            workplace.FuelDepotCoefficient2 = model.FuelDepotCoefficient2;
            workplace.FuelGroundCoefficient = model.FuelGroundCoefficient;
            workplace.FuelHungCoefficient = model.FuelHungCoefficient;
            workplace.FuelFloorCoefficient = model.FuelFloorCoefficient;
            workplace.FuelPassageCoefficient1 = model.FuelPassageCoefficient1;
            workplace.FuelPassageCoefficient2 = model.FuelPassageCoefficient2;
            workplace.TotalBrutCoefficientMetre = model.TotalBrutCoefficientMetre;
            workplace.TotalNetMetre = model.TotalNetMetre;
            workplace.TotalFuelMetre = model.TotalFuelMetre;

            await _workplaceRepository.UpdateAsync(workplace);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Workplace updated successfully: {Id}", model.Id);

            // Return updated model
            var result = await GetByIdAsync(model.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Workplace: {Id}", model.Id);
            return ResponseDto<WorkplaceDetailViewModel>.FailData(
                500,
                "İşyeri güncelleme başarısız",
                ex.Message,
                true
            );
        }
    }

    private T? GetValueOrDefault<T>(object value) where T : struct
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
