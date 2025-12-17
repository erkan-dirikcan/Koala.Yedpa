using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models.ViewModels;

public class CreateTransactionTypeViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorClass { get; set; }
    public string? Icon { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Active;
}

public class UpdateTransactionTypeViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorClass { get; set; }
    public string? Icon { get; set; }
    public StatusEnum Status { get; set; }
}

public class TransactionTypeViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorClass { get; set; }
    public string? Icon { get; set; }
    public StatusEnum Status { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public int TransactionCount { get; set; }
}

public class TransactionTypeSearchViewModel
{
    public string? Name { get; set; }
    public StatusEnum? Status { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
