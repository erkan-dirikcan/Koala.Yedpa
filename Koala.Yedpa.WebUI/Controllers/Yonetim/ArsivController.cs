using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers.Yonetim;

/// <summary>
/// Arşiv Yönetimi MVC Controller
/// </summary>
[Authorize]
public class ArsivController : Controller
{
    private readonly ILogger<ArsivController> _logger;

    public ArsivController(ILogger<ArsivController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Arşiv ana sayfa - Raf/Bölme/Koli ağaç görünümü
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Koli detay sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Detay(int id)
    {
        ViewData["KoliId"] = id;
        return View();
    }

    /// <summary>
    /// Yeni koli ekleme modal/popup
    /// </summary>
    [HttpGet]
    public IActionResult KoliEkle(int bolmeId)
    {
        ViewData["BolmeId"] = bolmeId;
        return PartialView("_KoliEkle");
    }
}
