using Hangfire;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.WebUI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LogoSyncController : ControllerBase
    {
        
        private readonly IDuesStatisticService _duesStatisticService;

        public LogoSyncController(IDuesStatisticService duesStatisticService)
        {
            _duesStatisticService = duesStatisticService;
        }

        [HttpPost("Site")]
        public IActionResult TriggerLogoSync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            BackgroundJob.Enqueue<ILogoSyncJobService>(x => x.SyncFromLogoAsync(userId));
            return Ok("Logo senkronizasyonu kuyruğa alındı.");
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
