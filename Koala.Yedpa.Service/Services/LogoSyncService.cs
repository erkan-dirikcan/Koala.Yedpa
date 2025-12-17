// Service/Services/LogoSyncService.cs
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Security.Claims;

namespace Service.Services
{
    public class LogoSyncService : ILogoSyncService
    {
        private readonly AppDbContext _context;
        private readonly ISqlProvider _sqlProvider;
        private readonly ILogger<LogoSyncService> _logger;
        private readonly ITransactionService _transactionService;
        private readonly ITransactionItemService _transactionItemService;
        private readonly string? _currentUserId;

        public LogoSyncService(
            AppDbContext context,
            ISqlProvider sqlProvider,
            ILogger<LogoSyncService> logger,
            ITransactionService transactionService,
            ITransactionItemService transactionItemService,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _context = context;
            _sqlProvider = sqlProvider;
            _logger = logger;
            _transactionService = transactionService;
            _transactionItemService = transactionItemService;
            _currentUserId = httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? null;
        }
        public async Task<ResponseDto<string>> SyncXt001211Async(string firm, string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(firm))
                return ResponseDto<string>.FailData(400, "Firma numarası boş olamaz.", "Firm parametresi gereklidir.", true);

            var effectiveUserId = userId ?? _currentUserId ?? null;

            // Transaction oluşturma işlemini service üzerinden yap
            var createTransactionModel = new CreateTransactionViewModel
            {
                TransactionTypeId = "c570d72f-d9c8-11f0-9657-e848b8c82000",
                UserId = effectiveUserId,
                Title = $"Logo Firma {firm} - XT001_211 Senkronizasyonu",
                Description = $"Firma: {firm}, LG_XT001_211 + LG_{firm}_CLCARD"
            };

            var transactionResponse = await _transactionService.CreateTransactionAsync(createTransactionModel);
            if (!transactionResponse.IsSuccess || transactionResponse.Data == null)
            {
                return ResponseDto<string>.FailData(500, "Transaction oluşturulamadı.", transactionResponse.Message, true);
            }

            var transaction = transactionResponse.Data;
            var transactionId = transaction.Id;

            try
            {
                string clcardTable = $"LG_{firm}_CLCARD";
                // Önce veritabanımızda mevcut LOGREF değerlerini HashSet olarak alalım
                var existingLogRefs = new HashSet<int>(await _context.LgXt001211
                    .Select(x => x.LogRef)
                    .ToListAsync());

                string query = $@"
                    SELECT
                        xt.*,
                        clc.CODE AS CLIENTCODE,
                        clc.DEFINITION_ AS CLIENTNAME,
                        grp.CODE AS GROUPCODE,
                        grp.DEFINITION_ AS GROUPNAME
                    FROM LG_XT001_211 xt
                    LEFT JOIN {clcardTable} clc ON xt.PARLOGREF = clc.LOGICALREF
                    LEFT JOIN {clcardTable} grp ON clc.PARENTCLREF = grp.LOGICALREF";

                var readResponse = _sqlProvider.SqlReader(query);
                await LogStepAsync(transactionId, $"Logo Firma {firm}'den veri çekiliyor...", readResponse);

                if (!readResponse.IsSuccess || readResponse.Data?.Rows.Count == 0)
                    return ResponseDto<string>.FailData(400, "Logo'dan veri alınamadı.", readResponse.Errors?.Errors ?? new List<string> { "Veri yok" }, true);

                var dt = readResponse.Data!;
                var entities = MapToEntities(dt, effectiveUserId);

                // INSERT edilecek yeni kayıtlar ve UPDATE edilecek mevcut kayıtlar
                var newEntities = entities.Where(e => !existingLogRefs.Contains(e.LogRef)).ToList();
                var existingEntities = entities.Where(e => existingLogRefs.Contains(e.LogRef)).ToList();

                await LogStepAsync(transactionId, "Veriler kaydediliyor...", async () =>
                {
                    var totalProcessed = 0;

                    // 1. YENİ KAYITLARI EKLE
                    if (newEntities.Any())
                    {
                        const int batchSize = 1000;
                        for (int i = 0; i < newEntities.Count; i += batchSize)
                        {
                            var batch = newEntities.Skip(i).Take(batchSize).ToList();
                            await _context.AddRangeAsync(batch);
                            await _context.SaveChangesAsync();
                        }
                        totalProcessed += newEntities.Count;
                    }

                    // 2. MEVCUT KAYITLARI GÜNCELLE (LOGREF, PARLOGREF, CLIENTCODE hariç)
                    if (existingEntities.Any())
                    {
                        foreach (var entity in existingEntities)
                        {
                            var existing = await _context.LgXt001211.FirstOrDefaultAsync(x => x.LogRef == entity.LogRef);
                            if (existing != null)
                            {
                                // LOGREF, PARLOGREF, CLIENTCODE hariç tüm alanları güncelle
                                existing.GroupCode = entity.GroupCode;
                                existing.GroupName = entity.GroupName;
                                existing.ClientName = entity.ClientName;
                                existing.CustomerType = entity.CustomerType;
                                existing.ResidenceTypeRef = entity.ResidenceTypeRef;
                                existing.ResidenceGroupRef = entity.ResidenceGroupRef;
                                existing.ParcelRef = entity.ParcelRef;
                                existing.PhaseRef = entity.PhaseRef;
                                existing.CauldronRef = entity.CauldronRef;
                                existing.ShareNo = entity.ShareNo;
                                existing.BegDate = entity.BegDate;
                                existing.EndDate = entity.EndDate;
                                existing.BlockRef = entity.BlockRef;
                                existing.IndDivNo = entity.IndDivNo;
                                existing.ResidenceNo = entity.ResidenceNo;
                                existing.DimGross = entity.DimGross;
                                existing.DimField = entity.DimField;
                                existing.PersonCount = entity.PersonCount;
                                existing.WaterMeterNo = entity.WaterMeterNo;
                                existing.CalMeterNo = entity.CalMeterNo;
                                existing.HotWaterMeterNo = entity.HotWaterMeterNo;
                                existing.ChiefReg = entity.ChiefReg;
                                existing.TaxPayer = entity.TaxPayer;
                                existing.IdentityNr = entity.IdentityNr;
                                existing.DeedInfo = entity.DeedInfo;
                                existing.ProfitingOwner = entity.ProfitingOwner;
                                existing.OfficialBegDate = entity.OfficialBegDate;
                                existing.OfficialEndDate = entity.OfficialEndDate;
                                existing.GasCoefficient = entity.GasCoefficient;
                                existing.ActiveResDate = entity.ActiveResDate;
                                existing.BudgetDepotMetre1 = entity.BudgetDepotMetre1;
                                existing.BudgetDepotMetre2 = entity.BudgetDepotMetre2;
                                existing.BudgetGroundMetre = entity.BudgetGroundMetre;
                                existing.BudgetHungMetre = entity.BudgetHungMetre;
                                existing.BudgetFloorMetre = entity.BudgetFloorMetre;
                                existing.BudgetPassageMetre1 = entity.BudgetPassageMetre1;
                                existing.BudgetPassageMetre2 = entity.BudgetPassageMetre2;
                                existing.BudgetDepotCoefficient1 = entity.BudgetDepotCoefficient1;
                                existing.BudgetDepotCoefficient2 = entity.BudgetDepotCoefficient2;
                                existing.BudgetGroundCoefficient = entity.BudgetGroundCoefficient;
                                existing.BudgetHungCoefficient = entity.BudgetHungCoefficient;
                                existing.BudgetFloorCoefficient = entity.BudgetFloorCoefficient;
                                existing.BudgetPassageCoefficient1 = entity.BudgetPassageCoefficient1;
                                existing.BudgetPassageCoefficient2 = entity.BudgetPassageCoefficient2;
                                existing.FuelDepotMetre1 = entity.FuelDepotMetre1;
                                existing.FuelDepotMetre2 = entity.FuelDepotMetre2;
                                existing.FuelGroundMetre = entity.FuelGroundMetre;
                                existing.FuelHungMetre = entity.FuelHungMetre;
                                existing.FuelFloorMetre = entity.FuelFloorMetre;
                                existing.FuelPassageMetre1 = entity.FuelPassageMetre1;
                                existing.FuelPassageMetre2 = entity.FuelPassageMetre2;
                                existing.FuelDepotCoefficient1 = entity.FuelDepotCoefficient1;
                                existing.FuelDepotCoefficient2 = entity.FuelDepotCoefficient2;
                                existing.FuelGroundCoefficient = entity.FuelGroundCoefficient;
                                existing.FuelHungCoefficient = entity.FuelHungCoefficient;
                                existing.FuelFloorCoefficient = entity.FuelFloorCoefficient;
                                existing.FuelPassageCoefficient1 = entity.FuelPassageCoefficient1;
                                existing.FuelPassageCoefficient2 = entity.FuelPassageCoefficient2;
                                existing.TotalBrutCoefficientMetre = entity.TotalBrutCoefficientMetre;
                                existing.TotalNetMetre = entity.TotalNetMetre;
                                existing.TotalFuelMetre = entity.TotalFuelMetre;
                            }
                        }
                        await _context.SaveChangesAsync();
                        totalProcessed += existingEntities.Count;
                    }

                    return ResponseDto<string>.SuccessData(200, $"{totalProcessed} kayıt işlendi ({newEntities.Count} yeni, {existingEntities.Count} güncellendi).", "OK");
                });

                // Transaction'ı tamamla
                var completeModel = new CompleteTransactionViewModel
                {
                    TransactionId = transactionId
                };
                await _transactionService.CompleteTransactionAsync(completeModel);

                return ResponseDto<string>.SuccessData(200, $"Firma {firm} senkronizasyonu tamamlandı.", transactionId);
            }
            catch (Exception ex)
            {
                // Exception'ı detaylı şekilde logla
                _logger.LogError(ex, "Logo senkronizasyonu sırasında hata oluştu. Firm: {Firm}, TransactionId: {TransactionId}, UserId: {UserId}", 
                    firm, transactionId, effectiveUserId);

                // Hata durumunda transaction item ekle
                await LogStepAsync(transactionId, "HATA: " + ex.Message, () => Task.FromResult(
                    ResponseDto<string>.FailData(500, ex.Message, ex.Message, true)
                ), false);

                return ResponseDto<string>.FailData(500, $"Firma {firm} senkronizasyonu başarısız.", ex.Message, true);
            }
        }
        private List<LgXt001211> MapToEntities(DataTable dt, string userId)
        {
            var list = new List<LgXt001211>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new LgXt001211
                {
                    Id = Tools.CreateGuidStr(),
                    LogRef = GetValueOrDefault<int>(row["LOGREF"]),
                    ParLogRef = GetValueOrDefault<int?>(row["PARLOGREF"]),
                    GroupCode = GetValueOrDefault<string>(row["GROUPCODE"]),
                    GroupName = GetValueOrDefault<string>(row["GROUPNAME"]),
                    ClientCode = GetValueOrDefault<string>(row["CLIENTCODE"]),
                    ClientName = GetValueOrDefault<string>(row["CLIENTNAME"]),
                    CustomerType = GetValueOrDefault<short?>(row["CUSTOMER_TYPE"]),
                    ResidenceTypeRef = GetValueOrDefault<int?>(row["RESIDENCETYPEREF"]),
                    ResidenceGroupRef = GetValueOrDefault<int?>(row["RESIDENCEGROUPREF"]),
                    ParcelRef = GetValueOrDefault<int?>(row["PARCELREF"]),
                    PhaseRef = GetValueOrDefault<int?>(row["PHASEREF"]),
                    CauldronRef = GetValueOrDefault<int?>(row["CAULDRONREF"]),
                    ShareNo = GetValueOrDefault<int?>(row["SHARENO"]),
                    BegDate = GetValueOrDefault<DateTime?>(row["BEGDATE"]),
                    EndDate = GetValueOrDefault<DateTime?>(row["ENDDATE"]),
                    BlockRef = GetValueOrDefault<int?>(row["BLOCKREF"]),
                    IndDivNo = GetValueOrDefault<string>(row["INDDIVNO"]),
                    ResidenceNo = GetValueOrDefault<string>(row["RESIDENCENO"]),
                    DimGross = GetValueOrDefault<double?>(row["DIMGROSS"]),
                    DimField = GetValueOrDefault<double?>(row["DIMFIELD"]),
                    PersonCount = GetValueOrDefault<short?>(row["PERSONCOUNT"]),
                    WaterMeterNo = GetValueOrDefault<string>(row["WATERMETERNO"]),
                    CalMeterNo = GetValueOrDefault<string>(row["CALMETERNO"]),
                    HotWaterMeterNo = GetValueOrDefault<string>(row["HOTWATERMETERNO"]),
                    ChiefReg = GetValueOrDefault<short?>(row["CHIEFREG"]),
                    TaxPayer = GetValueOrDefault<short?>(row["TAXPAYER"]),
                    IdentityNr = GetValueOrDefault<string>(row["IDENTITYNR"]),
                    DeedInfo = GetValueOrDefault<short?>(row["DEEDINFO"]),
                    ProfitingOwner = GetValueOrDefault<string>(row["PROFITINGOWNER"]),
                    OfficialBegDate = GetValueOrDefault<DateTime?>(row["OFFICIALBEGDATE"]),
                    OfficialEndDate = GetValueOrDefault<DateTime?>(row["OFFICIALENDDATE"]),
                    GasCoefficient = GetValueOrDefault<double?>(row["GASCOEFFICIENT"]),
                    ActiveResDate = GetValueOrDefault<DateTime?>(row["ACTIVERESDATE"]),
                    BudgetDepotMetre1 = GetValueOrDefault<double?>(row["BUDGETDEPOTMETRE1"]),
                    BudgetDepotMetre2 = GetValueOrDefault<double?>(row["BUDGETDEPOTMETRE2"]),
                    BudgetGroundMetre = GetValueOrDefault<double?>(row["BUDGETGROUNDMETRE"]),
                    BudgetHungMetre = GetValueOrDefault<double?>(row["BUDGETHUNGMETRE"]),
                    BudgetFloorMetre = GetValueOrDefault<double?>(row["BUDGETFLOORMETRE"]),
                    BudgetPassageMetre1 = GetValueOrDefault<double?>(row["BUDGETPASSAGEMETRE1"]),
                    BudgetPassageMetre2 = GetValueOrDefault<double?>(row["BUDGETPASSAGEMETRE2"]),
                    BudgetDepotCoefficient1 = GetValueOrDefault<double?>(row["BUDGETDEPOTCOEFFICIENT1"]),
                    BudgetDepotCoefficient2 = GetValueOrDefault<double?>(row["BUDGETDEPOTCOEFFICIENT2"]),
                    BudgetGroundCoefficient = GetValueOrDefault<double?>(row["BUDGETGROUNDCOEFFICIENT"]),
                    BudgetHungCoefficient = GetValueOrDefault<double?>(row["BUDGETHUNGCOEFFICIENT"]),
                    BudgetFloorCoefficient = GetValueOrDefault<double?>(row["BUDGETFLOORCOEFFICIENT"]),
                    BudgetPassageCoefficient1 = GetValueOrDefault<double?>(row["BUDGETPASSAGECOEFFICIENT1"]),
                    BudgetPassageCoefficient2 = GetValueOrDefault<double?>(row["BUDGETPASSAGECOEFFICIENT2"]),
                    FuelDepotMetre1 = GetValueOrDefault<double?>(row["FUELDEPOTMETRE1"]),
                    FuelDepotMetre2 = GetValueOrDefault<double?>(row["FUELDEPOTMETRE2"]),
                    FuelGroundMetre = GetValueOrDefault<double?>(row["FUELGROUNDMETRE"]),
                    FuelHungMetre = GetValueOrDefault<double?>(row["FUELHUNGMETRE"]),
                    FuelFloorMetre = GetValueOrDefault<double?>(row["FUELFLOORMETRE"]),
                    FuelPassageMetre1 = GetValueOrDefault<double?>(row["FUELPASSAGEMETRE1"]),
                    FuelPassageMetre2 = GetValueOrDefault<double?>(row["FUELPASSAGEMETRE2"]),
                    FuelDepotCoefficient1 = GetValueOrDefault<double?>(row["FUELDEPOTCOEFFICIENT1"]),
                    FuelDepotCoefficient2 = GetValueOrDefault<double?>(row["FUELDEPOTCOEFFICIENT2"]),
                    FuelGroundCoefficient = GetValueOrDefault<double?>(row["FUELGROUNDCOEFFICIENT"]),
                    FuelHungCoefficient = GetValueOrDefault<double?>(row["FUELHUNGCOEFFICIENT"]),
                    FuelFloorCoefficient = GetValueOrDefault<double?>(row["FUELFLOORCOEFFICIENT"]),
                    FuelPassageCoefficient1 = GetValueOrDefault<double?>(row["FUELPASSAGECOEFFICIENT1"]),
                    FuelPassageCoefficient2 = GetValueOrDefault<double?>(row["FUELPASSAGECOEFFICIENT2"]),
                    TotalBrutCoefficientMetre = GetValueOrDefault<double?>(row["TOTALBRUTCOEFFICIENTMETRE"]),
                    TotalNetMetre = GetValueOrDefault<double?>(row["TOTALNETMETRE"]),
                    TotalFuelMetre = GetValueOrDefault<double?>(row["TOTALFUELMETRE"])
                });
            }
            return list;
        }

        // YARDIMCI METOD
        private static T GetValueOrDefault<T>(object value)
        {
            if (value == DBNull.Value || value == null)
                return default(T);

            try
            {
                // Nullable tipler için özel işlem
                var type = typeof(T);
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var underlyingType = Nullable.GetUnderlyingType(type);
                    return (T)Convert.ChangeType(value, underlyingType);
                }

                return (T)Convert.ChangeType(value, type);
            }
            catch
            {
                return default(T);
            }
        }
      
        private async Task LogStepAsync(string transactionId, string description, ResponseDto<DataTable> response)
        {
            var itemModel = new CreateTransactionItemViewModel
            {
                TransactionId = transactionId,
                Description = $"{description} → {response.Message} ({response.Data?.Rows.Count ?? 0} kayıt)",
                IsSuccess = response.IsSuccess
            };

            await _transactionItemService.AddTransactionItemAsync(itemModel);
        }

        private async Task LogStepAsync(string transactionId, string description, Func<Task<ResponseDto<string>>> action, bool success = true)
        {
            var initialDescription = description;
            var isSuccess = success;

            try
            {
                var result = await action();
                initialDescription += $" → {result.Message}";
                isSuccess = result.IsSuccess;
            }
            catch (Exception ex)
            {
                initialDescription += $" → HATA: {ex.Message}";
                isSuccess = false;
            }

            var itemModel = new CreateTransactionItemViewModel
            {
                TransactionId = transactionId,
                Description = initialDescription,
                IsSuccess = isSuccess
            };

            await _transactionItemService.AddTransactionItemAsync(itemModel);
        }
       
    }
}

