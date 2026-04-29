using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers.Yonetim;

/// <summary>
/// Sözleşme Yönetimi MVC Controller
/// </summary>
[Authorize]
public class SozlesmeController : Controller
{
    private readonly ILogger<SozlesmeController> _logger;

    public SozlesmeController(ILogger<SozlesmeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Sözleşme listesi sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Sözleşme detay sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Detay(int id)
    {
        ViewData["SozlesmeId"] = id;
        return View();
    }

    /// <summary>
    /// Yeni sözleşme oluşturma sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Yeni()
    {
        return View();
    }

    /// <summary>
    /// Sözleşme düzenleme sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Duzenle(int id)
    {
        ViewData["SozlesmeID"] = id;
        return View();
    }

    /// <summary>
    /// Sözleşme PDF yazdırma
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Yazdir(int id)
    {
        // API'den PDF çek ve download et
        return await Task.FromResult(File(new byte[0], "application/pdf"));
    }
}
