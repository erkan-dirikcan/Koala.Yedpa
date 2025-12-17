using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers
{

    public class SettingsController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ISettingsService _settingsService;

        public SettingsController(IEmailService emailService, ISettingsService settingsService)
        {
            _emailService = emailService;
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<IActionResult> EmailSettings()
        {
            //EmailSettingViewModel
            var settings = await _settingsService.GetEmailSettingsAsync();
            if (settings.IsSuccess) return View(settings.Data);
            TempData["Error"] = settings;
            return View("Error");
        }
        [HttpPost]
        public async Task<IActionResult> EmailSettings(EmailSettingViewModel model)
        {
            var settings = await _settingsService.GetEmailSettingsAsync();
            if (!settings.IsSuccess)
            {
                TempData["Error"] = settings;
                return View("Error");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var res = await _settingsService.UpdateEmailSettingsAsync(model);
            if (!res.IsSuccess)
            {
                TempData["Error"] = res;
                return View("Error");
            }
            settings = await _settingsService.GetEmailSettingsAsync();
            if (!settings.IsSuccess)
            {
                TempData["Error"] = settings;
                return View("Error");
            }

            return View(settings.Data);
        }
        public async Task<IActionResult> LogoSettings()
        {
            var logoSetting = await _settingsService.GetLogoSettingsAsync();
            if (!logoSetting.IsSuccess)
            {
                TempData["Error"] = logoSetting;
                return View("Error");
            }
            return View(logoSetting.Data);
        }
        [HttpPost]
        public async Task<IActionResult> LogoSettings(LogoSettingViewModel model)
        {
            var res = await _settingsService.UpdateLogoSettingsAsync(model);
            if (!res.IsSuccess)
            {
                TempData["Error"] = res;
                return View("Error");
            }
            return View(model);
        }
        public async Task<IActionResult> LogoSqlSettings()
        {
            var logoSqlSetting = await _settingsService.GetLogoSqlSettingsAsync();
            if (!logoSqlSetting.IsSuccess)
            {
                TempData["Error"] = logoSqlSetting;
                return View("Error");
            }
            return View(logoSqlSetting.Data);
        }
        [HttpPost]
        public async Task<IActionResult> LogoSqlSettings(LogoSqlSettingViewModel model)
        {
            var res = await _settingsService.UpdateLogoSqlSettingsAsync(model);
            return View();
        }
        public async Task<IActionResult> LogoRestServiceSettings()
        {
            var res = await _settingsService.GetLogoRestServiceSettingsAsync();
            return View(res.Data);
        }
        [HttpPost]
        public async Task<IActionResult> LogoRestServiceSettings(LogoRestServiceSettingViewModel model)
        {
            var res = await _settingsService.UpdateLogoRestServiceSettingsAsync(model);
            return View();
        }

    }
}
