using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Koala.Yedpa.WebUI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Produces("application/json")]
    [SwaggerTag("Logo Cari & Dükkan Bilgileri API")]

    public class LogoClCardApiController : ControllerBase
    {
        private readonly IApiLogoSqlDataService _service;

        public LogoClCardApiController(IApiLogoSqlDataService service)
        {
            _service = service;
        }
        [HttpGet()]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Test1()
        {
            return Ok();
        }

        /// <summary>
        /// Tüm dükkan cari bilgilerini sayfalı getirir
        /// </summary>
        [HttpGet("ClCardInfoAll")]
        [SwaggerOperation(Summary = "Tüm dükkan cari bilgilerini sayfalı getirir")]
        [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<ClCardInfoViewModel>>))]
        [SwaggerResponse(401, "Yetkisiz erişim")]
        [SwaggerResponse(500, "Sunucu hatası")]
        public async Task<IActionResult> ClCardInfoAll(
            [FromQuery] int perPage = 50,
            [FromQuery] int pageNo = 1)
        {
            var result = await _service.GetAllClCardInfoAsync(perPage, pageNo);

            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Gelişmiş arama - tüm alanlarda filtreleme yapar
        /// </summary>
        [HttpPost("ClCardInfoSearch")]
        [SwaggerOperation(Summary = "Gelişmiş filtreleme ile dükkan cari araması")]
        [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<ClCardInfoViewModel>>))]
        [SwaggerResponse(400, "Geçersiz istek")]
        [SwaggerResponse(401, "Yetkisiz erişim")]
        [SwaggerResponse(500, "Sunucu hatası")]
        public async Task<IActionResult> Search(
            [FromBody] ClCardInfoSearchViewModel searchModel,
            [FromQuery] int perPage = 50,
            [FromQuery] int pageNo = 1)
        {
            if (searchModel == null)
                return BadRequest(ResponseListDto<List<ClCardInfoViewModel>>.FailData(
                    400, "Arama modeli boş olamaz", "Model null", true));

            var result = await _service.WhereClCardInfoAsync(searchModel, perPage, pageNo);

            return StatusCode(result.StatusCode, result);
        }


        [HttpGet("test")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [SwaggerOperation(Summary = "API erişim testi - token geçerli mi?")]
        public IActionResult Test()
        {
            //return Ok();
            return Ok(new { message = "LogoClCard API çalışıyor! Token geçerli.", user = User.Identity?.Name });
        }
    }


}

