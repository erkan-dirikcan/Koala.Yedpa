using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Koala.Yedpa.WebUI.Controllers
{
    public class QRCodeController : Controller
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<QRCodeController> _logger;

        public QRCodeController(IQRCodeService qrCodeService, ISettingsService settingsService, ILogger<QRCodeController> logger)
        {
            _qrCodeService = qrCodeService;
            _settingsService = settingsService;
            _logger = logger;
        }

        // GET: /QRCode/Index
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index called");
            var settingsResult = await _settingsService.GetQRCodeSettingsAsync();
            if (!settingsResult.IsSuccess)
            {
                _logger.LogWarning("Index: QR Code settings not found");
                TempData["Error"] = "QR Kod ayarları bulunamadı. Lütfen önce ayarları yapın.";
                return RedirectToAction("QRCodeSettings", "Settings");
            }

            ViewData["Title"] = "QR Kod Listesi";
            ViewData["ActivePage"] = "QRCodeIndex";
            ViewData["MenuToggle"] = "QRCode";

            return View();
        }

        // GET: /QRCode/List - AJAX ile batch listesini getirir
        [HttpGet]
        public async Task<IActionResult> List()
        {
            _logger.LogInformation("List called");
            var result = await _qrCodeService.GetBatchesAsync();

            if (!result.IsSuccess)
            {
                _logger.LogWarning("List: Failed to get batches - {Message}", result.Message);
                return Json(new { success = false, message = result.Message });
            }

            _logger.LogInformation("List: Retrieved {Count} batches", result.Data?.Count() ?? 0);
            return Json(new
            {
                success = true,
                data = result.Data.Select(b => new
                {
                    id = b.Id,
                    sqlQuery = b.SqlQuery,
                    qrCodeYear = b.QrCodeYear,
                    qrCodePreCode = b.QrCodePreCode,
                    qrCodeCount = b.QRCodeCount,
                    description = b.Description,
                    createdDate = b.CreateTime.ToString("dd.MM.yyyy HH:mm"),
                    status = b.Status == StatusEnum.Active ? "Aktif" : "Pasif"
                })
            });
        }

        // GET: /QRCode/Create - QR kod oluşturma tanımlama sayfası
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Create GET called");
            // Mevcut ayarları getir
            var settingsResult = await _settingsService.GetQRCodeSettingsAsync();

            var model = new QRCodeCreateViewModel();
            if (settingsResult.IsSuccess && settingsResult.Data != null)
            {
                // Ayarları varsayılan değerler olarak doldur
                model.QrCodeYear = settingsResult.Data.QrCodeYear;
                model.QrCodePreCode = settingsResult.Data.QrCodePreCode;
                model.SqlQuery = settingsResult.Data.QrSqlQuery;
            }

            ViewData["Title"] = "Yeni QR Kod Oluştur";
            ViewData["ActivePage"] = "QRCodeCreate";
            ViewData["MenuToggle"] = "QRCode";

            return View(model);
        }

        // POST: /QRCode/Create - QR kodları oluşturur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QRCodeCreateViewModel model)
        {
            _logger.LogInformation("Create POST called with Year={Year}, PreCode={PreCode}", model?.QrCodeYear, model?.QrCodePreCode);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create POST: Invalid model state");
                return Json(new { success = false, message = "Lütfen tüm alanları doldurun", errors = GetModelStateErrors() });
            }

            try
            {
                var result = await _qrCodeService.GenerateBulkQRCodesWithParamsAsync(
                    model.QrCodeYear,
                    model.QrCodePreCode,
                    model.SqlQuery,
                    model.Description);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Create POST: Failed to generate QR codes - {Message}", result.Message);
                    return Json(new { success = false, message = result.Message, errors = result.Errors?.Errors });
                }

                _logger.LogInformation("Create POST: Successfully generated QR codes");
                return Json(new
                {
                    success = true,
                    message = result.Message,
                    redirectUrl = Url.Action("Index")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create POST: Unexpected error");
                return Json(new { success = false, message = "Beklenmeyen hata: " + ex.Message, stackTrace = ex.StackTrace });
            }
        }

        private List<string> GetModelStateErrors()
        {
            var errors = new List<string>();
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            return errors;
        }

        // POST: /QRCode/Refresh - Mevcut QR kodları yeniden oluşturur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refresh()
        {
            _logger.LogInformation("Refresh called");
            var result = await _qrCodeService.RefreshQRCodesAsync();

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Refresh: Failed to refresh QR codes - {Message}", result.Message);
                return Json(new { success = false, message = result.Message });
            }

            _logger.LogInformation("Refresh: Successfully refreshed QR codes");
            return Json(new
            {
                success = true,
                message = result.Message
            });
        }

        // POST: /QRCode/Delete - Tüm QR kodları siler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            _logger.LogInformation("Delete called");
            var result = await _qrCodeService.DeleteQRCodesAsync(deleteFiles: true);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Delete: Failed to delete QR codes - {Message}", result.Message);
                return Json(new { success = false, message = result.Message });
            }

            _logger.LogInformation("Delete: Successfully deleted all QR codes");
            return Json(new
            {
                success = true,
                message = result.Message
            });
        }

        // POST: /QRCode/DeleteBatch - Belirli bir batch'i siler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBatch(int batchId)
        {
            _logger.LogInformation("DeleteBatch called for batch ID {BatchId}", batchId);
            var result = await _qrCodeService.DeleteBatchAsync(batchId, deleteFiles: true);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("DeleteBatch: Failed to delete batch {BatchId} - {Message}", batchId, result.Message);
                return Json(new { success = false, message = result.Message });
            }

            _logger.LogInformation("DeleteBatch: Successfully deleted batch {BatchId}", batchId);
            return Json(new
            {
                success = true,
                message = result.Message
            });
        }

        // GET: /QRCode/ViewBatch?batchId=1 - Oluşturulan QR görsellerini gösterir
        public async Task<IActionResult> ViewBatch(int batchId)
        {
            _logger.LogInformation("ViewBatch called for batch ID {BatchId}", batchId);
            var result = await _qrCodeService.GetQRCodesByBatchIdAsync(batchId);

            if (!result.IsSuccess || !result.Data.Any())
            {
                _logger.LogWarning("ViewBatch: No QR codes found for batch ID {BatchId}", batchId);
                TempData["Warning"] = $"Batch ID {batchId} için görüntülenecek QR kod bulunamadı.";
                return RedirectToAction("Index");
            }

            var activeCount = result.Data.Count(q => q.Status == StatusEnum.Active);
            _logger.LogInformation("ViewBatch: Found {Count} active QR codes for batch {BatchId}", activeCount, batchId);
            ViewData["Title"] = $"QR Kod Görselleri - Batch {batchId}";
            ViewBag.BatchId = batchId;
            return View(result.Data.Where(q => q.Status == StatusEnum.Active).ToList());
        }

        // GET: /QRCode/QRCodesByBatch?batchId=1 - AJAX ile belirli batch'in QR kodlarını getirir
        [HttpGet]
        public async Task<IActionResult> QRCodesByBatch(int batchId)
        {
            _logger.LogInformation("QRCodesByBatch called for batch ID {BatchId}", batchId);
            var result = await _qrCodeService.GetQRCodesByBatchIdAsync(batchId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("QRCodesByBatch: Failed to get QR codes for batch {BatchId}", batchId);
                return Json(new { success = false, message = result.Message });
            }

            _logger.LogInformation("QRCodesByBatch: Retrieved {Count} QR codes for batch {BatchId}", result.Data?.Count() ?? 0, batchId);
            return Json(new
            {
                success = true,
                data = result.Data.Select(q => new
                {
                    id = q.Id,
                    batchId = q.BatchId,
                    partnerNo = q.PartnerNo,
                    qrCodeNumber = q.QRCodeNumber,
                    qrImagePath = q.QRImagePath,
                    folderPath = q.FolderPath,
                    qrCodeYear = q.QrCodeYear,
                    createdDate = q.CreateTime.ToString("dd.MM.yyyy HH:mm"),
                    status = q.Status == StatusEnum.Active ? "Aktif" : "Pasif"
                })
            });
        }

        // GET: /QRCode/CurrentAccountDetail?partnerNo=xxx - Tekil QR detay
        public async Task<IActionResult> CurrentAccountDetail(string partnerNo)
        {
            _logger.LogInformation("CurrentAccountDetail called for partner {PartnerNo}", partnerNo);
            if (string.IsNullOrEmpty(partnerNo))
            {
                _logger.LogWarning("CurrentAccountDetail: PartnerNo is null or empty");
                return BadRequest("PartnerNo gereklidir");
            }

            var settings = await _settingsService.GetQRCodeSettingsAsync();
            if (!settings.IsSuccess)
            {
                _logger.LogWarning("CurrentAccountDetail: QR Code settings not found for partner {PartnerNo}", partnerNo);
                TempData["Error"] = "QR Kod ayarları bulunamadı";
                return RedirectToAction("Index");
            }

            var qrCodeResult = await _qrCodeService.GetQRCodeByPartnerNoAsync(partnerNo);
            string qrPath;

            if (qrCodeResult.IsSuccess && qrCodeResult.Data != null)
            {
                // Veritabanından QR kod bilgisi var
                qrPath = qrCodeResult.Data.QRImagePath ?? "";
            }
            else
            {
                // Veritabanında yok, path oluştur
                qrPath = $"/Uploads/Qr/{settings.Data.QrCodeYear}/{settings.Data.QrCodePreCode}-{partnerNo}.jpg";
            }

            _logger.LogInformation("CurrentAccountDetail: Successfully retrieved QR code for partner {PartnerNo}", partnerNo);
            ViewData["Title"] = $"QR Kod - {partnerNo}";
            ViewBag.PartnerNo = partnerNo;
            ViewBag.QrPath = qrPath;
            ViewBag.QrCodePreCode = settings.Data.QrCodePreCode;
            ViewBag.QrCodeYear = settings.Data.QrCodeYear;

            return View();
        }

        // GET: /QRCode/CreatePdf (eski action - deprecated)
        public async Task<IActionResult> CreatePdf()
        {
            _logger.LogInformation("CreatePdf called (deprecated action)");
            return RedirectToAction("Index");
        }
    }
}
