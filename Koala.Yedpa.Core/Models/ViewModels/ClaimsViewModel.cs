using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models.ViewModels;

public class CreateClaimsViewModel
{
    public string ModuleId { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
}
public class UpdateClaimsViewModel : CreateClaimsViewModel
{
    public string Id { get; set; } = Tools.CreateGuidStr();
}
public class ClaimsListViewModel
{
    public string Id { get; set; } = Tools.CreateGuidStr();
    public string ModuleId { get; set; }
    public string ModuleName { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
}
public class ClaimListForUserViewModels
{
    public string DisplayName { get; set; }
    public string Name { get; set; }
}
public class ClaimListForRoleViewModels
{
    public string ModuleId { get; set; }
    public string ModuleName { get; set; }
    public string DisplayName { get; set; }
    public string Name { get; set; }
}
public class SearchClaimViewModel
{
    public string? ModuleId { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}