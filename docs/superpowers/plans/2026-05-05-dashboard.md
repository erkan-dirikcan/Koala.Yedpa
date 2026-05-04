# Customizable Dashboard Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a drag-and-drop customizable dashboard with GridStack.js, Chart.js, claim-based widget authorization, and user preference persistence.

**Architecture:** GridStack.js manages widget layout with drag-and-drop and resize. Each widget is a partial view that loads data via AJAX from existing API endpoints. Claim-based authorization controls which widgets render per user. User preferences (positions, visibility) persist in a SQL table via EF Core.

**Tech Stack:** ASP.NET Core MVC, GridStack.js 10.x, Chart.js 4.x, jQuery AJAX, EF Core, Keen/Tech theme

---

## File Structure

### New Files

| File | Responsibility |
|------|---------------|
| `Koala.Yedpa.Core/Models/DashboardWidgetPreference.cs` | Entity for user widget preferences |
| `Koala.Yedpa.Core/Constants/DashboardWidgets.cs` | Widget ID, claim, default position constants |
| `Koala.Yedpa.WebUI/Views/Dashboard/Index.cshtml` | Replace - GridStack container |
| `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetBalanceSummary.cshtml` | W1 KPI cards |
| `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetPendingInvoices.cshtml` | W2 mini table + total |
| `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetOverduePayments.cshtml` | W3 alert card |
| `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetBalanceDistribution.cshtml` | W4 bar chart |
| `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetMonthlyTrend.cshtml` | W5 line chart |
| `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetRecentTransactions.cshtml` | W6 list |
| `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetDuesCollection.cshtml` | W7 doughnut chart |
| `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetMonthlyBudget.cshtml` | W8 KPI cards |
| `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetYearlyBudget.cshtml` | W9 grouped bar chart |
| `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetShopCount.cshtml` | W10 KPI card |
| `Koala.Yedpa.WebUI/wwwroot/js/dashboard/dashboard.js` | GridStack init, save/load, sidebar |
| `Koala.Yedpa.WebUI/wwwroot/js/dashboard/widgets.js` | Widget AJAX loader, Chart.js init |
| `Koala.Yedpa.WebUI/wwwroot/css/dashboard/dashboard.css` | Dashboard-specific styles |

### Modified Files

| File | Change |
|------|--------|
| `Koala.Yedpa.Repositories/AppDbContext.cs` | Add DbSet + modelBuilder for DashboardWidgetPreference |
| `Koala.Yedpa.WebUI/Controllers/DashboardController.cs` | Rewrite Index, add SaveLayout/ResetLayout actions |
| `Koala.Yedpa.WebUI/Views/Shared/_Layout.cshtml` | Add GridStack CSS + Chart.js CDN in head/scripts |

---

## Task 1: Entity & Database Setup

**Files:**
- Create: `Koala.Yedpa.Core/Models/DashboardWidgetPreference.cs`
- Create: `Koala.Yedpa.Core/Constants/DashboardWidgets.cs`
- Modify: `Koala.Yedpa.Repositories/AppDbContext.cs`

- [ ] **Step 1: Create DashboardWidgetPreference entity**

```csharp
// Koala.Yedpa.Core/Models/DashboardWidgetPreference.cs
using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models
{
    public class DashboardWidgetPreference
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string WidgetId { get; set; } = string.Empty;
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Visible { get; set; } = true;
    }
}
```

- [ ] **Step 2: Create DashboardWidgets constants class**

```csharp
// Koala.Yedpa.Core/Constants/DashboardWidgets.cs
namespace Koala.Yedpa.Core.Constants
{
    public class DashboardWidgetDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Claim { get; set; } = string.Empty;
        public string PartialView { get; set; } = string.Empty;
        public int DefaultX { get; set; }
        public int DefaultY { get; set; }
        public int DefaultWidth { get; set; }
        public int DefaultHeight { get; set; }
        public bool DefaultVisible { get; set; }
    }

    public static class DashboardWidgets
    {
        public static readonly List<DashboardWidgetDefinition> All = new()
        {
            new() { Id = "W1", Title = "Toplam Bakiye Özeti", Claim = "CurrentAccuant", PartialView = "_WidgetBalanceSummary", DefaultX = 0, DefaultY = 0, DefaultWidth = 12, DefaultHeight = 2, DefaultVisible = true },
            new() { Id = "W4", Title = "Cari Bakiye Dağılımı", Claim = "CurrentAccuant", PartialView = "_WidgetBalanceDistribution", DefaultX = 0, DefaultY = 2, DefaultWidth = 6, DefaultHeight = 4, DefaultVisible = true },
            new() { Id = "W7", Title = "Aidat Tahsilat Oranı", Claim = "BudgetManagement", PartialView = "_WidgetDuesCollection", DefaultX = 6, DefaultY = 2, DefaultWidth = 6, DefaultHeight = 4, DefaultVisible = true },
            new() { Id = "W2", Title = "Bekleyen Faturalar", Claim = "CurrentAccuant", PartialView = "_WidgetPendingInvoices", DefaultX = 0, DefaultY = 6, DefaultWidth = 6, DefaultHeight = 4, DefaultVisible = true },
            new() { Id = "W5", Title = "Aylık Tahsilat Trendi", Claim = "CurrentAccuant", PartialView = "_WidgetMonthlyTrend", DefaultX = 6, DefaultY = 6, DefaultWidth = 6, DefaultHeight = 4, DefaultVisible = true },
            new() { Id = "W8", Title = "Aylık Bütçe Özeti", Claim = "BudgetManagement", PartialView = "_WidgetMonthlyBudget", DefaultX = 0, DefaultY = 10, DefaultWidth = 12, DefaultHeight = 3, DefaultVisible = true },
            new() { Id = "W3", Title = "Vadesi Geçen Ödemeler", Claim = "CurrentAccuant", PartialView = "_WidgetOverduePayments", DefaultX = 0, DefaultY = 13, DefaultWidth = 6, DefaultHeight = 3, DefaultVisible = false },
            new() { Id = "W6", Title = "Son İşlemler", Claim = "CurrentAccuant", PartialView = "_WidgetRecentTransactions", DefaultX = 6, DefaultY = 13, DefaultWidth = 6, DefaultHeight = 4, DefaultVisible = false },
            new() { Id = "W9", Title = "Yıllık Bütçe Karşılaştırma", Claim = "BudgetManagement", PartialView = "_WidgetYearlyBudget", DefaultX = 0, DefaultY = 17, DefaultWidth = 12, DefaultHeight = 4, DefaultVisible = false },
            new() { Id = "W10", Title = "Aktif Dükkan Sayısı", Claim = "Management", PartialView = "_WidgetShopCount", DefaultX = 0, DefaultY = 21, DefaultWidth = 3, DefaultHeight = 2, DefaultVisible = false }
        };

        public static List<DashboardWidgetDefinition> GetDefaultsForUser(List<string> userClaims)
        {
            return All.Where(w => userClaims.Contains(w.Claim)).ToList();
        }
    }
}
```

- [ ] **Step 3: Add DbSet to AppDbContext**

In `Koala.Yedpa.Repositories/AppDbContext.cs`, add a new DbSet property:

```csharp
public DbSet<DashboardWidgetPreference> DashboardWidgetPreferences { get; set; }
```

Also add the modelBuilder line inside `OnModelCreating`:

```csharp
modelBuilder.Entity<DashboardWidgetPreference>();
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build --nologo`
Expected: 0 errors

- [ ] **Step 5: Create EF migration**

Run: `dotnet ef migrations add AddDashboardWidgetPreference --project Koala.Yedpa.Repositories --startup-project Koala.Yedpa.WebUI`
Expected: Migration file created

- [ ] **Step 6: Apply migration**

Run: `dotnet ef database update --project Koala.Yedpa.Repositories --startup-project Koala.Yedpa.WebUI`
Expected: Database updated

- [ ] **Step 7: Commit**

```bash
git add Koala.Yedpa.Core/Models/DashboardWidgetPreference.cs Koala.Yedpa.Core/Constants/DashboardWidgets.cs Koala.Yedpa.Repositories/AppDbContext.cs Koala.Yedpa.Repositories/Migrations/
git commit -m "feat: add DashboardWidgetPreference entity and widget definitions"
```

---

## Task 2: Dashboard Controller

**Files:**
- Modify: `Koala.Yedpa.WebUI/Controllers/DashboardController.cs`

- [ ] **Step 1: Rewrite DashboardController**

Replace the entire content of `Koala.Yedpa.WebUI/Controllers/DashboardController.cs`:

```csharp
using Koala.Yedpa.Core.Constants;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace Koala.Yedpa.WebUI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(
            ILogger<DashboardController> logger,
            AppDbContext context,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userClaims = await _userManager.GetClaimsAsync(user);
            var claimValues = userClaims.Select(c => c.Value).ToList();

            // Also check role claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                var roleObj = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role);
                if (roleObj != null)
                {
                    var roleClaims = await _context.RoleClaims
                        .Where(rc => rc.RoleId == roleObj.Id)
                        .ToListAsync();
                    claimValues.AddRange(roleClaims.Select(rc => rc.ClaimValue));
                }
            }

            // Filter widgets by user claims
            var authorizedWidgets = DashboardWidgets.All
                .Where(w => claimValues.Contains(w.Claim))
                .ToList();

            // Load user preferences
            var preferences = await _context.DashboardWidgetPreferences
                .Where(p => p.UserId == user.Id)
                .ToListAsync();

            // Build widget view models
            var widgetViewModels = new List<DashboardWidgetViewModel>();

            foreach (var widget in authorizedWidgets)
            {
                var pref = preferences.FirstOrDefault(p => p.WidgetId == widget.Id);
                widgetViewModels.Add(new DashboardWidgetViewModel
                {
                    Id = widget.Id,
                    Title = widget.Title,
                    PartialView = widget.PartialView,
                    GridX = pref?.GridX ?? widget.DefaultX,
                    GridY = pref?.GridY ?? widget.DefaultY,
                    Width = pref?.Width ?? widget.DefaultWidth,
                    Height = pref?.Height ?? widget.DefaultHeight,
                    Visible = pref?.Visible ?? widget.DefaultVisible
                });
            }

            // Also pass hidden widgets (authorized but not visible) for sidebar toggle
            ViewBag.HiddenWidgets = widgetViewModels.Where(w => !w.Visible).ToList();
            ViewBag.Widgets = widgetViewModels.Where(w => w.Visible).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveLayout([FromBody] List<WidgetLayoutItem> layout)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            foreach (var item in layout)
            {
                var existing = await _context.DashboardWidgetPreferences
                    .FirstOrDefaultAsync(p => p.UserId == user.Id && p.WidgetId == item.WidgetId);

                if (existing != null)
                {
                    existing.GridX = item.GridX;
                    existing.GridY = item.GridY;
                    existing.Width = item.Width;
                    existing.Height = item.Height;
                    existing.Visible = item.Visible;
                }
                else
                {
                    _context.DashboardWidgetPreferences.Add(new DashboardWidgetPreference
                    {
                        UserId = user.Id,
                        WidgetId = item.WidgetId,
                        GridX = item.GridX,
                        GridY = item.GridY,
                        Width = item.Width,
                        Height = item.Height,
                        Visible = item.Visible
                    });
                }
            }

            await _context.SaveChangesAsync();
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
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Visible { get; set; }
    }

    public class WidgetLayoutItem
    {
        public string WidgetId { get; set; } = string.Empty;
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Visible { get; set; }
    }
}
```

Note: This controller needs `using Koala.Yedpa.Repositories;` for AppDbContext — the WebUI project must reference the Repositories project. Verify this reference exists in `Koala.Yedpa.WebUI.csproj`. If not, add:

```xml
<ProjectReference Include="..\Koala.Yedpa.Repositories\Koala.Yedpa.Repositories.csproj" />
```

- [ ] **Step 2: Build and verify**

Run: `dotnet build Koala.Yedpa.WebUI --nologo`
Expected: 0 errors

- [ ] **Step 3: Commit**

```bash
git add Koala.Yedpa.WebUI/Controllers/DashboardController.cs
git commit -m "feat: rewrite DashboardController with claim-based widget authorization and layout persistence"
```

---

## Task 3: Client-Side Libraries & Layout Integration

**Files:**
- Modify: `Koala.Yedpa.WebUI/Views/Shared/_Layout.cshtml`
- Create: `Koala.Yedpa.WebUI/wwwroot/css/dashboard/dashboard.css`

- [ ] **Step 1: Add GridStack and Chart.js CDN to _Layout.cshtml**

In `Koala.Yedpa.WebUI/Views/Shared/_Layout.cshtml`, add these lines in the `<head>` section, after the existing CSS links:

```html
<!-- Dashboard: GridStack CSS -->
<link href="https://cdn.jsdelivr.net/npm/gridstack@10.3.1/dist/gridstack.min.css" rel="stylesheet" />
<link href="https://cdn.jsdelivr.net/npm/gridstack@10.3.1/dist/gridstack-extra.min.css" rel="stylesheet" />
<!-- Dashboard: Chart.js -->
<link href="https://cdn.jsdelivr.net/npm/chart.js@4.4.7/dist/chart.umd.min.css" rel="stylesheet" />
```

In the scripts section, before `@await RenderSectionAsync("Scripts", required: false)`, add:

```html
<!-- Dashboard: GridStack + Chart.js -->
<script src="https://cdn.jsdelivr.net/npm/gridstack@10.3.1/dist/gridstack-all.js"></script>
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.7/dist/chart.umd.min.js"></script>
```

- [ ] **Step 2: Create dashboard CSS**

Create file `Koala.Yedpa.WebUI/wwwroot/css/dashboard/dashboard.css`:

```css
/* Dashboard Grid */
.dashboard-grid {
    min-height: 400px;
}

.dashboard-grid .grid-stack-item {
    border: 1px solid #EBEDF3;
    border-radius: 0.75rem;
    background: #ffffff;
    overflow: hidden;
}

.dashboard-grid .grid-stack-item .grid-stack-item-content {
    padding: 0;
    inset: 0;
}

/* Widget Card */
.widget-card {
    height: 100%;
    display: flex;
    flex-direction: column;
    padding: 1.25rem;
}

.widget-card .widget-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
}

.widget-card .widget-title {
    font-size: 1.1rem;
    font-weight: 600;
    color: #3F4254;
}

.widget-card .widget-body {
    flex: 1;
    overflow-y: auto;
    position: relative;
}

/* KPI Cards */
.kpi-row {
    display: flex;
    gap: 1rem;
    flex-wrap: wrap;
}

.kpi-item {
    flex: 1;
    min-width: 140px;
    padding: 1rem;
    border-radius: 0.75rem;
    text-align: center;
}

.kpi-item .kpi-value {
    font-size: 1.75rem;
    font-weight: 700;
    line-height: 1.2;
}

.kpi-item .kpi-label {
    font-size: 0.85rem;
    color: #7E8299;
    margin-top: 0.25rem;
}

.kpi-primary { background: #E1F0FF; color: #3699FF; }
.kpi-success { background: #C9F7F5; color: #1BC5BD; }
.kpi-danger  { background: #FFE2E5; color: #F64E60; }
.kpi-warning { background: #FFF4DE; color: #FFA800; }

/* Widget Loading */
.widget-loading {
    display: flex;
    align-items: center;
    justify-content: center;
    height: 100%;
    min-height: 120px;
    color: #B5B5C3;
}

.widget-error {
    display: flex;
    align-items: center;
    justify-content: center;
    height: 100%;
    min-height: 120px;
    color: #F64E60;
    font-size: 0.9rem;
}

/* Widget Sidebar */
.widget-sidebar {
    position: fixed;
    right: -300px;
    top: 0;
    width: 300px;
    height: 100vh;
    background: #ffffff;
    box-shadow: -2px 0 15px rgba(0,0,0,0.1);
    z-index: 1050;
    transition: right 0.3s ease;
    padding: 1.5rem;
    overflow-y: auto;
}

.widget-sidebar.open {
    right: 0;
}

.widget-sidebar .sidebar-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
    padding-bottom: 1rem;
    border-bottom: 1px solid #EBEDF3;
}

.widget-sidebar .sidebar-title {
    font-size: 1.1rem;
    font-weight: 600;
    color: #3F4254;
}

.widget-sidebar .widget-toggle-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.75rem 0;
    border-bottom: 1px solid #F3F6F9;
}

/* Mini Table */
.widget-table {
    width: 100%;
    font-size: 0.85rem;
}

.widget-table th {
    color: #7E8299;
    font-weight: 500;
    padding: 0.5rem 0.25rem;
    border-bottom: 1px solid #EBEDF3;
    text-align: left;
}

.widget-table td {
    padding: 0.5rem 0.25rem;
    border-bottom: 1px solid #F3F6F9;
    color: #3F4254;
}

/* Dashboard toolbar */
.dashboard-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
}

.dashboard-toolbar .toolbar-actions {
    display: flex;
    gap: 0.5rem;
}
```

- [ ] **Step 3: Commit**

```bash
git add Koala.Yedpa.WebUI/Views/Shared/_Layout.cshtml Koala.Yedpa.WebUI/wwwroot/css/dashboard/dashboard.css
git commit -m "feat: add GridStack.js, Chart.js CDN and dashboard CSS"
```

---

## Task 4: Dashboard Index View

**Files:**
- Modify: `Koala.Yedpa.WebUI/Views/Dashboard/Index.cshtml`

- [ ] **Step 1: Replace Index.cshtml**

Replace the entire content of `Koala.Yedpa.WebUI/Views/Dashboard/Index.cshtml`:

```html
@using Koala.Yedpa.WebUI.Controllers
@model List<DashboardWidgetViewModel>

@{
    ViewData["Title"] = "Dashboard";
    ViewData["ActivePage"] = "Dashboard";
    var widgets = ViewBag.Widgets as List<DashboardWidgetViewModel> ?? new List<DashboardWidgetViewModel>();
    var hiddenWidgets = ViewBag.HiddenWidgets as List<DashboardWidgetViewModel> ?? new List<DashboardWidgetViewModel>();
    var allWidgets = widgets.Concat(hiddenWidgets).ToList();
}

<link href="~/css/dashboard/dashboard.css" rel="stylesheet" />

<div class="dashboard-toolbar">
    <h3 class="card-label font-weight-bolder text-dark mb-0">Dashboard</h3>
    <div class="toolbar-actions">
        <button type="button" class="btn btn-sm btn-light-primary" id="btnToggleSidebar">
            <i class="fas fa-th-large mr-1"></i> Widget'lar
        </button>
        <button type="button" class="btn btn-sm btn-light-danger" id="btnResetLayout">
            <i class="fas fa-undo mr-1"></i> Sıfırla
        </button>
    </div>
</div>

<div class="grid-stack dashboard-grid" id="dashboardGrid">
    @foreach (var widget in widgets)
    {
        <div class="grid-stack-item"
             data-gs-id="@widget.Id"
             data-gs-x="@widget.GridX"
             data-gs-y="@widget.GridY"
             data-gs-w="@widget.Width"
             data-gs-h="@widget.Height"
             data-gs-min-w="3"
             data-gs-min-h="2">
            <div class="grid-stack-item-content">
                <div class="widget-card" id="widget-@widget.Id">
                    <div class="widget-header">
                        <span class="widget-title">@widget.Title</span>
                        <button type="button" class="btn btn-icon btn-xs btn-light btn-circle widget-remove" data-widget-id="@widget.Id">
                            <i class="fas fa-times icon-xs"></i>
                        </button>
                    </div>
                    <div class="widget-body">
                        <div class="widget-loading">
                            <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
                        </div>
                    </div>
                </div>
            </div>
        </div>
    }
</div>

<!-- Widget Sidebar -->
<div class="widget-sidebar" id="widgetSidebar">
    <div class="sidebar-header">
        <span class="sidebar-title">Widget Yönetimi</span>
        <button type="button" class="btn btn-icon btn-sm btn-light btn-circle" id="btnCloseSidebar">
            <i class="fas fa-times"></i>
        </button>
    </div>
    @foreach (var widget in allWidgets)
    {
        <div class="widget-toggle-item">
            <span>@widget.Title</span>
            <label class="switch switch-sm">
                <input type="checkbox" class="widget-toggle" data-widget-id="@widget.Id" @(widget.Visible ? "checked" : "") />
                <span></span>
            </label>
        </div>
    }
</div>

<input type="hidden" id="allWidgetsData" value='@Html.Raw(System.Text.Json.JsonSerializer.Serialize(allWidgets.Select(w => new { w.Id, w.Title, w.PartialView, w.GridX, w.GridY, w.Width, w.Height, w.Visible }).ToList()))' />

@section Scripts
{
    <script src="~/js/dashboard/widgets.js"></script>
    <script src="~/js/dashboard/dashboard.js"></script>
}
```

- [ ] **Step 2: Commit**

```bash
git add Koala.Yedpa.WebUI/Views/Dashboard/Index.cshtml
git commit -m "feat: add GridStack dashboard Index view with widget sidebar"
```

---

## Task 5: Dashboard JavaScript

**Files:**
- Create: `Koala.Yedpa.WebUI/wwwroot/js/dashboard/dashboard.js`
- Create: `Koala.Yedpa.WebUI/wwwroot/js/dashboard/widgets.js`

- [ ] **Step 1: Create dashboard.js (GridStack init, save, sidebar)**

Create `Koala.Yedpa.WebUI/wwwroot/js/dashboard/dashboard.js`:

```javascript
// dashboard.js - GridStack init, layout save/load, sidebar
$(function () {
    // Init GridStack
    var grid = GridStack.init({
        column: 12,
        cellHeight: 70,
        minRow: 2,
        margin: 8,
        animate: true,
        float: false,
        resizable: { handles: 'se, sw' },
        draggable: { handle: '.widget-header' }
    });

    // Debounced save
    var saveTimeout;
    function debouncedSave() {
        clearTimeout(saveTimeout);
        saveTimeout = setTimeout(saveLayout, 800);
    }

    grid.on('change', function () {
        debouncedSave();
    });

    // Remove widget
    $(document).on('click', '.widget-remove', function () {
        var widgetId = $(this).data('widget-id');
        var el = grid.getGridItems().find(function (item) {
            return $(item).data('gs-id') === widgetId;
        });
        if (el) {
            grid.removeWidget(el);
            // Uncheck sidebar toggle
            $('.widget-toggle[data-widget-id="' + widgetId + '"]').prop('checked', false);
            debouncedSave();
        }
    });

    // Sidebar toggle
    $('#btnToggleSidebar').on('click', function () {
        $('#widgetSidebar').toggleClass('open');
    });
    $('#btnCloseSidebar').on('click', function () {
        $('#widgetSidebar').removeClass('open');
    });

    // Widget visibility toggle (sidebar)
    $(document).on('change', '.widget-toggle', function () {
        var widgetId = $(this).data('widget-id');
        var isChecked = $(this).is(':checked');

        if (isChecked) {
            addWidgetToGrid(widgetId);
        } else {
            removeWidgetFromGrid(widgetId);
        }
        debouncedSave();
    });

    function addWidgetToGrid(widgetId) {
        var allWidgets = JSON.parse($('#allWidgetsData').val());
        var widgetDef = allWidgets.find(function (w) { return w.Id === widgetId; });
        if (!widgetDef) return;

        var html = '<div class="grid-stack-item" data-gs-id="' + widgetId + '" data-gs-w="' + widgetDef.Width + '" data-gs-h="' + widgetDef.Height + '" data-gs-min-w="3" data-gs-min-h="2">' +
            '<div class="grid-stack-item-content">' +
            '<div class="widget-card" id="widget-' + widgetId + '">' +
            '<div class="widget-header">' +
            '<span class="widget-title">' + widgetDef.Title + '</span>' +
            '<button type="button" class="btn btn-icon btn-xs btn-light btn-circle widget-remove" data-widget-id="' + widgetId + '"><i class="fas fa-times icon-xs"></i></button>' +
            '</div>' +
            '<div class="widget-body"><div class="widget-loading"><i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...</div></div>' +
            '</div></div></div>';

        grid.addWidget(html);
        loadWidget(widgetId);
    }

    function removeWidgetFromGrid(widgetId) {
        var el = grid.getGridItems().find(function (item) {
            return $(item).data('gs-id') === widgetId;
        });
        if (el) grid.removeWidget(el);
    }

    // Reset layout
    $('#btnResetLayout').on('click', function () {
        Swal.fire({
            title: 'Layout Sıfırla',
            text: 'Varsayılan layout\'a dönmek istediğinize emin misiniz?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Evet, Sıfırla',
            cancelButtonText: 'İptal',
            confirmButtonColor: '#F64E60'
        }).then(function (result) {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Dashboard/ResetLayout',
                    method: 'POST',
                    success: function () {
                        location.reload();
                    },
                    error: function () {
                        toastr.error('Layout sıfırlanamadı');
                    }
                });
            }
        });
    });

    // Save layout
    function saveLayout() {
        var items = [];
        grid.getGridItems().each(function () {
            var node = $(this).data('_gridstack_node') || grid.engine.nodes.find(function (n) { return n.el === this; }.bind(this));
            if (!node) return;
            items.push({
                WidgetId: $(this).data('gs-id'),
                GridX: typeof node.x === 'number' ? node.x : 0,
                GridY: typeof node.y === 'number' ? node.y : 0,
                Width: typeof node.w === 'number' ? node.w : $(this).data('gs-w'),
                Height: typeof node.h === 'number' ? node.h : $(this).data('gs-h'),
                Visible: true
            });
        });

        // Include hidden widgets (from unchecked sidebar toggles)
        $('.widget-toggle:not(:checked)').each(function () {
            items.push({
                WidgetId: $(this).data('widget-id'),
                GridX: 0, GridY: 0, Width: 6, Height: 3, Visible: false
            });
        });

        $.ajax({
            url: '/Dashboard/SaveLayout',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(items),
            error: function () {
                toastr.error('Layout kaydedilemedi');
            }
        });
    }

    // Load all visible widgets
    window._dashboardGrid = grid;
});
```

- [ ] **Step 2: Create widgets.js (AJAX loader + Chart.js init)**

Create `Koala.Yedpa.WebUI/wwwroot/js/dashboard/widgets.js`:

```javascript
// widgets.js - Widget AJAX loader and Chart.js initializers
var chartColors = {
    primary: '#3699FF',
    success: '#1BC5BD',
    warning: '#FFA800',
    danger: '#F64E60',
    info: '#8950FC',
    gray: '#B5B5C3'
};

function loadWidget(widgetId) {
    var $body = $('#widget-' + widgetId + ' .widget-body');
    if ($body.length === 0) return;

    var loaders = {
        'W1': loadBalanceSummary,
        'W2': loadPendingInvoices,
        'W3': loadOverduePayments,
        'W4': loadBalanceDistribution,
        'W5': loadMonthlyTrend,
        'W6': loadRecentTransactions,
        'W7': loadDuesCollection,
        'W8': loadMonthlyBudget,
        'W9': loadYearlyBudget,
        'W10': loadShopCount
    };

    if (loaders[widgetId]) {
        loaders[widgetId]($body);
    }
}

function widgetError($el, msg) {
    $el.html('<div class="widget-error"><i class="fas fa-exclamation-circle mr-2"></i>' + (msg || 'Veri yüklenemedi') + '</div>');
}

function formatCurrency(val) {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', minimumFractionDigits: 2 }).format(val || 0);
}

function formatDate(val) {
    if (!val) return '-';
    return new Date(val).toLocaleDateString('tr-TR');
}

// W1: Balance Summary KPI
function loadBalanceSummary($el) {
    $.get('/api/LogoClCardApi/CustomerListWithBalance?perPage=1000').done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var data = res.data;
        var totalDebit = data.reduce(function (s, c) { return s + (c.balance > 0 ? c.balance : 0); }, 0);
        var totalCredit = data.reduce(function (s, c) { return s + (c.balance < 0 ? Math.abs(c.balance) : 0); }, 0);
        var net = totalDebit - totalCredit;
        $el.html(
            '<div class="kpi-row">' +
            '<div class="kpi-item kpi-danger"><div class="kpi-value">' + formatCurrency(totalDebit) + '</div><div class="kpi-label">Toplam Alacak</div></div>' +
            '<div class="kpi-item kpi-success"><div class="kpi-value">' + formatCurrency(totalCredit) + '</div><div class="kpi-label">Toplam Borç</div></div>' +
            '<div class="kpi-item kpi-primary"><div class="kpi-value">' + formatCurrency(net) + '</div><div class="kpi-label">Net Bakiye</div></div>' +
            '</div>');
    }).fail(function () { widgetError($el); });
}

// W2: Pending Invoices
function loadPendingInvoices($el) {
    $.get('/api/LogoClCardApi/PendingInvoices?perPage=10').done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var data = res.data;
        var total = data.reduce(function (s, i) { return s + i.remainingAmount; }, 0);
        var html = '<div class="kpi-item kpi-danger mb-3" style="border-radius:0.5rem;padding:0.75rem"><div class="kpi-value">' + formatCurrency(total) + '</div><div class="kpi-label">Toplam Bekleyen</div></div>';
        html += '<table class="widget-table"><thead><tr><th>Fatura</th><th>Cari</th><th>Tutar</th><th>Vade</th></tr></thead><tbody>';
        data.forEach(function (inv) {
            var rowClass = inv.remainingDays < 0 ? 'style="color:#F64E60"' : '';
            html += '<tr ' + rowClass + '><td>' + inv.invoiceNumber + '</td><td>' + inv.customerName.substring(0, 20) + '</td><td>' + formatCurrency(inv.remainingAmount) + '</td><td>' + formatDate(inv.dueDate) + '</td></tr>';
        });
        html += '</tbody></table>';
        $el.html(html);
    }).fail(function () { widgetError($el); });
}

// W3: Overdue Payments
function loadOverduePayments($el) {
    $.ajax({
        url: '/api/LogoClCardApi/PendingInvoicesSearch?perPage=10',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({})
    }).done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var overdue = res.data.filter(function (i) { return i.remainingDays < 0; });
        var total = overdue.reduce(function (s, i) { return s + i.remainingAmount; }, 0);
        var html = '<div class="kpi-item kpi-danger" style="border-radius:0.5rem;padding:0.75rem;margin-bottom:0.75rem"><div class="kpi-value">' + formatCurrency(total) + '</div><div class="kpi-label">Vadesi Geçen Toplam (' + overdue.length + ' fatura)</div></div>';
        html += '<table class="widget-table"><thead><tr><th>Fatura</th><th>Cari</th><th>Tutar</th><th>Gecikme</th></tr></thead><tbody>';
        overdue.slice(0, 5).forEach(function (inv) {
            html += '<tr style="color:#F64E60"><td>' + inv.invoiceNumber + '</td><td>' + inv.customerName.substring(0, 20) + '</td><td>' + formatCurrency(inv.remainingAmount) + '</td><td>' + Math.abs(inv.remainingDays) + ' gün</td></tr>';
        });
        html += '</tbody></table>';
        $el.html(html);
    }).fail(function () { widgetError($el); });
}

// W4: Balance Distribution Bar Chart
function loadBalanceDistribution($el) {
    $.get('/api/LogoClCardApi/CustomerListWithBalance?perPage=50').done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var sorted = res.data.filter(function (c) { return c.balance > 0; }).sort(function (a, b) { return b.balance - a.balance; }).slice(0, 10);
        var labels = sorted.map(function (c) { return (c.definition || c.code).substring(0, 15); });
        var values = sorted.map(function (c) { return c.balance; });
        $el.html('<canvas id="chartBalanceDist"></canvas>');
        new Chart(document.getElementById('chartBalanceDist'), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{ label: 'Bakiye (TL)', data: values, backgroundColor: chartColors.primary }]
            },
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
        });
    }).fail(function () { widgetError($el); });
}

// W5: Monthly Trend Line Chart
function loadMonthlyTrend($el) {
    // Placeholder - needs aggregated data endpoint
    $el.html('<div style="text-align:center;padding:2rem;color:#B5B5C3"><i class="fas fa-chart-line fa-2x mb-2"></i><p>Aylık trend için veri hazırlanıyor</p></div>');
}

// W6: Recent Transactions
function loadRecentTransactions($el) {
    $el.html('<div style="text-align:center;padding:2rem;color:#B5B5C3"><i class="fas fa-list fa-2x mb-2"></i><p>Son işlemler için veri hazırlanıyor</p></div>');
}

// W7: Dues Collection Doughnut
function loadDuesCollection($el) {
    $.get('/api/DuesStatisticApi/GetMonthlyBudgetSummary').done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var d = res.data;
        var collected = d.collectedAmount || 0;
        var remaining = d.remainingAmount || 0;
        $el.html('<canvas id="chartDues"></canvas>');
        new Chart(document.getElementById('chartDues'), {
            type: 'doughnut',
            data: {
                labels: ['Tahsil Edilen', 'Açık'],
                datasets: [{ data: [collected, remaining], backgroundColor: [chartColors.success, chartColors.danger] }]
            },
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } }
        });
    }).fail(function () { widgetError($el); });
}

// W8: Monthly Budget KPI
function loadMonthlyBudget($el) {
    $.get('/api/DuesStatisticApi/GetMonthlyBudgetSummary').done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var d = res.data;
        $el.html(
            '<div class="kpi-row">' +
            '<div class="kpi-item kpi-primary"><div class="kpi-value">' + formatCurrency(d.totalBudget || 0) + '</div><div class="kpi-label">Toplam Bütçe</div></div>' +
            '<div class="kpi-item kpi-success"><div class="kpi-value">' + formatCurrency(d.collectedAmount || 0) + '</div><div class="kpi-label">Tahsil Edilen</div></div>' +
            '<div class="kpi-item kpi-danger"><div class="kpi-value">' + formatCurrency(d.remainingAmount || 0) + '</div><div class="kpi-label">Kalan</div></div>' +
            '</div>');
    }).fail(function () { widgetError($el); });
}

// W9: Yearly Budget Comparison
function loadYearlyBudget($el) {
    $.get('/api/DuesStatisticApi/GetDistinctYears').done(function (res) {
        if (!res.isSuccess || !res.data || res.data.length === 0) { widgetError($el); return; }
        var years = res.data.slice(-3);
        var promises = years.map(function (y) {
            return $.get('/api/DuesStatisticApi/GetByYearAndType?year=' + y);
        });
        $.when.apply($, promises).done(function () {
            var labels = years.map(String);
            var budgets = [];
            var collections = [];
            for (var i = 0; i < arguments.length; i++) {
                var resp = arguments[i][0];
                budgets.push(resp.data ? resp.data.totalBudget || 0 : 0);
                collections.push(resp.data ? resp.data.collectedAmount || 0 : 0);
            }
            $el.html('<canvas id="chartYearlyBudget"></canvas>');
            new Chart(document.getElementById('chartYearlyBudget'), {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [
                        { label: 'Bütçe', data: budgets, backgroundColor: chartColors.primary },
                        { label: 'Tahsilat', data: collections, backgroundColor: chartColors.success }
                    ]
                },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } }, scales: { y: { beginAtZero: true } } }
            });
        });
    }).fail(function () { widgetError($el); });
}

// W10: Shop Count
function loadShopCount($el) {
    $.get('/api/LogoClCardApi/ClCardInfoAll?perPage=1').done(function (res) {
        if (!res.isSuccess) { widgetError($el); return; }
        var count = res.recordsTotal || 0;
        $el.html(
            '<div style="display:flex;align-items:center;justify-content:center;height:100%">' +
            '<div class="kpi-item kpi-primary" style="min-width:200px"><div class="kpi-value" style="font-size:2.5rem">' + count + '</div><div class="kpi-label">Aktif Dükkan</div></div>' +
            '</div>');
    }).fail(function () { widgetError($el); });
}

// Init all widgets on page load
$(function () {
    setTimeout(function () {
        var allWidgets = JSON.parse($('#allWidgetsData').val());
        allWidgets.forEach(function (w) {
            if (w.Visible) {
                loadWidget(w.Id);
            }
        });
    }, 300);
});
```

- [ ] **Step 3: Commit**

```bash
git add Koala.Yedpa.WebUI/wwwroot/js/dashboard/
git commit -m "feat: add dashboard GridStack init and widget AJAX loaders"
```

---

## Task 6: Widget Partial Views (Financial)

**Files:**
- Create: `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetBalanceSummary.cshtml`
- Create: `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetPendingInvoices.cshtml`
- Create: `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetOverduePayments.cshtml`
- Create: `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetBalanceDistribution.cshtml`
- Create: `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetMonthlyTrend.cshtml`
- Create: `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetRecentTransactions.cshtml`

All widget partials are placeholder shells — their actual content is rendered by the JavaScript AJAX loaders (widgets.js). The partial views exist so the controller can reference them in the widget definitions, enabling future server-side rendering if needed.

- [ ] **Step 1: Create all financial widget partials**

Each partial follows this minimal pattern (replace `WIDGET_NAME` and `TITLE`):

```html
@* _WidgetWIDGET_NAME.cshtml - Data loaded via AJAX (widgets.js) *@
<div class="widget-loading">
    <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
</div>
```

Create these 6 files:

`_WidgetBalanceSummary.cshtml`:
```html
<div class="widget-loading">
    <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
</div>
```

`_WidgetPendingInvoices.cshtml`:
```html
<div class="widget-loading">
    <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
</div>
```

`_WidgetOverduePayments.cshtml`:
```html
<div class="widget-loading">
    <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
</div>
```

`_WidgetBalanceDistribution.cshtml`:
```html
<div class="widget-loading">
    <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
</div>
```

`_WidgetMonthlyTrend.cshtml`:
```html
<div class="widget-loading">
    <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
</div>
```

`_WidgetRecentTransactions.cshtml`:
```html
<div class="widget-loading">
    <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
</div>
```

- [ ] **Step 2: Commit**

```bash
git add Koala.Yedpa.WebUI/Views/Dashboard/_Widget*.cshtml
git commit -m "feat: add financial widget partial views (W1-W6)"
```

---

## Task 7: Widget Partial Views (Budget + Operational)

**Files:**
- Create: `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetDuesCollection.cshtml`
- Create: `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetMonthlyBudget.cshtml`
- Create: `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetYearlyBudget.cshtml`
- Create: `Koala.Yedpa.WebUI/Views/Dashboard/_WidgetShopCount.cshtml`

- [ ] **Step 1: Create all budget and operational widget partials**

Same minimal pattern as Task 6. Create these 4 files:

`_WidgetDuesCollection.cshtml`:
```html
<div class="widget-loading">
    <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
</div>
```

`_WidgetMonthlyBudget.cshtml`:
```html
<div class="widget-loading">
    <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
</div>
```

`_WidgetYearlyBudget.cshtml`:
```html
<div class="widget-loading">
    <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
</div>
```

`_WidgetShopCount.cshtml`:
```html
<div class="widget-loading">
    <i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...
</div>
```

- [ ] **Step 2: Commit**

```bash
git add Koala.Yedpa.WebUI/Views/Dashboard/_WidgetDuesCollection.cshtml Koala.Yedpa.WebUI/Views/Dashboard/_WidgetMonthlyBudget.cshtml Koala.Yedpa.WebUI/Views/Dashboard/_WidgetYearlyBudget.cshtml Koala.Yedpa.WebUI/Views/Dashboard/_WidgetShopCount.cshtml
git commit -m "feat: add budget and operational widget partial views (W7-W10)"
```

---

## Task 8: Build Verification & Integration Test

**Files:** None new — verification only.

- [ ] **Step 1: Full solution build**

Run: `dotnet build --nologo`
Expected: 0 errors

- [ ] **Step 2: Verify dashboard page loads**

Run: `dotnet run --project Koala.Yedpa.WebUI`
Navigate to `/Dashboard` while logged in. Expected: GridStack grid renders with visible widgets, each showing a loading spinner, then data loading via AJAX.

- [ ] **Step 3: Verify widget interactions**

Test:
1. Drag a widget to a new position → check Network tab for POST `/Dashboard/SaveLayout`
2. Click "Widget'lar" button → sidebar opens with toggle switches
3. Uncheck a widget → widget removed from grid
4. Check it again → widget added back with data
5. Click "Sıfırla" → confirmation dialog → layout resets on confirm
6. Refresh page → verify positions persist

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "feat: complete customizable dashboard with GridStack, Chart.js, and claim-based widgets"
```
