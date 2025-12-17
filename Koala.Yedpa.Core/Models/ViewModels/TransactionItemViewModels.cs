namespace Koala.Yedpa.Core.Models.ViewModels;

public class CreateTransactionItemViewModel
{
    public string TransactionId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSuccess { get; set; } = true;
}

public class TransactionItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public class TransactionItemSearchViewModel
{
    public string TransactionId { get; set; } = string.Empty;
    public bool? IsSuccess { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
