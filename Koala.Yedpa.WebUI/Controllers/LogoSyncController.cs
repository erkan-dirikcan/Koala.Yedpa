using Koala.Yedpa.Core.Services;
using Koala.Yedpa.WebUI.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
   //[ApiExplorerSettings(IgnoreApi = true)]
    [Permission(PermissionCatalog.SystemMaintenance.LogoSync)]
    public class LogoSyncController : ControllerBase
    {

        private readonly IDuesStatisticService _duesStatisticService;

        public LogoSyncController(IDuesStatisticService duesStatisticService)
        {
            _duesStatisticService = duesStatisticService;
        }

        [HttpGet]
        public async Task<IActionResult> SyncDuesStatisticYearData()
        {
            var currentYear = DateTime.Now.Year.ToString();
            var result = await _duesStatisticService.SyncYearDataAsync(currentYear);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
            //return Ok("LogoSyncController is working.");
        }
    }
}
