using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers;

public class WorkplaceController : Controller
{
    private readonly IWorkplaceService _workplaceService;

    public WorkplaceController(IWorkplaceService workplaceService)
    {
        _workplaceService = workplaceService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "WorkplaceList";
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

        return View(result.Data);
    }
}
