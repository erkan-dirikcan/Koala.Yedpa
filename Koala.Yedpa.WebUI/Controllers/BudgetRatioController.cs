using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers
{
    public class BudgetRatioController : Controller
    {
        private readonly IBudgetRatioService _budgetRatioService;

        public BudgetRatioController(IBudgetRatioService budgetRatioService)
        {
            _budgetRatioService = budgetRatioService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _budgetRatioService.GetAllAsync();
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Data;
                return View("Error");
            }
            return View(result.Data);
        }
        public async Task<IActionResult> CreateBudget()
        {
            return View();
        }
    }
}
