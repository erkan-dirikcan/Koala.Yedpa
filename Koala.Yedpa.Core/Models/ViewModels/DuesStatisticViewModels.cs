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