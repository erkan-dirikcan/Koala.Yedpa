using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace Koala.Yedpa.WebUI.Controllers
{
    [Authorize]
    public class BudgetOrderController : Controller
    {
        private readonly ILogger<BudgetOrderController> _logger;
        private readonly IBudgetOrderService _budgetOrderService;
        private readonly UserManager<AppUser> _userManager;

        public BudgetOrderController(
            ILogger<BudgetOrderController> logger,
            IBudgetOrderService budgetOrderService,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _budgetOrderService = budgetOrderService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // TempData'dan mesajları ViewBag'e taşı
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                TempData.Keep("SuccessMessage"); // TempData'yı koru
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                TempData.Keep("ErrorMessage"); // TempData'yı koru
            }

            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Update(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("BudgetRatio ID gereklidir");
            }

            var result = await _budgetOrderService.GetBudgetRatioWithDuesAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(result.Message);
            }

            var (budgetRatio, duesStatistics) = result.Data;

            var model = new UpdateBudgetRatioViewModel
            {
                Id = budgetRatio.Id,
                Code = budgetRatio.Code,
                Description = budgetRatio.Description,
                Year = budgetRatio.Year,
                Ratio = budgetRatio.Ratio,
                TotalBugget = budgetRatio.TotalBugget,
                BuggetRatioMounths = budgetRatio.BuggetRatioMounths,
                BuggetType = budgetRatio.BuggetType
            };

            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "BudgetRatio ID gereklidir";
                return RedirectToAction("Index");
            }

            var result = await _budgetOrderService.GetBudgetRatioWithDuesAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message ?? "Kayıt bulunamadı";
                return RedirectToAction("Index");
            }

            var (budgetRatio, duesStatistics) = result.Data;

            var model = new BudgetOrderDetailsViewModel
            {
                Id = budgetRatio.Id,
                Code = budgetRatio.Code,
                Description = budgetRatio.Description,
                Year = budgetRatio.Year,
                Ratio = budgetRatio.Ratio,
                TotalBugget = budgetRatio.TotalBugget,
                BuggetRatioMounths = budgetRatio.BuggetRatioMounths,
                BuggetTypeText = budgetRatio.BuggetType.ToString(),
                DuesStatistics = duesStatistics?.ToList() ?? new List<DuesStatistic>()
            };

            ViewData["ActivePage"] = "BudgetOrderList";
            return View(model);
        }

        public async Task<IActionResult> Transfer(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "BudgetRatio ID gereklidir";
                return RedirectToAction("Index");
            }

            var result = await _budgetOrderService.GetBudgetRatioWithDuesAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message ?? "Kayıt bulunamadı";
                return RedirectToAction("Index");
            }

            var (budgetRatio, duesStatistics) = result.Data;

            // Mevcut kullanıcının ID'sini al
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id;

            var model = new BudgetOrderTransferViewModel
            {
                BudgetRatioId = budgetRatio.Id,
                Code = budgetRatio.Code,
                Description = budgetRatio.Description,
                Year = budgetRatio.Year,
                BuggetType = budgetRatio.BuggetType,
                DuesStatisticCount = duesStatistics?.Count() ?? 0
            };

            // Kullanıcı ID'sini ViewData ile view'a geç
            ViewData["CurrentUserId"] = currentUserId;

            return View(model);
        }

        public async Task<IActionResult> Review(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "BudgetRatio ID gereklidir";
                return RedirectToAction("Index");
            }

            var result = await _budgetOrderService.GetBudgetRatioWithDuesAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message ?? "Kayıt bulunamadı";
                return RedirectToAction("Index");
            }

            var (budgetRatio, duesStatistics) = result.Data;

            // İstatistikleri hesapla
            var totalCount = duesStatistics?.Count() ?? 0;
            var completedCount = duesStatistics?.Count(d => d.TransferStatus == TransferStatusEnum.Completed) ?? 0;
            var failedCount = duesStatistics?.Count(d => d.TransferStatus == TransferStatusEnum.Failed) ?? 0;
            var pendingCount = duesStatistics?.Count(d => d.TransferStatus == TransferStatusEnum.Pending) ?? 0;

            // Aktarılmamış kayıtlar (Pending veya Failed, Completed değil)
            var pendingItems = duesStatistics?
                .Where(d => d.TransferStatus != TransferStatusEnum.Completed)
                .Select(d => new DuesStatisticReviewItemViewModel
                {
                    Id = d.Id,
                    Code = d.Code,
                    DivName = d.DivName,
                    ClientCode = d.ClientCode,
                    Total = d.Total,
                    TransferStatus = d.TransferStatus,
                    ErrorMessage = null // Hata mesajını farklı bir yerden alabiliriz
                })
                .ToList() ?? new List<DuesStatisticReviewItemViewModel>();

            var model = new BudgetOrderReviewViewModel
            {
                BudgetRatioId = budgetRatio.Id,
                Code = budgetRatio.Code,
                Description = budgetRatio.Description,
                Year = budgetRatio.Year,
                BuggetType = budgetRatio.BuggetType,
                TotalCount = totalCount,
                CompletedCount = completedCount,
                FailedCount = failedCount,
                PendingCount = pendingCount,
                PendingItems = pendingItems
            };

            // Mevcut kullanıcının ID'sini al
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["CurrentUserId"] = currentUser?.Id;

            ViewData["ActivePage"] = "BudgetOrderList";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateBudgetRatioViewModel model)
        {
            _logger.LogInformation("Update POST başladı. Model ID: {Id}, Ratio: {Ratio}, MonthsFlag: {MonthsFlag}",
                model.Id, model.Ratio, model.BuggetRatioMounths);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState geçersiz. Hatalar: {Errors}", string.Join(", ", errors));
                return Json(new { success = false, message = "Model geçersiz: " + string.Join(", ", errors) });
            }

            // Get existing BudgetRatio
            var existingResult = await _budgetOrderService.GetBudgetRatioByIdAsync(model.Id);
            if (!existingResult.IsSuccess)
            {
                _logger.LogError("BudgetRatio bulunamadı: {Id}", model.Id);
                return Json(new { success = false, message = "Bütçe oranı bulunamadı" });
            }

            var budgetRatio = existingResult.Data;

            // Update editable fields only
            budgetRatio.Ratio = model.Ratio;
            budgetRatio.BuggetRatioMounths = model.BuggetRatioMounths;

            _logger.LogInformation("Güncelleme yapılıyor. ID: {Id}, NewRatio: {Ratio}, NewMonths: {Months}",
                budgetRatio.Id, budgetRatio.Ratio, budgetRatio.BuggetRatioMounths);

            var updateResult = await _budgetOrderService.UpdateBudgetRatioWithDuesAsync(budgetRatio, model.BuggetRatioMounths);

            if (!updateResult.IsSuccess)
            {
                _logger.LogError("Güncelleme hatası: {Message}", updateResult.Message);
                return Json(new { success = false, message = updateResult.Message });
            }

            _logger.LogInformation("Güncelleme başarılı: {Id}", model.Id);
            TempData["SuccessMessage"] = "Bütçe oranı başarıyla güncellendi";

            return Json(new { success = true, message = "Bütçe oranı başarıyla güncellendi", redirectUrl = "/BudgetOrder/Index" });
        }
    }
}
