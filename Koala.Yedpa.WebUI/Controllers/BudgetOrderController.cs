using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers
{
    [Authorize]
    public class BudgetOrderController : Controller
    {
        private readonly ILogger<BudgetOrderController> _logger;

        public BudgetOrderController(ILogger<BudgetOrderController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }
    }
}
