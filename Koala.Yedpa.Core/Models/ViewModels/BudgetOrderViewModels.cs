using Koala.Yedpa.Core.Dtos;
using System.ComponentModel.DataAnnotations;

namespace Koala.Yedpa.Core.Models.ViewModels
{
    /// <summary>
    /// Bütçe ve sipariş oluşturma istek modeli
    /// </summary>
    public class CreateBudgetOrderViewModel
    {
        [Required(ErrorMessage = "Kaynak yıl gereklidir")]
        public int SourceYear { get; set; }        // Kaynak yıl (örn: 2025)

        [Required(ErrorMessage = "Hedef yıl gereklidir")]
        public int TargetYear { get; set; }        // Hedef yıl (örn: 2026)

        [Required(ErrorMessage = "Bütçe oranı ID gereklidir")]
        public string BudgetRatioId { get; set; }  // BudgetRatio ID

        [Required(ErrorMessage = "Bütçe tipi gereklidir")]
        public BuggetTypeEnum BudgetType { get; set; } // Budget veya ExtraBudget

        [Required(ErrorMessage = "Seçilen aylar gereklidir")]
        public List<int> SelectedMonths { get; set; } = new List<int>(); // Seçilen aylar [1, 2, 3, ..., 12]

        public bool CreateOrders { get; set; }     // Sipariş oluşturulsun mu?

        public string? UserId { get; set; }        // İşlemi yapan kullanıcı
    }

    /// <summary>
    /// Bütçe hesaplama isteği modeli
    /// </summary>
    public class BudgetCalculationRequestViewModel
    {
        [Required(ErrorMessage = "Kaynak yıl gereklidir")]
        public int SourceYear { get; set; }

        [Required(ErrorMessage = "Bütçe tipi gereklidir")]
        public BuggetTypeEnum BudgetType { get; set; }

        public int SelectedMonthsFlag { get; set; } // Checkbox flag değerleri (1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048)

        public decimal? Ratio { get; set; }       // Oran (örn: 1.25)
        public decimal? TargetAmount { get; set; } // Hedef tutar (örn: 135770811)
    }

    /// <summary>
    /// Bütçe hesaplama sonucu modeli
    /// </summary>
    public class BudgetCalculationResultViewModel
    {
        public List<DuesStatisticListViewModel> CalculatedDuesStatistics { get; set; } = new List<DuesStatisticListViewModel>();
        public decimal SelectedMonthsTotal { get; set; }  // Seçili ayların kaynak yıl toplamı
        public decimal CalculatedTotal { get; set; }      // Hesaplanan yeni toplam
        public decimal AppliedRatio { get; set; }         // Uygulanan oran
        public decimal AppliedPercentage { get; set; }    // Uygulanan yüzde ((oran - 1) * 100)
    }

    /// <summary>
    /// Bütçe ve sipariş oluşturma sonuç modeli
    /// </summary>
    public class BudgetOrderResultViewModel
    {
        public List<DuesStatisticListViewModel> CreatedDuesStatistics { get; set; } = new List<DuesStatisticListViewModel>();
        public List<OrderResultViewModel> SuccessfulOrders { get; set; } = new List<OrderResultViewModel>();
        public List<OrderResultViewModel> FailedOrders { get; set; } = new List<OrderResultViewModel>();
        public int TotalCreated { get; set; }
        public int TotalOrdersSent { get; set; }
        public int TotalOrdersFailed { get; set; }
        public decimal SourceTotalAmount { get; set; }  // Kaynak yıl toplamı
        public decimal TargetTotalAmount { get; set; }  // Hedef yıl toplamı (oran uygulanmış)
        public decimal AppliedRatio { get; set; }       // Uygulanan oran
        public string? TransactionId { get; set; }      // İşlem takip ID'si
    }

    /// <summary>
    /// Sipariş sonucu modeli
    /// </summary>
    public class OrderResultViewModel
    {
        public string ClientCode { get; set; } = string.Empty;
        public string ClientRef { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? OrderNumber { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal OrderAmount { get; set; }  // Sipariş tutarı
    }

    /// <summary>
    /// Mevcut bütçe için sipariş oluşturma modeli
    /// </summary>
    public class CreateOrdersForExistingBudgetViewModel
    {
        [Required(ErrorMessage = "Bütçe oranı ID gereklidir")]
        public string BudgetRatioId { get; set; }

        [Required(ErrorMessage = "Seçilen aylar gereklidir")]
        public List<int> SelectedMonths { get; set; } = new List<int>();

        public string? UserId { get; set; }
    }

    /// <summary>
    /// Yeni bütçe kaydetme modeli (Create page'den gelen)
    /// </summary>
    public class SaveNewBudgetViewModel
    {
        [Required(ErrorMessage = "Kaynak yıl gereklidir")]
        public int SourceYear { get; set; }

        public int? TargetYear { get; set; }  // Extra Budget için null

        [Required(ErrorMessage = "Bütçe tipi gereklidir")]
        public BuggetTypeEnum BudgetType { get; set; }

        [Required(ErrorMessage = "Oran gereklidir")]
        public decimal Ratio { get; set; }

        [Required(ErrorMessage = "Seçilen aylar gereklidir")]
        public List<int> SelectedMonths { get; set; } = new List<int>();

        public List<DuesStatisticSaveItemViewModel> DuesData { get; set; } = new List<DuesStatisticSaveItemViewModel>();
    }

    /// <summary>
    /// DuesStatistic kaydetme öğesi
    /// </summary>
    public class DuesStatisticSaveItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? DivCode { get; set; }
        public string? DivName { get; set; }
        public long? DocTrackingNr { get; set; }
        public string? ClientCode { get; set; }
        public long? ClientRef { get; set; }

        public decimal January { get; set; }
        public decimal February { get; set; }
        public decimal March { get; set; }
        public decimal April { get; set; }
        public decimal May { get; set; }
        public decimal June { get; set; }
        public decimal July { get; set; }
        public decimal August { get; set; }
        public decimal September { get; set; }
        public decimal October { get; set; }
        public decimal November { get; set; }
        public decimal December { get; set; }
        public decimal Total { get; set; }
    }
}
