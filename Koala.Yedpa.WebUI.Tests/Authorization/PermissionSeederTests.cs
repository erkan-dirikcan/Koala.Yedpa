using System.Security.Claims;
using FluentAssertions;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.WebUI.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Koala.Yedpa.WebUI.Tests.Authorization;

public class PermissionSeederTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"seed-{Guid.NewGuid()}")
            .Options);

    [Fact]
    public async Task Seed_BosVeritabanina_TumModulVeClaimleriEkler()
    {
        await using var context = CreateContext();

        var eklenen = await PermissionSeeder.SeedModulesAndClaimsAsync(
            context, NullLogger.Instance, CancellationToken.None);

        var beklenenClaimSayisi = PermissionCatalog.AllPermissionNames.Count;
        var beklenenModulSayisi = PermissionCatalog.Modules.Count;

        eklenen.Should().Be(beklenenModulSayisi + beklenenClaimSayisi);
        (await context.Module.CountAsync()).Should().Be(beklenenModulSayisi);
        (await context.Claims.CountAsync()).Should().Be(beklenenClaimSayisi);
    }

    [Fact]
    public async Task Seed_IkinciCalistirmada_HicbirSeyEklemez()
    {
        await using var context = CreateContext();

        await PermissionSeeder.SeedModulesAndClaimsAsync(context, NullLogger.Instance, CancellationToken.None);
        var ikinciTur = await PermissionSeeder.SeedModulesAndClaimsAsync(context, NullLogger.Instance, CancellationToken.None);

        ikinciTur.Should().Be(0);
        (await context.Claims.CountAsync()).Should().Be(PermissionCatalog.AllPermissionNames.Count);
    }

    [Fact]
    public async Task Seed_MevcutModulunAdiniDegistirmez_SadeceEksikClaimiEkler()
    {
        await using var context = CreateContext();
        var katalogModul = PermissionCatalog.Modules.First();
        context.Module.Add(new Module
        {
            Id = "elle-eklenmis-id",
            Name = katalogModul.Name,
            DisplayName = "Kullanıcı Tarafından Değiştirilmiş",
            Description = "elle girilmiş açıklama"
        });
        await context.SaveChangesAsync();

        await PermissionSeeder.SeedModulesAndClaimsAsync(context, NullLogger.Instance, CancellationToken.None);

        var modul = await context.Module.SingleAsync(m => m.Name == katalogModul.Name);
        modul.Id.Should().Be("elle-eklenmis-id");
        modul.DisplayName.Should().Be("Kullanıcı Tarafından Değiştirilmiş");

        var claimler = await context.Claims.Where(c => c.ModuleId == "elle-eklenmis-id").ToListAsync();
        claimler.Should().HaveCount(katalogModul.Permissions.Count);
    }

    [Fact]
    public async Task Seed_EklenenClaimler_KatalogAdlariylaBirebirAyni()
    {
        await using var context = CreateContext();

        await PermissionSeeder.SeedModulesAndClaimsAsync(context, NullLogger.Instance, CancellationToken.None);

        var dbAdlari = await context.Claims.Select(c => c.Name).ToListAsync();
        dbAdlari.Should().BeEquivalentTo(PermissionCatalog.AllPermissionNames);
    }

    private static Mock<RoleManager<AppRole>> CreateRoleManagerMock() =>
        new(
            Mock.Of<IRoleStore<AppRole>>(),
            Array.Empty<IRoleValidator<AppRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<AppRole>>>());

    [Fact]
    public async Task GrantAllToSuperAdmin_RolBulunamazsa_SifirDonerVeClaimEklemez()
    {
        var roleManagerMock = CreateRoleManagerMock();
        roleManagerMock.Setup(m => m.Roles).Returns(new List<AppRole>().AsQueryable());

        var sonuc = await PermissionSeeder.GrantAllToSuperAdminAsync(roleManagerMock.Object, NullLogger.Instance);

        sonuc.Should().Be(0);
        roleManagerMock.Verify(
            m => m.AddClaimAsync(It.IsAny<AppRole>(), It.IsAny<Claim>()),
            Times.Never);
    }

    [Fact]
    public async Task GrantAllToSuperAdmin_RolDisplayNameIleBulunur_TumIzinlerEklenir()
    {
        var rol = new AppRole { Id = "role-1", Name = "SuperAdmin", DisplayName = PermissionSeeder.SuperAdminRoleDisplayName };
        var roleManagerMock = CreateRoleManagerMock();
        roleManagerMock.Setup(m => m.Roles).Returns(new List<AppRole> { rol }.AsQueryable());
        roleManagerMock.Setup(m => m.GetClaimsAsync(rol)).ReturnsAsync(new List<Claim>());
        roleManagerMock.Setup(m => m.AddClaimAsync(rol, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

        var sonuc = await PermissionSeeder.GrantAllToSuperAdminAsync(roleManagerMock.Object, NullLogger.Instance);

        sonuc.Should().Be(PermissionCatalog.AllPermissionNames.Count);
        roleManagerMock.Verify(
            m => m.AddClaimAsync(rol, It.IsAny<Claim>()),
            Times.Exactly(PermissionCatalog.AllPermissionNames.Count));
    }

    [Fact]
    public async Task GrantAllToSuperAdmin_RolNameIleDeBulunur_TumIzinlerEklenir()
    {
        var rol = new AppRole { Id = "role-2", Name = PermissionSeeder.SuperAdminRoleDisplayName, DisplayName = null };
        var roleManagerMock = CreateRoleManagerMock();
        roleManagerMock.Setup(m => m.Roles).Returns(new List<AppRole> { rol }.AsQueryable());
        roleManagerMock.Setup(m => m.GetClaimsAsync(rol)).ReturnsAsync(new List<Claim>());
        roleManagerMock.Setup(m => m.AddClaimAsync(rol, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

        var sonuc = await PermissionSeeder.GrantAllToSuperAdminAsync(roleManagerMock.Object, NullLogger.Instance);

        sonuc.Should().Be(PermissionCatalog.AllPermissionNames.Count);
    }

    [Fact]
    public async Task GrantAllToSuperAdmin_ZatenVerilmisIzinler_TekrarEklenmez()
    {
        var rol = new AppRole { Id = "role-3", Name = "SuperAdmin", DisplayName = PermissionSeeder.SuperAdminRoleDisplayName };
        var mevcutIzinAdlari = PermissionCatalog.AllPermissionNames.Take(3).ToList();
        var mevcutClaimler = mevcutIzinAdlari
            .Select(izin => new Claim(PermissionPolicyProvider.PermissionClaimType, izin))
            .ToList();

        var roleManagerMock = CreateRoleManagerMock();
        roleManagerMock.Setup(m => m.Roles).Returns(new List<AppRole> { rol }.AsQueryable());
        roleManagerMock.Setup(m => m.GetClaimsAsync(rol)).ReturnsAsync(mevcutClaimler);
        roleManagerMock.Setup(m => m.AddClaimAsync(rol, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

        var sonuc = await PermissionSeeder.GrantAllToSuperAdminAsync(roleManagerMock.Object, NullLogger.Instance);

        var beklenenEksikSayisi = PermissionCatalog.AllPermissionNames.Count - mevcutIzinAdlari.Count;
        sonuc.Should().Be(beklenenEksikSayisi);
        roleManagerMock.Verify(
            m => m.AddClaimAsync(rol, It.IsAny<Claim>()),
            Times.Exactly(beklenenEksikSayisi));
        foreach (var izin in mevcutIzinAdlari)
        {
            roleManagerMock.Verify(
                m => m.AddClaimAsync(rol, It.Is<Claim>(c => c.Value == izin)),
                Times.Never);
        }
    }
}
