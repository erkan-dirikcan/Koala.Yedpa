using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsApiController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetSettings()
        {
            // Placeholder for getting settings logic
            return Ok(new { Setting1 = "Value1", Setting2 = "Value2" });
        }
    }
}
