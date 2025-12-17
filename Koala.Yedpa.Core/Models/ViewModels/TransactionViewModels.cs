namespace Koala.Yedpa.Core.Models.ViewModels;

public class CreateTransactionViewModel
{
    public string TransactionTypeId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

public class UpdateTransactionViewModel
{
    public string TransactionId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CompleteTransactionViewModel
{
    public string TransactionId { get; set; } = string.Empty;
}

public class DeleteTransactionViewModel
{
    public string TransactionId { get; set; } = string.Empty;
}

public class TransactionSearchViewModel
{
    public string? UserId { get; set; }
    public string? TransactionTypeId { get; set; }
    public bool? IsCompleted { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class TransactionListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string TransactionNumber { get; set; } = string.Empty;
    public string TransactionTypeId { get; set; } = string.Empty;
    public string? TransactionTypeName { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public List<TransactionItemViewModel> TransactionItems { get; set; } = new();
}

public class TransactionDetailViewModel : TransactionListViewModel
{
    // Additional details can be added here
}
