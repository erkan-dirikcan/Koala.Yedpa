using Koala.Yedpa.Core.Constants;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.WebUI.Models;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.WebUI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Koala.Yedpa.WebUI.Controllers
{
    [Permission(PermissionCatalog.Dashboard.View)]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IApiLogoSqlDataService _logoService;
        private readonly IDuesStatisticService _duesService;

        public DashboardController(
            ILogger<DashboardController> logger,
            AppDbContext context,
            UserManager<AppUser> userManager,
            IApiLogoSqlDataService logoService,
            IDuesStatisticService duesService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _logoService = logoService;
            _duesService = duesService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var authorizedWidgets = DashboardWidgets.All.ToList();

            var preferences = await _context.DashboardWidgetPreferences
                .Where(p => p.UserId == user.Id)
                .ToListAsync();

            var widgetViewModels = new List<DashboardWidgetViewModel>();
            foreach (var widget in authorizedWidgets)
            {
                var pref = preferences.FirstOrDefault(p => p.WidgetId == widget.Id);
                widgetViewModels.Add(new DashboardWidgetViewModel
                {
                    Id = widget.Id,
                    Title = widget.Title,
                    PartialView = widget.PartialView,
                    Width = pref?.Width ?? widget.DefaultWidth,
                    Visible = pref?.Visible ?? widget.DefaultVisible,
                    SortOrder = pref?.GridY ?? widget.DefaultY
                });
            }

            widgetViewModels = widgetViewModels.OrderBy(w => w.SortOrder).ToList();

            ViewBag.HiddenWidgets = widgetViewModels.Where(w => !w.Visible).ToList();
            ViewBag.Widgets = widgetViewModels.Where(w => w.Visible).ToList();
            ViewBag.AllWidgets = widgetViewModels.ToList();

            await LoadLogoWidgetDataAsync();
            await LoadDuesWidgetDataAsync();
            await LoadAidatTahsilatWidgetDataAsync();

            return View();
        }

        private async Task LoadLogoWidgetDataAsync()
        {
            try
            {
                var balanceResult = await _logoService.GetCustomerListWithBalanceAsync(null, 1, 1000);
                if (balanceResult.IsSuccess && balanceResult.Data != null)
                {
                    var customers = balanceResult.Data;
                    var totalDebit = customers.Where(c => c.Balance > 0).Sum(c => c.Balance);
                    var totalCredit = customers.Where(c => c.Balance < 0).Sum(c => Math.Abs(c.Balance));

                    ViewBag.TotalDebit = totalDebit;
                    ViewBag.TotalCredit = totalCredit;
                    ViewBag.NetBalance = totalDebit - totalCredit;
                    ViewBag.TopDebtors = customers
                        .Where(c => c.Balance > 0)
                        .OrderByDescending(c => c.Balance)
                        .Take(10)
                        .ToList();
                }

                var pendingResult = await _logoService.GetPendingInvoicesAsync(1, 10);
                if (pendingResult.IsSuccess && pendingResult.Data != null)
                {
                    ViewBag.PendingInvoices = pendingResult.Data;
                    ViewBag.PendingTotal = pendingResult.Data.Sum(i => i.RemainingAmount);
                }

                var overdueResult = await _logoService.SearchPendingInvoicesAsync(new PendingInvoiceSearchViewModel(), 1, 100);
                if (overdueResult.IsSuccess && overdueResult.Data != null)
                {
                    var overdue = overdueResult.Data.Where(i => i.RemainingDays < 0).ToList();
                    ViewBag.OverdueInvoices = overdue;
                    ViewBag.OverdueTotal = overdue.Sum(i => i.RemainingAmount);
                }

                var shopResult = await _logoService.GetAllClCardInfoAsync(1, 1);
                if (shopResult.IsSuccess)
                {
                    ViewBag.ShopCount = shopResult.RecordsTotal;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Logo widget data");
            }
        }

        private async Task LoadDuesWidgetDataAsync()
        {
            try
            {
                var currentYear = DateTime.Now.Year;

                var budgetResult = await _duesService.GetMonthlyBudgetSummaryAsync(currentYear, BuggetTypeEnum.Budget);
                if (budgetResult.IsSuccess && budgetResult.Data != null)
                {
                    ViewBag.MonthlyBudget = budgetResult.Data;
                    var totalCollected = budgetResult.Data.MonthlyData.Sum(m => m.NewBudget);
                    ViewBag.BudgetTotalBudget = budgetResult.Data.TotalBudget;
                    ViewBag.BudgetCollected = totalCollected;
                    ViewBag.BudgetRemaining = budgetResult.Data.TotalBudget - totalCollected;
                }

                var yearsResult = await _duesService.GetDistinctYearsAsync();
                if (yearsResult.IsSuccess && yearsResult.Data != null && yearsResult.Data.Count > 0)
                {
                    var last3Years = yearsResult.Data.OrderByDescending(y => y).Take(3).ToList();
                    var yearlyData = new List<YearlyBudgetItem>();
                    foreach (var year in last3Years)
                    {
                        var yearData = await _duesService.GetMonthlyBudgetSummaryAsync(year, BuggetTypeEnum.Budget);
                        if (yearData.IsSuccess && yearData.Data != null)
                        {
                            yearlyData.Add(new YearlyBudgetItem
                            {
                                Year = year,
                                Budget = yearData.Data.TotalBudget,
                                Collected = yearData.Data.MonthlyData.Sum(m => m.NewBudget)
                            });
                        }
                    }
                    ViewBag.YearlyBudgetData = yearlyData;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Dues widget data");
            }
        }

        private async Task LoadAidatTahsilatWidgetDataAsync()
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;

                var result = await _logoService.GetAidatTahsilatKpiAsync(currentYear, currentMonth);
                if (result.IsSuccess && result.Data != null)
                {
                    ViewBag.AidatTahsilatKpi = result.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to load Aidat Tahsilat KPI: {Message}", result.Message);
                    ViewBag.AidatTahsilatKpi = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Aidat Tahsilat widget data");
                ViewBag.AidatTahsilatKpi = null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveLayout([FromBody] List<WidgetLayoutItem> layout)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (layout == null || layout.Count == 0)
            {
                _logger.LogWarning("SaveLayout called with empty layout for user {UserId}", user.Id);
                return Json(new { success = false, error = "Empty layout" });
            }

            _logger.LogInformation("SaveLayout: Saving {Count} widgets for user {UserId}", layout.Count, user.Id);

            foreach (var item in layout)
            {
                if (string.IsNullOrEmpty(item.WidgetId)) continue;

                _logger.LogDebug("SaveLayout: Widget={WidgetId}, Visible={Visible}, SortOrder={SortOrder}",
                    item.WidgetId, item.Visible, item.SortOrder);

                var existing = await _context.DashboardWidgetPreferences
                    .FirstOrDefaultAsync(p => p.UserId == user.Id && p.WidgetId == item.WidgetId);

                if (existing != null)
                {
                    existing.GridY = item.SortOrder;
                    existing.Width = item.Width;
                    existing.Visible = item.Visible;
                }
                else
                {
                    _context.DashboardWidgetPreferences.Add(new DashboardWidgetPreference
                    {
                        UserId = user.Id,
                        WidgetId = item.WidgetId,
                        GridX = 0,
                        GridY = item.SortOrder,
                        Width = item.Width,
                        Height = 3,
                        Visible = item.Visible
                    });
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("SaveLayout: Saved successfully for user {UserId}", user.Id);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ResetLayout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var prefs = _context.DashboardWidgetPreferences
                .Where(p => p.UserId == user.Id);
            _context.DashboardWidgetPreferences.RemoveRange(prefs);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class DashboardWidgetViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string PartialView { get; set; } = string.Empty;
        public int Width { get; set; }
        public bool Visible { get; set; }
        public int SortOrder { get; set; }
    }

    public class WidgetLayoutItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("widgetId")]
        public string WidgetId { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("width")]
        public int Width { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("visible")]
        public bool Visible { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("sortOrder")]
        public int SortOrder { get; set; }
    }

    public class YearlyBudgetItem
    {
        public int Year { get; set; }
        public decimal Budget { get; set; }
        public decimal Collected { get; set; }
    }
}
