using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Koala.Yedpa.WebApi.Controllers;

/// <summary>
/// Bütçe / Ek Bütçe Oranları
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class BudgetRatioApiController : ControllerBase
{
    private readonly IBudgetRatioService _budgetRatioService;

    public BudgetRatioApiController(IBudgetRatioService budgetRatioService)
    {
        _budgetRatioService = budgetRatioService;
    }

    /// <summary>
    /// Id'ye göre Bütçe / Ek Bütçe'yi alın
    /// </summary>
    /// <param name="id">Bütçe / Ek Bütçe Id</param>
    /// <returns>Bütçe / Ek Bütçe Detayı</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Id'ye göre Bütçe / Ek Bütçe'yi alın")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<ClCardInfoViewModel>>))]
    [SwaggerResponse(400, "Geçersiz istek")]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _budgetRatioService.GetByIdAsync(id);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Tüm Bütçe / Ek Bütçe Kayıtları
    /// </summary>
    /// <returns>Bütçe / Ek Bütçe Listesi</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Tüm Bütçe / Ek Bütçe Kayıtları")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<ClCardInfoViewModel>>))]
    [SwaggerResponse(400, "Geçersiz istek")]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _budgetRatioService.GetAllAsync();
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Yıla Göre Bütçe / Ek Bütçe Kayıtları
    /// </summary>
    /// <param name="year">Yıl</param>
    /// <returns>Yıla Bütçe / Ek Bütçe Listesi</returns>
    [HttpGet("year/{year}")]
    [SwaggerOperation(Summary = "Yıla Göre Bütçe / Ek Bütçe Kayıtları")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<ClCardInfoViewModel>>))]
    [SwaggerResponse(400, "Geçersiz istek")]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> GetByYear(int year)
    {
        var result = await _budgetRatioService.GetByYearAsync(year);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Yeni Bütçe / Ek Bütçe Ekle
    /// </summary>
    /// <param name="model">CreateBudgetRatioViewModel</param>
    /// <returns>Bütçe / Ek Bütçe Oluşturma Sonucu</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni Bütçe / Ek Bütçe Ekle")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<ClCardInfoViewModel>>))]
    [SwaggerResponse(400, "Geçersiz istek")]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> Create([FromBody] CreateBudgetRatioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _budgetRatioService.CreateAsync(model);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Bütçe / Ek Bütçe Güncelle
    /// </summary>
    /// <param name="model">UpdateBudgetRatioViewModel</param>
    /// <returns>Bütçe / Ek Bütçe Güncelleme Sonucu</returns>
    [HttpPut]
    [SwaggerOperation(Summary = "Bütçe / Ek Bütçe Güncelle")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<ClCardInfoViewModel>>))]
    [SwaggerResponse(400, "Geçersiz istek")]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> Update([FromBody] UpdateBudgetRatioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _budgetRatioService.UpdateAsync(model);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Bütçe / Ek Bütçe Sil
    /// </summary>
    /// <param name="id">BudgetRatio ID</param>
    /// <returns>Bütçe / Ek Bütçe Silme Sonucu</returns>
    [HttpDelete("{id}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<ClCardInfoViewModel>>))]
    [SwaggerResponse(400, "Geçersiz istek")]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _budgetRatioService.DeleteAsync(id);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Bütçe / Ek Bütçe Kayıt Var mı?
    /// </summary>
    /// <param name="code">Code</param>
    /// <param name="year">Year</param>
    /// <returns>Bütçe / Ek Bütçe Veri Tabanı Kontrol Sonucu</returns>
    [HttpGet("exists")]
    [SwaggerOperation(Summary = "Bütçe / Ek Bütçe Kayıt Var mı?")]
    [SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<ClCardInfoViewModel>>))]
    [SwaggerResponse(400, "Geçersiz istek")]
    [SwaggerResponse(401, "Yetkisiz erişim")]
    [SwaggerResponse(500, "Sunucu hatası")]
    public async Task<IActionResult> CheckExists([FromQuery] string code, [FromQuery] int year)
    {
        var result = await _budgetRatioService.CheckExistsAsync(code, year);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return StatusCode(result.StatusCode, result);
    }
}
