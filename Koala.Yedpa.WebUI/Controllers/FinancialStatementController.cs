using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Koala.Yedpa.WebUI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    [SwaggerTag("Financial Statement API")]

    public class FinancialStatementController : ControllerBase
    {
        private readonly IApiLogoSqlDataService _service;

        public FinancialStatementController(IApiLogoSqlDataService service)
        {
            _service = service;
        }

        /// <summary>
        /// Cari hesap özet bilgilerini getirir
        /// </summary>
        [HttpGet("GetClsStatementsSummert")]
        [SwaggerOperation(Summary = "Cari hesap özet bilgilerini getirir")]
        [SwaggerResponse(200, "Başarılı", typeof(ResponseDto<List<StatementSummeryViewModel>>))]
        [SwaggerResponse(401, "Yetkisiz erişim")]
        [SwaggerResponse(500, "Sunucu hatası")]
        public async Task<IActionResult> GetClsStatementsSummert()
        {
            var result = await _service.GetClsStatementsSummertAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Belirli bir cari hesap için detaylı ekstre getirir
        /// </summary>
        [HttpGet("GetClCardStatement")]
        [SwaggerOperation(Summary = "Belirli bir cari hesap için detaylı ekstre getirir")]
        [SwaggerResponse(200, "Başarılı", typeof(ResponseDto<List<ClCardStatementViewModel>>))]
        [SwaggerResponse(400, "Geçersiz istek")]
        [SwaggerResponse(401, "Yetkisiz erişim")]
        [SwaggerResponse(500, "Sunucu hatası")]
        public async Task<IActionResult> GetClCardStatement([FromQuery] string clCode)
        {
            if (string.IsNullOrWhiteSpace(clCode))
            {
                return BadRequest(ResponseDto<List<ClCardStatementViewModel>>.FailData(
                    400, "Cari kodu boş olamaz", "clCode parametresi gereklidir", true));
            }

            var result = await _service.GetClCardStatementAsync(clCode);
            return StatusCode(result.StatusCode, result);
        }
    }
}





