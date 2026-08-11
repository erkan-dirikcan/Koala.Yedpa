using System.ComponentModel.DataAnnotations;

namespace Koala.Yedpa.Core.Models.ViewModels;

public class CreateAppRoleViewModel
{
    public string? DisplayName { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

}
public class UpdateAppRoleViewModel : CreateAppRoleViewModel
{
    public string Id { get; set; } = string.Empty;
}
public class AppRoleListViewModel 
{
    public string Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
public class AppRoleUserViewModel
{
    public string Id { get; private set; }
    public string? DisplayName { get; set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public List<AppUserInfoViewModels> Users { get; private set; }
}
public class AsignRoleToUserViewModel
{
    public string Id { get; set; }

    /// <summary>Identity'nin gerçek rol adı. AddToRoleAsync/RemoveFromRoleAsync bunu ister.</summary>
    [Display(Name = "Rol Adı")]
    public string Name { get; set; }

    /// <summary>Ekranda gösterilen ad. Identity API'lerine ASLA gönderilmez.</summary>
    [Display(Name = "Görünen Ad")]
    public string DisplayName { get; set; }

    [Display(Name = "Rol Açıklaması")]
    public string Description { get; set; }

    [Display(Name = "Rol Atanmış mı?")]
    public bool IsExist { get; set; }
}
public class AddClaimToRoleViewModel
{
    public string RoleId { get; set; }
    public List<string> Claims { get; set; }
}