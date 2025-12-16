using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers
{
    public class SiteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Update(string id)
        {
            // Detay sayfası için gerekli işlemler
            return View();
        }
        public IActionResult Update2(string id)
        {
            // Detay sayfası için gerekli işlemler
            return View();
        }
    }
}
