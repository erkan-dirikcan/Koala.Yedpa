using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models.ViewModels;

public class DuesStatisticSummaryViewModel
{
    public string Year { get; set; }
    public int TotalCompanies { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public Dictionary<string, decimal> MonthlyTotals { get; set; }
}

public class YearOverviewViewModel
{
    public string Year { get; set; }
    public int CompanyCount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime LastSyncDate { get; set; }
}

public class MonthlySummaryViewModel
{
    public string Month { get; set; }
    public string MonthNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public int CompanyCount { get; set; }
    public decimal AverageAmount { get; set; }
}
public class SourceDuesDataViewModel
{
    public string Code { get; set; }
    public string DivCode { get; set; }
    public string DivName { get; set; }
    public long DocTrackingNr { get; set; }
    public string ClientCode { get; set; }
    public long ClientRef { get; set; }
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

public class DuesStatisticListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ClientCode { get; set; } = string.Empty;
    public long ClientRef { get; set; }
    public decimal Total { get; set; }
    public BuggetTypeEnum BudgetType { get; set; }
    public TransferStatusEnum TransferStatus { get; set; }

    // Optional fields for display
    public string? DivCode { get; set; }
    public string? DivName { get; set; }
    public long? DocTrackingNr { get; set; }
    public string? BuggetRatioId { get; set; }

    // Monthly amounts
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
}

public class MonthlyBudgetDataViewModel
{
    public decimal Budget { get; set; }
    public decimal ExtraBudget { get; set; }
    public decimal NewBudget { get; set; }
    public decimal NewExtraBudget { get; set; }
}

public class MonthlyBudgetSummaryViewModel
{
    public int Year { get; set; }
    public BuggetTypeEnum BudgetType { get; set; }
    public decimal TotalBudget { get; set; }
    public TransferStatusEnum TransferStatus { get; set; }
    public List<MonthlyBudgetDataViewModel> MonthlyData { get; set; } = new();
}