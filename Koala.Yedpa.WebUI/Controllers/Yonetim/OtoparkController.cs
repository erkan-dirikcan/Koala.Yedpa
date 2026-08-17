using Koala.Yedpa.WebUI.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers.Yonetim;

/// <summary>
/// Otopark Yönetimi MVC Controller
/// </summary>
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
    [Permission(PermissionCatalog.Management.OtoparkView)]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Giriş işlemi sayfası
    /// </summary>
    [HttpGet]
    [Permission(PermissionCatalog.Management.OtoparkManage)]
    public IActionResult Giris()
    {
        return View();
    }

    /// <summary>
    /// Çıkış işlemi sayfası
    /// </summary>
    [HttpGet]
    [Permission(PermissionCatalog.Management.OtoparkManage)]
    public IActionResult Cikis()
    {
        return View();
    }

    /// <summary>
    /// Abonelik yönetimi sayfası
    /// </summary>
    [HttpGet]
    [Permission(PermissionCatalog.Management.OtoparkManage)]
    public IActionResult Abonelik()
    {
        return View();
    }
}
