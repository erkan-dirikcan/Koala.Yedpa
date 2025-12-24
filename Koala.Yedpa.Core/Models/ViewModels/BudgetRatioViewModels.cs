using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models.ViewModels;

public class CreateBudgetRatioViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public decimal Ratio { get; set; }
    public decimal TotalBugget { get; set; }
    public BuggetRatioMounthEnum BuggetRatioMounths { get; set; }
    public BuggetTypeEnum BuggetType { get; set; }
}

public class UpdateBudgetRatioViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public decimal Ratio { get; set; }
    public decimal TotalBugget { get; set; }
    public BuggetRatioMounthEnum BuggetRatioMounths { get; set; }
    public BuggetTypeEnum BuggetType { get; set; }
}

public class BudgetRatioListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public decimal Ratio { get; set; }
    public decimal TotalBugget { get; set; }
    public BuggetRatioMounthEnum BuggetRatioMounths { get; set; }
    public BuggetTypeEnum BuggetType { get; set; }
    public StatusEnum Status { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? LastUpdateTime { get; set; }
}

public class BudgetRatioDetailViewModel : BudgetRatioListViewModel
{
    // Additional details can be added here
}

public class BudgetRatioSearchViewModel
{
    public string? Code { get; set; }
    public string? Year { get; set; }
    public BuggetTypeEnum? BuggetType { get; set; }
    public BuggetRatioMounthEnum? BuggetRatioMounths { get; set; }
    public StatusEnum? Status { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
