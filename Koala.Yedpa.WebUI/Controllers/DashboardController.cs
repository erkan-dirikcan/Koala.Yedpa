using Koala.Yedpa.Core.Constants;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.WebUI.Models;
using Koala.Yedpa.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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

            // Get user's claims from their roles
            var claimValues = new List<string>();
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role != null)
                {
                    var roleClaims = await _context.RoleClaims
                        .Where(rc => rc.RoleId == role.Id)
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

            ViewBag.HiddenWidgets = widgetViewModels.Where(w => !w.Visible).ToList();
            ViewBag.Widgets = widgetViewModels.Where(w => w.Visible).ToList();
            ViewBag.AllWidgets = widgetViewModels.ToList();

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
