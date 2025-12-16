using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models.ViewModels;

public class EmailTemplateListViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
public class EmailTemplateDetailViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public string CreateUserId { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public string? LastUpdateUserId { get; set; }
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
    public StatusEnum Status { get; set; } = StatusEnum.Active;
}
public class EmailTemplateCreateViewModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
}
public class EmailTemplateUpdateViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
}
public class EmailTemplateChangeStatusViewModel
{
    public string Id { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Active;
}