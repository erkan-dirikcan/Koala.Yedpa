using System.Reflection;
using FluentAssertions;
using Koala.Yedpa.WebUI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koala.Yedpa.WebUI.Tests.Authorization;

/// <summary>
/// Reflection tabanlı regresyon testleri: WebUI'daki her controller action'ının bir izne
/// (veya bilinçli olarak beyaz listelenmiş bir anonim erişime) bağlı olduğunu garanti eder.
/// Amaç: biri yeni bir controller/action ekleyip izin koymayı unutursa bu testin KIRMIZI olması.
/// </summary>
public class PermissionCoverageTests
{
    /// <summary>[AllowAnonymous] taşımasına bilinçli olarak izin verilen action'ların TAM listesi.
    /// Buraya yeni bir satır eklemek demek, o uca kimlik doğrulamasız erişim açmak demektir —
    /// bu yüzden liste büyürse bunun bilinçli bir karar olduğundan emin ol.</summary>
    private static readonly string[] AnonimErisimBeyazListesi =
    [
        "UserController.Login",
        "UserController.ForgetPassword",
        "UserController.ResetPassword",
        "UserController.ForgetConfirm",
        "UserController.AccessDenied",
        "DashboardController.Error"
    ];

    /// <summary>
    /// İzne DEĞİL, yalnızca kimlik doğrulamasına bağlı olması BİLİNÇLİ olan action'lar.
    /// Bunlar kullanıcının kendi hesabı üzerindeki self-servis işlemleridir; global
    /// FallbackPolicy sayesinde anonim erişime kapalıdırlar ama ayrı bir izin istemezler.
    ///
    /// Neden izne bağlanmadılar: bu uçlara izin şartı koymak, izni verilmemiş bir kullanıcının
    /// oturumunu kapatamaması veya kendi şifresini değiştirememesi anlamına gelir — yani
    /// güvenliği artırmaz, kullanıcıyı oturumda kilitler.
    ///
    /// Buraya yeni satır eklemek, o ucu "giriş yapan HERKES erişebilir" yapmak demektir.
    /// </summary>
    private static readonly string[] SadeceKimlikDogrulamasiYeterliBeyazListe =
    [
        "UserController.Logout",
        "UserController.ChangePassword",
        "UserController.UserProfile"
    ];

    private static List<Type> ControllerTiplerini()
    {
        return typeof(PermissionCatalog).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .ToList();
    }

    /// <summary>Sadece o controller tipinde TANIMLANMIŞ (Controller/ControllerBase'den miras
    /// gelmeyen — örn. View, Json, RedirectToAction) public, instance, non-special-name,
    /// [NonAction] olmayan metotları döner; yani gerçek action'ları.</summary>
    private static List<MethodInfo> ActionMetotlariniGetir(Type controllerType)
    {
        return controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.DeclaringType == controllerType)
            .Where(m => !m.IsSpecialName)
            .Where(m => m.GetCustomAttributes(typeof(NonActionAttribute), inherit: true).Length == 0)
            .ToList();
    }

    [Fact]
    public void HerControllerAction_BirIzneVeyaBeyazListelenmisAnonimErisimeBagliOlmali()
    {
        var eksikActionlar = new List<string>();

        foreach (var controllerType in ControllerTiplerini())
        {
            var controllerSinifindaIzinVar = controllerType
                .GetCustomAttributes(typeof(PermissionAttribute), inherit: true)
                .Length > 0;

            foreach (var action in ActionMetotlariniGetir(controllerType))
            {
                var actionAdi = $"{controllerType.Name}.{action.Name}";

                var actionUzerindeIzinVar = action
                    .GetCustomAttributes(typeof(PermissionAttribute), inherit: true)
                    .Length > 0;

                var actionAnonimeIzinliMi = action
                    .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true)
                    .Length > 0 && AnonimErisimBeyazListesi.Contains(actionAdi);

                var selfServisMi = SadeceKimlikDogrulamasiYeterliBeyazListe.Contains(actionAdi);

                var gecerliMi = controllerSinifindaIzinVar || actionUzerindeIzinVar
                                || actionAnonimeIzinliMi || selfServisMi;

                if (!gecerliMi)
                {
                    eksikActionlar.Add(actionAdi);
                }
            }
        }

        eksikActionlar.Should().BeEmpty(
            because: $"aşağıdaki action'lar herhangi bir izne bağlı değil (ne [Permission], ne beyaz listeli " +
                     $"[AllowAnonymous], ne de self-servis istisnası): " +
                     $"{string.Join(", ", eksikActionlar.OrderBy(x => x))}");
    }

    [Fact]
    public void SelfServisBeyazListesindekiActionlar_GercektenVarOlmali()
    {
        // Beyaz listede artık var olmayan bir action kalırsa liste sessizce çürür;
        // silinen bir action'ın istisnası ileride yanlışlıkla başka bir şeyi kapsayabilir.
        var tumActionAdlari = ControllerTiplerini()
            .SelectMany(controllerType => ActionMetotlariniGetir(controllerType)
                .Select(action => $"{controllerType.Name}.{action.Name}"))
            .Distinct()
            .ToList();

        foreach (var beyazListeKaydi in SadeceKimlikDogrulamasiYeterliBeyazListe)
        {
            tumActionAdlari.Should().Contain(beyazListeKaydi,
                because: $"'{beyazListeKaydi}' self-servis beyaz listesinde ama böyle bir action artık yok");
        }
    }

    [Fact]
    public void AnonimErisimliActionlar_BeyazListeyleBirebiriAyniOlmali()
    {
        var anonimActionlar = ControllerTiplerini()
            .SelectMany(controllerType => ActionMetotlariniGetir(controllerType)
                .Where(action => action.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).Length > 0)
                .Select(action => $"{controllerType.Name}.{action.Name}"))
            .Distinct()
            .ToList();

        anonimActionlar.Should().BeEquivalentTo(AnonimErisimBeyazListesi,
            because: "[AllowAnonymous] kullanan her action bilinçli bir karar olmalı; " +
                     "beyaz listede olmayan bir anonim erişim veya beyaz listede olup artık kullanılmayan bir kayıt bu testi kırmalı");
    }

    /// <summary>Policy adı '|' ile ayrılmış birden fazla izin içerebilir (VEYA mantığı);
    /// katalog kontrolü tek tek izin adları üzerinden yapılmalı.</summary>
    private static IEnumerable<string> IzinAdlariniAyikla(PermissionAttribute attribute)
    {
        return attribute.Policy!.Split(
            PermissionAttribute.Separator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    [Fact]
    public void KullanilanIzinAdlari_KatalogdaTanimliOlmali()
    {
        var kullanilanIzinAdlari = new List<string>();

        foreach (var controllerType in ControllerTiplerini())
        {
            var sinifIzinleri = controllerType
                .GetCustomAttributes(typeof(PermissionAttribute), inherit: true)
                .Cast<PermissionAttribute>()
                .SelectMany(IzinAdlariniAyikla);
            kullanilanIzinAdlari.AddRange(sinifIzinleri);

            foreach (var action in ActionMetotlariniGetir(controllerType))
            {
                var metotIzinleri = action
                    .GetCustomAttributes(typeof(PermissionAttribute), inherit: true)
                    .Cast<PermissionAttribute>()
                    .SelectMany(IzinAdlariniAyikla);
                kullanilanIzinAdlari.AddRange(metotIzinleri);
            }
        }

        var katalogdaOlmayanIzinler = kullanilanIzinAdlari
            .Distinct()
            .Where(izin => !PermissionCatalog.AllPermissionNames.Contains(izin))
            .OrderBy(x => x)
            .ToList();

        katalogdaOlmayanIzinler.Should().BeEmpty(
            because: $"controller'larda kullanılan şu izin adları PermissionCatalog.AllPermissionNames içinde tanımlı değil " +
                     $"(yazım hatası olabilir — fail-closed davranışı yüzünden sessizce herkesi reddeder): " +
                     $"{string.Join(", ", katalogdaOlmayanIzinler)}");
    }
}
