using FluentAssertions;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.WebUI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Koala.Yedpa.WebUI.Tests.Controllers;

/// <summary>
/// 18.08.2026 canlı olayının regresyon testleri: POST Login, PasswordSignInAsync
/// sonucundaki res.Succeeded kontrolünü ATLAMAMALI. Bu kontrol eskiden yoktu;
/// şifre yanlış olduğunda IsLockedOut/IsNotAllowed/RequiresTwoFactor dallarının
/// hiçbirine girmeden Redirect(returnUrl) satırına düşüyordu (bkz. UserController.cs
/// satır ~116-128 açıklaması). 1. test bu davranışı bekçiler; kırmızı gelirse
/// düzeltme geri alınmış demektir.
/// </summary>
public class UserControllerLoginTests
{
    private static AppUser AktifKullanici() => new()
    {
        Id = "user-1",
        UserName = "kullanici1",
        Email = "kullanici1@koala.test",
        Status = StatusEnum.Active
    };

    private static (UserController Controller, Mock<UserManager<AppUser>> UserManager, Mock<SignInManager<AppUser>> SignInManager, Mock<IUrlHelper> UrlHelper)
        CreateSut(AppUser? user)
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);

        var signInManager = IdentityMocks.CreateSignInManagerMock(userManager.Object);

        var urlHelper = new Mock<IUrlHelper>();
        // Varsayılan: her adres "yerel" kabul edilsin (istisna testinde ayrıca setup edilecek).
        urlHelper.Setup(x => x.IsLocalUrl(It.IsAny<string>())).Returns(true);
        urlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/Dashboard/Index");

        var controller = new UserController(
            NullLogger<UserController>.Instance,
            null!,
            userManager.Object,
            signInManager.Object,
            null!,
            Mock.Of<IHttpContextAccessor>(),
            null!)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>()),
            Url = urlHelper.Object
        };

        return (controller, userManager, signInManager, urlHelper);
    }

    [Fact]
    public async Task Login_SifreYanlisIse_ViewDoner_YonlendirmeYapmaz()
    {
        var user = AktifKullanici();
        var (controller, _, signInManager, _) = CreateSut(user);
        signInManager
            .Setup(x => x.PasswordSignInAsync(user, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);

        var sonuc = await controller.Login(
            new LoginViewModel { Email = user.UserName, Password = "yanlis-sifre" },
            new ForgetPasswordViewModel());

        sonuc.Should().BeOfType<ViewResult>();
        controller.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Login_HesapKilitliyse_KilitMesajiIleViewDoner()
    {
        var user = AktifKullanici();
        var (controller, _, signInManager, _) = CreateSut(user);
        signInManager
            .Setup(x => x.PasswordSignInAsync(user, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.LockedOut);

        var sonuc = await controller.Login(
            new LoginViewModel { Email = user.UserName, Password = "herhangi" },
            new ForgetPasswordViewModel());

        sonuc.Should().BeOfType<ViewResult>();
        controller.ModelState.IsValid.Should().BeFalse();
        controller.ModelState[string.Empty]!.Errors
            .Should().Contain(e => e.ErrorMessage.Contains("Kilitli"));
    }

    [Fact]
    public async Task Login_BasariliGirisde_Yonlendirir()
    {
        var user = AktifKullanici();
        var (controller, _, signInManager, _) = CreateSut(user);
        signInManager
            .Setup(x => x.PasswordSignInAsync(user, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        var sonuc = await controller.Login(
            new LoginViewModel { Email = user.UserName, Password = "dogru-sifre" },
            new ForgetPasswordViewModel(),
            returnUrl: "/Dashboard/Index");

        sonuc.Should().BeOfType<RedirectResult>();
    }

    [Fact]
    public async Task Login_UygulamaDisiReturnUrl_KabulEdilmez_DashboardaDuser()
    {
        var user = AktifKullanici();
        var (controller, _, signInManager, urlHelper) = CreateSut(user);
        signInManager
            .Setup(x => x.PasswordSignInAsync(user, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        // Open redirect denemesi: uygulama dışı bir adres.
        const string kotuNiyetliAdres = "https://evil.example.com";
        urlHelper.Setup(x => x.IsLocalUrl(kotuNiyetliAdres)).Returns(false);

        var sonuc = await controller.Login(
            new LoginViewModel { Email = user.UserName, Password = "dogru-sifre" },
            new ForgetPasswordViewModel(),
            returnUrl: kotuNiyetliAdres);

        var redirect = sonuc.Should().BeOfType<RedirectResult>().Which;
        redirect.Url.Should().NotBe(kotuNiyetliAdres);
        redirect.Url.Should().Be("/Dashboard/Index");
    }

    [Fact]
    public async Task Login_KullaniciBulunamazsa_ViewDoner()
    {
        var (controller, _, signInManager, _) = CreateSut(null);

        var sonuc = await controller.Login(
            new LoginViewModel { Email = "olmayan@koala.test", Password = "herhangi" },
            new ForgetPasswordViewModel());

        sonuc.Should().BeOfType<ViewResult>();
        controller.ModelState.IsValid.Should().BeFalse();
        signInManager.Verify(
            x => x.PasswordSignInAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_KullaniciPasifIse_ViewDoner_SifreKontroluHicYapilmaz()
    {
        var user = AktifKullanici();
        user.Status = StatusEnum.Passive;
        var (controller, _, signInManager, _) = CreateSut(user);

        var sonuc = await controller.Login(
            new LoginViewModel { Email = user.UserName, Password = "herhangi" },
            new ForgetPasswordViewModel());

        sonuc.Should().BeOfType<ViewResult>();
        controller.ModelState.IsValid.Should().BeFalse();
        signInManager.Verify(
            x => x.PasswordSignInAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }
}
