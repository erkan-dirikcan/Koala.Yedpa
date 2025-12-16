using Hangfire;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Koala.Yedpa.WebUI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LogoSyncController : ControllerBase
    {
        [HttpPost("Site")]
        public IActionResult TriggerLogoSync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            BackgroundJob.Enqueue<ILogoSyncJobService>(x => x.SyncFromLogoAsync(userId));
            return Ok("Logo senkronizasyonu kuyruğa alındı.");
        }
    }
}
