using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers.Yonetim;

/// <summary>
/// Otopark Yönetimi MVC Controller
/// </summary>
[Authorize]
public class OtoparkController : Controller
{
    private readonly ILogger<OtoparkController> _logger;

    public OtoparkController(ILogger<OtoparkController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Otopark durumu/listesi sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Giriş işlemi sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Giris()
    {
        return View();
    }

    /// <summary>
    /// Çıkış işlemi sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Cikis()
    {
        return View();
    }

    /// <summary>
    /// Abonelik yönetimi sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Abonelik()
    {
        return View();
    }
}
