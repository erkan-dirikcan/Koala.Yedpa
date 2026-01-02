using Dapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Koala.Yedpa.Core.Providers;

namespace Koala.Yedpa.Service.Services;


public class DuesStatisticService : IDuesStatisticService
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly IDuesStatisticRepository _duesStatisticRepository;
    private readonly ILogger<DuesStatisticService> _logger;
    //private readonly string _sourceConnectionString;
    private readonly ISqlProvider _sqlProvider;
    private readonly IDapperProvider _dapperProvider;

    public DuesStatisticService(
        IUnitOfWork<AppDbContext> unitOfWork,
        IDuesStatisticRepository duesStatisticRepository,
        ILogger<DuesStatisticService> logger,
        //string sourceConnectionString,
        ISqlProvider sqlProvider, IDapperProvider dapperProvider)
    {
        _unitOfWork = unitOfWork;
        _duesStatisticRepository = duesStatisticRepository;
        _logger = logger;
        //_sourceConnectionString = sourceConnectionString;
        _sqlProvider = sqlProvider;
        _dapperProvider = dapperProvider;
    }

    public async Task<ResponseDto<DuesStatistic>> GetByIdAsync(string id)
    {
        try
        {
            var res = await _duesStatisticRepository.GetByIdAsync(id);
            return ResponseDto<DuesStatistic>.SuccessData(200, "Kayıt Bilgisi Başarıyla Alındı", res);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error getting dues statistic by ID: {Id}", id);
            return ResponseDto<DuesStatistic>.FailData(400, "Kayıt Bilgisi Alınırken Bi,r Sorunla Karşılaşıldı",
                ex.Message, true);
            //throw;
        }
    }

    public async Task<ResponseDto<IEnumerable<DuesStatistic>>> GetAllAsync()
    {
        try
        {
            var res = await _duesStatisticRepository.GetAllAsync();

            return ResponseDto<IEnumerable<DuesStatistic>>.SuccessData(200, "Kayıt Bilgileri Başarıyla Alındı", res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all dues statistics");
            return ResponseDto<IEnumerable<DuesStatistic>>.FailData(400,
                "Kayıt Bilgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<IEnumerable<DuesStatistic>>> GetByYearAsync(string year)
    {
        try
        {
            var res = await _duesStatisticRepository.GetByYearAsync(year);
            return ResponseDto<IEnumerable<DuesStatistic>>.SuccessData(200, "Kayıt Bilgileri Başarıyla Alındı", res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dues statistics for year: {Year}", year);
            return ResponseDto<IEnumerable<DuesStatistic>>.FailData(400,
                "Kayıt Bİlgileri Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<DuesStatistic>> CreateAsync(DuesStatistic duesStatistic)
    {
        try
        {
            // Check if record already exists
            var exists = await _duesStatisticRepository.ExistsAsync(duesStatistic.Code, duesStatistic.Year);
            if (exists)
            {
                throw new InvalidOperationException($"A record with code {duesStatistic.Code} for year {duesStatistic.Year} already exists.");
            }

            var id = await _duesStatisticRepository.AddAsync(duesStatistic);
            await _unitOfWork.CommitAsync();

            var res = await _duesStatisticRepository.GetByIdAsync(id);
            return ResponseDto<DuesStatistic>.SuccessData(200, "Kayıt Başarıyla Oluşturuldu", res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dues statistic for code: {Code}", duesStatistic.Code);
            return ResponseDto<DuesStatistic>.FailData(400, "Kayıt Oluşturulurken Bir Sorunla Karşılaşıldı",
                ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateAsync(DuesStatistic duesStatistic)
    {
        try
        {
            await _duesStatisticRepository.UpdateAsync(duesStatistic);
            await _unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Kayıt Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dues statistic ID: {Id}", duesStatistic.Id);
            return ResponseDto.Fail(400, "Kayıt Güncellenirken Bir Sorunla Karşılaşıldı",
                ex.Message, true);
        }
    }

    public async Task<ResponseDto> DeleteAsync(string id)
    {
        try
        {
            await _duesStatisticRepository.DeleteAsync(id);
            await _unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Kayıt Başarıyla Silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dues statistic ID: {Id}", id);
            return ResponseDto.Fail(400, "Kayıt Silinirken Bir Sorunla Karşılaşıldı",
                ex.Message, true);
        }
    }

    public async Task<ResponseDto<bool>> CheckExistsAsync(string code, string year)
    {
        try
        {
            var res = await _duesStatisticRepository.ExistsAsync(code, year);
            return ResponseDto<bool>.SuccessData(200, "Kayıt Varlık Durumu Başarıyla Alındı", res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence for code: {Code}, year: {Year}", code, year);
            return ResponseDto<bool>.FailData(400, "Kayıt Varlık Durumu Alınırken Bir Sorunla Karşılaşıldı",
                ex.Message, true);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="year"></param>
    /// <param name="budgetType"></param>
    /// <returns></returns>
    public async Task<ResponseDto<int>> ImportFromSourceDatabaseAsync(string year, BuggetTypeEnum budgetType = BuggetTypeEnum.Budget)
    {
        try
        {
            _logger.LogInformation("Starting import from source database for year: {Year}", year);

            // First, delete existing data for the year
            await _duesStatisticRepository.DeleteByYearAsync(year);

            // Get data from source database using Dapper
            var sourceData = await GetDataFromSourceDatabaseAsync(year);
            if (!sourceData.IsSuccess)
            {
                //ResponseDto<List<SourceDuesDataViewModel>>
                return ResponseDto<int>.FailData(sourceData.StatusCode, sourceData.Message, sourceData.Errors.Errors,
                    true);
            }
            _logger.LogInformation("Retrieved {Count} records from source database", sourceData.Data.Count);

            // Map to entity
            var entities = sourceData.Data.Select(dto => new DuesStatistic
            {
                Id = Guid.NewGuid().ToString(),
                Code = dto.Code,
                Year = year,
                DivCode = dto.DivCode,
                DivName = dto.DivName,
                DocTrackingNr = dto.DocTrackingNr,
                ClientCode = dto.ClientCode,
                ClientRef = dto.ClientRef,
                BudgetType = budgetType,
                January = dto.January,
                February = dto.February,
                March = dto.March,
                April = dto.April,
                May = dto.May,
                June = dto.June,
                July = dto.July,
                August = dto.August,
                September = dto.September,
                October = dto.October,
                November = dto.November,
                December = dto.December,
                Total = dto.Total,
                CreateTime = DateTime.UtcNow
            }).ToList();

            // Bulk insert
            await _duesStatisticRepository.BulkInsertAsync(entities);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully imported {Count} records for year: {Year}", entities.Count, year);

            return ResponseDto<int>.SuccessData(200, "Veri Başarıyla İçe Aktarıldı", entities.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing data from source database for year: {Year}", year);
            return ResponseDto<int>.FailData(400, "Veri İçe Aktarılırken Bir Sorunla Karşılaşıldı",
                ex.Message, true);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public async Task<ResponseDto<bool>> SyncYearDataAsync(string year)
    {
        try
        {
            _logger.LogInformation("Starting sync for year: {Year}", year);

            // Get current data from your database
            var existingData = (await _duesStatisticRepository.GetByYearAsync(year)).ToList();
            _logger.LogInformation("Found {Count} existing records for year: {Year}", existingData.Count, year);

            // Get fresh data from source
            var sourceData = await GetDataFromSourceDatabaseAsync(year);
            if (!sourceData.IsSuccess)
            {
                return ResponseDto<bool>.FailData(sourceData.StatusCode, sourceData.Message, sourceData.Errors.Errors,
                    true);
            }
            _logger.LogInformation("Retrieved {Count} records from source for year: {Year}", sourceData.Data.Count, year);

            var updates = new List<DuesStatistic>();
            var additions = new List<DuesStatistic>();

            foreach (var sourceItem in sourceData.Data)
            {
                var existing = existingData.FirstOrDefault(e =>
                    e.Code == sourceItem.Code && e.Year == year);

                if (existing != null)
                {
                    // Update existing record
                    existing.DivName = sourceItem.DivName;
                    existing.January = sourceItem.January;
                    existing.February = sourceItem.February;
                    existing.March = sourceItem.March;
                    existing.April = sourceItem.April;
                    existing.May = sourceItem.May;
                    existing.June = sourceItem.June;
                    existing.July = sourceItem.July;
                    existing.August = sourceItem.August;
                    existing.September = sourceItem.September;
                    existing.October = sourceItem.October;
                    existing.November = sourceItem.November;
                    existing.December = sourceItem.December;
                    existing.Total = sourceItem.Total;
                    existing.LastUpdateTime = DateTime.UtcNow;

                    updates.Add(existing);
                }
                else
                {
                    // Add new record
                    additions.Add(new DuesStatistic
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = sourceItem.Code,
                        Year = year,
                        DivCode = sourceItem.DivCode,
                        DivName = sourceItem.DivName,
                        DocTrackingNr = sourceItem.DocTrackingNr,
                        ClientCode = sourceItem.ClientCode,
                        ClientRef = sourceItem.ClientRef,
                        BudgetType = BuggetTypeEnum.Budget,
                        January = sourceItem.January,
                        February = sourceItem.February,
                        March = sourceItem.March,
                        April = sourceItem.April,
                        May = sourceItem.May,
                        June = sourceItem.June,
                        July = sourceItem.July,
                        August = sourceItem.August,
                        September = sourceItem.September,
                        October = sourceItem.October,
                        November = sourceItem.November,
                        December = sourceItem.December,
                        Total = sourceItem.Total,
                        CreateTime = DateTime.UtcNow,
                        BuggetRatioId = "",
                        TransferStatus = TransferStatusEnum.FromLogo
                    });
                }
            }

            // Apply updates
            foreach (var update in updates)
            {
                await _duesStatisticRepository.UpdateAsync(update);
            }

            // Apply additions
            if (additions.Any())
            {
                await _duesStatisticRepository.BulkInsertAsync(additions);
            }

            // Save all changes
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Sync completed. Updates: {Updates}, Additions: {Additions}",
                updates.Count, additions.Count);
            return ResponseDto<bool>.SuccessData(200, "Veri Başarıyla Senkronize Edildi", true);
            //return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing data for year: {Year}", year);
            return ResponseDto<bool>.FailData(400, "Veri Senkronize Edilirken Bir Sorunla Karşılaşıldı",
                ex.Message, true);
        }
    }

    public async Task<ResponseDto<DuesStatisticSummaryViewModel>> GetYearSummaryAsync(string year)
    {
        try
        {
            var data = (await _duesStatisticRepository.GetByYearAsync(year)).ToList();

            if (!data.Any())
                return ResponseDto<DuesStatisticSummaryViewModel>.FailData(404, "Kayıt Bilgisi Alınırken Bİr Sorunla Karşılaşıldı", "Bu yıl bilgisine ait kayıt bulunamadı", true);


            var summary = new DuesStatisticSummaryViewModel
            {
                Year = year,
                TotalCompanies = data.Count,
                TotalAmount = data.Sum(d => d.Total),
                AverageAmount = data.Average(d => d.Total),
                MonthlyTotals = new Dictionary<string, decimal>
                {
                    ["January"] = data.Sum(d => d.January),
                    ["February"] = data.Sum(d => d.February),
                    ["March"] = data.Sum(d => d.March),
                    ["April"] = data.Sum(d => d.April),
                    ["May"] = data.Sum(d => d.May),
                    ["June"] = data.Sum(d => d.June),
                    ["July"] = data.Sum(d => d.July),
                    ["August"] = data.Sum(d => d.August),
                    ["September"] = data.Sum(d => d.September),
                    ["October"] = data.Sum(d => d.October),
                    ["November"] = data.Sum(d => d.November),
                    ["December"] = data.Sum(d => d.December)
                }
            };
            return ResponseDto<DuesStatisticSummaryViewModel>.SuccessData(200, "Kayıt Bilgisi Başarıyla Alındı", summary);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting year summary for year: {Year}", year);
            return ResponseDto<DuesStatisticSummaryViewModel>.FailData(400,
                "Kayıt Bilgisi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<IEnumerable<YearOverviewViewModel>>> GetYearOverviewsAsync()
    {
        try
        {
            var allData = (await _duesStatisticRepository.GetAllAsync()).ToList();

            var res = allData
                .GroupBy(d => d.Year)
                .Select(g => new YearOverviewViewModel
                {
                    Year = g.Key,
                    CompanyCount = g.Count(),
                    TotalAmount = g.Sum(d => d.Total),
                    LastSyncDate = g.Max(d => d.CreateTime)
                })
                .OrderByDescending(o => o.Year);
            return ResponseDto<IEnumerable<YearOverviewViewModel>>.SuccessData(200, "Kayıt Bilgisi Başarıyla Alındı", res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting year overviews");
            return ResponseDto<IEnumerable<YearOverviewViewModel>>.FailData(400,
                 "Kayıt Bilgisi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<IEnumerable<MonthlySummaryViewModel>>> GetMonthlySummaryAsync(string year)
    {
        try
        {
            var data = (await _duesStatisticRepository.GetByYearAsync(year)).ToList();

            if (!data.Any())
                return ResponseDto<IEnumerable<MonthlySummaryViewModel>>.FailData(404,
                    "Kayıt Bilgisi Alınırken Bir Sorunla KArşılaşıldı.", "Bu yıl bilgisine sahip data bulunamadı",
                    true);



            var months = new[]
            {
                    new { Name = "January", Number = "01", Property = nameof(DuesStatistic.January) },
                    new { Name = "February", Number = "02", Property = nameof(DuesStatistic.February) },
                    new { Name = "March", Number = "03", Property = nameof(DuesStatistic.March) },
                    new { Name = "April", Number = "04", Property = nameof(DuesStatistic.April) },
                    new { Name = "May", Number = "05", Property = nameof(DuesStatistic.May) },
                    new { Name = "June", Number = "06", Property = nameof(DuesStatistic.June) },
                    new { Name = "July", Number = "07", Property = nameof(DuesStatistic.July) },
                    new { Name = "August", Number = "08", Property = nameof(DuesStatistic.August) },
                    new { Name = "September", Number = "09", Property = nameof(DuesStatistic.September) },
                    new { Name = "October", Number = "10", Property = nameof(DuesStatistic.October) },
                    new { Name = "November", Number = "11", Property = nameof(DuesStatistic.November) },
                    new { Name = "December", Number = "12", Property = nameof(DuesStatistic.December) }
                };
            var res = months.Select(m =>
            {
                var monthAmount = data.Sum(d => (decimal)d.GetType().GetProperty(m.Property).GetValue(d));
                var companiesWithData = data.Count(d => (decimal)d.GetType().GetProperty(m.Property).GetValue(d) > 0);

                return new MonthlySummaryViewModel()
                {
                    Month = m.Name,
                    MonthNumber = m.Number,
                    TotalAmount = monthAmount,
                    CompanyCount = companiesWithData,
                    AverageAmount = companiesWithData > 0 ? monthAmount / companiesWithData : 0
                };
            }).ToList();
            return ResponseDto<IEnumerable<MonthlySummaryViewModel>>.SuccessData(200, "Kayıt Bilgisi Başarıyla Alındı", res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly summary for year: {Year}", year);
            return ResponseDto<IEnumerable<MonthlySummaryViewModel>>.FailData(400,
                "Kayıt Bilgisi Alınırken Bir Sorunla Karşılaşıldı", ex.Message, true);

        }
    }

    public async Task<ResponseDto<DuesStatistic>> GetByWorkplaceCodeAsync(string workplaceCode, string year)
    {
        try
        {
            var res = await _duesStatisticRepository.GetByWorkplaceCodeAsync(workplaceCode, year);
            return ResponseDto<DuesStatistic>.SuccessData(200, "Kayıt Bilgisi Başarıyla Alındı", res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dues statistic by workplace code: {WorkplaceCode} for year: {Year}",
                workplaceCode, year);
            return ResponseDto<DuesStatistic>.FailData(400, "Kayıt Bilgisi Alınırken Bir Sorunla Karşılaşıldı",
                ex.Message, true);
        }
    }

    private async Task<ResponseDto<List<SourceDuesDataViewModel>>> GetDataFromSourceDatabaseAsync(string year)
    {
        try
        {
            var query = @"
                            SELECT [KOD1] AS Code
                                  ,[ISYERI] AS DivCode
                                  ,[ISYERIADI] AS DivName
                                  ,[DOCTRACKINGNR] AS DocTrackingNr
                                  ,[UYEFIRMA] AS ClientCode
                                  ,[CLIENTREF] AS ClientRef
                                  ,ISNULL([OCAK], 0) AS January
                                  ,ISNULL([SUBAT], 0) AS February
                                  ,ISNULL([MART], 0) AS March
                                  ,ISNULL([NISAN], 0) AS April
                                  ,ISNULL([MAYIS], 0) AS May
                                  ,ISNULL([HAZIRAN], 0) AS June
                                  ,ISNULL([TEMMUZ], 0) AS July
                                  ,ISNULL([AGUSTOS], 0) AS August
                                  ,ISNULL([EYLUL], 0) AS September
                                  ,ISNULL([EKIM], 0) AS October
                                  ,ISNULL([KASIM], 0) AS November
                                  ,ISNULL([ARALIK], 0) AS December
                                  ,ISNULL([TOPLAM], 0) AS Total
                              FROM [YEDPA].[dbo].[AL_AIDAT_RAKAMLARI_PERFORMANS] ORDER BY KOD1
                            ";
            //SELECT 
            //    KOD1 AS Code,
            //    ISYERI AS DivCode,
            //    ISYERIADI AS DivName,
            //    DOCTRACKINGNR AS DocTrackingNr,  // Burada dikkat! Modelde DocTrackingNr
            //    UYEFIRMA AS ClientCode,          // Modelde ClientCode
            //    CLIENTREF AS ClientRef,
            //    ISNULL(OCAK, 0) AS January,
            //    ISNULL(SUBAT, 0) AS February,
            //    ISNULL(MART, 0) AS March,
            //    ISNULL(NISAN, 0) AS April,
            //    ISNULL(MAYIS, 0) AS May,
            //    ISNULL(HAZIRAN, 0) AS June,
            //    ISNULL(TEMMUZ, 0) AS July,
            //    ISNULL(AGUSTOS, 0) AS August,
            //    ISNULL(EYLUL, 0) AS September,
            //    ISNULL(EKIM, 0) AS October,
            //    ISNULL(KASIM, 0) AS November,
            //    ISNULL(ARALIK, 0) AS December,
            //    ISNULL(TOPLAM, 0) AS Total
            //FROM [dbo].[AL_2025_AIDAT_RAKAMLARI]
            //ORDER BY KOD1";

            // 1. Dapper'ı inject ettiğinizi varsayarak:
            var result = await _dapperProvider.QueryAsync<SourceDuesDataViewModel>(query);

            if (!result.IsSuccess)
            {
                return ResponseDto<List<SourceDuesDataViewModel>>.FailData(
                    result.StatusCode,
                    "Kaynak Veritabanından Veri Alınırken Bir Sorunla Karşılaşıldı",
                    result.Errors.Errors,
                    true
                );
            }

            // 2. Dapper otomatik olarak map etti, direkt dön
            return ResponseDto<List<SourceDuesDataViewModel>>.SuccessData(
                200,
                "Veri Başarıyla Alındı",
                result.Data.ToList()  // IEnumerable'dan List'e çevir
            );
        }
        catch (Exception ex)
        {
            return ResponseDto<List<SourceDuesDataViewModel>>.FailData(
                500,
                "Kaynak Veritabanına Bağlanırken Hata Oluştu",
                ex.Message,
                true
            );
        }
    }


    // Private helper method to get data from source database
    //private async Task<ResponseDto<List<SourceDuesDataViewModel>>> GetDataFromSourceDatabaseAsync(string year)
    //{
    //    var query = @"
    //            SELECT 
    //                KOD1 AS Code,
    //                ISYERI AS DivCode,
    //                ISYERIADI AS DivName,
    //                DOCTRACKINGNR AS DivTrackingNumber,
    //                UYEFIRMA AS MemberCompanyCode,
    //                CLIENTREF AS ClientRef,
    //                ISNULL(OCAK, 0) AS January,
    //                ISNULL(SUBAT, 0) AS February,
    //                ISNULL(MART, 0) AS March,
    //                ISNULL(NISAN, 0) AS April,
    //                ISNULL(MAYIS, 0) AS May,
    //                ISNULL(HAZIRAN, 0) AS June,
    //                ISNULL(TEMMUZ, 0) AS July,
    //                ISNULL(AGUSTOS, 0) AS August,
    //                ISNULL(EYLUL, 0) AS September,
    //                ISNULL(EKIM, 0) AS October,
    //                ISNULL(KASIM, 0) AS November,
    //                ISNULL(ARALIK, 0) AS December,
    //                ISNULL(TOPLAM, 0) AS Total
    //            FROM [dbo].[AL_2025_AIDAT_RAKAMLARI]
    //            ORDER BY KOD1";

    //    var results = _sqlProvider.SqlReader(query);//await connection.QueryAsync<SourceDuesDataViewModel>(query);
    //    if (!results.IsSuccess)
    //    {
    //        return ResponseDto<List<SourceDuesDataViewModel>>.FailData(results.StatusCode, "Kaynak Veritabanından Veri Alınırken Bir Sorunla Karşılaşıldı", results.Errors.Errors, true);
    //    }

    //    var retVal = new List<SourceDuesDataViewModel>();
    //    for (int i = 0; i < results.Data.Rows.Count; i++)
    //    {
    //        var model = new SourceDuesDataViewModel
    //        {
    //            Code = results.Data.Rows[i]["Code"].ToString(),
    //            DivCode = results.Data.Rows[i]["DivCode"].ToString(),
    //            DivName = results.Data.Rows[i]["DivName"].ToString(),
    //            DocTrackingNr = Convert.ToInt64(results.Data.Rows[i]["DivTrackingNumber"]),
    //            ClientCode = results.Data.Rows[i]["MemberCompanyCode"].ToString(),
    //            ClientRef = Convert.ToInt64(results.Data.Rows[i]["ClientRef"]),
    //            January = Convert.ToDecimal(results.Data.Rows[i]["January"]),
    //            February = Convert.ToDecimal(results.Data.Rows[i]["February"]),
    //            March = Convert.ToDecimal(results.Data.Rows[i]["March"]),
    //            April = Convert.ToDecimal(results.Data.Rows[i]["April"]),
    //            May = Convert.ToDecimal(results.Data.Rows[i]["May"]),
    //            June = Convert.ToDecimal(results.Data.Rows[i]["June"]),
    //            July = Convert.ToDecimal(results.Data.Rows[i]["July"]),
    //            August = Convert.ToDecimal(results.Data.Rows[i]["August"]),
    //            September = Convert.ToDecimal(results.Data.Rows[i]["September"]),
    //            October = Convert.ToDecimal(results.Data.Rows[i]["October"]),
    //            November = Convert.ToDecimal(results.Data.Rows[i]["November"]),
    //            December = Convert.ToDecimal(results.Data.Rows[i]["December"]),
    //            Total = Convert.ToDecimal(results.Data.Rows[i]["Total"])


    //        };
    //        retVal.Add(model);
    //    }
    //    return ResponseDto<List<SourceDuesDataViewModel>>.SuccessData(200, "Veri Başarıyla Alındı", retVal);
    //}


}
