namespace Koala.Yedpa.Core.Constants
{
    public class DashboardWidgetDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Claim { get; set; } = string.Empty;
        public string PartialView { get; set; } = string.Empty;
        public int DefaultX { get; set; }
        public int DefaultY { get; set; }
        public int DefaultWidth { get; set; }
        public int DefaultHeight { get; set; }
        public bool DefaultVisible { get; set; }
    }

    public static class DashboardWidgets
    {
        public static readonly List<DashboardWidgetDefinition> All = new()
        {
            new() { Id = "W1", Title = "Toplam Bakiye Özeti", Claim = "CurrentAccuant", PartialView = "_WidgetBalanceSummary", DefaultX = 0, DefaultY = 0, DefaultWidth = 12, DefaultHeight = 2, DefaultVisible = true },
            new() { Id = "W4", Title = "Cari Bakiye Dağılımı", Claim = "CurrentAccuant", PartialView = "_WidgetBalanceDistribution", DefaultX = 0, DefaultY = 2, DefaultWidth = 6, DefaultHeight = 4, DefaultVisible = true },
            new() { Id = "W7", Title = "Aidat Tahsilat Oranı", Claim = "BudgetManagement", PartialView = "_WidgetDuesCollection", DefaultX = 6, DefaultY = 2, DefaultWidth = 6, DefaultHeight = 4, DefaultVisible = true },
            new() { Id = "W2", Title = "Bekleyen Faturalar", Claim = "CurrentAccuant", PartialView = "_WidgetPendingInvoices", DefaultX = 0, DefaultY = 6, DefaultWidth = 6, DefaultHeight = 4, DefaultVisible = true },
            new() { Id = "W5", Title = "Aylık Tahsilat Trendi", Claim = "CurrentAccuant", PartialView = "_WidgetMonthlyTrend", DefaultX = 6, DefaultY = 6, DefaultWidth = 6, DefaultHeight = 4, DefaultVisible = true },
            new() { Id = "W8", Title = "Aylık Bütçe Özeti", Claim = "BudgetManagement", PartialView = "_WidgetMonthlyBudget", DefaultX = 0, DefaultY = 10, DefaultWidth = 12, DefaultHeight = 3, DefaultVisible = true },
            new() { Id = "W3", Title = "Vadesi Geçen Ödemeler", Claim = "CurrentAccuant", PartialView = "_WidgetOverduePayments", DefaultX = 0, DefaultY = 13, DefaultWidth = 6, DefaultHeight = 3, DefaultVisible = false },
            new() { Id = "W6", Title = "Son İşlemler", Claim = "CurrentAccuant", PartialView = "_WidgetRecentTransactions", DefaultX = 6, DefaultY = 13, DefaultWidth = 6, DefaultHeight = 4, DefaultVisible = false },
            new() { Id = "W9", Title = "Yıllık Bütçe Karşılaştırma", Claim = "BudgetManagement", PartialView = "_WidgetYearlyBudget", DefaultX = 0, DefaultY = 17, DefaultWidth = 12, DefaultHeight = 4, DefaultVisible = false },
            new() { Id = "W10", Title = "Aktif Dükkan Sayısı", Claim = "Management", PartialView = "_WidgetShopCount", DefaultX = 0, DefaultY = 21, DefaultWidth = 3, DefaultHeight = 2, DefaultVisible = false }
        };
    }
}
