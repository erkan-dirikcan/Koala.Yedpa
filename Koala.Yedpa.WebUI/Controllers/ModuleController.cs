using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers
{
    public class ModuleController(IModuleService moduleService) : Controller
    {
        private readonly IModuleService _service = moduleService ?? throw new ArgumentNullException(nameof(moduleService));

        public async Task<IActionResult> Index()
        {
            var res = await _service.GetAllModuleAsync();
            var retVal = res.Data.Where(x => x.Status != StatusEnum.Deleted);
            return View(retVal);
        }

        public IActionResult CreateModule()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateModule(CreateModuleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var res = await _service.CreateModuleAsync(model);
                if (res.IsSuccess)
                {
                    return RedirectToAction("Index", "Module");
                }
                ModelState.AddModelError("", res.Message);
            }
            else
            {
                ModelState.AddModelError("", "Model is not valid");
            }
            return View(model);
        }

        public async Task<IActionResult> UpdateModule(string id)
        {
            var res = await _service.GetUpdateModel(id);
            if (res.IsSuccess) return View(res.Data);
            TempData["Error"] = res;
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateModule(UpdateModuleViewModel model)
        {
            var res = await _service.UpdateModule(model, model.Id);
            if (!res.IsSuccess) return RedirectToAction("Index", "Module");
            TempData["Error"] = res;
            return View("Error");
        }

        [HttpPost] 
        public async Task<JsonResult> ChangeStatus(ModuleChangeStatusViewModel model)
        {
            var res = await _service.ModuleChangeStatus(model);
            return Json(res);
        }
    }
}
