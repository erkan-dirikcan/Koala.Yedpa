using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Controllers
{
    public class SiteController : Controller
    {
        private readonly ISiteService _service;

        public SiteController(ISiteService service)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetPagedList([FromBody] DataTableRequest request)
        {
            if (request == null)
            {
                return Json(new { draw = 0, recordsTotal = 0, recordsFiltered = 0, data = new List<object>() });
            }

            // Column mapping
            var columnMap = new Dictionary<int, string>
            {
                { 0, "logref" },
                { 1, "groupcode" },
                { 2, "groupname" },
                { 3, "clientcode" },
                { 4, "clientname" },
                { 5, null }, // CustomerType - sıralama yapılmayacak
                { 6, "begdate" },
                { 7, "enddate" },
                { 8, null }, // TotalBrutCoefficientMetre - sıralama yapılmayacak
                { 9, null }, // TotalNetMetre - sıralama yapılmayacak
                { 10, null }, // TotalFuelMetre - sıralama yapılmayacak
                { 11, null } // İşlemler - sıralama yapılmayacak
            };

            string? orderColumn = null;
            bool orderAscending = true;

            if (request.Order != null && request.Order.Any())
            {
                var order = request.Order.First();
                if (columnMap.ContainsKey(order.Column) && columnMap[order.Column] != null)
                {
                    orderColumn = columnMap[order.Column];
                    orderAscending = order.Dir?.ToLower() != "desc";
                }
            }

            var result = await _service.GetPagedListAsync(
                request.Start,
                request.Length,
                request.Search?.Value,
                orderColumn,
                orderAscending
            );

            if (!result.IsSuccess)
            {
                return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new List<object>() });
            }

            return Json(new
            {
                draw = request.Draw,
                recordsTotal = result.RecordsTotal,
                recordsFiltered = result.RecordsFiltered,
                data = result.Data
            });
        }

        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] = "Geçersiz kayıt ID'si.";
                return RedirectToAction("Index");
            }

            var result = await _service.GetByIdAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                TempData["ErrorMessage"] = result.Message ?? "Kayıt bulunamadı.";
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(LgXt001211UpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Form verileri geçersiz. Lütfen tüm alanları kontrol edin.";
                return View(model);
            }

            var result = await _service.UpdateLgXt001211(model);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message ?? "Güncelleme işlemi başarısız oldu.";
                return View(model);
            }

            TempData["InfoMessage"] = "Site bilgileri başarıyla güncellendi.";
            return RedirectToAction("Index");
        }
    }
}
