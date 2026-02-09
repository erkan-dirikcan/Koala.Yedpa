using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers;

public class WorkplaceController : Controller
{
    private readonly IWorkplaceService _workplaceService;
    private readonly ILogger<WorkplaceController> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public WorkplaceController(
        IWorkplaceService workplaceService,
        ILogger<WorkplaceController> logger,
        IWebHostEnvironment webHostEnvironment)
    {
        _workplaceService = workplaceService;
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "WorkplaceIndex";
        var result = await _workplaceService.GetAllAsync();
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = "İşyerleri listesi getirilemedi.";
            return View(new List<WorkplaceListViewModel>());
        }
        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["ErrorMessage"] = "Geçersiz kayıt ID'si.";
            return RedirectToAction("Index");
        }

        var result = await _workplaceService.GetByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Kayıt bulunamadı.";
            return RedirectToAction("Index");
        }

        ViewData["ActivePage"] = "WorkplaceDetail";
        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Update(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["ErrorMessage"] = "Geçersiz kayıt ID'si.";
            return RedirectToAction("Index");
        }

        var result = await _workplaceService.GetByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Kayıt bulunamadı.";
            return RedirectToAction("Index");
        }

        ViewData["ActivePage"] = "WorkplaceUpdate";
        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(WorkplaceDetailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Girdiğiniz bilgiler doğrulanamadı.";
            return View(model);
        }

        var result = await _workplaceService.UpdateAsync(model);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Message ?? "Güncelleme başarısız.";
            return View(model);
        }

        TempData["SuccessMessage"] = "İşyeri bilgileri başarıyla güncellendi.";
        return RedirectToAction("Detail", new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendBulkBudgetEmails(int year)
    {
        try
        {
            _logger.LogInformation("SendBulkBudgetEmails called for year: {Year}", year);

            var result = await _workplaceService.SendBulkBudgetEmailsAsync(year);

            if (result.IsSuccess)
            {
                var data = result.Data;
                TempData["SuccessMessage"] = $"Toplu mail gönderimi tamamlandı. " +
                    $"Toplam İşyeri: {data.TotalWorkplaces}, " +
                    $"Başarılı: {data.TotalEmailsSent}, " +
                    $"Başarısız: {data.TotalEmailsFailed}";
                return Json(new { success = true, message = TempData["SuccessMessage"], data });
            }
            else
            {
                TempData["ErrorMessage"] = result.Message ?? "Toplu mail gönderimi başarısız.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk budget emails");
            TempData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
            return Json(new { success = false, message = TempData["ErrorMessage"] });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateBudgetExcel(int year)
    {
        try
        {
            _logger.LogInformation("GenerateBudgetExcel called for year: {Year}", year);

            var result = await _workplaceService.GenerateBudgetExcelAsync(year);

            if (result.IsSuccess && result.Data != null)
            {
                return File(
                    result.Data,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Butce_{year}.xlsx"
                );
            }
            else
            {
                TempData["ErrorMessage"] = result.Message ?? "Excel oluşturma başarısız.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating budget Excel");
            TempData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
            return Json(new { success = false, message = TempData["ErrorMessage"] });
        }
    }
}
