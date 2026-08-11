using FluentAssertions;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.WebUI.Authorization;
using Koala.Yedpa.WebUI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Koala.Yedpa.WebUI.Tests.Controllers;

public class UserControllerTests
{
    /// <summary>Testlerin çoğunun kullandığı varsayılan rol: Id "rol-1", gerçek adı "Muhasebe".</summary>
    private static AppRole MuhasebeRolu() =>
        new() { Id = "rol-1", Name = "Muhasebe", DisplayName = "Muhasebe Departmanı" };

    private static AppRole SuperYoneticiRolu() =>
        new() { Id = "rol-super", Name = "SuperAdmin", DisplayName = PermissionSeeder.SuperAdminRoleDisplayName };

    private static (UserController Controller, Mock<UserManager<AppUser>> UserManager) CreateSut(
        AppUser user, IEnumerable<string> mevcutRoller, IEnumerable<AppRole>? sunucudakiRoller = null)
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(mevcutRoller.ToList());
        userManager.Setup(x => x.AddToRoleAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.RemoveFromRoleAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Rol adı POST'ta istemciden değil, sunucudaki bu listeden Id ile çözülür.
        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager
            .Setup(x => x.Roles)
            .Returns((sunucudakiRoller ?? [MuhasebeRolu()]).ToList().AsQueryable());

        var controller = new UserController(
            NullLogger<UserController>.Instance,
            null!,
            userManager.Object,
            null!,
            roleManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            null!)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };

        return (controller, userManager);
    }

    [Fact]
    public async Task AsignRoleToUser_DisplayNameNamedenFarkliRolde_IdentityyeGercekRolAdiGider()
    {
        var user = new AppUser { Id = "user-1", UserName = "kullanici1" };
        var (controller, userManager) = CreateSut(user, mevcutRoller: []);

        await controller.AsignRoleToUser(
            [
                new AsignRoleToUserViewModel
                {
                    Id = "rol-1", Name = "Muhasebe", DisplayName = "Muhasebe Departmanı", IsExist = true
                }
            ],
            userId: "user-1");

        userManager.Verify(x => x.AddToRoleAsync(user, "Muhasebe"), Times.Once);
        userManager.Verify(x => x.AddToRoleAsync(user, "Muhasebe Departmanı"), Times.Never);
    }

    [Fact]
    public async Task AsignRoleToUser_ZatenAtanmisRol_TekrarEklenmez()
    {
        var user = new AppUser { Id = "user-1", UserName = "kullanici1" };
        var (controller, userManager) = CreateSut(user, mevcutRoller: ["Muhasebe"]);

        await controller.AsignRoleToUser(
            [
                new AsignRoleToUserViewModel
                {
                    Id = "rol-1", Name = "Muhasebe", DisplayName = "Muhasebe Departmanı", IsExist = true
                }
            ],
            userId: "user-1");

        userManager.Verify(x => x.AddToRoleAsync(user, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AsignRoleToUser_IsaretiKaldirilanRol_Cikarilir()
    {
        var user = new AppUser { Id = "user-1", UserName = "kullanici1" };
        var (controller, userManager) = CreateSut(user, mevcutRoller: ["Muhasebe"]);

        await controller.AsignRoleToUser(
            [
                new AsignRoleToUserViewModel
                {
                    Id = "rol-1", Name = "Muhasebe", DisplayName = "Muhasebe Departmanı", IsExist = false
                }
            ],
            userId: "user-1");

        userManager.Verify(x => x.RemoveFromRoleAsync(user, "Muhasebe"), Times.Once);
    }

    [Fact]
    public async Task AsignRoleToUser_RolDegisikliginden_SonraGuvenlikDamgasiYenilenir()
    {
        var user = new AppUser { Id = "user-1", UserName = "kullanici1" };
        var (controller, userManager) = CreateSut(user, mevcutRoller: []);

        await controller.AsignRoleToUser(
            [
                new AsignRoleToUserViewModel
                {
                    Id = "rol-1", Name = "Muhasebe", DisplayName = "Muhasebe Departmanı", IsExist = true
                }
            ],
            userId: "user-1");

        userManager.Verify(x => x.UpdateSecurityStampAsync(user), Times.Once);
    }

    [Fact]
    public async Task AsignRoleToUser_PostaSuperYoneticiRoluGonderilirse_HicRolAtanmaz()
    {
        var user = new AppUser { Id = "user-1", UserName = "kullanici1" };
        var (controller, userManager) = CreateSut(
            user,
            mevcutRoller: [],
            sunucudakiRoller: [MuhasebeRolu(), SuperYoneticiRolu()]);

        // Saldırı: gizli input DevTools'ta "Süper Yönetici" yapılıp IsExist=true gönderiliyor.
        await controller.AsignRoleToUser(
            [
                new AsignRoleToUserViewModel
                {
                    Id = "rol-super",
                    Name = PermissionSeeder.SuperAdminRoleDisplayName,
                    DisplayName = PermissionSeeder.SuperAdminRoleDisplayName,
                    IsExist = true
                }
            ],
            userId: "user-1");

        userManager.Verify(x => x.AddToRoleAsync(user, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AsignRoleToUser_GizliAlanSuperYoneticiYapilsaBile_SunucudakiGercekRolAdiKullanilir()
    {
        var user = new AppUser { Id = "user-1", UserName = "kullanici1" };
        var (controller, userManager) = CreateSut(
            user,
            mevcutRoller: [],
            sunucudakiRoller: [MuhasebeRolu(), SuperYoneticiRolu()]);

        // Id normal rolün, gönderilen Name ise "Süper Yönetici" — istemciye güvenilmemeli.
        await controller.AsignRoleToUser(
            [
                new AsignRoleToUserViewModel
                {
                    Id = "rol-1",
                    Name = PermissionSeeder.SuperAdminRoleDisplayName,
                    DisplayName = PermissionSeeder.SuperAdminRoleDisplayName,
                    IsExist = true
                }
            ],
            userId: "user-1");

        userManager.Verify(x => x.AddToRoleAsync(user, "Muhasebe"), Times.Once);
        userManager.Verify(
            x => x.AddToRoleAsync(user, PermissionSeeder.SuperAdminRoleDisplayName), Times.Never);
        userManager.Verify(x => x.AddToRoleAsync(user, "SuperAdmin"), Times.Never);
    }

    [Fact]
    public async Task AsignRoleToUser_GonderilenAdUydurmaOlsaBile_SunucudakiGercekRolAdiKullanilir()
    {
        var user = new AppUser { Id = "user-1", UserName = "kullanici1" };
        var (controller, userManager) = CreateSut(user, mevcutRoller: []);

        await controller.AsignRoleToUser(
            [
                new AsignRoleToUserViewModel
                {
                    Id = "rol-1", Name = "sahte", DisplayName = "sahte", IsExist = true
                }
            ],
            userId: "user-1");

        userManager.Verify(x => x.AddToRoleAsync(user, "Muhasebe"), Times.Once);
        userManager.Verify(x => x.AddToRoleAsync(user, "sahte"), Times.Never);
    }

    [Fact]
    public async Task AsignRoleToUser_SunucudaOlmayanRolId_Yoksayilir()
    {
        var user = new AppUser { Id = "user-1", UserName = "kullanici1" };
        var (controller, userManager) = CreateSut(user, mevcutRoller: []);

        await controller.AsignRoleToUser(
            [
                new AsignRoleToUserViewModel
                {
                    Id = "olmayan-rol", Name = "Muhasebe", DisplayName = "Muhasebe", IsExist = true
                }
            ],
            userId: "user-1");

        userManager.Verify(x => x.AddToRoleAsync(user, It.IsAny<string>()), Times.Never);
    }
}
