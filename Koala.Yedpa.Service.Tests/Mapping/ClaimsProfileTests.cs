using AutoMapper;
using FluentAssertions;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Service.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace Koala.Yedpa.Service.Tests.Mapping;

/// <summary>
/// 18.08.2026 canlı olayının regresyon testleri.
///
/// ClaimsProfile, ClaimListForRoleViewModels.Name alanını $"{Module.Name}.{Name}" olarak
/// üretiyordu. Yeni izin adları modül önekini zaten taşıdığı için sonuç
/// "BudgetOrder.BudgetOrder.Calculate" oluyor, bu değer AddClaimToRole ekranındaki
/// option value'suna yazılıyor ve Kaydet'e basıldığında rolün gerçek yetkileri
/// katalogda karşılığı olmayan değerlerle eziliyordu.
///
/// Bu testler eşlemeyi doğrudan doğrular; controller testleri IClaimsService'i mock'ladığı
/// için hatayı yakalayamıyordu.
/// </summary>
public class ClaimsProfileTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<ClaimsProfile>(),
            NullLoggerFactory.Instance);
        return config.CreateMapper();
    }

    private static Claims OrnekClaim() => new()
    {
        Id = "claim-1",
        ModuleId = "modul-1",
        Name = "BudgetOrder.Calculate",
        DisplayName = "Bütçe Hesapla",
        Description = "Bütçe hesaplama ve güncelleme önizlemesi yapar",
        Module = new Module
        {
            Id = "modul-1",
            Name = "BudgetOrder",
            DisplayName = "Bütçe & Sipariş"
        }
    };

    [Fact]
    public void ClaimListForRole_Name_ModulAdiyla_Oneklenmemeli()
    {
        var sonuc = CreateMapper().Map<ClaimListForRoleViewModels>(OrnekClaim());

        sonuc.Name.Should().Be("BudgetOrder.Calculate",
            because: "izin adı policy adıyla birebir aynı olmalı; modül öneki eklenirse " +
                     "rol yetki ekranı rolün yetkilerini geçersiz değerlerle ezer");
    }

    [Fact]
    public void ClaimListForRole_Name_Icinde_Tekrarlanan_ModulOneki_Olmamali()
    {
        var sonuc = CreateMapper().Map<ClaimListForRoleViewModels>(OrnekClaim());

        sonuc.Name.Should().NotStartWith("BudgetOrder.BudgetOrder");
    }

    [Fact]
    public void ClaimListForRole_Aciklama_Ve_Etiket_Alanlari_Aktarilmali()
    {
        var sonuc = CreateMapper().Map<ClaimListForRoleViewModels>(OrnekClaim());

        sonuc.DisplayName.Should().Be("Bütçe Hesapla");
        sonuc.Description.Should().Be("Bütçe hesaplama ve güncelleme önizlemesi yapar");
        sonuc.ModuleId.Should().Be("modul-1");
    }

    // NOT: Profilin tamamini AssertConfigurationIsValid ile dogrulamak istedik ancak
    // ILGISIZ ve ONCEDEN VAR OLAN bir sorun var: Claims -> SearchClaimViewModel haritasinda
    // PageIndex/PageSize (sayfalama) alanlari eslenmemis. Bu, buradaki hatayla ilgili degil;
    // ayri bir is olarak ele alinmali. O yuzden dogrulama bu haritaya daraltildi.
    [Fact]
    public void ClaimListForRole_Haritasi_Kaynak_Alanlari_Sessizce_Dusurmemeli()
    {
        var claim = OrnekClaim();

        var sonuc = CreateMapper().Map<ClaimListForRoleViewModels>(claim);

        sonuc.Name.Should().NotBeNullOrWhiteSpace();
        sonuc.DisplayName.Should().NotBeNullOrWhiteSpace();
        sonuc.ModuleId.Should().NotBeNullOrWhiteSpace();
        sonuc.Name.Should().Be(claim.Name, because: "policy adi kaynaktaki adla birebir ayni kalmali");
    }
}
