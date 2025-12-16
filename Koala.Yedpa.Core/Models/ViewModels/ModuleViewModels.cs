using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models.ViewModels;

public class CreateModuleViewModel
{
    public string Name { get; set; }
    public string? DisplayName { get; set; }
    public string Description { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Active;
}
public class UpdateModuleViewModel : CreateModuleViewModel
{
    public string Id { get; set; } = Tools.CreateGuidStr();
}
public class ModuleListViewModel
{
    public string Id { get; set; } = Tools.CreateGuidStr();
    public string Name { get; set; }
    public string? DisplayName { get; set; }
    public string Description { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Active;
    public List<ClaimsListViewModel> ModuleClaims { get; set; } = new List<ClaimsListViewModel>();
}

public class ModuleChangeStatusViewModel
{
    public string Id { get; set; }
    public StatusEnum Status { get; set; }
}
public class SearchModuleViewModel
{
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public StatusEnum? Status { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
}