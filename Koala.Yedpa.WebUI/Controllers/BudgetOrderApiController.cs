using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Service.Services.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Koala.Yedpa.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    public class BudgetOrderApiController : ControllerBase
    {
        private readonly IBudgetOrderService _budgetOrderService;
        private readonly ILogger<BudgetOrderApiController> _logger;
        private readonly DuesStatisticTransferQueue _transferQueue;

        public BudgetOrderApiController(
            IBudgetOrderService budgetOrderService,
            ILogger<BudgetOrderApiController> logger,
            DuesStatisticTransferQueue transferQueue)
        {
            _budgetOrderService = budgetOrderService;
            _logger = logger;
            _transferQueue = transferQueue;
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

                if (model.DuesData == null || !model.DuesData.Any())
                {
                    return BadRequest(new
                    {
                        isSuccess = false,
                        statusCode = 400,
                        message = "Kaydedilecek veri bulunamadı"
                    });
                }

                // Hedef yıl belirle
                var targetYear = model.TargetYear ?? model.SourceYear;

                // Seçilen ayları flag'e çevir
                var selectedMonthsFlag = CalculateMonthsFlag(model.SelectedMonths);

                // Yeni bir BudgetRatio oluştur
                var budgetRatio = new Core.Models.BudgetRatio
                {
                    Id = Guid.NewGuid().ToString(),
                    Year = targetYear,
                    Ratio = model.Ratio - 1, // ViewModel'de oran 1.25 şeklinde, BudgetRatio'da 0.25 olarak saklanır
                    BuggetType = model.BudgetType,
                    BuggetRatioMounths = selectedMonthsFlag, // Seçilen ayları kaydet
                    Status = StatusEnum.Pending, // Yeni bütçe Pending durumunda başlar
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now,
                    Code = $"BR{targetYear}{DateTime.Now:yyyyMMddHHmmss}",
                    Description = $"{model.SourceYear} yılından {targetYear} yılına oluşturulan bütçe"
                };

                var budgetRatioResult = await _budgetOrderService.CreateBudgetRatioAsync(budgetRatio);
                if (!budgetRatioResult.IsSuccess)
                {
                    return StatusCode(500, new
                    {
                        isSuccess = false,
                        statusCode = 500,
                        message = "BudgetRatio oluşturulamadı",
                        error = budgetRatioResult.Message
                    });
                }

                // Her bir DuesData için DuesStatistic kaydı oluştur
                var createdCount = 0;
                var failedCount = 0;

                foreach (var duesItem in model.DuesData)
                {
                    try
                    {
                        var duesStatistic = new Core.Models.DuesStatistic
                        {
                            Id = Guid.NewGuid().ToString(),
                            Year = targetYear.ToString(),
                            Code = duesItem.Code,
                            DivCode = duesItem.DivCode,
                            DivName = duesItem.DivName,
                            DocTrackingNr = duesItem.DocTrackingNr ?? 0,
                            ClientCode = duesItem.ClientCode ?? string.Empty,
                            ClientRef = duesItem.ClientRef ?? 0,
                            BudgetType = model.BudgetType,
                            BuggetRatioId = budgetRatio.Id,
                            TransferStatus = TransferStatusEnum.Pending,
                            // Hesaplanmış aylık değerler
                            January = duesItem.January,
                            February = duesItem.February,
                            March = duesItem.March,
                            April = duesItem.April,
                            May = duesItem.May,
                            June = duesItem.June,
                            July = duesItem.July,
                            August = duesItem.August,
                            September = duesItem.September,
                            October = duesItem.October,
                            November = duesItem.November,
                            December = duesItem.December,
                            Total = duesItem.Total
                        };

                        var createResult = await _budgetOrderService.CreateDuesStatisticAsync(duesStatistic);
                        if (createResult.IsSuccess)
                        {
                            createdCount++;
                        }
                        else
                        {
                            _logger.LogError("DuesStatistic kaydedilemedi: {Code}, Error: {Error}",
                                duesItem.Code, createResult.Message);
                            failedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "DuesStatistic kaydedilirken hata: {Code}", duesItem.Code);
                        failedCount++;
                    }
                }

                // Toplam bütçeyi hesapla ve BudgetRatio'yu güncelle
                if (createdCount > 0)
                {
                    var totalBudget = model.DuesData.Sum(d => d.Total);

                    // BudgetRatio'yu güncelle
                    budgetRatio.TotalBugget = totalBudget;
                    budgetRatio.LastUpdateTime = DateTime.Now;

                    var updateResult = await _budgetOrderService.UpdateBudgetRatioAsync(budgetRatio);
                    if (!updateResult.IsSuccess)
                    {
                        _logger.LogWarning("BudgetRatio TotalBugget güncellenemedi: {Id}", budgetRatio.Id);
                    }
                }

                var result = new
                {
                    isSuccess = true,
                    statusCode = 200,
                    message = $"{model.SourceYear} yılından {targetYear} yılına {createdCount} adet bütçe kaydı başarıyla oluşturuldu." +
                             (failedCount > 0 ? $" {failedCount} adet kayıt başarısız oldu." : ""),
                    data = new
                    {
                        targetYear = targetYear,
                        sourceYear = model.SourceYear,
                        budgetType = model.BudgetType,
                        ratio = model.Ratio,
                        createdCount = createdCount,
                        failedCount = failedCount,
                        budgetRatioId = budgetRatio.Id
                    }
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
        /// Bütçe güncelleme önizleme
        /// </summary>
        /// <param name="request">Önizleme isteği</param>
        /// <returns>Önizleme sonucu</returns>
        [HttpPost("PreviewUpdate")]
        public async Task<IActionResult> PreviewUpdate([FromBody] PreviewUpdateRequestViewModel request)
        {
            var result = await _budgetOrderService.PreviewUpdateAsync(
                request.Id,
                request.Ratio,
                request.TargetAmount,
                (BuggetRatioMounthEnum)request.SelectedMonthsFlag
            );

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

        /// <summary>
        /// BudgetRatio ID'sine göre DuesStatistic ID'lerini getir
        /// </summary>
        /// <param name="budgetRatioId">BudgetRatio ID</param>
        /// <returns>DuesStatistic ID'leri</returns>
        [HttpGet("GetDuesStatisticIds")]
        public async Task<IActionResult> GetDuesStatisticIds([FromQuery] string budgetRatioId)
        {
            _logger.LogInformation("GetDuesStatisticIds çağrıldı. BudgetRatioId: {BudgetRatioId}", budgetRatioId);

            if (string.IsNullOrEmpty(budgetRatioId))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "BudgetRatio ID gereklidir"
                });
            }

            var result = await _budgetOrderService.GetBudgetRatioWithDuesAsync(budgetRatioId);

            if (!result.IsSuccess)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = result.Message
                });
            }

            var (budgetRatio, duesStatistics) = result.Data;

            // DuesStatistic ID'lerini dön
            var ids = duesStatistics?.Select(d => d.Id).ToList() ?? new List<string>();

            return Ok(new
            {
                isSuccess = true,
                message = "Kayıtlar başarıyla getirildi",
                data = ids
            });
        }

        /// <summary>
        /// DuesStatistic kayıtlarını Logo'ya aktar
        /// </summary>
        /// <param name="model">Aktarım modeli</param>
        /// <returns>Aktarım sonuçları</returns>
        [HttpPost("Transfer")]
        public async Task<IActionResult> TransferDuesStatistics([FromBody] TransferDuesStatisticsViewModel model)
        {
            _logger.LogInformation("Transfer job kuyruğa eklendi. Kayıt sayısı: {Count}, Debug Mod: {IsDebugMode}",
                model.DuesStatisticIds.Count, model.IsDebugMode);

            // BudgetRatio'yu Locked yap (aktarım başlatıldığında)
            if (!string.IsNullOrEmpty(model.BudgetRatioId))
            {
                var lockResult = await _budgetOrderService.LockBudgetRatioAsync(model.BudgetRatioId);
                if (!lockResult.IsSuccess)
                {
                    _logger.LogWarning("BudgetRatio kilitlenemedi: {BudgetRatioId}, Hata: {Error}",
                        model.BudgetRatioId, lockResult.Message);
                    return StatusCode(500, new
                    {
                        isSuccess = false,
                        message = "BudgetRatio kilitlenemedi",
                        error = lockResult.Message
                    });
                }
                _logger.LogInformation("BudgetRatio Locked yapıldı: {BudgetRatioId}", model.BudgetRatioId);
            }

            // Arka planda çalıştır (BackgroundService)
            // Current user'ı HttpContext'ten al (client'dan gelen değere güvenme)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var workItem = new DuesStatisticTransferWorkItem
            {
                DuesStatisticIds = model.DuesStatisticIds,
                UserId = currentUserId,
                IsDebugMode = model.IsDebugMode
            };

            await _transferQueue.EnqueueAsync(workItem);
            _logger.LogInformation("Transfer job BackgroundService queue'ya eklendi. Job ID: {JobId}, UserId: {UserId}, Queue'daki bekleyen job sayısı: {Count}",
                workItem.JobId, workItem.UserId ?? "null", _transferQueue.Count);

            // Hemen yanıt döndür
            return Ok(ResponseDto<object>.SuccessData(200,
                "Aktarım işlemi arka planda başlatıldı. Tamamlandığında e-posta ile bilgilendirileceksiniz.",
                new { jobId = workItem.JobId }));
        }

        /// <summary>
        /// Seçilen ayları flag değerine çevir
        /// </summary>
        /// <param name="selectedMonths">Seçilen ay numaraları (1-12)</param>
        /// <returns>Flag değeri</returns>
        private BuggetRatioMounthEnum CalculateMonthsFlag(List<int> selectedMonths)
        {
            if (selectedMonths == null || !selectedMonths.Any())
            {
                return 0;
            }

            var monthFlags = new[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };
            var flag = 0;

            foreach (var month in selectedMonths.Where(m => m >= 1 && m <= 12))
            {
                flag |= monthFlags[month - 1];
            }

            return (BuggetRatioMounthEnum)flag;
        }
    }
}
