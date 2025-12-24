using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers;

/// <summary>
/// BudgetRatio API Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BudgetRatioApiController : ControllerBase
{
    private readonly IBudgetRatioService _budgetRatioService;

    public BudgetRatioApiController(IBudgetRatioService budgetRatioService)
    {
        _budgetRatioService = budgetRatioService;
    }

    /// <summary>
    /// Get BudgetRatio by ID
    /// </summary>
    /// <param name="id">BudgetRatio ID</param>
    /// <returns>BudgetRatio Detail</returns>
    [HttpGet("{id}")]
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
    /// Get all BudgetRatios
    /// </summary>
    /// <returns>List of BudgetRatios</returns>
    [HttpGet]
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
    /// Get BudgetRatios by Year
    /// </summary>
    /// <param name="year">Year</param>
    /// <returns>List of BudgetRatios for the year</returns>
    [HttpGet("year/{year}")]
    public async Task<IActionResult> GetByYear(string year)
    {
        var result = await _budgetRatioService.GetByYearAsync(year);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Create new BudgetRatio
    /// </summary>
    /// <param name="model">CreateBudgetRatioViewModel</param>
    /// <returns>Created BudgetRatio</returns>
    [HttpPost]
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
    /// Update BudgetRatio
    /// </summary>
    /// <param name="model">UpdateBudgetRatioViewModel</param>
    /// <returns>Update result</returns>
    [HttpPut]
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
    /// Delete BudgetRatio
    /// </summary>
    /// <param name="id">BudgetRatio ID</param>
    /// <returns>Delete result</returns>
    [HttpDelete("{id}")]
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
    /// Check if BudgetRatio exists
    /// </summary>
    /// <param name="code">Code</param>
    /// <param name="year">Year</param>
    /// <returns>Exists result</returns>
    [HttpGet("exists")]
    public async Task<IActionResult> CheckExists([FromQuery] string code, [FromQuery] string year)
    {
        var result = await _budgetRatioService.CheckExistsAsync(code, year);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return StatusCode(result.StatusCode, result);
    }
}

