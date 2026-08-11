using FluentAssertions;
using Koala.Yedpa.WebUI.Authorization;

namespace Koala.Yedpa.WebUI.Tests.Authorization;

public class PermissionCatalogTests
{
    [Fact]
    public void Modules_Bos_Olmamali()
    {
        PermissionCatalog.Modules.Should().NotBeEmpty();
    }

    [Fact]
    public void TumIzinAdlari_Benzersiz_Olmali()
    {
        var names = PermissionCatalog.Modules.SelectMany(m => m.Permissions).Select(p => p.Name).ToList();

        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ModulAdlari_Benzersiz_Olmali()
    {
        PermissionCatalog.Modules.Select(m => m.Name).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void IzinAdlari_ModulAdiIleBaslamali_Ve_Nokta_Icermeli()
    {
        foreach (var module in PermissionCatalog.Modules)
        {
            foreach (var permission in module.Permissions)
            {
                permission.Name.Should().StartWith($"{module.Name}.",
                    because: $"'{permission.Name}' izni '{module.Name}' modülüne ait");
            }
        }
    }

    [Fact]
    public void IzinAdlari_TurkceKarakter_Ve_Bosluk_Icermemeli()
    {
        var gecersiz = PermissionCatalog.AllPermissionNames
            .Where(n => n.Any(c => !(char.IsLetterOrDigit(c) && c < 128) && c != '.'))
            .ToList();

        gecersiz.Should().BeEmpty();
    }

    [Fact]
    public void AllPermissionNames_TumModullerinIzinleriniIcermeli()
    {
        var beklenen = PermissionCatalog.Modules.SelectMany(m => m.Permissions).Select(p => p.Name);

        PermissionCatalog.AllPermissionNames.Should().BeEquivalentTo(beklenen);
    }

    [Fact]
    public void DisplayName_Ve_Description_Bos_Olmamali()
    {
        foreach (var module in PermissionCatalog.Modules)
        {
            module.DisplayName.Should().NotBeNullOrWhiteSpace();
            module.Description.Should().NotBeNullOrWhiteSpace();

            foreach (var permission in module.Permissions)
            {
                permission.DisplayName.Should().NotBeNullOrWhiteSpace();
                permission.Description.Should().NotBeNullOrWhiteSpace();
            }
        }
    }
}
