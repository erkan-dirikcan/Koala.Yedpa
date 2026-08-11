using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.WebUI.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers
{
    public class ClaimsController(IClaimsService service, IModuleService moduleService) : Controller
    {
        private readonly IClaimsService _service = service ?? throw new ArgumentNullException(nameof(service));
        private readonly IModuleService _moduleService = moduleService ?? throw new ArgumentNullException(nameof(moduleService));

        [Permission(PermissionCatalog.ModuleManagement.ClaimList)]
        public async Task<IActionResult> ModuleClaims(string moduleId)
        {
            var module = await _moduleService.GetModuleByIdAsync(moduleId);
            var res = await _service.GetModuleClaims(moduleId);
            if (!res.IsSuccess)
            {
                TempData["Error"] = res;
                return View("Error");
            }

            ViewData["Module"] = module.Data;
            return View(res.Data);
        }

        //[HttpPost]
        //public async Task<IActionResult> Index(SearchClaimViewModel model)
        //{
        //    var claims = await _service.FindClaims(model);
        //    if (!claims.IsSuccess)
        //    {
        //        TempData["Error"] = res;
        //        return View("Error");
        //    }

        //    return View(claims.Data);
        //}



        [HttpGet]
        [Permission(PermissionCatalog.ModuleManagement.ClaimCreate)]
        public async Task<IActionResult> CreateClaim(string moduleId)
        {
            var module = await _moduleService.GetModuleByIdAsync(moduleId);
            ViewData["Module"] = module.Data;
            return View(new CreateClaimsViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(PermissionCatalog.ModuleManagement.ClaimCreate)]
        public async Task<IActionResult> CreateClaim(CreateClaimsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var module = await _moduleService.GetModuleByIdAsync(model.ModuleId);
            ViewData["Module"] = module.Data;
            var response = await _service.CreateClaim(model);
            if (!response.IsSuccess)
            {
                return View(model);
            }

            return RedirectToAction("ModuleClaims", "Claims", new { moduleId = model.ModuleId });

        }

        [HttpGet]
        [Permission(PermissionCatalog.ModuleManagement.ClaimUpdate)]
        public async Task<IActionResult> UpdateClaim(string id)
        {
            var res = await _service.GetClaimById(id);
            if (res.IsSuccess) return View(res.Data);
            TempData["Error"] = res;
            return View("Error");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(PermissionCatalog.ModuleManagement.ClaimUpdate)]
        public async Task<IActionResult> UpdateClaim(UpdateClaimsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var res = await _service.UpdateClaim(model);
            if (res.IsSuccess)
            {
                return RedirectToAction("ModuleClaims", "Claims", new { moduleId = model.ModuleId });
            }
            TempData["Error"] = res;
            return View("Error");
        }

        [HttpGet]
        [Permission(PermissionCatalog.ModuleManagement.ClaimDelete)]
        public async Task<JsonResult> DeleteClaim(string id)
        {
            var claim = await _service.GetClaimById(id);
            if (!claim.IsSuccess)
            {
                return Json(claim);

                //TempData["Error"] = claim;
                //return View("Error");
            }
            var moduleId = claim.Data.ModuleId;
            var res = await _service.DeleteClaim(id);
            return Json(res);
            //TempData["Error"] = res;
            //return View("Error");
        }
    }
}
