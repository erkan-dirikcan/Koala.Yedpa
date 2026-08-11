using System.Security.Claims;
using FluentAssertions;
using Koala.Yedpa.WebUI.Authorization;

namespace Koala.Yedpa.WebUI.Tests.Authorization;

public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal KullaniciOlustur(bool girisYapmis, params string[] izinler)
    {
        var claims = izinler.Select(i => new Claim("Permission", i)).ToList();
        var identity = girisYapmis
            ? new ClaimsIdentity(claims, "TestAuth")
            : new ClaimsIdentity(claims);
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void HasPermission_IzniOlanIcin_True_Doner()
    {
        var user = KullaniciOlustur(true, "ModuleManagement.List");

        user.HasPermission("ModuleManagement.List").Should().BeTrue();
    }

    [Fact]
    public void HasPermission_IzniOlmayanIcin_False_Doner()
    {
        var user = KullaniciOlustur(true, "ModuleManagement.List");

        user.HasPermission("ModuleManagement.Create").Should().BeFalse();
    }

    [Fact]
    public void HasPermission_GirisYapmamisKullanici_Icin_False_Doner()
    {
        var user = KullaniciOlustur(false, "ModuleManagement.List");

        user.HasPermission("ModuleManagement.List").Should().BeFalse();
    }

    [Fact]
    public void HasPermission_Null_Kullanici_Icin_False_Doner()
    {
        ClaimsPrincipal? user = null;

        user.HasPermission("ModuleManagement.List").Should().BeFalse();
    }

    [Fact]
    public void HasPermission_BuyukKucukHarf_Duyarli_Olmali()
    {
        var user = KullaniciOlustur(true, "ModuleManagement.List");

        user.HasPermission("modulemanagement.list").Should().BeFalse();
    }

    [Fact]
    public void HasAnyPermission_IzinlerdenBiriVarsa_True_Doner()
    {
        var user = KullaniciOlustur(true, "ModuleManagement.List");

        user.HasAnyPermission("ModuleManagement.Create", "ModuleManagement.List").Should().BeTrue();
    }

    [Fact]
    public void HasAnyPermission_HicbiriYoksa_False_Doner()
    {
        var user = KullaniciOlustur(true, "ModuleManagement.List");

        user.HasAnyPermission("RoleManagement.List", "UserManagement.List").Should().BeFalse();
    }

    [Fact]
    public void HasAnyPermission_BosDizi_Icin_False_Doner()
    {
        var user = KullaniciOlustur(true, "ModuleManagement.List");

        user.HasAnyPermission().Should().BeFalse();
    }
}
