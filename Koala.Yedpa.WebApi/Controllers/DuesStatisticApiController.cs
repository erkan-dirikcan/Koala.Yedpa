using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebApi.Controllers;

/// <summary>
/// Dues Statistics API Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class DuesStatisticApiController : ControllerBase
{
    private readonly IDuesStatisticService _duesStatisticService;
    private readonly ILogger<DuesStatisticApiController> _logger;

    public DuesStatisticApiController(
        IDuesStatisticService duesStatisticService,
        ILogger<DuesStatisticApiController> logger)
    {
        _duesStatisticService = duesStatisticService;
        _logger = logger;
    }

    /// <summary>
    /// Get distinct years from DuesStatistics
    /// </summary>
    /// <returns>List of distinct years</returns>
    [HttpGet("GetDistinctYears")]
    public async Task<IActionResult> GetDistinctYears()
    {
        try
        {
            var result = await _duesStatisticService.GetDistinctYearsAsync();

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distinct years");
            return StatusCode(500, ResponseDto<List<int>>.Fail(500, "Yıllar alınırken bir hata oluştu", new List<string> { ex.Message }, true));
        }
    }

    /// <summary>
    /// Get monthly budget summary for a specific year and budget type
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="budgetType">Budget type (1=Budget, 2=ExtraBudget)</param>
    /// <returns>Monthly budget summary</returns>
    [HttpGet("GetMonthlyBudgetSummary")]
    public async Task<IActionResult> GetMonthlyBudgetSummary([FromQuery] int year, [FromQuery] BuggetTypeEnum budgetType)
    {
        try
        {
            var result = await _duesStatisticService.GetMonthlyBudgetSummaryAsync(year, budgetType);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return StatusCode(result.StatusCode, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly budget summary for year: {Year}, budgetType: {BudgetType}", year, budgetType);
            return StatusCode(500, ResponseDto<MonthlyBudgetSummaryViewModel>.FailData(500, "Aylık bütçe özeti alınırken bir hata oluştu", new List<string> { ex.Message }, true));
        }
    }

    /// <summary>
    /// Get DuesStatistics by year and budget type
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="budgetType">Budget type (1=Budget, 2=ExtraBudget) - Optional, defaults to 1</param>
    /// <returns>List of DuesStatistics</returns>
    [HttpGet("GetByYearAndType")]
    public async Task<IActionResult> GetByYearAndType([FromQuery] int year, [FromQuery] BuggetTypeEnum? budgetType)
    {
        try
        {
            var yearString = year.ToString();
            var result = await _duesStatisticService.GetByYearAsync(yearString);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }

            // Filter by budget type - if not specified, default to Budget (1)
            var targetBudgetType = budgetType ?? BuggetTypeEnum.Budget;

            var filteredData = result.Data
                .Where(d => d.BudgetType == targetBudgetType)
                .Select(d => new
                {
                    id = d.Id,
                    code = d.Code,
                    divCode = d.DivCode,
                    divName = d.DivName,
                    docTrackingNr = d.DocTrackingNr,
                    clientCode = d.ClientCode,
                    clientRef = d.ClientRef,
                    january = d.January,
                    february = d.February,
                    march = d.March,
                    april = d.April,
                    may = d.May,
                    june = d.June,
                    july = d.July,
                    august = d.August,
                    september = d.September,
                    october = d.October,
                    november = d.November,
                    december = d.December,
                    total = d.Total
                })
                .ToList();

            var response = new
            {
                isSuccess = true,
                statusCode = 200,
                message = "Veriler başarıyla alındı",
                data = filteredData
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dues statistics for year: {Year}, budgetType: {BudgetType}", year, budgetType ?? BuggetTypeEnum.Budget);
            return StatusCode(500, new
            {
                isSuccess = false,
                statusCode = 500,
                message = "Veriler alınırken bir hata oluştu",
                errors = new { errors = new[] { ex.Message } }
            });
        }
    }

    /// <summary>
    /// Get paginated list of DuesStatistics (for DataTable)
    /// </summary>
    /// <param name="request">DataTable request</param>
    /// <returns>Paginated DuesStatistics list</returns>
    [HttpPost("GetPagedList")]
    public async Task<IActionResult> GetPagedList([FromBody] DataTableRequest request)
    {
        try
        {
            // Get all data (in a real scenario, implement proper server-side pagination)
            var allDataResult = await _duesStatisticService.GetAllAsync();

            if (!allDataResult.IsSuccess)
            {
                return StatusCode(allDataResult.StatusCode, allDataResult);
            }

            var allData = allDataResult.Data.ToList();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                var searchValue = request.Search.Value.ToLower();
                allData = allData.Where(d =>
                    d.Year.ToLower().Contains(searchValue) ||
                    d.Code.ToLower().Contains(searchValue) ||
                    (d.ClientCode != null && d.ClientCode.ToLower().Contains(searchValue))
                ).ToList();
            }

            // Apply sorting
            if (request.Order != null && request.Order.Count > 0)
            {
                var sortColumn = request.Columns[request.Order[0].Column].Data;
                var sortDirection = request.Order[0].Dir == "asc";

                // Simple sorting implementation (you can enhance this with reflection or a library)
                allData = sortDirection
                    ? allData.OrderBy(d => d.Year).ToList()
                    : allData.OrderByDescending(d => d.Year).ToList();
            }

            // Apply pagination
            var totalRecords = allData.Count;
            var pagedData = allData.Skip(request.Start).Take(request.Length).ToList();

            var result = new
            {
                isSuccess = true,
                statusCode = 200,
                message = "Veriler başarıyla alındı",
                data = pagedData.Select(d => new DuesStatisticListViewModel
                {
                    Id = d.Id,
                    Year = d.Year,
                    Code = d.Code,
                    ClientCode = d.ClientCode,
                    ClientRef = d.ClientRef,
                    Total = d.Total,
                    BudgetType = d.BudgetType,
                    TransferStatus = d.TransferStatus,
                    DivCode = d.DivCode,
                    DivName = d.DivName,
                    DocTrackingNr = d.DocTrackingNr,
                    BuggetRatioId = d.BuggetRatioId,
                    January = d.January,
                    February = d.February,
                    March = d.March,
                    April = d.April,
                    May = d.May,
                    June = d.June,
                    July = d.July,
                    August = d.August,
                    September = d.September,
                    October = d.October,
                    November = d.November,
                    December = d.December
                }).ToList(),
                recordsTotal = totalRecords,
                recordsFiltered = allData.Count
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged list");
            return StatusCode(500, new
            {
                isSuccess = false,
                statusCode = 500,
                message = "Veriler alınırken bir hata oluştu",
                errors = new { errors = new[] { ex.Message } }
            });
        }
    }
}
