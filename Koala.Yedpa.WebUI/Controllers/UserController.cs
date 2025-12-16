using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.WebUI.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Globalization;
using System.Numerics;
using System.Security.Claims;

namespace Koala.Yedpa.WebUI.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IFileProvider _fileProvider;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public UserController(ILogger<UserController> logger, IFileProvider fileProvider, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _logger = logger;
            _fileProvider = fileProvider;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }
        public async Task<IActionResult> Index()
        {
            var url = Request.Host;
            var users = await _userManager.Users.Where(x => x.Status != StatusEnum.Deleted).ToListAsync();

            var rootFolder = _fileProvider.GetDirectoryContents("wwwroot");

            var path = rootFolder.First(x => x.Name == "assets").PhysicalPath!;

            var retVal = users.Select(item => new UserListViewModel
            {
                Status = item.Status,
                Avatar = item.Avatar, //  Path.Combine(path, $"media\\users\\{item.Avatar}"),
                Email = item.Email,
                FirstName = item.FirstName,
                Id = item.Id,
                LastName = item.LastName,
                MiddleName = item.MiddleName,
                PhoneNumber = item.PhoneNumber,
                UserName = item.UserName
            })
                .ToList();

            return View(retVal);
        }

        [AllowAnonymous]
        public IActionResult? Login(string returnUrl = "/")
        {

            var model = (new LoginViewModel(), new ForgetPasswordViewModel());
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([Bind(Prefix = "Item1")] LoginViewModel loginModel, [Bind(Prefix = "Item2")] ForgetPasswordViewModel forgetModel, string returnUrl = "/")
        {
            var model = (loginModel, forgetModel);
            var user = await _userManager.FindByNameAsync(loginModel.Email ?? "");
            //var model = (loginModel, forgetModel);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı Bilgilerinizi kontrol ederek tekrar deneyiniz");
                return View(model);
            }

            if (user.Status != StatusEnum.Active)
            {
                switch (user.Status)
                {
                    case StatusEnum.Passive:
                    case StatusEnum.Pending:
                        ModelState.AddModelError("", "Lütfen aktif bir kullanıcı ile giriş yapmayı deneyin.");
                        return View(model);
                    case StatusEnum.Deleted:
                        ModelState.AddModelError("", "Giriş yapmak istediğiniz kullanıcı bilgilerine ulaşılamadı.");
                        return View(model);
                }
            }
            //TODO : Try Catch eklenecek

            var res = await _signInManager.PasswordSignInAsync(user, loginModel.Password, loginModel.RememberMe, true);
            if (res.IsLockedOut)
            {
                ModelState.AddModelErrorList(new List<string> { "Hesabınız Kilitli, 15 dakika boyunca giriş yapamazsınız. Lütfen daha sonra tekrar deneyiniz." });
                return View(model);
            }

            if (res.IsNotAllowed)
            {
                ModelState.AddModelErrorList(new List<string> { "Erişim Yetkiniz bulunmamaktadır." });
                return View(model);
            }

            if (res.RequiresTwoFactor)
            {
                //TODO : Bu alan daha sonra doldurulacak
            }


            returnUrl = returnUrl ?? Url.Action("Index", "Dashboard");
            return Redirect(returnUrl);

        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ForgetPassword([Bind(Prefix = "Item1")] LoginViewModel loginModel, [Bind(Prefix = "Item2")] ForgetPasswordViewModel forgetModel)
        {
            var url = Request.Host;
            var model = (loginModel, forgetModel);
            //TODO : Try Catch eklenecek

            if (string.IsNullOrEmpty(forgetModel.Email))
            {
                ModelState.AddModelError(forgetModel.Email, "E-Posta Adresi Boş Bırakılamaz");
                return RedirectToAction("CreateUser", "User", model);
            }

            var user = await _userManager.FindByEmailAsync(forgetModel.Email);
            if (user == null)
            {
                ModelState.AddModelError(forgetModel.Email, "E-Posta Adresi Boş Bırakılamaz");
                TempData["Eposta"] = forgetModel.Email;
                return RedirectToAction("ForgetConfirm", "User");
            }

            var passwordResetTokenstring = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = "https://" + url.Value + Url.Action("ResetPassword", "User", new { userId = user.Id, token = passwordResetTokenstring });
            TempData["Eposta"] = forgetModel.Email;
            await _emailService.SendResetPasswordEmailAsync(new ResetPasswordEmailDto
            { Email = user.Email, Name = user.FirstName, Lastname = user.LastName, ResetLink = resetUrl });
            //TODO : Mail Gönderilecek

            return RedirectToAction("ForgetConfirm", "User");
        }
        [AllowAnonymous]
        public IActionResult ResetPassword(string userId, string token)
        {
            TempData["UserId"] = userId;
            TempData["Token"] = token;

            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //TODO : Try Catch eklenecek

            var userId = TempData["UserId"];
            var token = TempData["Token"];
            if (userId == null || token == null)
            {
                throw new Exception("İşlem sırasında bir hata meydana geldi");
            }
            var user = await _userManager.FindByIdAsync(userId!.ToString());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı bilgilerine ulaşılamadı.");
                return View(model);
            }

            var res = await _userManager.ResetPasswordAsync(user, token!.ToString(), model.Password);
            if (res.Succeeded)
            {
                await _emailService.SendChangePasswordEmailAsync(new CustomEmailDto
                { Email = user.Email, Lastname = user.LastName, Name = user.FirstName });
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Şifreniz değiştirilirken bazı sorunlarla karşılaşıldı");
            foreach (var error in res.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Code + " - " + error.Description);

            }
            return View(model);

        }
        [AllowAnonymous]
        public IActionResult ForgetConfirm()
        {
            var model = new ForgetPasswordViewModel { Email = TempData["Eposta"]?.ToString() };
            return View(model);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //TODO : Try Catch eklenecek
            var currentUser = User;
            var user = await _userManager.GetUserAsync(User);

            try
            {
                var res = await _userManager.ChangePasswordAsync(user!, model.OldPassword, model.NewPassword);
                if (!res.Succeeded)
                {
                    foreach (var identityError in res.Errors)
                    {
                        ModelState.AddModelError(string.Empty, identityError.Code + " - " + identityError.Description);
                    }
                }
                TempData["SuccessMessage"] = "Şifreniz Başarıyla değiştirildi";
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Şifre Değiştirilirken Bir Sorunla Karşılaşıldı");
                return View(model);
            }

            await _signInManager.SignOutAsync();
            return View();
        }
       

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            return View(new CreateAppUserViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateAppUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var IsAdded = _userManager.Users.Any(x => x.UserName == model.Email || x.Email == model.Email);
            if (IsAdded)
            {
                ModelState.AddModelError("", "Bu Kullanıcı Adı veya E-Posta Adresi Zaten Kayıtlı");
                return View(model);
            }
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Status = StatusEnum.Active,
                Id = Tools.CreateGuidStr(),
                LockoutEnabled = true
            };
            if (model.Avatar != null && model.Avatar.Length > 0)
            {
                var rootFolder = _fileProvider.GetDirectoryContents("wwwroot");

                var fileName = $"{Tools.CreateGuidStr()}{Path.GetExtension(model.Avatar.FileName)}";
                var path = rootFolder.First(x => x.Name == "assets").PhysicalPath!;
                var newPath = Path.Combine(path, $"media\\users\\{fileName}");
                await using var stream = new FileStream(newPath, FileMode.Create);
                await model.Avatar.CopyToAsync(stream);
                user.Avatar = fileName;
                stream.Close();
            }
            else
            {
                user.Avatar = "logo002.png";
            }
            var res = _userManager.CreateAsync(user, model.Password);
            if (!res.Result.Succeeded)
            {
                ModelState.AddModelError("", res.Result.Errors.FirstOrDefault().Description);
                return View(model);
            }
            TempData["InfoMessage"] = $"{model.ToString()} isimli kullanıcı {model.Email} Kullanıcı adıyla Oluşturuldu";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = ResponseDto.Fail(404, "Kullanıcı Bulunamadı", "Böyle bir kullanıcı bulunamadı", true);
                return View("Error");
            }
            var retVal = new UpdateAppUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Removed = "No"
            };
            TempData["Avatar"] = user.Avatar;
            return View(retVal);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(UpdateAppUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            TempData["Avatar"] = user.Avatar;
            if (user == null)
            {
                TempData["Error"] = ResponseDto.Fail(404, "Kullanıcı Bulunamadı", "Böyle bir kullanıcı bulunamadı", true);
                return View("Error");
            }
            user!.FirstName = model.FirstName;
            user!.MiddleName = model.MiddleName;
            user!.LastName = model.LastName;
            user!.PhoneNumber = model.PhoneNumber;

            if (model.Avatar == null && string.Equals(model.Removed, "Yes", StringComparison.CurrentCultureIgnoreCase))
            {
                user.Avatar = "logo002.png";
            }
            if (model.Avatar != null && model.Avatar.Length > 0)
            {
                var rootFolder = _fileProvider.GetDirectoryContents("wwwroot");

                var fileName = $"{Tools.CreateGuidStr()}{Path.GetExtension(model.Avatar.FileName)}";
                var path = rootFolder.First(x => x.Name == "assets").PhysicalPath!;
                var newPath = Path.Combine(path, $"media\\users\\{fileName}");
                await using var stream = new FileStream(newPath, FileMode.Create);
                await model.Avatar.CopyToAsync(stream);
                user.Avatar = fileName;
                stream.Close();
            }

            try
            {
                var res = await _userManager.UpdateAsync(user);
                //if (res.Succeeded)
                //{
                //    await _userManager.UpdateSecurityStampAsync(user);
                //    await _signInManager.SignOutAsync();
                //    await _signInManager.SignOutAsync(user);
                //}
            }
            catch (Exception ex)
            {
                TempData["Error"] = ResponseDto.Fail(404, "Kullanıcı Güncellenirken Bir Sorunla Karşılaşıldı", ex.Message, true);
                return View("Error");
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> AsignRoleToUser(string userId)
        {
            ViewBag.UserId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            var retVal = new AppUserInfoViewModels
            {
                Avatar = user.Avatar,
                Email = user.Email,
                FirstName = user.FirstName,
                Id = user.Id,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName
            };
            var roles = await _roleManager.Roles.ToListAsync();
            var model = new List<AsignRoleToUserViewModel>();
            ViewData["UserInfo"] = retVal;

            foreach (var item in roles)
            {
                if (item.DisplayName== "Süper Yönetici")
                {
                    continue;
                }
                var modelItem = new AsignRoleToUserViewModel
                {
                    Name = item.DisplayName,
                    Description = item.Description,
                    Id = item.Id
                };
                var useRoles = await _userManager.GetRolesAsync(user!);
                modelItem.IsExist = useRoles.Contains(item.Name!);
                model.Add(modelItem);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AsignRoleToUser(List<AsignRoleToUserViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var retVal = new AppUserInfoViewModels
            {
                Avatar = user.Avatar,
                Email = user.Email,
                FirstName = user.FirstName,
                Id = user.Id,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName
            };
            ViewData["UserInfo"] = retVal;
            foreach (var item in model)
            {
                if (item.IsExist)
                {
                    await _userManager.AddToRoleAsync(user!, item.Name);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user!, item.Name);
                }
            }

            TempData["InfoMessage"] = "Kullanıcı Rolleri Başarıyla Güncellendi";
            return RedirectToAction("Index", "User");
        }
        #region Json Results

        [HttpPost]
        public async Task<JsonResult> UserChangeStatus(UpdateUserStatusViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return Json(ResponseDto.Fail(404, "Kullanıcı Bulunamadı", "Böyle bir kullanıcı bulunamadı", true));
            }
            user.Status = model.Status;
            try
            {
                var res = await _userManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                }
            }
            catch (Exception ex)
            {
                return Json(ResponseDto.Fail(404, "Kullanıcı Güncellenirken Bir Sorunla Karşılaşıldı", ex.Message, true));
            }

            return Json(ResponseDto.Success(200, "Kullanıcı Başarıyla Güncellendi"));
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }
        public async Task<IActionResult> UserProfile()
        {
            var currentUser = User;
            var user = await _userManager.GetUserAsync(User);
            var model = new UserProfileUpdateViewModel
            {
                Email = user.Email,
                LastName = user.LastName,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                PhoneNumber = user.PhoneNumber,
                Id = user.Id,
                Removed = "No"

            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> UserProfile(UserProfileUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var currentUser = User;
            var user = await _userManager.GetUserAsync(User);

            user!.LastName = model.LastName;
            user!.FirstName = model.FirstName;
            user!.MiddleName = model.MiddleName;
            user!.PhoneNumber = model.PhoneNumber;
            user!.Email = model.Email;

            if (model.Avatar != null && model.Avatar.Length > 0)
            {
                var rootFolder = _fileProvider.GetDirectoryContents("wwwroot");

                var fileName = $"{Tools.CreateGuidStr()}{Path.GetExtension(model.Avatar.FileName)}";
                var newPath = Path.Combine(rootFolder.First(x => x.Name == "media").PhysicalPath!, $"avatar\\{fileName}");
                await using var stream = new FileStream(newPath, FileMode.Create);
                await model.Avatar.CopyToAsync(stream);
                user.Avatar = fileName;
                stream.Close();
            }
            //TODO : Try Catch eklenecek
            try
            {
                var res = await _userManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil güncellenirken bir sorunla karşılaşıldı");
            }
            return View();
        }


        #endregion
    }
}
