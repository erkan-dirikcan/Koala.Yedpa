using AutoMapper;
using ClosedXML.Excel;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Globalization;
using System.Text.RegularExpressions;

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
    private readonly IMessage34EmailService _message34EmailService;
    private readonly IDuesStatisticService _duesStatisticService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IQRCodeService _qrCodeService;
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
        IMessage34EmailService message34EmailService,
        IDuesStatisticService duesStatisticService,
        IEmailTemplateService emailTemplateService,
        ISettingsService settingsService,
        IWebHostEnvironment webHostEnvironment,
        IQRCodeService qrCodeService,
        IHttpContextAccessor? httpContextAccessor = null  )
    {
        _workplaceRepository = workplaceRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _sqlProvider = sqlProvider;
        _context = unitOfWork.Context;
        _logger = logger;
        _apiLogoSqlDataService = apiLogoSqlDataService;
        _message34EmailService = message34EmailService;
        _duesStatisticService = duesStatisticService;
        _emailTemplateService = emailTemplateService;
        _webHostEnvironment = webHostEnvironment;
        _qrCodeService = qrCodeService;
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
                CurrentAccounts = currentAccountsList,
                QRCodeNumber = workplace.QRCodeNumber,
                QRCodeImagePath = workplace.QRCodeImagePath,
                QRCodeUrl = !string.IsNullOrEmpty(workplace.QRCodeImagePath) ? $"/{workplace.QRCodeImagePath.TrimStart('/')}" : null,
                QRCodeGeneratedDate = workplace.QRCodeGeneratedDate
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
                CurrentAccounts = currentAccountsList,
                QRCodeNumber = workplace.QRCodeNumber,
                QRCodeImagePath = workplace.QRCodeImagePath,
                QRCodeUrl = !string.IsNullOrEmpty(workplace.QRCodeImagePath) ? $"/{workplace.QRCodeImagePath.TrimStart('/')}" : null,
                QRCodeGeneratedDate = workplace.QRCodeGeneratedDate
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

    public async Task<ResponseDto<string>> GenerateQRCodeForWorkplaceAsync(string id)
    {
        try
        {
            var workplace = await _workplaceRepository.GetByIdAsync(id);
            if (workplace == null)
            {
                return ResponseDto<string>.FailData(404, "İşyeri bulunamadı", $"ID: {id}", false);
            }

            // Get partner number from current account (first one)
            var currentAccountsResult = await _apiLogoSqlDataService.GetWorkplaceCurrentAccountsAsync();
            if (!currentAccountsResult.IsSuccess || currentAccountsResult.Data == null)
            {
                return ResponseDto<string>.FailData(500, "Cari hesaplar alınamadı", "Current accounts bilgisi bulunamadı", true);
            }

            var currentAccounts = currentAccountsResult.Data.TryGetValue(workplace.Code, out var accounts)
                ? accounts
                : new List<WorkplaceCurrentAccounts>();

            if (!currentAccounts.Any())
            {
                return ResponseDto<string>.FailData(404, "Cari hesap bulunamadı", $"İşyeri kodu: {workplace.Code}", false);
            }

            // Use LogicalRef of first current account as PartnerNo
            var partnerNo = currentAccounts.First().LogicalRef.ToString();
            var qrNumber = $"G11522-Yd{partnerNo}";

            // Generate and save QR code
            var qrResult = await _qrCodeService.GenerateAndSaveQRCodeAsync(workplace.Code, partnerNo);

            if (!qrResult.IsSuccess)
            {
                return qrResult;
            }

            // Update workplace with QR code info
            workplace.QRCodeNumber = qrNumber;
            workplace.QRCodeImagePath = qrResult.Data;
            workplace.QRCodeGeneratedDate = DateTime.Now;

            await _workplaceRepository.UpdateAsync(workplace);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("QR kod oluşturuldu ve işyerine kaydedildi. Workplace: {WorkplaceCode}", workplace.Code);

            return ResponseDto<string>.SuccessData(200, "QR kod başarıyla oluşturuldu", qrResult.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kod oluşturulurken hata oluştu. WorkplaceId: {WorkplaceId}", id);
            return ResponseDto<string>.FailData(500, "QR kod oluşturma başarısız", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<string>>> GenerateBulkQRCodesAsync()
    {
        try
        {
            var workplaces = await _workplaceRepository.GetAllAsync();
            var generatedPaths = new List<string>();

            foreach (var workplace in workplaces)
            {
                if (!string.IsNullOrEmpty(workplace.QRCodeNumber))
                {
                    continue; // Skip if already has QR code
                }

                var result = await GenerateQRCodeForWorkplaceAsync(workplace.Id);
                if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
                {
                    generatedPaths.Add(result.Data);
                }
            }

            _logger.LogInformation("Toplu QR kod oluşturma tamamlandı. {Count} adet QR kod oluşturuldu", generatedPaths.Count);
            return ResponseDto<List<string>>.SuccessData(200, $"Toplu QR kod oluşturma tamamlandı. {generatedPaths.Count} QR kod oluşturuldu", generatedPaths);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu QR kod oluşturulurken hata oluştu");
            return ResponseDto<List<string>>.FailData(500, "Toplu QR kod oluşturma başarısız", ex.Message, true);
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

    public async Task<ResponseDto<WorkplaceBulkEmailResultViewModel>> SendBulkBudgetEmailsAsync(int year)
    {
        try
        {
            _logger.LogInformation("Starting bulk budget email sending for year: {Year}", year);

            var result = new WorkplaceBulkEmailResultViewModel();
            var yearString = year.ToString();

            // 1. Email template'ini al ("BuggetOrder")
            var templateResult = await _emailTemplateService.GetByNameAsyc("BuggetOrder");
            if (!templateResult.IsSuccess || templateResult.Data == null)
            {
                return ResponseDto<WorkplaceBulkEmailResultViewModel>.FailData(500, "Email template bulunamadı", "'BuggetOrder' adlı email template bulunamadı", true);
            }

            var emailTemplate = templateResult.Data.Content;

            // 2. Tüm işyerlerini al
            var workplacesResponse = await GetAllAsync();
            if (!workplacesResponse.IsSuccess || workplacesResponse.Data == null)
            {
                return ResponseDto<WorkplaceBulkEmailResultViewModel>.FailData(500, "İşyerleri alınamadı", "İşyerleri listesi getirilemedi", true);
            }

            var workplaces = workplacesResponse.Data;
            result.TotalWorkplaces = workplaces.Count;

            // 3. Cari bilgilerini Logo veritabanından çek (Child row'da kullanılan SQL sorgusu ile aynı)
            var cariQuery = $@"SELECT WP.CODE AS WPCODE,WP.DEFINITION_ AS WPADDRESS,CL.LOGICALREF AS CLREFFERANCE ,CL.CODE AS CLCODE,CL.DEFINITION_ AS CLCTITLE,CL.EMAILADDR AS EMAILADDR
                        FROM LG_{LogoSetting.Firm}_CLCARD AS CL
                        INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS WP ON WP.LOGICALREF=CL.PARENTCLREF
                        WHERE CL.SPECODE NOT IN ('KIRMIZI','MAVİ','YEŞİL')
                          AND LEFT(TRIM(CL.CODE),1)='1' AND CL.ACTIVE=0
                        ORDER BY WP.CODE";

            var cariResult = _sqlProvider.SqlReader(cariQuery);

            // Her işyeri kodu için birden fazla cari hesap olabilir
            var cariDict = new Dictionary<string, List<WorkplaceCurrentAccounts>>();
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
                            cariDict[wpCode] = new List<WorkplaceCurrentAccounts>();
                        }
                        cariDict[wpCode].Add(new WorkplaceCurrentAccounts
                        {
                            LogicalRef = logicalRef,
                            Code = code,
                            Definition = definition,
                            EmailAddress = emailAddress
                        });
                    }
                }
            }

            // 4. Tüm DuesStatistics verilerini al (DivCode ile eşleştirmek için)
            var duesStatsResponse = await _duesStatisticService.GetByYearAsync(yearString);
            if (!duesStatsResponse.IsSuccess || duesStatsResponse.Data == null)
            {
                return ResponseDto<WorkplaceBulkEmailResultViewModel>.FailData(500, "Dues verileri alınamadı", $"{year} yılına ait bütçe verileri bulunamadı", true);
            }

            // DivCode -> DuesStatistic map oluştur
            var duesStatsDict = duesStatsResponse.Data
                .GroupBy(d => d.DivCode)
                .ToDictionary(g => g.Key, g => g.First());

            // 5. Her işyeri için cari hesaplarına mail gönder
            foreach (var workplace in workplaces)
            {
                // İlgili işyeri için DuesStatistic bul
                if (!duesStatsDict.TryGetValue(workplace.Code, out var duesStatistic))
                {
                    _logger.LogWarning("DuesStatistic not found for workplace code: {WorkplaceCode}", workplace.Code);
                    continue;
                }

                // SQL'den gelen cari hesapları kontrol et
                if (!cariDict.TryGetValue(workplace.Code, out var currentAccounts) || !currentAccounts.Any())
                {
                    _logger.LogWarning("No current accounts found for workplace: {WorkplaceCode}", workplace.Code);
                    continue;
                }

                // Her cari hesap için mail gönder
                foreach (var currentAccount in currentAccounts)
                {
                    // E-posta filtreleme: null, boş, geçersiz format, a@a.com
                    if (string.IsNullOrWhiteSpace(currentAccount.EmailAddress) ||
                        !IsValidEmail(currentAccount.EmailAddress))
                    {
                        var invalidResult = new WorkplaceEmailResult
                        {
                            WorkplaceCode = workplace.Code,
                            WorkplaceName = workplace.Definition,
                            CurrentAccountName = currentAccount.Definition,
                            EmailAddress = currentAccount.EmailAddress ?? "BOŞ",
                            IsSuccess = false,
                            ErrorMessage = "Geçersiz e-posta adresi"
                        };
                        result.FailedEmails.Add(invalidResult);
                        result.TotalEmailsFailed++;
                        _logger.LogWarning("Invalid email address for current account: {Code} - {Email}",
                            currentAccount.Code, currentAccount.EmailAddress ?? "BOŞ");
                        continue;
                    }

                    try
                    {
                        // HTML tablo formatında ödeme planı oluştur
                        var bodyHtml = BuildEmailBodyHtml(workplace.Definition, duesStatistic);

                        // Template değişkenlerini replace et
                        var content = emailTemplate
                            .Replace("[[Name]]", currentAccount.Definition.ToUpper())
                            .Replace("[[Body]]", bodyHtml);

                        var emailDto = new EmailDto
                        {
                            Email = currentAccount.EmailAddress,
                            Title = $"Bütçe Ödeme Planı - {year} - {workplace.Definition}",
                            Content = content,
                            FromName = "Yedpa Ticaret Merkezi",
                            FromEmail = "tahsilat@e.yedpa.com.tr",
                            ReplyEmail = "tahsilat@yedpa.com.tr"
                        };

                        // Mail gönder
                        var emailResponse = await _message34EmailService.SendTransactionEmailAsync(emailDto);

                        var emailResult = new WorkplaceEmailResult
                        {
                            WorkplaceCode = workplace.Code,
                            WorkplaceName = workplace.Definition,
                            CurrentAccountName = currentAccount.Definition,
                            EmailAddress = currentAccount.EmailAddress
                        };

                        if (emailResponse.IsSuccess)
                        {
                            emailResult.IsSuccess = true;
                            result.SuccessfulEmails.Add(emailResult);
                            result.TotalEmailsSent++;
                            _logger.LogInformation("Email sent successfully to {Email} for workplace {WorkplaceCode}", currentAccount.EmailAddress, workplace.Code);
                        }
                        else
                        {
                            emailResult.IsSuccess = false;
                            emailResult.ErrorMessage = emailResponse.Message;
                            result.FailedEmails.Add(emailResult);
                            result.TotalEmailsFailed++;
                            _logger.LogError("Failed to send email to {Email} for workplace {WorkplaceCode}: {Error}",
                                currentAccount.EmailAddress, workplace.Code, emailResponse.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        var emailResult = new WorkplaceEmailResult
                        {
                            WorkplaceCode = workplace.Code,
                            WorkplaceName = workplace.Definition,
                            CurrentAccountName = currentAccount.Definition,
                            EmailAddress = currentAccount.EmailAddress,
                            IsSuccess = false,
                            ErrorMessage = ex.Message
                        };
                        result.FailedEmails.Add(emailResult);
                        result.TotalEmailsFailed++;
                        _logger.LogError(ex, "Error sending email to {Email} for workplace {WorkplaceCode}",
                            currentAccount.EmailAddress, workplace.Code);
                    }
                }
            }

            _logger.LogInformation("Bulk email sending completed. Sent: {Sent}, Failed: {Failed}",
                result.TotalEmailsSent, result.TotalEmailsFailed);

            // Excel raporu oluştur ve özet mail gönder
            await GenerateExcelReportAndSendSummaryEmail(result, year);

            return ResponseDto<WorkplaceBulkEmailResultViewModel>.SuccessData(200,
                $"Toplu mail gönderimi tamamlandı. Başarılı: {result.TotalEmailsSent}, Başarısız: {result.TotalEmailsFailed}",
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk budget emails for year: {Year}", year);
            return ResponseDto<WorkplaceBulkEmailResultViewModel>.FailData(500, "Toplu mail gönderimi başarısız", ex.Message, true);
        }
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // a@a.com kontrolü
        if (email.Trim().ToLower() == "a@a.com")
            return false;

        // Basit e-posta format kontrolü (regex)
        try
        {
            // E-posta pattern: en az bir @, domain kısmında en az bir .
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email.Trim(), pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private async Task GenerateExcelReportAndSendSummaryEmail(WorkplaceBulkEmailResultViewModel result, int year)
    {
        try
        {
            _logger.LogInformation("Generating Excel report and sending summary email");

            // Excel oluştur
            using (var workbook = new XLWorkbook())
            {
                // Sheet 1: Gönderilenler
                var sentSheet = workbook.Worksheets.Add("Gönderilenler");
                sentSheet.Cell("A1").Value = "İşyeri Kodu";
                sentSheet.Cell("B1").Value = "İşyeri Adı";
                sentSheet.Cell("C1").Value = "Cari Hesap Adı";
                sentSheet.Cell("D1").Value = "E-Posta Adresi";

                // Header stil
                var headerRange = sentSheet.Range(1, 1, 1, 4);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#011560");
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Verileri doldur
                int row = 2;
                foreach (var email in result.SuccessfulEmails)
                {
                    sentSheet.Cell(row, 1).Value = email.WorkplaceCode;
                    sentSheet.Cell(row, 2).Value = email.WorkplaceName;
                    sentSheet.Cell(row, 3).Value = email.CurrentAccountName;
                    sentSheet.Cell(row, 4).Value = email.EmailAddress;
                    row++;
                }

                // Sheet 2: Gönderilmeyenler
                var failedSheet = workbook.Worksheets.Add("Gönderilmeyenler");
                failedSheet.Cell("A1").Value = "İşyeri Kodu";
                failedSheet.Cell("B1").Value = "İşyeri Adı";
                failedSheet.Cell("C1").Value = "Cari Hesap Adı";
                failedSheet.Cell("D1").Value = "E-Posta Adresi";
                failedSheet.Cell("E1").Value = "Hata Mesajı";

                // Header stil
                var failedHeaderRange = failedSheet.Range(1, 1, 1, 5);
                failedHeaderRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#dc3545");
                failedHeaderRange.Style.Font.FontColor = XLColor.White;
                failedHeaderRange.Style.Font.Bold = true;

                // Verileri doldur
                row = 2;
                foreach (var email in result.FailedEmails)
                {
                    failedSheet.Cell(row, 1).Value = email.WorkplaceCode;
                    failedSheet.Cell(row, 2).Value = email.WorkplaceName;
                    failedSheet.Cell(row, 3).Value = email.CurrentAccountName;
                    failedSheet.Cell(row, 4).Value = email.EmailAddress;
                    failedSheet.Cell(row, 5).Value = email.ErrorMessage ?? "Bilinmeyen hata";
                    row++;
                }

                // Auto-fit columns
                sentSheet.Columns().AdjustToContents();
                failedSheet.Columns().AdjustToContents();

                // Excel'i byte array olarak kaydet
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var excelBytes = stream.ToArray();

                    // Mail içeriğini oluştur
                    var bodyHtml = $@"
<p>Bütçe ödeme planı mail gönderimi tamamlandı.</p>

<table border=""1"" cellpadding=""5"" cellspacing=""0"" style=""border-collapse: collapse; width: 100%; max-width: 600px; margin-top: 10px; font-size: 13px;"">
    <tr style=""background-color: #011560; color: white; font-weight: bold;"">
        <td style=""padding: 8px;"">Toplam İşyeri</td>
        <td style=""padding: 8px; text-align: right;"">{result.TotalWorkplaces}</td>
    </tr>
    <tr style=""background-color: #28a745; color: white; font-weight: bold;"">
        <td style=""padding: 8px;"">Başarılı Gönderim</td>
        <td style=""padding: 8px; text-align: right;"">{result.TotalEmailsSent}</td>
    </tr>
    <tr style=""background-color: #dc3545; color: white; font-weight: bold;"">
        <td style=""padding: 8px;"">Başarısız Gönderim</td>
        <td style=""padding: 8px; text-align: right;"">{result.TotalEmailsFailed}</td>
    </tr>
</table>

<p style=""margin-top: 15px;"">Detaylı rapor ekteki Excel dosyasındadır.</p>
<p>Yıl: {year}</p>";

                    // Default template'ini al
                    var templateResult = await _emailTemplateService.GetByNameAsyc("Default");
                    var emailTemplate = templateResult.IsSuccess && templateResult.Data != null
                        ? templateResult.Data.Content
                        : "[[Body]]";

                    // Template değişkenlerini replace et
                    var content = emailTemplate.Replace("[[Name]]", "Yönetici")
                                             .Replace("[[Body]]", bodyHtml);

                    // EmailDto oluştur (Excel attachment ile)
                    var emailDto = new EmailDto
                    {
                        Email = "erkan@sistem-bilgisayar.com.tr",
                        Title = $"Bütçe Maili Gönderim Raporu - {year}",
                        Content = content,
                        FromName = "Yedpa Ticaret Merkezi",
                        FromEmail = "tahsilat@e.yedpa.com.tr",
                        Attachments = new List<EmailAttachmentDto>
                        {
                            new EmailAttachmentDto
                            {
                                FileName = $"ButceMailRaporu_{year}.xlsx",
                                Content = excelBytes,
                                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                            }
                        }
                    };

                    // Mail gönder
                    await _message34EmailService.SendTransactionEmailAsync(emailDto);

                    _logger.LogInformation("Summary email sent to erkan@sistem-bilgisayar.com.tr");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel report or sending summary email");
        }
    }

    private string BuildEmailBodyHtml(string workplaceName, DuesStatistic duesStatistic)
    {
        var culture = new CultureInfo("tr-TR");

        // HTML tablo formatında ödeme planı oluştur
        var bodyHtml = $@"
<p>{workplaceName} adresinde bulunan dükkan bütçe ödeme planı aşağıdadır.</p>

<table border=""1"" cellpadding=""3"" cellspacing=""0"" style=""border-collapse: collapse; width: 100%; max-width: 500px; margin-top: 10px; font-size: 13px;"">
    <thead>
        <tr style=""background-color: #011560; color: white;"">
            <th style=""padding: 4px 6px; text-align: left; border: 1px solid #011560;"">Ay</th>
            <th style=""padding: 4px 6px; text-align: right; border: 1px solid #011560;"">Tutar</th>
        </tr>
    </thead>
    <tbody>";

        // Aylık ödemeler
        var months = new[]
        {
            ("Ocak", duesStatistic.January),
            ("Şubat", duesStatistic.February),
            ("Mart", duesStatistic.March),
            ("Nisan", duesStatistic.April),
            ("Mayıs", duesStatistic.May),
            ("Haziran", duesStatistic.June),
            ("Temmuz", duesStatistic.July),
            ("Ağustos", duesStatistic.August),
            ("Eylül", duesStatistic.September),
            ("Ekim", duesStatistic.October),
            ("Kasım", duesStatistic.November),
            ("Aralık", duesStatistic.December)
        };

        foreach (var (monthName, amount) in months)
        {
            bodyHtml += $@"
        <tr style=""line-height: 1.2;"">
            <td style=""padding: 3px 6px; border: 1px solid #ddd;"">{monthName}</td>
            <td style=""padding: 3px 6px; text-align: right; border: 1px solid #ddd;"">{amount.ToString("N2", culture)} TL</td>
        </tr>";
        }

        bodyHtml += @"
    </tbody>
    <tfoot>
        <tr style=""background-color: #f0f0f0; font-weight: bold;"">
            <td style=""padding: 4px 6px; border: 1px solid #ddd;"">Toplam</td>
            <td style=""padding: 4px 6px; text-align: right; border: 1px solid #ddd;"">" + duesStatistic.Total.ToString("N2", culture) + @" TL</td>
        </tr>
    </tfoot>
</table>";

        return bodyHtml;
    }

    public async Task<ResponseDto<byte[]>> GenerateBudgetExcelAsync(int year)
    {
        try
        {
            _logger.LogInformation("Generating budget Excel for year: {Year}", year);

            var yearString = year.ToString();

            // 1. MailTemplate.xlsx dosyasını oku
            // ContentRootPath bir üst dizini işaret ediyor olabilir, birden fazla konum dene
            var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "MailTemplate.xlsx");
            _logger.LogInformation("Looking for MailTemplate.xlsx at: {Path}", templatePath);

            if (!System.IO.File.Exists(templatePath))
            {
                // Alternatif konumları dene - bir üst dizin, wwwroot, Pdf klasörü
                var alternativePaths = new[]
                {
                    Path.Combine(_webHostEnvironment.ContentRootPath, "..", "MailTemplate.xlsx"),
                    Path.Combine(_webHostEnvironment.WebRootPath, "MailTemplate.xlsx"),
                    Path.Combine(_webHostEnvironment.WebRootPath, "Pdf", "MailTemplate.xlsx"),
                    Path.Combine(Directory.GetCurrentDirectory(), "MailTemplate.xlsx"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "MailTemplate.xlsx")
                };

                foreach (var altPath in alternativePaths)
                {
                    _logger.LogInformation("Checking alternative path: {Path}", altPath);
                    if (System.IO.File.Exists(altPath))
                    {
                        templatePath = Path.GetFullPath(altPath);
                        _logger.LogInformation("MailTemplate.xlsx found at: {Path}", templatePath);
                        break;
                    }
                }

                // Hala bulunamadıysa hata ver
                if (!System.IO.File.Exists(templatePath))
                {
                    _logger.LogError("MailTemplate.xlsx not found. Searched paths: {Path}", templatePath);
                    return ResponseDto<byte[]>.FailData(404, "MailTemplate.xlsx bulunamadı",
                        $"Dosya yolu: {templatePath}", false);
                }
            }

            // 2. Tüm DuesStatistics verilerini al
            var duesStatsResponse = await _duesStatisticService.GetByYearAsync(yearString);
            if (!duesStatsResponse.IsSuccess || duesStatsResponse.Data == null)
            {
                return ResponseDto<byte[]>.FailData(500, "Dues verileri alınamadı",
                    $"{year} yılına ait bütçe verileri bulunamadı", true);
            }

            // DivCode -> DuesStatistic map oluştur
            var duesStatsDict = duesStatsResponse.Data
                .GroupBy(d => d.DivCode)
                .ToDictionary(g => g.Key, g => g.First());

            // 3. Tüm işyerlerini al
            var workplacesResponse = await GetAllAsync();
            if (!workplacesResponse.IsSuccess || workplacesResponse.Data == null)
            {
                return ResponseDto<byte[]>.FailData(500, "İşyerleri alınamadı",
                    "İşyerleri listesi getirilemedi", true);
            }

            var workplaces = workplacesResponse.Data;

            // 4. Cari bilgilerini Logo veritabanından çek
            var cariQuery = $@"SELECT WP.CODE AS WPCODE,WP.DEFINITION_ AS WPADDRESS,CL.LOGICALREF AS CLREFFERANCE ,CL.CODE AS CLCODE,CL.DEFINITION_ AS CLCTITLE,CL.EMAILADDR AS EMAILADDR
                        FROM LG_{LogoSetting.Firm}_CLCARD AS CL
                        INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS WP ON WP.LOGICALREF=CL.PARENTCLREF
                        WHERE CL.SPECODE NOT IN ('KIRMIZI','MAVİ','YEŞİL')
                          AND LEFT(TRIM(CL.CODE),1)='1' AND CL.ACTIVE=0
                        ORDER BY WP.CODE";

            var cariResult = _sqlProvider.SqlReader(cariQuery);

            // Her işyeri kodu için birden fazla cari hesap olabilir
            var cariDict = new Dictionary<string, List<WorkplaceCurrentAccounts>>();
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
                            cariDict[wpCode] = new List<WorkplaceCurrentAccounts>();
                        }
                        cariDict[wpCode].Add(new WorkplaceCurrentAccounts
                        {
                            LogicalRef = logicalRef,
                            Code = code,
                            Definition = definition,
                            EmailAddress = emailAddress
                        });
                    }
                }
            }

            // 5. Excel oluştur
            using (var workbook = new XLWorkbook(templatePath))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    return ResponseDto<byte[]>.FailData(500, "Excel şablonu boş",
                        "MailTemplate.xlsx içinde worksheet bulunamadı", true);
                }

                // Başlangıç satırı (varsayılan olarak 2. satırdan başla, 1. satır header)
                int row = 2;

                var culture = new CultureInfo("tr-TR");

                // 6. Her işyeri ve cari hesap için Excel satırı ekle
                foreach (var workplace in workplaces)
                {
                    // İlgili işyeri için DuesStatistic bul
                    if (!duesStatsDict.TryGetValue(workplace.Code, out var duesStatistic))
                    {
                        _logger.LogWarning("DuesStatistic not found for workplace code: {WorkplaceCode}", workplace.Code);
                        continue;
                    }

                    // SQL'den gelen cari hesapları kontrol et
                    if (!cariDict.TryGetValue(workplace.Code, out var currentAccounts) || !currentAccounts.Any())
                    {
                        _logger.LogWarning("No current accounts found for workplace: {WorkplaceCode}", workplace.Code);
                        continue;
                    }

                    // Her cari hesap için bir satır ekle
                    foreach (var currentAccount in currentAccounts)
                    {
                        worksheet.Cell(row, 1).Value = currentAccount.EmailAddress; // e-posta
                        worksheet.Cell(row, 2).Value = currentAccount.Definition;   // unvan
                        worksheet.Cell(row, 3).Value = workplace.Definition;        // adres

                        // Aylık tutarlar
                        worksheet.Cell(row, 4).Value = duesStatistic.January;   // Ocak
                        worksheet.Cell(row, 5).Value = duesStatistic.February;  // Şubat
                        worksheet.Cell(row, 6).Value = duesStatistic.March;     // Mart
                        worksheet.Cell(row, 7).Value = duesStatistic.April;     // Nisan
                        worksheet.Cell(row, 8).Value = duesStatistic.May;       // Mayıs
                        worksheet.Cell(row, 9).Value = duesStatistic.June;      // Haziran
                        worksheet.Cell(row, 10).Value = duesStatistic.July;     // Temmuz
                        worksheet.Cell(row, 11).Value = duesStatistic.August;   // Ağustos
                        worksheet.Cell(row, 12).Value = duesStatistic.September;// Eylül
                        worksheet.Cell(row, 13).Value = duesStatistic.October;  // Ekim
                        worksheet.Cell(row, 14).Value = duesStatistic.November; // Kasım
                        worksheet.Cell(row, 15).Value = duesStatistic.December; // Aralık
                        worksheet.Cell(row, 16).Value = duesStatistic.Total;    // Toplam

                        row++;
                    }
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Excel'i byte array olarak kaydet
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var excelBytes = stream.ToArray();

                    _logger.LogInformation("Budget Excel generated successfully for year: {Year}, Rows: {Row}",
                        year, row - 2);

                    return ResponseDto<byte[]>.SuccessData(200,
                        $"Bütçe Excel dosyası başarıyla oluşturuldu. Toplam {row - 2} kayıt.",
                        excelBytes);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating budget Excel for year: {Year}", year);
            return ResponseDto<byte[]>.FailData(500, "Bütçe Excel oluşturma başarısız", ex.Message, true);
        }
    }
}
