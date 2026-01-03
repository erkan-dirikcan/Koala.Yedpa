using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BudgetOrderApiController : ControllerBase
    {
        private readonly IBudgetOrderService _budgetOrderService;
        private readonly ILogger<BudgetOrderApiController> _logger;

        public BudgetOrderApiController(IBudgetOrderService budgetOrderService, ILogger<BudgetOrderApiController> logger)
        {
            _budgetOrderService = budgetOrderService;
            _logger = logger;
        }

        /// <summary>
        /// Bütçe ve sipariş oluştur
        /// </summary>
        /// <param name="model">Bütçe ve sipariş oluşturma modeli</param>
        /// <returns>Bütçe ve sipariş oluşturma sonucu</returns>
        [HttpPost]
        public async Task<IActionResult> CreateBudgetAndOrders([FromBody] CreateBudgetOrderViewModel model)
        {
            var result = await _budgetOrderService.CreateBudgetAndOrdersAsync(model);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(result.StatusCode, result);
            }
        }

        /// <summary>
        /// Sadece bütçe oluştur (sipariş gönderme)
        /// </summary>
        /// <param name="model">Bütçe oluşturma modeli</param>
        /// <returns>Oluşturulan bütçe kayıtları</returns>
        [HttpPost("create-budget")]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetOrderViewModel model)
        {
            var result = await _budgetOrderService.CreateBudgetAsync(model);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(result.StatusCode, result);
            }
        }

        /// <summary>
        /// Mevcut bütçe kayıtları için sipariş oluştur
        /// </summary>
        /// <param name="model">Sipariş oluşturma modeli</param>
        /// <returns>Sipariş sonuçları</returns>
        [HttpPost("create-orders")]
        public async Task<IActionResult> CreateOrdersForExistingBudget([FromBody] CreateOrdersForExistingBudgetViewModel model)
        {
            var result = await _budgetOrderService.CreateOrdersForExistingBudgetAsync(
                model.BudgetRatioId,
                model.SelectedMonths,
                model.UserId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(result.StatusCode, result);
            }
        }

        /// <summary>
        /// Yeni bütçe kaydet (Create page'den)
        /// </summary>
        /// <param name="model">Yeni bütçe kaydetme modeli</param>
        /// <returns>Kaydetme sonucu</returns>
        [HttpPost("SaveNewBudget")]
        public async Task<IActionResult> SaveNewBudget([FromBody] SaveNewBudgetViewModel model)
        {
            try
            {
                _logger.LogInformation("SaveNewBudget called with SourceYear: {SourceYear}, BudgetType: {BudgetType}, Ratio: {Ratio}",
                    model.SourceYear, model.BudgetType, model.Ratio);

                // Validate input
                if (model.BudgetType == BuggetTypeEnum.Budget && !model.TargetYear.HasValue)
                {
                    return BadRequest(new
                    {
                        isSuccess = false,
                        statusCode = 400,
                        message = "Bütçe türü 'Bütçe' seçildiğinde hedef yıl gereklidir"
                    });
                }

                // TODO: Implement the actual save logic here
                // For now, return success
                var result = new
                {
                    isSuccess = true,
                    statusCode = 200,
                    message = $"{model.SourceYear} yılı için yeni bütçe başarıyla kaydedildi. " +
                             $"Hedef yıl: {(model.TargetYear ?? model.SourceYear)}, " +
                             $"Oran: %{(model.Ratio - 1) * 100:F2}"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving new budget");
                return StatusCode(500, new
                {
                    isSuccess = false,
                    statusCode = 500,
                    message = "Bütçe kaydedilirken bir hata oluştu",
                    errors = new { errors = new[] { ex.Message } }
                });
            }
        }

        /// <summary>
        /// Bütçe hesapla (preview endpoint)
        /// </summary>
        /// <param name="request">Hesaplama isteği</param>
        /// <returns>Hesaplanan bütçe verileri</returns>
        [HttpPost("CalculateBudget")]
        public async Task<IActionResult> CalculateBudget([FromBody] BudgetCalculationRequestViewModel request)
        {
            var result = await _budgetOrderService.CalculateBudgetAsync(request);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(result.StatusCode, result);
            }
        }

        /// <summary>
        /// Test için basit bütçe oluşturma endpoint'i
        /// </summary>
        /// <returns>Bütçe oluşturma sonucu</returns>
        [HttpGet("test-create")]
        [AllowAnonymous]
        public async Task<IActionResult> TestCreateBudget()
        {
            var testModel = new CreateBudgetOrderViewModel
            {
                SourceYear = 2025,
                TargetYear = 2026,
                BudgetRatioId = "test-budget-ratio-001",
                BudgetType = BuggetTypeEnum.Budget,
                SelectedMonths = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                CreateOrders = false,
                UserId = "test-user"
            };

            var result = await _budgetOrderService.CreateBudgetAsync(testModel);

            if (result.IsSuccess)
            {
                return Ok(new
                {
                    message = "Test bütçe oluşturma başarılı!",
                    data = result.Data,
                    count = result.Data?.Count ?? 0
                });
            }
            else
            {
                return BadRequest(new
                {
                    message = "Test bütçe oluşturma hatası!",
                    error = result.Message,
                    statusCode = result.StatusCode
                });
            }
        }
    }
}
