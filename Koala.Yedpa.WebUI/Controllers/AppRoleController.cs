using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Koala.Yedpa.WebUI.Controllers
{
    public class AppRoleController : Controller
    {
        private readonly ILogger<AppRoleController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClaimsService _claimService;
        private readonly IModuleService _moduleService;

        public AppRoleController(ILogger<AppRoleController> logger, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager, IHttpContextAccessor httpContextAccessor, IClaimsService claimsService, IModuleService moduleService)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _claimService = claimsService;
            _moduleService = moduleService;
        }

        public IActionResult Index()
        {
            
            var roles = _roleManager.Roles.ToList();
            var retVal = roles.Select(role => new AppRoleListViewModel
            {
                Id = role.Id,
                Description = role.Description,
                DisplayName = role.DisplayName,
                Name = role.Name
            }).ToList();
            return View(retVal);
        }

      


        [HttpGet]
        public IActionResult CreateRole()
        {
            return View(new CreateAppRoleViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateAppRoleViewModel model)
        {
            try
            {
                await _roleManager.CreateAsync(new AppRole
                {
                    Name = model.Name,
                    Description = model.Description,
                    DisplayName = model.DisplayName,
                    StatusEnum = Core.Dtos.StatusEnum.Active,
                    Id = Tools.CreateGuidStr()
                });

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult UpdateRole(string id)
        {
            var role = _roleManager.Roles.FirstOrDefault(x => x.Id == id);
            if (role == null)
            {
                var infoMessage = TempData["InfoMessage"] == null ? "" : TempData["InfoMessage"].ToString();
                TempData["ErrorMessage"] = "Güncellenmek istenilen rol bilgilerine ulaşılamadı";
                return RedirectToAction("Index");
            }

            var retVal = new UpdateAppRoleViewModel
            {
                Id = role.Id,
                Description = role.Description,
                DisplayName = role.DisplayName,
                Name = role.Name
            };
            return View(retVal);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateRole(UpdateAppRoleViewModel model)
        {
            var appRole = await _roleManager.FindByIdAsync(model.Id);
            appRole.DisplayName = model.DisplayName;
            appRole.Description = model.Description;
            appRole.Name = model.Name;
            try
            {
                await _roleManager.UpdateAsync(appRole);
                return RedirectToAction("Index", "AppRole");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    ResponseDto.Fail(400, "Rol Güncellenirken Bir Sorunla Karşılaşıldı", ex.Message, true);
                return View("error");
            }

        }


        [HttpGet]
        public async Task<IActionResult> AddClaimToRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = Core.Dtos.ResponseDto.Fail(404, "Role Id Bilgisi Alınamadı", "Role Id Bilgisi Alınamadı", true);
                return View("Error");
            }
            var role = await _roleManager.FindByIdAsync(id);
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var claims = await _claimService.GetClaimToRoleList();
            var claimData = new List<SelectListDto<string>>();
            var modules = await _moduleService.GetAllModuleAsync();
            foreach (var claim in claims.Data)
            {
                var isSelected = roleClaims.Any(x => x.Value == $"{claim.Name}");

                claimData.Add(new SelectListDto<string>
                {
                    IsSelected = isSelected,
                    Key = $"{modules.Data.FirstOrDefault(x => x.Id == claim.ModuleId).DisplayName} - {claim.DisplayName}",
                    Val = claim.Name
                });
            }
            claimData = claimData.OrderBy(x => x.Key).ToList();
            TempData["Claims"] = claimData.OrderBy(x => x.Key).ToList();
            ViewData["RoleInfo"] = $"{role.Name}";
            var model = new AddClaimToRoleViewModel { RoleId = id, Claims = new List<string>() };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddClaimToRole(AddClaimToRoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var claims = await _claimService.GetClaimToRoleList();
            var claimData = new List<SelectListDto<string>>();
            foreach (var claim in claims.Data)
            {
                var isSelected = roleClaims.Any(x => x.Value == claim.Name);
                claimData.Add(new SelectListDto<string>
                {
                    IsSelected = isSelected,
                    Key = claim.DisplayName,
                    Val = claim.Name
                });
            }
            claimData = claimData.OrderBy(x => x.Key).ToList();
            TempData["Claims"] = claimData;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var currentClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in currentClaims)
            {
                if (claim.Type == "Permission")
                {
                    await _roleManager.RemoveClaimAsync(role, claim);

                }
            }

            foreach (var item in model.Claims)
            {
                await _roleManager.AddClaimAsync(role, new Claim("Permission", item));
            }
            TempData.Clear();
            return RedirectToAction("Index");
        }



        [HttpPost]
        public async Task<JsonResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return Json(ResponseDto.Fail(404, "Böyle bir rol bulunamadı", "Böyle bir rol bulunamadı", true));
            }
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                return Json(ResponseDto.Fail(400, "Bu rolü silmek için önce bu role atanmış kullanıcıları kaldırmalısınız.", "Bu rolü silmek için önce bu role atanmış kullanıcıları kaldırmalısınız.", true));
            }
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return Json(ResponseDto.Success(200, "Rol başarıyla silindi."));
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Json(ResponseDto.Fail(500, "Rol silinirken bir hata oluştu.", errors, true));
            }
        }




    }
}