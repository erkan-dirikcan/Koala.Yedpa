using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers.Yonetim;

/// <summary>
/// Arıza Yönetimi MVC Controller
/// </summary>
[Authorize]
public class ArizaController : Controller
{
    private readonly ILogger<ArizaController> _logger;

    public ArizaController(ILogger<ArizaController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Arıza listesi sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Arıza detay sayfası (hareketler ile birlikte)
    /// </summary>
    [HttpGet]
    public IActionResult Detay(int id)
    {
        ViewData["ArizaId"] = id;
        return View();
    }

    /// <summary>
    /// Yeni arıza oluşturma sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Yeni()
    {
        return View();
    }

    /// <summary>
    /// Arıza atama/personel atama sayfası
    /// </summary>
    [HttpGet]
    public IActionResult Atama(int id)
    {
        ViewData["ArizaId"] = id;
        return View();
    }
}
