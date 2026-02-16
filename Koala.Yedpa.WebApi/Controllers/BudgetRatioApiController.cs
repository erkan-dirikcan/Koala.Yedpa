using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<BudgetRatioApiController> _logger;

    public BudgetRatioApiController(IBudgetRatioService budgetRatioService, ILogger<BudgetRatioApiController> logger)
    {
        _budgetRatioService = budgetRatioService;
        _logger = logger;
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
        _logger.LogInformation("GetById called with ID {BudgetRatioId}", id);
        var result = await _budgetRatioService.GetByIdAsync(id);
        if (result.IsSuccess)
        {
            _logger.LogInformation("GetById: Successfully retrieved budget ratio with ID {BudgetRatioId}", id);
            return Ok(result);
        }
        _logger.LogWarning("GetById: Failed to retrieve budget ratio with ID {BudgetRatioId}, StatusCode: {StatusCode}", id, result.StatusCode);
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
        _logger.LogInformation("GetAll called");
        var result = await _budgetRatioService.GetAllAsync();
        if (result.IsSuccess)
        {
            _logger.LogInformation("GetAll: Successfully retrieved all budget ratios");
            return Ok(result);
        }
        _logger.LogWarning("GetAll: Failed to retrieve budget ratios, StatusCode: {StatusCode}", result.StatusCode);
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
        _logger.LogInformation("GetByYear called with year {Year}", year);
        var result = await _budgetRatioService.GetByYearAsync(year);
        if (result.IsSuccess)
        {
            _logger.LogInformation("GetByYear: Successfully retrieved budget ratios for year {Year}", year);
            return Ok(result);
        }
        _logger.LogWarning("GetByYear: Failed to retrieve budget ratios for year {Year}, StatusCode: {StatusCode}", year, result.StatusCode);
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
        _logger.LogInformation("Create called with Code={Code}, Year={Year}", model?.Code, model?.Year);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Create: Invalid model state");
            return BadRequest(ModelState);
        }

        var result = await _budgetRatioService.CreateAsync(model);
        if (result.IsSuccess)
        {
            _logger.LogInformation("Create: Successfully created budget ratio with ID {BudgetRatioId}", result.Data.Id);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }
        _logger.LogWarning("Create: Failed to create budget ratio, StatusCode: {StatusCode}", result.StatusCode);
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
        _logger.LogInformation("Update called for budget ratio ID {BudgetRatioId}", model?.Id);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Update: Invalid model state");
            return BadRequest(ModelState);
        }

        var result = await _budgetRatioService.UpdateAsync(model);
        if (result.IsSuccess)
        {
            _logger.LogInformation("Update: Successfully updated budget ratio with ID {BudgetRatioId}", model.Id);
            return Ok(result);
        }
        _logger.LogWarning("Update: Failed to update budget ratio with ID {BudgetRatioId}, StatusCode: {StatusCode}", model.Id, result.StatusCode);
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
        _logger.LogInformation("Delete called for budget ratio ID {BudgetRatioId}", id);
        var result = await _budgetRatioService.DeleteAsync(id);
        if (result.IsSuccess)
        {
            _logger.LogInformation("Delete: Successfully deleted budget ratio with ID {BudgetRatioId}", id);
            return Ok(result);
        }
        _logger.LogWarning("Delete: Failed to delete budget ratio with ID {BudgetRatioId}, StatusCode: {StatusCode}", id, result.StatusCode);
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
        _logger.LogInformation("CheckExists called with Code={Code}, Year={Year}", code, year);
        var result = await _budgetRatioService.CheckExistsAsync(code, year);
        if (result.IsSuccess)
        {
            _logger.LogInformation("CheckExists: Successfully checked existence for Code={Code}, Year={Year}", code, year);
            return Ok(result);
        }
        _logger.LogWarning("CheckExists: Failed for Code={Code}, Year={Year}, StatusCode: {StatusCode}", code, year, result.StatusCode);
        return StatusCode(result.StatusCode, result);
    }
}
