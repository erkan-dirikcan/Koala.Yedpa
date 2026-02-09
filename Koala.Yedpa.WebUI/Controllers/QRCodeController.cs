using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Koala.Yedpa.WebUI.Controllers
{
    public class QRCodeController : Controller
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly ISettingsService _settingsService;

        public QRCodeController(IQRCodeService qrCodeService, ISettingsService settingsService)
        {
            _qrCodeService = qrCodeService;
            _settingsService = settingsService;
        }

        // GET: /QRCode/Index
        public async Task<IActionResult> Index()
        {
            var settingsResult = await _settingsService.GetQRCodeSettingsAsync();
            if (!settingsResult.IsSuccess)
            {
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
            var result = await _qrCodeService.GetBatchesAsync();

            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = result.Message });
            }

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

        // POST: /QRCode/Create - Yeni QR kodlar oluşturur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            var result = await _qrCodeService.GenerateBulkQRCodesAsync();

            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }

        // POST: /QRCode/Refresh - Mevcut QR kodları yeniden oluşturur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refresh()
        {
            var result = await _qrCodeService.RefreshQRCodesAsync();

            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = result.Message });
            }

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
            var result = await _qrCodeService.DeleteQRCodesAsync(deleteFiles: true);

            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = result.Message });
            }

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
            var result = await _qrCodeService.DeleteBatchAsync(batchId, deleteFiles: true);

            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new
            {
                success = true,
                message = result.Message
            });
        }

        // GET: /QRCode/ViewBatch?batchId=1 - Oluşturulan QR görsellerini gösterir
        public async Task<IActionResult> ViewBatch(int batchId)
        {
            var result = await _qrCodeService.GetQRCodesByBatchIdAsync(batchId);

            if (!result.IsSuccess || !result.Data.Any())
            {
                TempData["Warning"] = $"Batch ID {batchId} için görüntülenecek QR kod bulunamadı.";
                return RedirectToAction("Index");
            }

            ViewData["Title"] = $"QR Kod Görselleri - Batch {batchId}";
            ViewBag.BatchId = batchId;
            return View(result.Data.Where(q => q.Status == StatusEnum.Active).ToList());
        }

        // GET: /QRCode/QRCodesByBatch?batchId=1 - AJAX ile belirli batch'in QR kodlarını getirir
        [HttpGet]
        public async Task<IActionResult> QRCodesByBatch(int batchId)
        {
            var result = await _qrCodeService.GetQRCodesByBatchIdAsync(batchId);

            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = result.Message });
            }

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
            if (string.IsNullOrEmpty(partnerNo))
            {
                return BadRequest("PartnerNo gereklidir");
            }

            var settings = await _settingsService.GetQRCodeSettingsAsync();
            if (!settings.IsSuccess)
            {
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
            return RedirectToAction("Index");
        }
    }
}
