using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Service.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Koala.Yedpa.WebUI.Controllers
{
    /// <summary>
    /// Toplu Faturalandırma — WebUI MVC controller (JsonResult).
    /// Ön yüz AJAX çağrıları buraya gelir: cookie auth geçerli, API token gerekmez.
    /// (ApiController token bekler; ön yüz onu doğrudan çağıramaz.)
    /// </summary>
    [Authorize]
    public class BulkInvoiceController : Controller
    {
        private readonly IBulkInvoiceService _service;
        private readonly ILogger<BulkInvoiceController> _logger;

        public BulkInvoiceController(IBulkInvoiceService service, ILogger<BulkInvoiceController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard alert kontrolü - ayın 15'inden sonra ve o ay için session var mı.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckAlert()
        {
            var result = await _service.CheckAlertAsync();
            return Json(result);
        }

        /// <summary>
        /// Faturalandırılmamış ORFLINE satırlarını çeker.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPendingLines()
        {
            var result = await _service.GetPendingLinesAsync();
            return Json(result);
        }

        /// <summary>
        /// Yeni toplu faturalandırma oturumu oluşturur ve kuyruğa alır.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSession([FromBody] CreateBulkInvoiceSessionDto model)
        {
            // Yeni akış: yalnızca fatura tarihi gerekir. Aktarım job'ı o ayın tüm bekleyen
            // AIDAT satırlarını çalışma anında çeker (UI'dan satır seçimi alınmaz).
            if (model == null || model.InvoiceDate == default)
            {
                return Json(ResponseDto<int>.FailData(400, "Fatura tarihi gereklidir", "InvoiceDate empty", true));
            }

            var username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
            var result = await _service.CreateSessionAsync(model, username);
            return Json(result);
        }

        /// <summary>
        /// Oturum durumunu getirir (progress tracking için).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSessionStatus([FromQuery] int id)
        {
            if (id <= 0)
            {
                return Json(ResponseDto<BulkInvoiceSessionDto>.FailData(400, "Geçersiz oturum ID", "SessionId must be positive", true));
            }

            var result = await _service.GetSessionStatusAsync(id);
            return Json(result);
        }

        /// <summary>
        /// Toplu Faturalandırma Yönetimi sayfası — oturumlar + aktarım satırları + yeniden aktarım.
        /// </summary>
        [HttpGet]
        public IActionResult Manage() => View();

        /// <summary>Tüm oturumları (özet sayılarla) JSON döner.</summary>
        [HttpGet]
        public async Task<IActionResult> Sessions()
            => Json(await _service.GetSessionsAsync());

        /// <summary>Bir oturumun aktarım satırlarını JSON döner.</summary>
        [HttpGet]
        public async Task<IActionResult> Items([FromQuery] int sessionId)
        {
            if (sessionId <= 0)
                return Json(ResponseDto<List<BulkInvoiceItemDto>>.FailData(400, "Geçersiz oturum ID", "sessionId", true));
            return Json(await _service.GetSessionItemsAsync(sessionId));
        }

        /// <summary>Oturumun başarısız satırlarını arka planda yeniden aktarır.</summary>
        [HttpPost]
        public IActionResult RetryFailed([FromQuery] int sessionId)
        {
            if (sessionId <= 0)
                return Json(ResponseDto<string>.FailData(400, "Geçersiz oturum ID", "sessionId", true));

            BackgroundJob.Enqueue<BulkInvoiceJobs>(j => j.RetryFailedAsync(sessionId));
            return Json(ResponseDto<string>.SuccessData(200, "Yeniden aktarım kuyruğa alındı", "queued"));
        }
    }
}
