using ClosedXML.Excel;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.LogoJsonModels;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.Repositories.Repositories;
using Koala.Yedpa.Repositories.UnitOfWork;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Koala.Yedpa.Service.Services
{
    public class BudgetOrderService : IBudgetOrderService
    {
        private readonly IDuesStatisticService _duesStatisticService;
        private readonly IBudgetRatioService _budgetRatioService;
        private readonly IUnitOfWork<AppDbContext> _unitOfWork;
        private readonly ILogoRestServiceProvider _logoRestService;
        private readonly ITransactionService _transactionService;
        private readonly ITransactionItemService _transactionItemService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<BudgetOrderService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IApiLogoSqlDataService _apiLogoSqlDataService;

        public BudgetOrderService(
            IDuesStatisticService duesStatisticService,
            IBudgetRatioService budgetRatioService,
            IUnitOfWork<AppDbContext> unitOfWork,
            ILogoRestServiceProvider logoRestService,
            ITransactionService transactionService,
            ITransactionItemService transactionItemService,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            ILogger<BudgetOrderService> logger,
            ILoggerFactory loggerFactory,
            IApiLogoSqlDataService apiLogoSqlDataService)
        {
            _duesStatisticService = duesStatisticService;
            _budgetRatioService = budgetRatioService;
            _unitOfWork = unitOfWork;
            _logoRestService = logoRestService;
            _transactionService = transactionService;
            _transactionItemService = transactionItemService;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
            _loggerFactory = loggerFactory;
            _apiLogoSqlDataService = apiLogoSqlDataService;
        }

        public async Task<ResponseDto<BudgetOrderResultViewModel>> CreateBudgetAndOrdersAsync(CreateBudgetOrderViewModel model)
        {
            string? transactionId = null;
            try
            {
                _logger.LogInformation("Bütçe ve sipariş oluşturma işlemi başladı. SourceYear: {SourceYear}, TargetYear: {TargetYear}", model.SourceYear, model.TargetYear);

                // 1. Transaction oluştur
                var transactionModel = new CreateTransactionViewModel
                {
                    TransactionTypeId = "c570d72f-d9c8-11f0-9657-e848b8c82000", // BudgetOrder transaction type (varsayılan, değiştirilmeli)
                    Title = $"{model.TargetYear} Bütçe ve Sipariş Oluşturma",
                    Description = $"{model.SourceYear} yılı verilerinden {model.TargetYear} yılına bütçe/sipariş oluşturuluyor. BudgetType: {model.BudgetType}"
                };

                var transactionResult = await _transactionService.CreateTransactionAsync(transactionModel);
                if (!transactionResult.IsSuccess)
                {
                    return ResponseDto<BudgetOrderResultViewModel>.FailData(500, "Transaction oluşturulamadı", transactionResult.Message, true);
                }

                transactionId = transactionResult.Data.Id;
                _logger.LogInformation("Transaction oluşturuldu: {TransactionId}", transactionId);

                // 2. BudgetRatio al
                var budgetRatioResult = await _budgetRatioService.GetByIdAsync(model.BudgetRatioId);
                if (!budgetRatioResult.IsSuccess || budgetRatioResult.Data == null)
                {
                    await _transactionItemService.AddTransactionItemAsync(new CreateTransactionItemViewModel
                    {
                        TransactionId = transactionId,
                        Description = "Bütçe oranı bulunamadı",
                        IsSuccess = false
                    });
                    return ResponseDto<BudgetOrderResultViewModel>.FailData(404, "Bütçe oranı bulunamadı", budgetRatioResult.Message, true);
                }

                var budgetRatio = budgetRatioResult.Data;
                var result = new BudgetOrderResultViewModel
                {
                    AppliedRatio = budgetRatio.Ratio
                };

                // 3. SourceYear için DuesStatistics kayıtlarını getir
                var sourceDuesResult = await _duesStatisticService.GetByYearAsync(model.SourceYear.ToString());
                if (!sourceDuesResult.IsSuccess || sourceDuesResult.Data == null || !sourceDuesResult.Data.Any())
                {
                    await _transactionItemService.AddTransactionItemAsync(new CreateTransactionItemViewModel
                    {
                        TransactionId = transactionId,
                        Description = $"{model.SourceYear} yılına ait bütçe kaydı bulunamadı",
                        IsSuccess = false
                    });
                    return ResponseDto<BudgetOrderResultViewModel>.FailData(404, $"Kaynak yıl bulunamadı: {model.SourceYear}", "", true);
                }

                // 4. SourceYear toplamını hesapla
                var sourceDuesStatistics = sourceDuesResult.Data
                    .Where(d => d.BudgetType == model.BudgetType)
                    .ToList();

                result.SourceTotalAmount = sourceDuesStatistics.Sum(d => d.Total);
                result.TargetTotalAmount = result.SourceTotalAmount * (1 + budgetRatio.Ratio);

                _logger.LogInformation("SourceYear Toplam: {SourceTotal}, TargetTotal: {TargetTotal}, Ratio: {Ratio}",
                    result.SourceTotalAmount, result.TargetTotalAmount, budgetRatio.Ratio);

                await _transactionItemService.AddTransactionItemAsync(new CreateTransactionItemViewModel
                {
                    TransactionId = transactionId,
                    Description = $"Kaynak yıl toplamı: {result.SourceTotalAmount:C2}, Hedef toplam: {result.TargetTotalAmount:C2}",
                    IsSuccess = true
                });

                // 5. Her cari için bütçe oluştur (ay ay oran uygula)
                var createdDuesStatistics = new List<DuesStatisticListViewModel>();

                foreach (var sourceDues in sourceDuesStatistics)
                {
                    // Cari kaydı yoksa atla
                    if (sourceDues == null)
                    {
                        _logger.LogWarning("Cari bulunamadı: {ClientCode}", sourceDues.ClientCode);
                        continue;
                    }

                    // DuesStatistic oluştur
                    var newDues = new DuesStatistic
                    {
                        Id = Guid.NewGuid().ToString(),
                        Year = model.TargetYear.ToString(),
                        Code = sourceDues.Code,
                        DivCode = sourceDues.DivCode,
                        DivName = sourceDues.DivName,
                        DocTrackingNr = sourceDues.DocTrackingNr,
                        ClientCode = sourceDues.ClientCode,
                        ClientRef = sourceDues.ClientRef,
                        BudgetType = model.BudgetType,
                        BuggetRatioId = model.BudgetRatioId,
                        TransferStatus = TransferStatusEnum.Pending
                    };

                    // Her ay için ayrı ayrı oran uygula
                    var multiplier = 1 + budgetRatio.Ratio;
                    newDues.January = model.SelectedMonths.Contains(1) ? sourceDues.January * multiplier : 0;
                    newDues.February = model.SelectedMonths.Contains(2) ? sourceDues.February * multiplier : 0;
                    newDues.March = model.SelectedMonths.Contains(3) ? sourceDues.March * multiplier : 0;
                    newDues.April = model.SelectedMonths.Contains(4) ? sourceDues.April * multiplier : 0;
                    newDues.May = model.SelectedMonths.Contains(5) ? sourceDues.May * multiplier : 0;
                    newDues.June = model.SelectedMonths.Contains(6) ? sourceDues.June * multiplier : 0;
                    newDues.July = model.SelectedMonths.Contains(7) ? sourceDues.July * multiplier : 0;
                    newDues.August = model.SelectedMonths.Contains(8) ? sourceDues.August * multiplier : 0;
                    newDues.September = model.SelectedMonths.Contains(9) ? sourceDues.September * multiplier : 0;
                    newDues.October = model.SelectedMonths.Contains(10) ? sourceDues.October * multiplier : 0;
                    newDues.November = model.SelectedMonths.Contains(11) ? sourceDues.November * multiplier : 0;
                    newDues.December = model.SelectedMonths.Contains(12) ? sourceDues.December * multiplier : 0;

                    // Toplam = aylar toplamı
                    newDues.Total = newDues.January + newDues.February + newDues.March + newDues.April +
                                   newDues.May + newDues.June + newDues.July + newDues.August +
                                   newDues.September + newDues.October + newDues.November + newDues.December;

                    // DuesStatistic'i kaydet
                    var createResult = await _duesStatisticService.CreateAsync(newDues);

                    if (createResult.IsSuccess)
                    {
                        createdDuesStatistics.Add(new DuesStatisticListViewModel
                        {
                            Id = newDues.Id,
                            Year = newDues.Year,
                            Code = newDues.Code,
                            ClientCode = newDues.ClientCode,
                            ClientRef = newDues.ClientRef,
                            Total = newDues.Total,
                            BudgetType = newDues.BudgetType,
                            TransferStatus = newDues.TransferStatus,
                            DivCode = newDues.DivCode,
                            DivName = newDues.DivName,
                            DocTrackingNr = newDues.DocTrackingNr,
                            BuggetRatioId = newDues.BuggetRatioId
                        });
                    }
                    else
                    {
                        _logger.LogError("DuesStatistic kaydedilemedi: {ClientCode}", sourceDues.ClientCode);
                    }
                }

                result.CreatedDuesStatistics = createdDuesStatistics;
                result.TotalCreated = createdDuesStatistics.Count;

                await _transactionItemService.AddTransactionItemAsync(new CreateTransactionItemViewModel
                {
                    TransactionId = transactionId,
                    Description = $"{result.TotalCreated} adet DuesStatistic kaydı oluşturuldu",
                    IsSuccess = true
                });

                // 8. Sipariş oluştur (eğer istendiyse)
                if (model.CreateOrders)
                {
                    var orderResults = await CreateOrdersInternalAsync(createdDuesStatistics, model.SelectedMonths, model.TargetYear, transactionId);

                    result.SuccessfulOrders = orderResults.Where(r => r.IsSuccess).ToList();
                    result.FailedOrders = orderResults.Where(r => !r.IsSuccess).ToList();
                    result.TotalOrdersSent = orderResults.Count;
                    result.TotalOrdersFailed = result.FailedOrders.Count;

                    // Hatalı olanları mail ile raporla
                    if (result.FailedOrders.Any())
                    {
                        await SendErrorReportAsync(result, model.TargetYear);
                    }
                }

                result.TransactionId = transactionId;

                await _transactionItemService.AddTransactionItemAsync(new CreateTransactionItemViewModel
                {
                    TransactionId = transactionId,
                    Description = "İşlem başarıyla tamamlandı",
                    IsSuccess = true
                });

                _logger.LogInformation("Bütçe ve sipariş oluşturma işlemi başarıyla tamamlandı. TransactionId: {TransactionId}", transactionId);

                return ResponseDto<BudgetOrderResultViewModel>.SuccessData(200, "İşlem başarıyla tamamlandı", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bütçe ve sipariş oluşturma işlemi hatası");

                if (!string.IsNullOrEmpty(transactionId))
                {
                    await _transactionItemService.AddTransactionItemAsync(new CreateTransactionItemViewModel
                    {
                        TransactionId = transactionId,
                        Description = $"İşlem hatası: {ex.Message}",
                        IsSuccess = false
                    });
                }

                return ResponseDto<BudgetOrderResultViewModel>.FailData(500, "İşlem hatası", ex.Message, true);
            }
        }

        public async Task<ResponseDto<List<DuesStatisticListViewModel>>> CreateBudgetAsync(CreateBudgetOrderViewModel model)
        {
            // Sadece bütçe oluştur, sipariş gönderme
            model.CreateOrders = false;
            var result = await CreateBudgetAndOrdersAsync(model);

            if (!result.IsSuccess)
            {
                return ResponseDto<List<DuesStatisticListViewModel>>.FailData(result.StatusCode, result.Message, result.Message, true);
            }

            return ResponseDto<List<DuesStatisticListViewModel>>.SuccessData(200, "Bütçe başarıyla oluşturuldu", result.Data.CreatedDuesStatistics);
        }

        public async Task<ResponseDto<List<OrderResultViewModel>>> CreateOrdersForExistingBudgetAsync(string budgetRatioId, List<int> selectedMonths, string? userId = null)
        {
            try
            {
                // Mevcut DuesStatistics kayıtlarını bul
                var budgetRatioResult = await _budgetRatioService.GetByIdAsync(budgetRatioId);
                if (!budgetRatioResult.IsSuccess || budgetRatioResult.Data == null)
                {
                    return ResponseDto<List<OrderResultViewModel>>.FailData(404, "Bütçe oranı bulunamadı", "", true);
                }

                var targetYear = int.Parse(budgetRatioResult.Data.Year.ToString());
                var budgetType = budgetRatioResult.Data.BuggetType;

                var duesResult = await _duesStatisticService.GetByYearAsync(targetYear.ToString());
                if (!duesResult.IsSuccess || duesResult.Data == null)
                {
                    return ResponseDto<List<OrderResultViewModel>>.FailData(404, "Bütçe kayıtları bulunamadı", "", true);
                }

                var duesStatistics = duesResult.Data
                    .Where(d => d.BudgetType == budgetType && d.BuggetRatioId == budgetRatioId)
                    .Select(d => new DuesStatisticListViewModel
                    {
                        Id = d.Id,
                        Year = d.Year,
                        Code = d.Code,
                        ClientCode = d.ClientCode,
                        ClientRef = d.ClientRef,
                        Total = d.Total,
                        BudgetType = d.BudgetType,
                        TransferStatus = d.TransferStatus,
                        DivCode = d.DivCode,
                        DivName = d.DivName,
                        DocTrackingNr = d.DocTrackingNr,
                        BuggetRatioId = d.BuggetRatioId,
                        January = d.January,
                        February = d.February,
                        March = d.March,
                        April = d.April,
                        May = d.May,
                        June = d.June,
                        July = d.July,
                        August = d.August,
                        September = d.September,
                        October = d.October,
                        November = d.November,
                        December = d.December
                    })
                    .ToList();

                // Transaction oluştur
                var transactionModel = new CreateTransactionViewModel
                {
                    TransactionTypeId = "c570d72f-d9c8-11f0-9657-e848b8c82000",
                    Title = $"{targetYear} Mevcut Bütçe İçin Sipariş Oluşturma",
                    Description = $"{duesStatistics.Count} adet kayıt için sipariş oluşturuluyor"
                };

                var transactionResult = await _transactionService.CreateTransactionAsync(transactionModel);
                if (!transactionResult.IsSuccess)
                {
                    return ResponseDto<List<OrderResultViewModel>>.FailData(500, "Transaction oluşturulamadı", "", true);
                }

                // Siparişleri oluştur
                var orderResults = await CreateOrdersInternalAsync(duesStatistics, selectedMonths, targetYear, transactionResult.Data.Id);

                return ResponseDto<List<OrderResultViewModel>>.SuccessData(200, "Siparişler başarıyla oluşturuldu", orderResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mevcut bütçe için sipariş oluşturma hatası");
                return ResponseDto<List<OrderResultViewModel>>.FailData(500, "İşlem hatası", ex.Message, true);
            }
        }

        private async Task<List<OrderResultViewModel>> CreateOrdersInternalAsync(
            List<DuesStatisticListViewModel> duesStatistics,
            List<int> selectedMonths,
            int targetYear,
            string transactionId)
        {
            var results = new List<OrderResultViewModel>();

            foreach (var dues in duesStatistics)
            {
                try
                {
                    // SalesOrder oluştur
                    var salesOrder = new SalesOrdersJsonViewModel()
                    {
                        NUMBER = "~",
                        DOC_TRACKING_NR = dues.DocTrackingNr?.ToString() ?? "0",
                        DOC_NUMBER = "AIDAT",
                        AUTH_CODE = "AIDAT",
                        ARP_CODE = dues.ClientCode,
                        NOTES1 = $"{targetYear} yılı için AIDAT {(dues.BudgetType == BuggetTypeEnum.Budget ? "bütçe" : "ek bütçe")} tahakkuku",
                        NOTES2 = $"Yıllık {(dues.BudgetType == BuggetTypeEnum.Budget ? "Bütçe" : "Ek Bütçe")}",
                        ORDER_STATUS = "4",
                        DATE = new DateTime(targetYear, selectedMonths.First(), 1),
                        DATE_CREATED = DateTime.Now,
                        CURRSEL_TOTAL = "1",
                        AFFECT_RISK = "0",
                        EINVOICE = "1",
                        EINSTEAD_OF_DISPATCH = "1",
                        ORGLOGOID = "",
                        LABEL_LIST = "",
                        DEFNFLDSLIST = ""
                    };

                    // Aylar TRANSACTION olarak ekle
                    var transactions = new TRANSACTIONS { items = new List<Item>() };

                    foreach (var month in selectedMonths)
                    {
                        var monthlyAmount = GetMonthlyAmount(dues, month);

                        if (monthlyAmount <= 0)
                            continue;

                        var monthName = GetMonthName(month);
                        var monthDate = new DateTime(targetYear, month, 1);

                        transactions.items.Add(new Item
                        {
                            TYPE = "4",
                            MASTER_CODE = "600.11.0001",
                            QUANTITY = "1",
                            PRICE = monthlyAmount.ToString("0.00").Replace(",", "."),
                            VAT_RATE = "20",
                            TRANS_DESCRIPTION = monthName,
                            UNIT_CODE = "ADET",
                            UNIT_CONV1 = "1",
                            UNIT_CONV2 = "2",
                            VAT_INCLUDED = "1",
                            ORDER_CLOSED = "0",
                            DUE_DATE = monthDate,
                            DEFNFLDS = "",
                            MULTI_ADD_TAX = "0",
                            EDT_CURR = "1"
                        });
                    }

                    salesOrder.TRANSACTIONS = transactions;

                    // JSON oluştur
                    var json = JsonConvert.SerializeObject(salesOrder, Formatting.Indented);

                    // POST /salesOrders
                    var response = await _logoRestService.HttpPost("salesOrders", json);

                    var orderResult = new OrderResultViewModel
                    {
                        ClientCode = dues.ClientCode,
                        ClientRef = dues.ClientRef.ToString(),
                        OrderAmount = dues.Total,
                        IsSuccess = response.IsSuccess
                    };

                    if (response.IsSuccess)
                    {
                        orderResult.OrderNumber = "BAŞARILI"; // Logo'dan dönen number alınabilir
                        _logger.LogInformation("Sipariş başarıyla oluşturuldu: {ClientCode}", dues.ClientCode);

                        await _transactionItemService.AddTransactionItemAsync(new CreateTransactionItemViewModel
                        {
                            TransactionId = transactionId,
                            Description = $"Sipariş oluşturuldu: {dues.ClientCode}",
                            IsSuccess = true
                        });
                    }
                    else
                    {
                        orderResult.ErrorMessage = response.Message;
                        _logger.LogError("Sipariş oluşturulamadı: {ClientCode}, Hata: {Error}", dues.ClientCode, response.Message);

                        await _transactionItemService.AddTransactionItemAsync(new CreateTransactionItemViewModel
                        {
                            TransactionId = transactionId,
                            Description = $"Sipariş hatası: {dues.ClientCode} - {response.Message}",
                            IsSuccess = false
                        });
                    }

                    results.Add(orderResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sipariş oluşturma hatası: {ClientCode}", dues.ClientCode);

                    results.Add(new OrderResultViewModel
                    {
                        ClientCode = dues.ClientCode,
                        ClientRef = dues.ClientRef.ToString(),
                        IsSuccess = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return results;
        }

        private decimal GetMonthlyAmount(DuesStatisticListViewModel dues, int month)
        {
            return month switch
            {
                1 => dues.January,
                2 => dues.February,
                3 => dues.March,
                4 => dues.April,
                5 => dues.May,
                6 => dues.June,
                7 => dues.July,
                8 => dues.August,
                9 => dues.September,
                10 => dues.October,
                11 => dues.November,
                12 => dues.December,
                _ => 0
            };
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "OCAK",
                2 => "SUBAT",
                3 => "MART",
                4 => "NISAN",
                5 => "MAYIS",
                6 => "HAZIRAN",
                7 => "TEMMUZ",
                8 => "AGUSTOS",
                9 => "EYLUL",
                10 => "EKIM",
                11 => "KASIM",
                12 => "ARALIK",
                _ => ""
            };
        }

        private async Task SendErrorReportAsync(BudgetOrderResultViewModel result, int targetYear)
        {
            try
            {
                // Excel workbook oluştur
                using var workbook = new XLWorkbook();

                // 1. Başarılı Siparişler Sheet'i
                if (result.SuccessfulOrders != null && result.SuccessfulOrders.Any())
                {
                    var successfulSheet = workbook.Worksheets.Add("Başarılı Siparişler");

                    // Header
                    successfulSheet.Cell("A1").Value = "Cari Kodu";
                    successfulSheet.Cell("B1").Value = "Cari Ref";
                    successfulSheet.Cell("C1").Value = "Sipariş Tutarı";
                    successfulSheet.Cell("D1").Value = "Sipariş Numarası";
                    successfulSheet.Cell("E1").Value = "Durum";

                    // Header formatla
                    var headerRange = successfulSheet.Range(1, 1, 1, 5);
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Verileri doldur
                    int row = 2;
                    foreach (var order in result.SuccessfulOrders)
                    {
                        successfulSheet.Cell(row, 1).Value = order.ClientCode;
                        successfulSheet.Cell(row, 2).Value = order.ClientRef;
                        successfulSheet.Cell(row, 3).Value = order.OrderAmount;
                        successfulSheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                        successfulSheet.Cell(row, 4).Value = order.OrderNumber ?? "BAŞARILI";
                        successfulSheet.Cell(row, 5).Value = "BAŞARILI";
                        row++;
                    }

                    // Sütun genişliklerini ayarla
                    successfulSheet.Columns().AdjustToContents();
                }

                // 2. Başarısız Siparişler Sheet'i
                if (result.FailedOrders != null && result.FailedOrders.Any())
                {
                    var failedSheet = workbook.Worksheets.Add("Başarısız Siparişler");

                    // Header
                    failedSheet.Cell("A1").Value = "Cari Kodu";
                    failedSheet.Cell("B1").Value = "Cari Ref";
                    failedSheet.Cell("C1").Value = "Sipariş Tutarı";
                    failedSheet.Cell("D1").Value = "Hata Mesajı";
                    failedSheet.Cell("E1").Value = "Durum";

                    // Header formatla
                    var headerRange = failedSheet.Range(1, 1, 1, 5);
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightPink;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Verileri doldur
                    int row = 2;
                    foreach (var order in result.FailedOrders)
                    {
                        failedSheet.Cell(row, 1).Value = order.ClientCode;
                        failedSheet.Cell(row, 2).Value = order.ClientRef;
                        failedSheet.Cell(row, 3).Value = order.OrderAmount;
                        failedSheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                        failedSheet.Cell(row, 4).Value = order.ErrorMessage ?? "Bilinmeyen Hata";
                        failedSheet.Cell(row, 5).Value = "BAŞARISIZ";
                        row++;
                    }

                    // Sütun genişliklerini ayarla
                    failedSheet.Columns().AdjustToContents();
                }

                // Excel'i byte array'e çevir
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var excelBytes = stream.ToArray();

                // Mail içeriğini oluştur
                var body = new StringBuilder();
                body.AppendLine($"Sayın Yetkili,");
                body.AppendLine();
                body.AppendLine($"{targetYear} yılı bütçe/sipariş oluşturma işlemi tamamlandı.");
                body.AppendLine();
                body.AppendLine("<strong>ÖZET:</strong>");
                body.AppendLine($"- Toplam Cari Sayısı: {result.TotalCreated}");
                body.AppendLine($"- Başarılı Sipariş: {result.TotalOrdersSent - result.TotalOrdersFailed}");
                body.AppendLine($"- Başarısız Sipariş: {result.TotalOrdersFailed}");
                body.AppendLine($"- Kaynak Toplam Tutar: {result.SourceTotalAmount:C2}");
                body.AppendLine($"- Hedef Toplam Tutar: {result.TargetTotalAmount:C2}");
                body.AppendLine($"- Uygulanan Oran: %{result.AppliedRatio * 100}");
                body.AppendLine();
                body.AppendLine("<strong>Dosya Eki:</strong>");
                body.AppendLine("Detaylı bilgi ekteki Excel dosyasında yer almaktadır.");
                body.AppendLine();
                body.AppendLine("İyi çalışmalar.");

                // E-posta şablonunu al
                var template = await _emailTemplateService.GetByNameAsyc("Default");
                string mailContent;

                if (!template.IsSuccess || template.Data == null)
                {
                    mailContent = body.ToString();
                }
                else
                {
                    mailContent = template.Data.Content
                        .Replace("[[Title]]", "Bütçe/Sipariş Oluşturma Raporu")
                        .Replace("[[Date]]", DateTime.Now.ToLongDateString())
                        .Replace("[[Name]]", "Erkan DİRİKCAN")
                        .Replace("[[Body]]", body.ToString());
                }

                // Excel dosyasını ekle
                var email = new CustomEmailDto
                {
                    Name = "Erkan",
                    Lastname = "DİRİKCAN",
                    Email = "erkan@sistem-bilgisayar.com",
                    Content = mailContent,
                    Title = $"{targetYear} Bütçe/Sipariş Oluşturma Raporu",
                    Attachments = new List<EmailAttachmentDto>
                    {
                        new EmailAttachmentDto
                        {
                            FileName = $"Butce_Siparis_Raporu_{targetYear}.xlsx",
                            Content = excelBytes,
                            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                        }
                    }
                };

                await _emailService.SendCustomMail(email);
                _logger.LogInformation("Hata raporu maili gönderildi - Başarılı: {Successful}, Başarısız: {Failed}",
                    result.TotalOrdersSent - result.TotalOrdersFailed, result.TotalOrdersFailed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hata raporu maili gönderilemedi");
            }
        }

        public async Task<ResponseDto<BudgetCalculationResultViewModel>> CalculateBudgetAsync(BudgetCalculationRequestViewModel request)
        {
            try
            {
                _logger.LogInformation("Bütçe hesaplama başladı. SourceYear: {SourceYear}, BudgetType: {BudgetType}, SelectedMonthsFlag: {SelectedMonthsFlag}",
                    request.SourceYear, request.BudgetType, request.SelectedMonthsFlag);

                // 1. Validate input - Flag değerini parse et ve ay numaralarına çevir
                var selectedMonths = ParseMonthsFlag(request.SelectedMonthsFlag);
                if (!selectedMonths.Any())
                {
                    return ResponseDto<BudgetCalculationResultViewModel>.FailData(400, "En az bir ay seçmelisiniz", "Validation error", true);
                }

                if (!request.Ratio.HasValue && !request.TargetAmount.HasValue)
                {
                    return ResponseDto<BudgetCalculationResultViewModel>.FailData(400, "Oran veya hedef tutar giriniz", "Validation error", true);
                }

                // 2. Get source year dues statistics
                var sourceDuesResult = await _duesStatisticService.GetByYearAsync(request.SourceYear.ToString());

                if (!sourceDuesResult.IsSuccess || sourceDuesResult.Data == null || !sourceDuesResult.Data.Any())
                {
                    return ResponseDto<BudgetCalculationResultViewModel>.FailData(404,
                        $"Kaynak yıl ({request.SourceYear}) için veri bulunamadı", "Not found", true);
                }

                // Filter by budget type
                var sourceDuesStatistics = sourceDuesResult.Data
                    .Where(d => d.BudgetType == request.BudgetType)
                    .Select(d => new DuesStatisticListViewModel
                    {
                        Id = d.Id,
                        Code = d.Code,
                        Year = d.Year,
                        DivCode = d.DivCode,
                        DivName = d.DivName,
                        DocTrackingNr = d.DocTrackingNr,
                        ClientCode = d.ClientCode,
                        ClientRef = d.ClientRef,
                        January = d.January,
                        February = d.February,
                        March = d.March,
                        April = d.April,
                        May = d.May,
                        June = d.June,
                        July = d.July,
                        August = d.August,
                        September = d.September,
                        October = d.October,
                        November = d.November,
                        December = d.December,
                        Total = d.Total
                    })
                    .ToList();

                if (!sourceDuesStatistics.Any())
                {
                    return ResponseDto<BudgetCalculationResultViewModel>.FailData(404,
                        $"Kaynak yıl ({request.SourceYear}) ve bütçe türü için veri bulunamadı", "Not found", true);
                }

                // 3. Calculate selected months total
                decimal selectedMonthsTotal = 0;
                foreach (var dues in sourceDuesStatistics)
                {
                    foreach (var month in selectedMonths)
                    {
                        selectedMonthsTotal += GetMonthlyValue(dues, month);
                    }
                }

                // 4. Determine ratio
                decimal calculatedRatio;
                if (request.Ratio.HasValue && request.Ratio.Value > 0)
                {
                    calculatedRatio = request.Ratio.Value;
                }
                else if (request.TargetAmount.HasValue && request.TargetAmount.Value > 0)
                {
                    if (selectedMonthsTotal <= 0)
                    {
                        return ResponseDto<BudgetCalculationResultViewModel>.FailData(400,
                            "Seçili ayların toplamı 0 olamaz", "Validation error", true);
                    }
                    calculatedRatio = request.TargetAmount.Value / selectedMonthsTotal;
                }
                else
                {
                    return ResponseDto<BudgetCalculationResultViewModel>.FailData(400,
                        "Geçerli bir oran veya hedef tutar giriniz", "Validation error", true);
                }

                if (calculatedRatio <= 0)
                {
                    return ResponseDto<BudgetCalculationResultViewModel>.FailData(400,
                        "Hesaplanan oran 0 veya negatif olamaz", "Validation error", true);
                }

                // 5. Apply ratio to selected months
                var calculatedDuesStatistics = new List<DuesStatisticListViewModel>();
                foreach (var sourceDues in sourceDuesStatistics)
                {
                    // Yeni cari bilgilerini Logo'dan çek
                    string newClientCode = sourceDues.ClientCode; // Varsayılan olarak mevcut kodu kullan
                    long newClientRef = sourceDues.ClientRef;     // Varsayılan olarak mevcut ref'i kullan

                    try
                    {
                        // DivCode (işyeri kodu) ile Logo'dan yeni cari bilgilerini çek
                        var clientInfoResult = await _apiLogoSqlDataService.GetClientInfoByWorkplaceCodeAsync(sourceDues.DivCode);
                        if (clientInfoResult.IsSuccess && !string.IsNullOrWhiteSpace(clientInfoResult.Data.ClientCode))
                        {
                            newClientCode = clientInfoResult.Data.ClientCode;
                            newClientRef = clientInfoResult.Data.ClientRef;
                            _logger.LogInformation("Yeni cari bilgisi çekildi: {DivCode} -> {NewClientCode} (Ref: {NewClientRef})",
                                sourceDues.DivCode, newClientCode, newClientRef);
                        }
                        else
                        {
                            _logger.LogWarning("Yeni cari bilgisi bulunamadı: {DivCode}, mevcut bilgiler kullanılacak: {OldClientCode} (Ref: {OldClientRef})",
                                sourceDues.DivCode, sourceDues.ClientCode, sourceDues.ClientRef);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Yeni cari bilgisi çekilirken hata oluştu: {DivCode}", sourceDues.DivCode);
                    }

                    var calculatedDues = new DuesStatisticListViewModel
                    {
                        Id = sourceDues.Id,
                        Code = sourceDues.Code,
                        Year = sourceDues.Year.ToString(),
                        DivCode = sourceDues.DivCode,
                        DivName = sourceDues.DivName,
                        DocTrackingNr = sourceDues.DocTrackingNr,
                        ClientCode = newClientCode, // Yeni cari kodu
                        ClientRef = newClientRef      // Yeni cari referansı
                    };

                    // Apply ratio to selected months only
                    calculatedDues.January = selectedMonths.Contains(1) ? sourceDues.January * calculatedRatio : 0;
                    calculatedDues.February = selectedMonths.Contains(2) ? sourceDues.February * calculatedRatio : 0;
                    calculatedDues.March = selectedMonths.Contains(3) ? sourceDues.March * calculatedRatio : 0;
                    calculatedDues.April = selectedMonths.Contains(4) ? sourceDues.April * calculatedRatio : 0;
                    calculatedDues.May = selectedMonths.Contains(5) ? sourceDues.May * calculatedRatio : 0;
                    calculatedDues.June = selectedMonths.Contains(6) ? sourceDues.June * calculatedRatio : 0;
                    calculatedDues.July = selectedMonths.Contains(7) ? sourceDues.July * calculatedRatio : 0;
                    calculatedDues.August = selectedMonths.Contains(8) ? sourceDues.August * calculatedRatio : 0;
                    calculatedDues.September = selectedMonths.Contains(9) ? sourceDues.September * calculatedRatio : 0;
                    calculatedDues.October = selectedMonths.Contains(10) ? sourceDues.October * calculatedRatio : 0;
                    calculatedDues.November = selectedMonths.Contains(11) ? sourceDues.November * calculatedRatio : 0;
                    calculatedDues.December = selectedMonths.Contains(12) ? sourceDues.December * calculatedRatio : 0;

                    // Calculate total
                    calculatedDues.Total = calculatedDues.January + calculatedDues.February + calculatedDues.March +
                                           calculatedDues.April + calculatedDues.May + calculatedDues.June +
                                           calculatedDues.July + calculatedDues.August + calculatedDues.September +
                                           calculatedDues.October + calculatedDues.November + calculatedDues.December;

                    calculatedDuesStatistics.Add(calculatedDues);
                }

                // 6. Calculate totals
                decimal calculatedTotal = calculatedDuesStatistics.Sum(d => d.Total);

                var result = new BudgetCalculationResultViewModel
                {
                    CalculatedDuesStatistics = calculatedDuesStatistics,
                    SelectedMonthsTotal = selectedMonthsTotal,
                    CalculatedTotal = calculatedTotal,
                    AppliedRatio = calculatedRatio,
                    AppliedPercentage = (calculatedRatio - 1) * 100
                };

                _logger.LogInformation("Bütçe hesaplama tamamlandı. SelectedMonthsTotal: {SelectedMonthsTotal}, CalculatedTotal: {CalculatedTotal}, Ratio: {Ratio}",
                    selectedMonthsTotal, calculatedTotal, calculatedRatio);

                return ResponseDto<BudgetCalculationResultViewModel>.SuccessData(200, "Bütçe hesaplama başarılı", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bütçe hesaplama hatası");
                return ResponseDto<BudgetCalculationResultViewModel>.FailData(500, "Bütçe hesaplanırken bir hata oluştu", ex.Message, true);
            }
        }

        private List<int> ParseMonthsFlag(int flag)
        {
            var months = new List<int>();
            int[] monthFlags = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };
            int[] monthNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            for (int i = 0; i < monthFlags.Length; i++)
            {
                if ((flag & monthFlags[i]) != 0)
                {
                    months.Add(monthNumbers[i]);
                }
            }

            return months;
        }

        private decimal GetMonthlyValue(DuesStatisticListViewModel dues, int month)
        {
            return month switch
            {
                1 => dues.January,
                2 => dues.February,
                3 => dues.March,
                4 => dues.April,
                5 => dues.May,
                6 => dues.June,
                7 => dues.July,
                8 => dues.August,
                9 => dues.September,
                10 => dues.October,
                11 => dues.November,
                12 => dues.December,
                _ => 0
            };
        }

        public async Task<ResponseDto<bool>> CreateBudgetRatioAsync(BudgetRatio budgetRatio)
        {
            try
            {
                // UnitOfWork üzerinden repository al (aynı DbContext)
                await _unitOfWork.BudgetRatioRepository.AddAsync(budgetRatio);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("BudgetRatio başarıyla oluşturuldu: {Id}", budgetRatio.Id);
                return ResponseDto<bool>.SuccessData(200, "BudgetRatio başarıyla oluşturuldu", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BudgetRatio oluşturma hatası");
                return ResponseDto<bool>.FailData(500, "BudgetRatio oluşturulurken hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<bool>> CreateDuesStatisticAsync(DuesStatistic duesStatistic)
        {
            try
            {
                // UnitOfWork üzerinden repository al (aynı DbContext)
                await _unitOfWork.DuesStatisticRepository.AddAsync(duesStatistic);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("DuesStatistic başarıyla oluşturuldu: {Code}", duesStatistic.Code);
                return ResponseDto<bool>.SuccessData(200, "DuesStatistic başarıyla oluşturuldu", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DuesStatistic oluşturma hatası: {Code}", duesStatistic.Code);
                return ResponseDto<bool>.FailData(500, "DuesStatistic oluşturulurken hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<bool>> UpdateBudgetRatioAsync(BudgetRatio budgetRatio)
        {
            try
            {
                // UnitOfWork üzerinden repository al (aynı DbContext)
                await _unitOfWork.BudgetRatioRepository.UpdateAsync(budgetRatio);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("BudgetRatio başarıyla güncellendi: {Id}", budgetRatio.Id);
                return ResponseDto<bool>.SuccessData(200, "BudgetRatio başarıyla güncellendi", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BudgetRatio güncelleme hatası: {Id}", budgetRatio.Id);
                return ResponseDto<bool>.FailData(500, "BudgetRatio güncellenirken hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<BudgetRatio>> GetBudgetRatioByIdAsync(string id)
        {
            try
            {
                var budgetRatio = await _unitOfWork.BudgetRatioRepository.GetByIdAsync(id);
                if (budgetRatio == null)
                {
                    return ResponseDto<BudgetRatio>.FailData(404, "BudgetRatio bulunamadı", "", true);
                }

                return ResponseDto<BudgetRatio>.SuccessData(200, "BudgetRatio getirildi", budgetRatio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BudgetRatio getirme hatası: {Id}", id);
                return ResponseDto<BudgetRatio>.FailData(500, "BudgetRatio getirilirken hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<(BudgetRatio budgetRatio, List<DuesStatistic> duesStatistics)>> GetBudgetRatioWithDuesAsync(string id)
        {
            try
            {
                var budgetRatio = await _unitOfWork.BudgetRatioRepository.GetByIdAsync(id);
                if (budgetRatio == null)
                {
                    return ResponseDto<(BudgetRatio budgetRatio, List<DuesStatistic> duesStatistics)>.FailData(404, "BudgetRatio bulunamadı", "", true);
                }

                var allDues = await _unitOfWork.DuesStatisticRepository.GetAllAsync();
                var duesStatistics = allDues.Where(d => d.BuggetRatioId == id).ToList();

                return ResponseDto<(BudgetRatio budgetRatio, List<DuesStatistic> duesStatistics)>.SuccessData(200, "Veriler getirildi", (budgetRatio, duesStatistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BudgetRatio ve DuesStatistic getirme hatası: {Id}", id);
                return ResponseDto<(BudgetRatio budgetRatio, List<DuesStatistic> duesStatistics)>.FailData(500, "Veriler getirilirken hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<bool>> UpdateBudgetRatioWithDuesAsync(BudgetRatio budgetRatio, BuggetRatioMounthEnum selectedMonthsFlag)
        {
            try
            {
                // BudgetRatio'yu güncelle
                budgetRatio.BuggetRatioMounths = selectedMonthsFlag;
                budgetRatio.LastUpdateTime = DateTime.Now;
                await _unitOfWork.BudgetRatioRepository.UpdateAsync(budgetRatio);

                // Eski DuesStatistic kayıtlarını sil
                var allDues = await _unitOfWork.DuesStatisticRepository.GetAllAsync();
                var existingDues = allDues.Where(d => d.BuggetRatioId == budgetRatio.Id).ToList();

                if (existingDues.Any())
                {
                    foreach (var dues in existingDues)
                    {
                        await _unitOfWork.DuesStatisticRepository.DeleteAsync(dues.Id);
                    }
                }

                // Toplam bütçeyi hesapla
                var totalBudget = 0m;

                // Yeni DuesStatistic kayıtlarını oluştur (kaynak yıldan yeniden hesapla)
                var sourceYear = budgetRatio.BuggetType == BuggetTypeEnum.Budget ? budgetRatio.Year - 1 : budgetRatio.Year;
                var allSourceDues = await _unitOfWork.DuesStatisticRepository.GetAllAsync();
                var sourceDuesStatistics = allSourceDues
                    .Where(d => d.Year == sourceYear.ToString() && d.BudgetType == BuggetTypeEnum.Budget)
                    .ToList();

                foreach (var sourceDues in sourceDuesStatistics)
                {
                    // Seçilen aylara göre yeni değerleri hesapla
                    var newDues = CalculateDuesForUpdate(sourceDues, budgetRatio.Ratio, selectedMonthsFlag, budgetRatio.Id);

                    await _unitOfWork.DuesStatisticRepository.AddAsync(newDues);
                    totalBudget += newDues.Total;
                }

                // TotalBugget'i güncelle
                budgetRatio.TotalBugget = totalBudget;
                await _unitOfWork.BudgetRatioRepository.UpdateAsync(budgetRatio);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("BudgetRatio ve DuesStatistic kayıtları başarıyla güncellendi: {Id}", budgetRatio.Id);
                return ResponseDto<bool>.SuccessData(200, "Güncelleme başarılı", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BudgetRatio güncelleme hatası: {Id}", budgetRatio.Id);
                return ResponseDto<bool>.FailData(500, "Güncellenirken hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<PreviewUpdateResultViewModel>> PreviewUpdateAsync(string budgetRatioId, decimal? newRatio, decimal? targetAmount, BuggetRatioMounthEnum newMonthsFlag)
        {
            try
            {
                // Mevcut BudgetRatio'ı getir
                var budgetRatio = await _unitOfWork.BudgetRatioRepository.GetByIdAsync(budgetRatioId);
                if (budgetRatio == null)
                {
                    return ResponseDto<PreviewUpdateResultViewModel>.FailData(404, "BudgetRatio bulunamadı", "", true);
                }

                // Kaynak yılı belirle
                var sourceYear = budgetRatio.BuggetType == BuggetTypeEnum.Budget ? budgetRatio.Year - 1 : budgetRatio.Year;

                // Kaynak yıldan DuesStatistic kayıtlarını getir
                var allDues = await _unitOfWork.DuesStatisticRepository.GetAllAsync();
                var sourceDuesStatistics = allDues
                    .Where(d => d.Year == sourceYear.ToString() && d.BudgetType == BuggetTypeEnum.Budget)
                    .ToList();

                if (!sourceDuesStatistics.Any())
                {
                    return ResponseDto<PreviewUpdateResultViewModel>.FailData(404, $"Kaynak yıl ({sourceYear}) için veri bulunamadı", "", true);
                }

                // Kaynak yılın seçili aylardaki toplamını hesapla
                var sourceSelectedMonthsTotal = sourceDuesStatistics
                    .Sum(d => GetSelectedMonthsTotal(d, newMonthsFlag));

                // Oran hesapla
                decimal calculatedRatio = 0;
                if (newRatio.HasValue)
                {
                    // Oran verildi - direkt kullan
                    calculatedRatio = newRatio.Value;
                }
                else if (targetAmount.HasValue)
                {
                    // Toplam bütçe verildi - oranı hesapla
                    if (sourceSelectedMonthsTotal == 0)
                    {
                        return ResponseDto<PreviewUpdateResultViewModel>.FailData(400, "Seçili aylar için kaynak verisi yok", "", true);
                    }
                    calculatedRatio = (targetAmount.Value - sourceSelectedMonthsTotal) / sourceSelectedMonthsTotal;
                }
                else
                {
                    return ResponseDto<PreviewUpdateResultViewModel>.FailData(400, "Oran veya toplam bütçe verilmelidir", "", true);
                }

                // Önizleme sonucunu hazırla
                var result = new PreviewUpdateResultViewModel();
                var newTotal = 0m;

                foreach (var sourceDues in sourceDuesStatistics)
                {
                    // Kaynak yıldan yeni değerleri hesapla
                    var newDues = CalculateDuesForUpdate(sourceDues, calculatedRatio, newMonthsFlag, budgetRatioId);

                    // Kaynak yıldan mevcut seçili aylar toplamı
                    var sourceSelectedMonthsTotalForRow = GetSelectedMonthsTotal(sourceDues, newMonthsFlag);

                    newTotal += newDues.Total;

                    result.Rows.Add(new PreviewUpdateRowViewModel
                    {
                        DivName = sourceDues.DivName,
                        Code = sourceDues.Code,
                        ClientCode = sourceDues.ClientCode,
                        CurrentSelectedMonths = sourceSelectedMonthsTotalForRow,  // Kaynak yıldan
                        CurrentTotal = sourceDues.Total,  // Kaynak yıldan toplam
                        NewSelectedMonths = GetSelectedMonthsTotal(newDues, newMonthsFlag),
                        NewTotal = newDues.Total
                    });
                }

                result.NewTotal = newTotal;
                result.TotalDiff = newTotal - budgetRatio.TotalBugget;
                result.CalculatedRatio = calculatedRatio;

                return ResponseDto<PreviewUpdateResultViewModel>.SuccessData(200, "Önizleme hazır", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bütçe güncelleme önizleme hatası: {Id}", budgetRatioId);
                return ResponseDto<PreviewUpdateResultViewModel>.FailData(500, "Önizleme sırasında hata oluştu", ex.Message, true);
            }
        }

        private decimal GetSelectedMonthsTotal(DuesStatistic dues, BuggetRatioMounthEnum selectedMonths)
        {
            var total = 0m;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.January)) total += dues.January;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.February)) total += dues.February;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.March)) total += dues.March;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.April)) total += dues.April;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.May)) total += dues.May;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.June)) total += dues.June;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.July)) total += dues.July;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.August)) total += dues.August;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.September)) total += dues.September;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.October)) total += dues.October;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.November)) total += dues.November;
            if (selectedMonths.HasFlag(BuggetRatioMounthEnum.December)) total += dues.December;
            return total;
        }

        private DuesStatistic CalculateDuesForUpdate(DuesStatistic source, decimal ratio, BuggetRatioMounthEnum selectedMonths, string budgetRatioId)
        {
            var newDues = new DuesStatistic
            {
                Id = Guid.NewGuid().ToString(),
                Year = source.Year,
                Code = source.Code,
                DivCode = source.DivCode,
                DivName = source.DivName,
                DocTrackingNr = source.DocTrackingNr,
                ClientCode = source.ClientCode,
                ClientRef = source.ClientRef,
                BudgetType = BuggetTypeEnum.Budget, // Güncellenmiş bütçe
                BuggetRatioId = budgetRatioId,
                TransferStatus = TransferStatusEnum.Pending,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now,
                // Aylık değerleri hesapla
                January = selectedMonths.HasFlag(BuggetRatioMounthEnum.January) ? source.January * (1 + ratio) : 0,
                February = selectedMonths.HasFlag(BuggetRatioMounthEnum.February) ? source.February * (1 + ratio) : 0,
                March = selectedMonths.HasFlag(BuggetRatioMounthEnum.March) ? source.March * (1 + ratio) : 0,
                April = selectedMonths.HasFlag(BuggetRatioMounthEnum.April) ? source.April * (1 + ratio) : 0,
                May = selectedMonths.HasFlag(BuggetRatioMounthEnum.May) ? source.May * (1 + ratio) : 0,
                June = selectedMonths.HasFlag(BuggetRatioMounthEnum.June) ? source.June * (1 + ratio) : 0,
                July = selectedMonths.HasFlag(BuggetRatioMounthEnum.July) ? source.July * (1 + ratio) : 0,
                August = selectedMonths.HasFlag(BuggetRatioMounthEnum.August) ? source.August * (1 + ratio) : 0,
                September = selectedMonths.HasFlag(BuggetRatioMounthEnum.September) ? source.September * (1 + ratio) : 0,
                October = selectedMonths.HasFlag(BuggetRatioMounthEnum.October) ? source.October * (1 + ratio) : 0,
                November = selectedMonths.HasFlag(BuggetRatioMounthEnum.November) ? source.November * (1 + ratio) : 0,
                December = selectedMonths.HasFlag(BuggetRatioMounthEnum.December) ? source.December * (1 + ratio) : 0,
                Total = 0 // Hesaplanacak
            };

            // Toplamı hesapla
            newDues.Total = newDues.January + newDues.February + newDues.March + newDues.April +
                            newDues.May + newDues.June + newDues.July + newDues.August +
                            newDues.September + newDues.October + newDues.November + newDues.December;

            return newDues;
        }

        public async Task<ResponseDto<List<OrderResultViewModel>>> TransferDuesStatisticsToLogoAsync(
            List<string> duesStatisticIds,
            string? userId = null,
            bool isDebugMode = false)
        {
            // TransferService için logger oluştur
            var transferLogger = _loggerFactory.CreateLogger<BudgetOrderTransferService>();

            // TransferService'i kullanarak aktarım yap
            var transferService = new BudgetOrderTransferService(
                _unitOfWork,
                _logoRestService,
                transferLogger,
                _emailService);

            return await transferService.TransferDuesStatisticsToLogoAsync(duesStatisticIds, userId, isDebugMode);
        }

        public async Task<ResponseDto<bool>> LockBudgetRatioAsync(string budgetRatioId)
        {
            try
            {
                var budgetRatio = await _unitOfWork.BudgetRatioRepository.GetByIdAsync(budgetRatioId);
                if (budgetRatio == null)
                {
                    return ResponseDto<bool>.FailData(404, "BudgetRatio bulunamadı", "", true);
                }

                // Zaten Locked ise başarılı say
                if (budgetRatio.Status == StatusEnum.Locked)
                {
                    _logger.LogInformation("BudgetRatio zaten Locked: {Id}", budgetRatioId);
                    return ResponseDto<bool>.SuccessData(200, "BudgetRatio zaten kilitli", true);
                }

                // Durumu Locked yap
                budgetRatio.Status = StatusEnum.Locked;
                budgetRatio.LastUpdateTime = DateTime.Now;
                await _unitOfWork.BudgetRatioRepository.UpdateAsync(budgetRatio);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("BudgetRatio Locked yapıldı: {Id} - {Code}", budgetRatio.Id, budgetRatio.Code);
                return ResponseDto<bool>.SuccessData(200, "BudgetRatio başarıyla kilitlendi", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BudgetRatio kilitleme hatası: {Id}", budgetRatioId);
                return ResponseDto<bool>.FailData(500, "BudgetRatio kilitlenirken hata oluştu", ex.Message, true);
            }
        }
    }
}
