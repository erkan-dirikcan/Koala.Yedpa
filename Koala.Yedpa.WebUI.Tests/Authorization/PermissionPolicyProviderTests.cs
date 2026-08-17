using FluentAssertions;
using Koala.Yedpa.WebUI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;

namespace Koala.Yedpa.WebUI.Tests.Authorization;

public class PermissionPolicyProviderTests
{
    private static PermissionPolicyProvider CreateProvider(Action<AuthorizationOptions>? configure = null)
    {
        var options = new AuthorizationOptions();
        configure?.Invoke(options);
        return new PermissionPolicyProvider(Options.Create(options));
    }

    [Fact]
    public async Task GetPolicyAsync_CokluIzin_Icin_HerhangiBiriniKabulEden_Sart_Uretir()
    {
        var provider = CreateProvider();
        var attribute = new PermissionAttribute("Workplace.View", "BudgetOrder.View");

        var policy = await provider.GetPolicyAsync(attribute.Policy!);

        var claimRequirement = policy!.Requirements.OfType<ClaimsAuthorizationRequirement>().Single();
        claimRequirement.ClaimType.Should().Be("Permission");
        // RequireClaim birden fazla değerle "herhangi biri" (VEYA) anlamına gelir.
        claimRequirement.AllowedValues.Should().BeEquivalentTo(["Workplace.View", "BudgetOrder.View"]);
    }

    [Fact]
    public void PermissionAttribute_TekIzinde_PolicyAdini_AynenKullanir()
    {
        var attribute = new PermissionAttribute("Workplace.View");

        attribute.Policy.Should().Be("Workplace.View",
            because: "tek izinli kullanımda ayraç eklenmemeli, davranış eskisiyle birebir aynı kalmalı");
    }

    [Fact]
    public void PermissionAttribute_BosIzinListesi_Icin_Hata_Firlatir()
    {
        var act = () => new PermissionAttribute();

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task GetPolicyAsync_BilinmeyenPolicyIcin_PermissionClaimSarti_Uretir()
    {
        var provider = CreateProvider();

        var policy = await provider.GetPolicyAsync("ModuleManagement.Create");

        policy.Should().NotBeNull();
        var claimRequirement = policy!.Requirements.OfType<ClaimsAuthorizationRequirement>().Single();
        claimRequirement.ClaimType.Should().Be("Permission");
        claimRequirement.AllowedValues.Should().ContainSingle()
            .Which.Should().Be("ModuleManagement.Create");
    }

    [Fact]
    public async Task GetPolicyAsync_UretilenPolicy_KimlikDogrulamasi_Da_Ister()
    {
        var provider = CreateProvider();

        var policy = await provider.GetPolicyAsync("ModuleManagement.Create");

        policy!.Requirements.OfType<DenyAnonymousAuthorizationRequirement>().Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPolicyAsync_ElleTanimliPolicy_Varsa_Onu_Dondurur()
    {
        var provider = CreateProvider(o =>
            o.AddPolicy("ElleTanimli", p => p.RequireClaim("scope", "sc-000000")));

        var policy = await provider.GetPolicyAsync("ElleTanimli");

        var claimRequirement = policy!.Requirements.OfType<ClaimsAuthorizationRequirement>().Single();
        claimRequirement.ClaimType.Should().Be("scope");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPolicyAsync_BosPolicyAdi_Icin_Null_Dondurur(string? policyName)
    {
        var provider = CreateProvider();

        var policy = await provider.GetPolicyAsync(policyName!);

        policy.Should().BeNull();
    }
}
