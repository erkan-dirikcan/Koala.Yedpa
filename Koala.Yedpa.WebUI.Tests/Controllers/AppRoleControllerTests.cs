using System.Security.Claims;
using FluentAssertions;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.WebUI.Authorization;
using Koala.Yedpa.WebUI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Koala.Yedpa.WebUI.Tests.Controllers;

public class AppRoleControllerTests
{
    private static (AppRoleController Controller, Mock<RoleManager<AppRole>> RoleManager) CreateSut(AppRole rol)
    {
        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(x => x.FindByIdAsync(rol.Id)).ReturnsAsync(rol);
        roleManager.Setup(x => x.GetClaimsAsync(rol)).ReturnsAsync(new List<Claim>
        {
            new(PermissionPolicyProvider.PermissionClaimType, "ModuleManagement.List")
        });
        roleManager.Setup(x => x.RemoveClaimAsync(It.IsAny<AppRole>(), It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);
        roleManager.Setup(x => x.AddClaimAsync(It.IsAny<AppRole>(), It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        var claimsService = new Mock<IClaimsService>();
        claimsService.Setup(x => x.GetClaimToRoleList()).ReturnsAsync(
            ResponseDto<IEnumerable<ClaimListForRoleViewModels>>.SuccessData(200, "ok",
            [
                new ClaimListForRoleViewModels
                {
                    ModuleId = "mod-1", ModuleName = "ModuleManagement",
                    Name = "ModuleManagement.List", DisplayName = "Modül Listesi"
                }
            ]));

        var moduleService = new Mock<IModuleService>();
        moduleService.Setup(x => x.GetAllModuleAsync()).ReturnsAsync(
            ResponseDto<IEnumerable<ModuleListViewModel>>.SuccessData(200, "ok",
            [
                new ModuleListViewModel { Id = "mod-1", Name = "ModuleManagement", DisplayName = "Modül Yönetimi" }
            ]));

        var controller = new AppRoleController(
            NullLogger<AppRoleController>.Instance,
            null!,
            roleManager.Object,
            null!,
            Mock.Of<IHttpContextAccessor>(),
            claimsService.Object,
            moduleService.Object)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };

        return (controller, roleManager);
    }

    [Fact]
    public async Task AddClaimToRole_HicYetkiSecilmezse_PatlamAdan_TumYetkileriKaldirir()
    {
        var rol = new AppRole { Id = "rol-1", Name = "Muhasebe", DisplayName = "Muhasebe" };
        var (controller, roleManager) = CreateSut(rol);

        // Dual-listbox'ta hiçbir öğe seçilmezse model binder Claims = null gönderir.
        var sonuc = await controller.AddClaimToRole(new AddClaimToRoleViewModel { RoleId = "rol-1", Claims = null! });

        sonuc.Should().BeOfType<RedirectToActionResult>();
        roleManager.Verify(x => x.RemoveClaimAsync(rol, It.IsAny<Claim>()), Times.Once);
        roleManager.Verify(x => x.AddClaimAsync(rol, It.IsAny<Claim>()), Times.Never);
    }

    [Fact]
    public async Task AddClaimToRole_SecilenYetkileri_PermissionTipiyle_Ekler()
    {
        var rol = new AppRole { Id = "rol-1", Name = "Muhasebe", DisplayName = "Muhasebe" };
        var (controller, roleManager) = CreateSut(rol);

        await controller.AddClaimToRole(new AddClaimToRoleViewModel
        {
            RoleId = "rol-1",
            Claims = ["ModuleManagement.List", "ModuleManagement.Create"]
        });

        roleManager.Verify(x => x.AddClaimAsync(rol,
            It.Is<Claim>(c => c.Type == "Permission" && c.Value == "ModuleManagement.List")), Times.Once);
        roleManager.Verify(x => x.AddClaimAsync(rol,
            It.Is<Claim>(c => c.Type == "Permission" && c.Value == "ModuleManagement.Create")), Times.Once);
    }

    [Fact]
    public async Task AddClaimToRole_RolBulunamazsa_ErrorView_Doner()
    {
        var rol = new AppRole { Id = "rol-1", Name = "Muhasebe", DisplayName = "Muhasebe" };
        var (controller, roleManager) = CreateSut(rol);
        roleManager.Setup(x => x.FindByIdAsync("yok")).ReturnsAsync((AppRole?)null);

        var sonuc = await controller.AddClaimToRole(new AddClaimToRoleViewModel { RoleId = "yok", Claims = [] });

        sonuc.Should().BeOfType<ViewResult>()
            .Which.ViewName.Should().Be("Error");
    }
}
