# WebUI Modül → Claim → Rol → Kullanıcı Yetki Yönetimi Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** WebUI'da "Modül tanımla → Claim tanımla → Rol tanımla → Role claim ata → Rolü kullanıcıya ata → `.cs`/`.cshtml` içinde yetki kontrolü yap" akışını uçtan uca çalışır hale getirmek.

**Architecture:** Mevcut `Module (1) → (N) Claims` yapısı korunur. `Claims.Name` doğrudan authorization policy adı olarak kullanılır. Kodda kullanılan izinlerin tek doğruluk kaynağı `PermissionCatalog` sabit sınıfıdır; bir seeder bu katalogu her açılışta idempotent şekilde DB'ye yazar, böylece `AddClaimToRole` ekranında kodun beklediği izinler otomatik görünür. `PermissionPolicyProvider` istenen policy adını `RequireClaim("Permission", <ad>)` politikasına çevirir — DB'ye gitmez, cache gerektirmez. Global `FallbackPolicy` ile tüm endpoint'ler varsayılan olarak kilitlidir.

**Tech Stack:** ASP.NET Core 10.0 MVC, ASP.NET Core Identity (`AddIdentity<AppUser, AppRole>`, cookie auth), EF Core 10 (Code-First, MSSQL), xUnit + FluentAssertions + Moq, Metronic 7 / Bootstrap 4.6.

## Global Constraints

- Hedef framework: `net10.0`. Yeni test projesi de `net10.0` olmalı.
- View/JS yazarken **Bootstrap 4.6 (Metronic 7)** sözdizimi zorunlu: `data-dismiss`, `data-toggle`, `text-right`, `mr-2`, `font-weight-bold`, `label label-light-success label-inline`. BS5/Metronic 8 sınıfları (`data-bs-*`, `btn-close`, `text-end`, `me-2`, `fw-bold`, `badge badge-*`) **kullanılmayacak** — hata vermez, sessizce bozuk görünür.
- Rol claim'i tipi sabittir: `ClaimType = "Permission"`, `ClaimValue = Claims.Name`. Mevcut `AppRoleController.AddClaimToRole` bu formatta yazıyor, değiştirilmeyecek.
- Policy adı = `Claims.Name` = `PermissionCatalog` sabitinin değeri. Üçü birebir aynı string olmak zorunda.
- İzin adlandırması: `<ModulAdi>.<Eylem>` (örn. `ModuleManagement.Create`). Nokta ayraç, PascalCase, Türkçe karakter yok.
- Test framework'ü xUnit; assertion'lar FluentAssertions (`.Should()`); mock'lar Moq. Mevcut `Koala.Yedpa.Service.Tests` stilini takip et.
- Migration komutları `Koala.Yedpa.Repositories` project + `Koala.Yedpa.WebUI` startup project ile çalıştırılır.
- Her task sonunda `dotnet build` 0 error olmalı.

## ⚠️ Kilitlenme Riski (Task 3 öncesi MUTLAKA oku)

Task 3 global `FallbackPolicy` ekliyor, Task 4 controller'lara `[Permission]` koyuyor. Eğer o an giriş yapan kullanıcının rolünde ilgili claim'ler yoksa, **yetki yönetimi ekranlarına da giremezsin** ve kendini sistemden kilitlersin.

Korunma: Task 2'nin seeder'ı "Süper Yönetici" rolüne katalogdaki **tüm** claim'leri veriyor. Task 3'e geçmeden önce Task 2 Step 9'daki doğrulamayı yap: kendi kullanıcının bu role atanmış olduğunu SQL ile teyit et.

Ayrıca `SecurityStampValidatorOptions.ValidationInterval = 120 saniye` ([StartupExtention.cs:70](../../../Koala.Yedpa.WebUI/Extentions/StartupExtention.cs)). Rol claim'leri değiştikten sonra cookie en geç 120 sn içinde yenilenir. Test ederken ya 2 dakika bekle ya da çıkış/giriş yap.

---

## File Structure

**Yeni dosyalar:**

| Dosya | Sorumluluk |
|---|---|
| `Koala.Yedpa.WebUI/Authorization/PermissionCatalog.cs` | Kodda kullanılan tüm izinlerin ve ait oldukları modüllerin tek kaynağı |
| `Koala.Yedpa.WebUI/Authorization/PermissionAttribute.cs` | `[Permission("X")]` — `AuthorizeAttribute` sarmalayıcısı |
| `Koala.Yedpa.WebUI/Authorization/PermissionPolicyProvider.cs` | Policy adını `RequireClaim("Permission", ad)` politikasına çevirir |
| `Koala.Yedpa.WebUI/Authorization/PermissionSeeder.cs` | Katalogu DB'ye idempotent yazar + Süper Yönetici rolünü doldurur |
| `Koala.Yedpa.WebUI/Authorization/ClaimsPrincipalExtensions.cs` | `User.HasPermission("X")` — view'lar için |
| `Koala.Yedpa.WebUI.Tests/` | Yeni xUnit test projesi |

**Değişecek dosyalar:**

| Dosya | Değişiklik |
|---|---|
| `Koala.Yedpa.WebUI/Program.cs` | FallbackPolicy, PermissionPolicyProvider ve PermissionSeeder kaydı; ölü `AuthorizationRulesInitializer` kaldırma |
| `Koala.Yedpa.WebUI/Extentions/StartupExtention.cs` | Ölü `AddAuthorizationRules` / `DynamicAuthorizationPolicyProvider` / `AuthorizationRulesInitializer` silme |
| `Koala.Yedpa.WebUI/Controllers/{Module,Claims,AppRole,User}Controller.cs` | `[Permission]` uygulama + mevcut bug'lar |
| `Koala.Yedpa.WebUI/Controllers/{Dashboard,BulkInvoice,BudgetOrder,Settings}Controller.cs` | `[Permission]` uygulama |
| `Koala.Yedpa.WebUI/Views/Shared/_MainManuPartial.cshtml` | Menü öğelerini izne göre gizleme |
| `Koala.Yedpa.WebUI/Views/_ViewImports.cshtml` | `HasPermission` extension için `@using` |
| `Koala.Yedpa.Core/Models/ViewModels/AppRoleViewModel.cs` | `AsignRoleToUserViewModel.DisplayName` eklenmesi |
| `Koala.Yedpa.Repositories/Configurations/ClaimsConfiguration.cs` | `Claims.Name` unique index |

---

### Task 1: PermissionCatalog + PermissionAttribute + PermissionPolicyProvider

Yetki altyapısının çekirdeği. Bu task'tan sonra `[Permission("X")]` yazılabilir hale gelir ama henüz hiçbir controller'a uygulanmaz.

**Files:**
- Create: `Koala.Yedpa.WebUI/Authorization/PermissionCatalog.cs`
- Create: `Koala.Yedpa.WebUI/Authorization/PermissionAttribute.cs`
- Create: `Koala.Yedpa.WebUI/Authorization/PermissionPolicyProvider.cs`
- Create: `Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj`
- Test: `Koala.Yedpa.WebUI.Tests/Authorization/PermissionPolicyProviderTests.cs`
- Test: `Koala.Yedpa.WebUI.Tests/Authorization/PermissionCatalogTests.cs`
- Modify: `Koala.Yedpa.sln`

**Interfaces:**
- Produces:
  - `PermissionCatalog.Modules` → `IReadOnlyList<PermissionModule>`
  - `PermissionModule(string Name, string DisplayName, string Description, IReadOnlyList<PermissionDefinition> Permissions)`
  - `PermissionDefinition(string Name, string DisplayName, string Description)`
  - `PermissionCatalog.AllPermissionNames` → `IReadOnlyList<string>`
  - `PermissionCatalog.<Grup>.<Sabit>` string sabitleri (örn. `PermissionCatalog.ModuleManagement.Create`)
  - `PermissionAttribute(string permission) : AuthorizeAttribute`
  - `PermissionPolicyProvider : DefaultAuthorizationPolicyProvider`
  - `PermissionPolicyProvider.PermissionClaimType` → `"Permission"` sabiti

- [ ] **Step 1: Test projesini oluştur ve solution'a ekle**

```bash
dotnet new xunit -n Koala.Yedpa.WebUI.Tests -o Koala.Yedpa.WebUI.Tests --framework net10.0
dotnet sln Koala.Yedpa.sln add Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj
```

- [ ] **Step 2: Test projesinin csproj'unu düzenle**

`Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj` içeriğini tamamen bununla değiştir:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="8.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="8.8.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.0" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Koala.Yedpa.Core\Koala.Yedpa.Core.csproj" />
    <ProjectReference Include="..\Koala.Yedpa.Repositories\Koala.Yedpa.Repositories.csproj" />
    <ProjectReference Include="..\Koala.Yedpa.WebUI\Koala.Yedpa.WebUI.csproj" />
  </ItemGroup>

</Project>
```

`dotnet new xunit` ile gelen `UnitTest1.cs` dosyasını sil.

- [ ] **Step 3: Failing test yaz — PermissionPolicyProvider**

`Koala.Yedpa.WebUI.Tests/Authorization/PermissionPolicyProviderTests.cs`:

```csharp
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
```

- [ ] **Step 4: Testi çalıştır, derlenmediğini gör**

```bash
dotnet test Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj
```

Beklenen: derleme hatası — `PermissionPolicyProvider` tipi bulunamıyor (CS0246).

- [ ] **Step 5: PermissionPolicyProvider'ı yaz**

`Koala.Yedpa.WebUI/Authorization/PermissionPolicyProvider.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Koala.Yedpa.WebUI.Authorization;

/// <summary>
/// Policy adını doğrudan bir "Permission" claim şartına çevirir.
/// Böylece her izin için elle policy tanımlamaya gerek kalmaz:
/// [Permission("ModuleManagement.Create")] → RequireClaim("Permission", "ModuleManagement.Create")
///
/// Veritabanına gitmez. Claim'in DB'de tanımlı olup olmadığını kontrol etmek
/// güvenlik açısından gereksizdir: kullanıcıda o claim yoksa zaten reddedilir.
/// DB'de olmayan bir izin adı yazılırsa sonuç "herkes reddedilir" olur (fail-closed).
/// </summary>
public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    /// <summary>Rol/kullanıcı claim'lerinde kullanılan sabit claim tipi.</summary>
    public const string PermissionClaimType = "Permission";

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName))
        {
            return null;
        }

        // Elle AddPolicy ile tanımlanmış bir policy varsa o kazanır.
        var existing = await base.GetPolicyAsync(policyName);
        if (existing is not null)
        {
            return existing;
        }

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(PermissionClaimType, policyName)
            .Build();
    }
}
```

- [ ] **Step 6: Testleri çalıştır, geçtiğini gör**

```bash
dotnet test Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj --filter "FullyQualifiedName~PermissionPolicyProviderTests"
```

Beklenen: 6 test PASS (Theory 3 case dahil).

- [ ] **Step 7: Failing test yaz — PermissionCatalog**

`Koala.Yedpa.WebUI.Tests/Authorization/PermissionCatalogTests.cs`:

```csharp
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
```

- [ ] **Step 8: Testi çalıştır, derlenmediğini gör**

```bash
dotnet test Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj --filter "FullyQualifiedName~PermissionCatalogTests"
```

Beklenen: derleme hatası — `PermissionCatalog` bulunamıyor (CS0103/CS0246).

- [ ] **Step 9: PermissionCatalog'u yaz**

`Koala.Yedpa.WebUI/Authorization/PermissionCatalog.cs`:

```csharp
namespace Koala.Yedpa.WebUI.Authorization;

/// <summary>Katalogdaki tek bir izin tanımı. Name → Claims.Name → policy adı.</summary>
public sealed record PermissionDefinition(string Name, string DisplayName, string Description);

/// <summary>Katalogdaki bir modül ve altındaki izinler.</summary>
public sealed record PermissionModule(
    string Name,
    string DisplayName,
    string Description,
    IReadOnlyList<PermissionDefinition> Permissions);

/// <summary>
/// Kodda kullanılan tüm izinlerin TEK doğruluk kaynağı.
/// Buraya eklenen her izin, PermissionSeeder tarafından uygulama açılışında
/// Module + Claims tablolarına idempotent şekilde yazılır ve
/// AppRole/AddClaimToRole ekranında otomatik görünür.
///
/// Yeni izin eklerken:
/// 1. İlgili gruba bir sabit ekle
/// 2. Modules listesindeki ilgili modülün Permissions dizisine PermissionDefinition ekle
/// 3. Controller/action üzerine [Permission(...)] koy
/// </summary>
public static class PermissionCatalog
{
    public static class UserManagement
    {
        public const string List = "UserManagement.List";
        public const string Create = "UserManagement.Create";
        public const string Update = "UserManagement.Update";
        public const string ChangeStatus = "UserManagement.ChangeStatus";
        public const string AssignRole = "UserManagement.AssignRole";
    }

    public static class RoleManagement
    {
        public const string List = "RoleManagement.List";
        public const string Create = "RoleManagement.Create";
        public const string Update = "RoleManagement.Update";
        public const string Delete = "RoleManagement.Delete";
        public const string AssignClaim = "RoleManagement.AssignClaim";
    }

    public static class ModuleManagement
    {
        public const string List = "ModuleManagement.List";
        public const string Create = "ModuleManagement.Create";
        public const string Update = "ModuleManagement.Update";
        public const string ChangeStatus = "ModuleManagement.ChangeStatus";
        public const string ClaimList = "ModuleManagement.ClaimList";
        public const string ClaimCreate = "ModuleManagement.ClaimCreate";
        public const string ClaimUpdate = "ModuleManagement.ClaimUpdate";
        public const string ClaimDelete = "ModuleManagement.ClaimDelete";
    }

    public static class Dashboard
    {
        public const string View = "Dashboard.View";
    }

    public static class BulkInvoice
    {
        public const string View = "BulkInvoice.View";
        public const string Transfer = "BulkInvoice.Transfer";
    }

    public static class BudgetOrder
    {
        public const string View = "BudgetOrder.View";
    }

    public static class Settings
    {
        public const string View = "Settings.View";
        public const string Update = "Settings.Update";
    }

    public static readonly IReadOnlyList<PermissionModule> Modules =
    [
        new PermissionModule("UserManagement", "Kullanıcı Yönetimi",
            "Kullanıcı listeleme, oluşturma, güncelleme ve rol atama işlemleri",
        [
            new(UserManagement.List, "Kullanıcı Listesi", "Kullanıcı listesini görüntüler"),
            new(UserManagement.Create, "Kullanıcı Ekle", "Yeni kullanıcı oluşturur"),
            new(UserManagement.Update, "Kullanıcı Güncelle", "Mevcut kullanıcıyı günceller"),
            new(UserManagement.ChangeStatus, "Kullanıcı Durumu Değiştir", "Kullanıcıyı aktif/pasif yapar"),
            new(UserManagement.AssignRole, "Kullanıcıya Rol Ata", "Kullanıcının rollerini düzenler")
        ]),

        new PermissionModule("RoleManagement", "Rol Yönetimi",
            "Rol tanımlama ve rollere yetki (claim) atama işlemleri",
        [
            new(RoleManagement.List, "Rol Listesi", "Rol listesini görüntüler"),
            new(RoleManagement.Create, "Rol Ekle", "Yeni rol oluşturur"),
            new(RoleManagement.Update, "Rol Güncelle", "Mevcut rolü günceller"),
            new(RoleManagement.Delete, "Rol Sil", "Rolü siler"),
            new(RoleManagement.AssignClaim, "Role Yetki Ata", "Rolün yetkilerini düzenler")
        ]),

        new PermissionModule("ModuleManagement", "Modül Yönetimi",
            "Modül ve modül altındaki yetki (claim) tanımlama işlemleri",
        [
            new(ModuleManagement.List, "Modül Listesi", "Modül listesini görüntüler"),
            new(ModuleManagement.Create, "Modül Ekle", "Yeni modül oluşturur"),
            new(ModuleManagement.Update, "Modül Güncelle", "Mevcut modülü günceller"),
            new(ModuleManagement.ChangeStatus, "Modül Durumu Değiştir", "Modülü aktif/pasif yapar"),
            new(ModuleManagement.ClaimList, "Yetki Listesi", "Modüle ait yetkileri görüntüler"),
            new(ModuleManagement.ClaimCreate, "Yetki Ekle", "Modüle yeni yetki tanımlar"),
            new(ModuleManagement.ClaimUpdate, "Yetki Güncelle", "Mevcut yetkiyi günceller"),
            new(ModuleManagement.ClaimDelete, "Yetki Sil", "Yetkiyi siler")
        ]),

        new PermissionModule("Dashboard", "Kokpit",
            "Ana kokpit ekranı",
        [
            new(Dashboard.View, "Kokpiti Görüntüle", "Ana kokpit ekranını açar")
        ]),

        new PermissionModule("BulkInvoice", "Toplu Faturalandırma",
            "Aidat siparişlerinden toplu fatura oluşturma işlemleri",
        [
            new(BulkInvoice.View, "Toplu Faturalandırmayı Görüntüle", "Toplu faturalandırma ekranını açar"),
            new(BulkInvoice.Transfer, "Fatura Aktarımı Başlat", "Logo'ya fatura aktarımını tetikler")
        ]),

        new PermissionModule("BudgetOrder", "Bütçe Emri",
            "Bütçe emri ekranları",
        [
            new(BudgetOrder.View, "Bütçe Emrini Görüntüle", "Bütçe emri ekranını açar")
        ]),

        new PermissionModule("Settings", "Ayarlar",
            "Logo bağlantı ve sistem ayarları",
        [
            new(Settings.View, "Ayarları Görüntüle", "Ayar ekranlarını açar"),
            new(Settings.Update, "Ayarları Güncelle", "Ayarları kaydeder")
        ])
    ];

    /// <summary>Katalogdaki tüm izin adları (düz liste).</summary>
    public static readonly IReadOnlyList<string> AllPermissionNames =
        Modules.SelectMany(m => m.Permissions).Select(p => p.Name).ToList();
}
```

- [ ] **Step 10: Testleri çalıştır, geçtiğini gör**

```bash
dotnet test Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj
```

Beklenen: tüm testler PASS.

- [ ] **Step 11: PermissionAttribute'u yaz**

`Koala.Yedpa.WebUI/Authorization/PermissionAttribute.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;

namespace Koala.Yedpa.WebUI.Authorization;

/// <summary>
/// [Permission(PermissionCatalog.ModuleManagement.Create)] şeklinde kullanılır.
/// Policy adı doğrudan izin adıdır; PermissionPolicyProvider bunu
/// RequireClaim("Permission", izinAdi) şartına çevirir.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class PermissionAttribute : AuthorizeAttribute
{
    public PermissionAttribute(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        Policy = permission;
    }
}
```

- [ ] **Step 12: Build + test**

```bash
dotnet build Koala.Yedpa.sln
```

Beklenen: 0 error.

```bash
dotnet test Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj
```

Beklenen: tüm testler PASS.

- [ ] **Step 13: Commit**

```bash
git add Koala.Yedpa.WebUI/Authorization Koala.Yedpa.WebUI.Tests Koala.Yedpa.sln
git commit -m "feat(auth): izin katalogu, Permission attribute ve policy provider"
```

---

### Task 2: PermissionSeeder — katalogu DB'ye yaz, Süper Yönetici'yi doldur

Kodun beklediği izinlerin `Module` + `Claims` tablolarında var olmasını garanti eder. Bu olmadan `AddClaimToRole` ekranında kodun kullandığı izinler görünmez.

**Files:**
- Create: `Koala.Yedpa.WebUI/Authorization/PermissionSeeder.cs`
- Modify: `Koala.Yedpa.WebUI/Program.cs`
- Test: `Koala.Yedpa.WebUI.Tests/Authorization/PermissionSeederTests.cs`

**Interfaces:**
- Consumes: `PermissionCatalog.Modules`, `PermissionPolicyProvider.PermissionClaimType` (Task 1)
- Produces:
  - `PermissionSeeder.SeedModulesAndClaimsAsync(AppDbContext context, ILogger logger, CancellationToken ct)` → `Task<int>` (eklenen kayıt sayısı)
  - `PermissionSeeder.GrantAllToSuperAdminAsync(RoleManager<AppRole> roleManager, ILogger logger)` → `Task<int>` (eklenen claim sayısı)
  - `PermissionSeeder.SuperAdminRoleDisplayName` → `"Süper Yönetici"` sabiti

- [ ] **Step 1: Failing test yaz**

`Koala.Yedpa.WebUI.Tests/Authorization/PermissionSeederTests.cs`:

```csharp
using FluentAssertions;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.WebUI.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

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
}
```

- [ ] **Step 2: Testi çalıştır, derlenmediğini gör**

```bash
dotnet test Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj --filter "FullyQualifiedName~PermissionSeederTests"
```

Beklenen: derleme hatası — `PermissionSeeder` bulunamıyor.

- [ ] **Step 3: PermissionSeeder'ı yaz**

`Koala.Yedpa.WebUI/Authorization/PermissionSeeder.cs`:

```csharp
using System.Security.Claims;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.WebUI.Authorization;

/// <summary>
/// PermissionCatalog'u veritabanına yansıtır. Idempotent:
/// var olan modül/claim kayıtlarına dokunmaz, sadece eksikleri ekler.
/// Kullanıcının ekrandan girdiği DisplayName/Description değerleri korunur.
/// </summary>
public static class PermissionSeeder
{
    public const string SuperAdminRoleDisplayName = "Süper Yönetici";

    /// <summary>Eksik modül ve claim kayıtlarını ekler. Eklenen toplam kayıt sayısını döner.</summary>
    public static async Task<int> SeedModulesAndClaimsAsync(
        AppDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var eklenen = 0;

        var mevcutModuller = await context.Module.ToListAsync(cancellationToken);
        var mevcutClaimAdlari = await context.Claims
            .Select(c => c.Name!)
            .ToListAsync(cancellationToken);
        var claimAdSeti = new HashSet<string>(mevcutClaimAdlari, StringComparer.Ordinal);

        foreach (var katalogModul in PermissionCatalog.Modules)
        {
            var modul = mevcutModuller.FirstOrDefault(m =>
                string.Equals(m.Name, katalogModul.Name, StringComparison.Ordinal));

            if (modul is null)
            {
                modul = new Module
                {
                    Id = Tools.CreateGuidStr(),
                    Name = katalogModul.Name,
                    DisplayName = katalogModul.DisplayName,
                    Description = katalogModul.Description,
                    Status = StatusEnum.Active
                };
                context.Module.Add(modul);
                eklenen++;
                logger.LogInformation("PermissionSeeder: '{ModulAdi}' modülü eklendi", katalogModul.Name);
            }

            foreach (var izin in katalogModul.Permissions)
            {
                if (claimAdSeti.Contains(izin.Name))
                {
                    continue;
                }

                context.Claims.Add(new Claims
                {
                    Id = Tools.CreateGuidStr(),
                    ModuleId = modul.Id,
                    Name = izin.Name,
                    DisplayName = izin.DisplayName,
                    Description = izin.Description
                });
                claimAdSeti.Add(izin.Name);
                eklenen++;
                logger.LogInformation("PermissionSeeder: '{IzinAdi}' yetkisi eklendi", izin.Name);
            }
        }

        if (eklenen > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return eklenen;
    }

    /// <summary>
    /// "Süper Yönetici" rolüne katalogdaki tüm izinleri verir. Rol yoksa uyarı loglar ve 0 döner.
    /// Yönetici rolünün yeni eklenen izinleri otomatik alması içindir.
    /// </summary>
    public static async Task<int> GrantAllToSuperAdminAsync(
        RoleManager<AppRole> roleManager,
        ILogger logger)
    {
        var rol = await roleManager.Roles.FirstOrDefaultAsync(r =>
            r.DisplayName == SuperAdminRoleDisplayName || r.Name == SuperAdminRoleDisplayName);

        if (rol is null)
        {
            logger.LogWarning(
                "PermissionSeeder: '{RolAdi}' rolü bulunamadı, otomatik yetkilendirme atlandı",
                SuperAdminRoleDisplayName);
            return 0;
        }

        var mevcutClaimler = await roleManager.GetClaimsAsync(rol);
        var mevcutIzinler = new HashSet<string>(
            mevcutClaimler
                .Where(c => c.Type == PermissionPolicyProvider.PermissionClaimType)
                .Select(c => c.Value),
            StringComparer.Ordinal);

        var eklenen = 0;
        foreach (var izinAdi in PermissionCatalog.AllPermissionNames)
        {
            if (mevcutIzinler.Contains(izinAdi))
            {
                continue;
            }

            var sonuc = await roleManager.AddClaimAsync(
                rol, new Claim(PermissionPolicyProvider.PermissionClaimType, izinAdi));

            if (sonuc.Succeeded)
            {
                eklenen++;
            }
            else
            {
                logger.LogError("PermissionSeeder: '{IzinAdi}' Süper Yönetici rolüne eklenemedi: {Hatalar}",
                    izinAdi, string.Join(", ", sonuc.Errors.Select(e => e.Description)));
            }
        }

        if (eklenen > 0)
        {
            logger.LogInformation(
                "PermissionSeeder: Süper Yönetici rolüne {Sayi} yeni yetki eklendi", eklenen);
        }

        return eklenen;
    }
}
```

- [ ] **Step 4: Testleri çalıştır, geçtiğini gör**

```bash
dotnet test Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj --filter "FullyQualifiedName~PermissionSeederTests"
```

Beklenen: 4 test PASS.

- [ ] **Step 5: Seeder'ı Program.cs'e bağla**

[Program.cs:198](../../../Koala.Yedpa.WebUI/Program.cs) — "TRANSACTION TYPE ID GÜNCELLEME" bloğunun hemen ardına, `if (!app.Environment.IsDevelopment())` satırından önce ekle:

```csharp
            // YETKİ KATALOĞU SEED (idempotent)
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    var eklenenKayit = await PermissionSeeder.SeedModulesAndClaimsAsync(
                        context, logger, CancellationToken.None);
                    var eklenenYetki = await PermissionSeeder.GrantAllToSuperAdminAsync(roleManager, logger);

                    logger.LogInformation(
                        "Yetki kataloğu senkronize edildi. Yeni kayıt: {Kayit}, Süper Yönetici'ye eklenen yetki: {Yetki}",
                        eklenenKayit, eklenenYetki);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Yetki kataloğu seed edilirken hata oluştu");
                }
            }
```

Aynı dosyanın using bloğuna ekle:

```csharp
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.WebUI.Authorization;
using Microsoft.AspNetCore.Identity;
```

- [ ] **Step 6: Build**

```bash
dotnet build Koala.Yedpa.sln
```

Beklenen: 0 error.

- [ ] **Step 7: Uygulamayı çalıştır ve seed'i doğrula**

```bash
dotnet run --project Koala.Yedpa.WebUI
```

Log'da `Yetki kataloğu senkronize edildi. Yeni kayıt: 37, ...` benzeri satırı gör (7 modül + 30 izin = 37 ilk turda). Uygulamayı durdur.

- [ ] **Step 8: DB'de doğrula**

MSSQL'de çalıştır:

```sql
SELECT m.Name AS Modul, c.Name AS Yetki, c.DisplayName
FROM Claims c JOIN Module m ON m.Id = c.ModuleId
ORDER BY m.Name, c.Name;
```

Beklenen: `PermissionCatalog.AllPermissionNames` ile birebir aynı 30 satır.

- [ ] **Step 9: ⚠️ KİLİTLENME KONTROLÜ — kendi kullanıcını doğrula**

```sql
SELECT u.UserName, r.Name AS RolAdi, r.DisplayName,
       (SELECT COUNT(*) FROM AspNetRoleClaims rc
        WHERE rc.RoleId = r.Id AND rc.ClaimType = 'Permission') AS YetkiSayisi
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE u.UserName = 'erkan.dirikcan@gmail.com';
```

Beklenen: en az bir satır, `YetkiSayisi = 30`.

**Sonuç 0 satır ise DUR.** Task 3'e geçme. Önce `/User/AsignRoleToUser` ekranından (henüz kilitli değil) kendini "Süper Yönetici" rolüne ata, sonra bu sorguyu tekrar çalıştır.

**"Süper Yönetici" rolü hiç yoksa:** `/AppRole/CreateRole` ekranından `Name` ve `DisplayName` alanlarının **ikisine de** `Süper Yönetici` yaz, oluştur, uygulamayı yeniden başlat (seeder rolü doldurur), sonra kendini bu role ata.

- [ ] **Step 10: Commit**

```bash
git add Koala.Yedpa.WebUI/Authorization/PermissionSeeder.cs Koala.Yedpa.WebUI/Program.cs Koala.Yedpa.WebUI.Tests
git commit -m "feat(auth): izin katalogunu DB'ye yansitan idempotent seeder"
```

---

### Task 3: Mevcut bug'lar — rol atama ve claim atama

Kullanıcının akışındaki 4. ve 5. adımları güvenilir hale getirir. Task 4'ün kilitlemesinden önce yapılmalı ki hata ayıklama kolay olsun.

**Files:**
- Modify: `Koala.Yedpa.Core/Models/ViewModels/AppRoleViewModel.cs:31-40`
- Modify: `Koala.Yedpa.WebUI/Controllers/UserController.cs:374-441`
- Modify: `Koala.Yedpa.WebUI/Controllers/AppRoleController.cs:119-188`
- Modify: `Koala.Yedpa.WebUI/Views/User/AsignRoleToUser.cshtml:48,62`
- Test: `Koala.Yedpa.WebUI.Tests/Controllers/AppRoleControllerTests.cs`
- Test: `Koala.Yedpa.WebUI.Tests/IdentityMocks.cs`

**Interfaces:**
- Consumes: `PermissionPolicyProvider.PermissionClaimType` (Task 1)
- Produces: `IdentityMocks.CreateRoleManagerMock()` → `Mock<RoleManager<AppRole>>`, test yardımcısı

- [ ] **Step 1: Identity mock yardımcısını yaz**

`Koala.Yedpa.WebUI.Tests/IdentityMocks.cs`:

```csharp
using Koala.Yedpa.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Koala.Yedpa.WebUI.Tests;

/// <summary>
/// RoleManager/UserManager sanal (virtual) metotlara sahiptir; Moq ile
/// doğrudan mock'lanabilir ancak constructor'ları bağımlılık ister.
/// Bu yardımcı o bağımlılıkları boş mock'larla doldurur.
/// </summary>
public static class IdentityMocks
{
    public static Mock<RoleManager<AppRole>> CreateRoleManagerMock()
    {
        var store = new Mock<IRoleStore<AppRole>>();
        return new Mock<RoleManager<AppRole>>(
            store.Object,
            Array.Empty<IRoleValidator<AppRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<AppRole>>>())
        {
            CallBase = false
        };
    }
}
```

- [ ] **Step 2: Failing test yaz — AddClaimToRole**

`Koala.Yedpa.WebUI.Tests/Controllers/AppRoleControllerTests.cs`:

```csharp
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
```

- [ ] **Step 3: Testleri çalıştır, kırmızı olduğunu gör**

```bash
dotnet test Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj --filter "FullyQualifiedName~AppRoleControllerTests"
```

Beklenen: `AddClaimToRole_HicYetkiSecilmezse...` NullReferenceException, `AddClaimToRole_RolBulunamazsa...` NullReferenceException ile FAIL.

- [ ] **Step 4: AddClaimToRole POST'unu düzelt**

[AppRoleController.cs:149-188](../../../Koala.Yedpa.WebUI/Controllers/AppRoleController.cs) — `[HttpPost] AddClaimToRole` metodunun tamamını bununla değiştir:

```csharp
        [HttpPost]
        public async Task<IActionResult> AddClaimToRole(AddClaimToRoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
            {
                TempData["Error"] = ResponseDto.Fail(404, "Rol Bulunamadı", "Yetkilendirilecek rol bulunamadı", true);
                return View("Error");
            }

            var secilenler = model.Claims ?? new List<string>();

            if (!ModelState.IsValid)
            {
                await FillClaimSelectListAsync(role);
                ViewData["RoleInfo"] = role.Name;
                return View(model);
            }

            var currentClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in currentClaims.Where(c => c.Type == PermissionPolicyProvider.PermissionClaimType))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            foreach (var item in secilenler)
            {
                await _roleManager.AddClaimAsync(
                    role, new Claim(PermissionPolicyProvider.PermissionClaimType, item));
            }

            TempData["InfoMessage"] = $"'{role.DisplayName ?? role.Name}' rolünün yetkileri güncellendi.";
            return RedirectToAction("Index");
        }
```

- [ ] **Step 5: Ortak select-list doldurmayı tek metoda al**

[AppRoleController.cs:118-148](../../../Koala.Yedpa.WebUI/Controllers/AppRoleController.cs) — `[HttpGet] AddClaimToRole` metodunun tamamını bununla değiştir ve altına yeni private metodu ekle:

```csharp
        [HttpGet]
        public async Task<IActionResult> AddClaimToRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = ResponseDto.Fail(404, "Role Id Bilgisi Alınamadı", "Role Id Bilgisi Alınamadı", true);
                return View("Error");
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["Error"] = ResponseDto.Fail(404, "Rol Bulunamadı", "Yetkilendirilecek rol bulunamadı", true);
                return View("Error");
            }

            await FillClaimSelectListAsync(role);
            ViewData["RoleInfo"] = role.DisplayName ?? role.Name;

            return View(new AddClaimToRoleViewModel { RoleId = id, Claims = new List<string>() });
        }

        /// <summary>
        /// TempData["Claims"]'i "Modül - Yetki" etiketli, rolde seçili olanları işaretlenmiş
        /// select listesiyle doldurur. GET ve POST-invalid yollarının ikisi de bunu kullanır
        /// ki ekran her iki durumda da aynı görünsün.
        /// </summary>
        private async Task FillClaimSelectListAsync(AppRole role)
        {
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var claims = await _claimService.GetClaimToRoleList();
            var modules = await _moduleService.GetAllModuleAsync();

            var claimData = new List<SelectListDto<string>>();
            foreach (var claim in claims.Data ?? Enumerable.Empty<ClaimListForRoleViewModels>())
            {
                var module = modules.Data?.FirstOrDefault(x => x.Id == claim.ModuleId);
                var moduleLabel = module?.DisplayName ?? module?.Name ?? "Tanımsız Modül";

                claimData.Add(new SelectListDto<string>
                {
                    IsSelected = roleClaims.Any(x =>
                        x.Type == PermissionPolicyProvider.PermissionClaimType && x.Value == claim.Name),
                    Key = $"{moduleLabel} - {claim.DisplayName}",
                    Val = claim.Name
                });
            }

            TempData["Claims"] = claimData.OrderBy(x => x.Key).ToList();
        }
```

Dosyanın using bloğuna ekle:

```csharp
using Koala.Yedpa.WebUI.Authorization;
```

- [ ] **Step 6: Testleri çalıştır, yeşil olduğunu gör**

```bash
dotnet test Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj --filter "FullyQualifiedName~AppRoleControllerTests"
```

Beklenen: 3 test PASS.

- [ ] **Step 7: AsignRoleToUserViewModel'e DisplayName ekle**

[AppRoleViewModel.cs:31-40](../../../Koala.Yedpa.Core/Models/ViewModels/AppRoleViewModel.cs) — `AsignRoleToUserViewModel` sınıfını bununla değiştir:

```csharp
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
```

- [ ] **Step 8: UserController.AsignRoleToUser GET'ini düzelt**

[UserController.cs:393-408](../../../Koala.Yedpa.WebUI/Controllers/UserController.cs) — `foreach (var item in roles)` bloğunu bununla değiştir:

```csharp
            var useRoles = await _userManager.GetRolesAsync(user!);
            foreach (var item in roles)
            {
                if (item.Name == PermissionSeeder.SuperAdminRoleDisplayName ||
                    item.DisplayName == PermissionSeeder.SuperAdminRoleDisplayName)
                {
                    continue;
                }

                model.Add(new AsignRoleToUserViewModel
                {
                    Id = item.Id,
                    Name = item.Name!,
                    DisplayName = item.DisplayName ?? item.Name!,
                    Description = item.Description,
                    IsExist = useRoles.Contains(item.Name!)
                });
            }
```

Not: `GetRolesAsync` döngü içinden dışına alındı — her rol için tekrar DB'ye gitmesine gerek yok.

- [ ] **Step 9: UserController.AsignRoleToUser POST'unu düzelt**

[UserController.cs:427-437](../../../Koala.Yedpa.WebUI/Controllers/UserController.cs) — `foreach (var item in model)` bloğunu bununla değiştir:

```csharp
            var mevcutRoller = await _userManager.GetRolesAsync(user!);
            foreach (var item in model)
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                {
                    continue;
                }

                var zatenAtanmis = mevcutRoller.Contains(item.Name);

                if (item.IsExist && !zatenAtanmis)
                {
                    await _userManager.AddToRoleAsync(user!, item.Name);
                }
                else if (!item.IsExist && zatenAtanmis)
                {
                    await _userManager.RemoveFromRoleAsync(user!, item.Name);
                }
            }

            // Rol değişikliğinin cookie'ye yansıması için güvenlik damgasını yenile.
            await _userManager.UpdateSecurityStampAsync(user!);
```

Dosyanın using bloğuna ekle:

```csharp
using Koala.Yedpa.WebUI.Authorization;
```

- [ ] **Step 10: AsignRoleToUser view'ını güncelle**

[AsignRoleToUser.cshtml:48](../../../Koala.Yedpa.WebUI/Views/User/AsignRoleToUser.cshtml) satırını değiştir:

```html
                                                <td>@Model[i].DisplayName</td>
```

[AsignRoleToUser.cshtml:62-63](../../../Koala.Yedpa.WebUI/Views/User/AsignRoleToUser.cshtml) hidden input'ların olduğu yere `DisplayName` için de hidden ekle:

```html
                                            <input type="hidden" asp-for="@Model[i].Name" />
                                            <input type="hidden" asp-for="@Model[i].DisplayName" />
                                            <input type="hidden" asp-for="@Model[i].Id" />
```

- [ ] **Step 11: Build + tüm testler**

```bash
dotnet build Koala.Yedpa.sln
```

Beklenen: 0 error.

```bash
dotnet test Koala.Yedpa.sln
```

Beklenen: tüm testler PASS.

- [ ] **Step 12: Elle doğrula**

```bash
dotnet run --project Koala.Yedpa.WebUI
```

1. `/AppRole/CreateRole` → `Name = TestRol`, `DisplayName = Test Rolü` ile rol oluştur
2. `/AppRole` → Test Rolü satırında yetki ekranını aç, hiçbir şey seçmeden Kaydet → hata almadan listeye dönmeli
3. Aynı ekranda `Modül Yönetimi - Modül Listesi` seç, Kaydet
4. `/User` → bir kullanıcıda rol atama ekranını aç, `Test Rolü` işaretle, Kaydet → hata almamalı
5. SQL ile doğrula:

```sql
SELECT r.Name, rc.ClaimType, rc.ClaimValue
FROM AspNetRoles r LEFT JOIN AspNetRoleClaims rc ON rc.RoleId = r.Id
WHERE r.Name = 'TestRol';
```

Beklenen: `Permission` / `ModuleManagement.List` satırı.

- [ ] **Step 13: Commit**

```bash
git add Koala.Yedpa.Core/Models/ViewModels/AppRoleViewModel.cs Koala.Yedpa.WebUI/Controllers/AppRoleController.cs Koala.Yedpa.WebUI/Controllers/UserController.cs Koala.Yedpa.WebUI/Views/User/AsignRoleToUser.cshtml Koala.Yedpa.WebUI.Tests
git commit -m "fix(auth): rol atamada DisplayName/Name karisikligi ve bos claim listesi NRE"
```

---

### Task 4: Kapalı kapı — FallbackPolicy, provider kaydı, ölü kod temizliği

Bu task'tan sonra giriş yapmayan hiç kimse hiçbir sayfaya giremez.

**Files:**
- Modify: `Koala.Yedpa.WebUI/Program.cs:39`
- Modify: `Koala.Yedpa.WebUI/Program.cs:114`
- Modify: `Koala.Yedpa.WebUI/Extentions/StartupExtention.cs:179-255`
- Modify: `Koala.Yedpa.WebUI/Controllers/ConnectionTestController.cs`, `TestEmailController.cs` (gerekirse)

**Interfaces:**
- Consumes: `PermissionPolicyProvider` (Task 1)

- [ ] **Step 1: Task 2 Step 9'daki kilitlenme kontrolünü tekrar çalıştır**

```sql
SELECT u.UserName,
       (SELECT COUNT(*) FROM AspNetRoleClaims rc
        JOIN AspNetUserRoles ur2 ON ur2.RoleId = rc.RoleId
        WHERE ur2.UserId = u.Id AND rc.ClaimType = 'Permission') AS ToplamYetki
FROM AspNetUsers u
WHERE u.UserName = 'erkan.dirikcan@gmail.com';
```

`ToplamYetki` 30 değilse **DUR** ve Task 2 Step 9'a dön.

- [ ] **Step 2: Program.cs'de FallbackPolicy ve provider kaydını yap**

[Program.cs:38-39](../../../Koala.Yedpa.WebUI/Program.cs) — `builder.Services.AddAuthorization();` satırını bununla değiştir:

```csharp
            // Kapalı kapı: aksi belirtilmedikçe her endpoint kimlik doğrulaması ister.
            // [AllowAnonymous] ile açıkça muaf tutulanlar serbesttir.
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // [Permission("X")] → RequireClaim("Permission", "X")
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
```

Program.cs using bloğuna ekle:

```csharp
using Microsoft.AspNetCore.Authorization;
```

- [ ] **Step 3: Ölü AuthorizationRulesInitializer kaydını kaldır**

[Program.cs:114](../../../Koala.Yedpa.WebUI/Program.cs) satırını **sil**:

```csharp
            builder.Services.AddHostedService<AuthorizationRulesInitializer>();
```

Bu servisin gövdesi boştu; her açılışta Claims tablosunu okuyup hiçbir şey yapmıyordu.

- [ ] **Step 4: StartupExtention.cs'deki ölü kodu sil**

[StartupExtention.cs:179-254](../../../Koala.Yedpa.WebUI/Extentions/StartupExtention.cs) aralığını, yani şu üç bloğun tamamını **sil**:
- `//public static void AddAuthorizationRules(...)` yorum bloğu (179-195)
- `public class AuthorizationRulesInitializer` sınıfı (198-218)
- `public static class AuthorizationRulesExtensions` sınıfı (219-226)
- `public class DynamicAuthorizationPolicyProvider` sınıfı (227-254)

`DynamicAuthorizationPolicyProvider` hiçbir zaman DI'a kaydedilmemişti (ölü kod) ve yerini `PermissionPolicyProvider` aldı.

Silme sonrası dosya `StartupExtention` sınıfının kapanış `}` ve namespace kapanış `}` ile bitmeli. Artık kullanılmayan using'leri temizle: `Microsoft.AspNetCore.Authorization`, `Microsoft.Extensions.Options`.

- [ ] **Step 5: Build**

```bash
dotnet build Koala.Yedpa.sln
```

Beklenen: 0 error. Hata alırsan `AuthorizationRulesInitializer` veya `DynamicAuthorizationPolicyProvider`'a kalan bir referans vardır — grep'le bul:

```bash
grep -rn "AuthorizationRulesInitializer\|DynamicAuthorizationPolicyProvider\|AddAuthorizationRules" --include=*.cs .
```

- [ ] **Step 6: Anonim erişmesi gereken endpoint'leri kontrol et**

```bash
grep -rn "AllowAnonymous" --include=*.cs Koala.Yedpa.WebUI/
```

Beklenen: `UserController` içindeki Login (GET+POST), ForgetPassword, ResetPassword (GET+POST), ForgetConfirm, AccessDenied.

`ConnectionTestController` ve `TestEmailController` teşhis amaçlıysa `[Authorize]` altında kalmalı — bunlara `[AllowAnonymous]` **ekleme**.

- [ ] **Step 7: Anonim erişimi elle doğrula**

```bash
dotnet run --project Koala.Yedpa.WebUI
```

Tarayıcıda gizli/incognito pencerede (oturum yok):

| URL | Beklenen |
|---|---|
| `/Module` | `/User/Login`'e yönlenir |
| `/AppRole` | `/User/Login`'e yönlenir |
| `/User` | `/User/Login`'e yönlenir |
| `/Dashboard` | `/User/Login`'e yönlenir |
| `/User/Login` | Login sayfası açılır |
| `/swagger` | Swagger UI açılır (middleware, endpoint değil) |

`/Module` login'e yönlenmiyorsa FallbackPolicy devreye girmemiştir — Step 2'yi kontrol et.

- [ ] **Step 8: Giriş yapılmış halde regresyon kontrolü**

Kendi kullanıcınla giriş yap, şu sayfaların hâlâ açıldığını gör: `/Dashboard`, `/Module`, `/AppRole`, `/User`, `/BulkInvoice`, `/Settings/LogoSettings`.

- [ ] **Step 9: Commit**

```bash
git add Koala.Yedpa.WebUI/Program.cs Koala.Yedpa.WebUI/Extentions/StartupExtention.cs
git commit -m "feat(auth): global FallbackPolicy ve PermissionPolicyProvider kaydi, olu kod temizligi"
```

---

### Task 5: Controller'lara [Permission] uygula

**Files:**
- Modify: `Koala.Yedpa.WebUI/Controllers/ModuleController.cs`
- Modify: `Koala.Yedpa.WebUI/Controllers/ClaimsController.cs`
- Modify: `Koala.Yedpa.WebUI/Controllers/AppRoleController.cs`
- Modify: `Koala.Yedpa.WebUI/Controllers/UserController.cs`
- Modify: `Koala.Yedpa.WebUI/Controllers/DashboardController.cs`
- Modify: `Koala.Yedpa.WebUI/Controllers/BulkInvoiceController.cs`
- Modify: `Koala.Yedpa.WebUI/Controllers/BudgetOrderController.cs`
- Modify: `Koala.Yedpa.WebUI/Controllers/SettingsController.cs`

**Interfaces:**
- Consumes: `PermissionAttribute`, `PermissionCatalog` (Task 1)

Kapsam notu: bu turda yalnızca yukarıdaki controller'lar izin bazlı korunur. `LogoSyncController`, `WorkplaceController`, `FinancialStatementController`, `QRCodeController`, `ConnectionTestController`, `TestEmailController` ve `*ApiController`'lar Task 4'ün FallbackPolicy'si sayesinde **giriş şartına** tabidir; ince yetkilendirme sonraki tura bırakılır.

- [ ] **Step 1: ModuleController'a izinleri ekle**

[ModuleController.cs](../../../Koala.Yedpa.WebUI/Controllers/ModuleController.cs) — her action'ın üstüne ekle:

```csharp
using Koala.Yedpa.WebUI.Authorization;
// ...
        [Permission(PermissionCatalog.ModuleManagement.List)]
        public async Task<IActionResult> Index()

        [Permission(PermissionCatalog.ModuleManagement.Create)]
        public IActionResult CreateModule()

        [HttpPost]
        [Permission(PermissionCatalog.ModuleManagement.Create)]
        public async Task<IActionResult> CreateModule(CreateModuleViewModel model)

        [Permission(PermissionCatalog.ModuleManagement.Update)]
        public async Task<IActionResult> UpdateModule(string id)

        [HttpPost]
        [Permission(PermissionCatalog.ModuleManagement.Update)]
        public async Task<IActionResult> UpdateModule(UpdateModuleViewModel model)

        [HttpPost]
        [Permission(PermissionCatalog.ModuleManagement.ChangeStatus)]
        public async Task<JsonResult> ChangeStatus(ModuleChangeStatusViewModel model)
```

- [ ] **Step 2: ClaimsController'a izinleri ekle**

[ClaimsController.cs](../../../Koala.Yedpa.WebUI/Controllers/ClaimsController.cs):

```csharp
using Koala.Yedpa.WebUI.Authorization;
// ...
        [Permission(PermissionCatalog.ModuleManagement.ClaimList)]
        public async Task<IActionResult> ModuleClaims(string moduleId)

        [HttpGet]
        [Permission(PermissionCatalog.ModuleManagement.ClaimCreate)]
        public async Task<IActionResult> CreateClaim(string moduleId)

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(PermissionCatalog.ModuleManagement.ClaimCreate)]
        public async Task<IActionResult> CreateClaim(CreateClaimsViewModel model)

        [HttpGet]
        [Permission(PermissionCatalog.ModuleManagement.ClaimUpdate)]
        public async Task<IActionResult> UpdateClaim(string id)

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(PermissionCatalog.ModuleManagement.ClaimUpdate)]
        public async Task<IActionResult> UpdateClaim(UpdateClaimsViewModel model)

        [HttpGet]
        [Permission(PermissionCatalog.ModuleManagement.ClaimDelete)]
        public async Task<JsonResult> DeleteClaim(string id)
```

- [ ] **Step 3: AppRoleController'a izinleri ekle**

[AppRoleController.cs](../../../Koala.Yedpa.WebUI/Controllers/AppRoleController.cs):

```csharp
        [Permission(PermissionCatalog.RoleManagement.List)]
        public IActionResult Index()

        [HttpGet]
        [Permission(PermissionCatalog.RoleManagement.Create)]
        public IActionResult CreateRole()

        [HttpPost]
        [Permission(PermissionCatalog.RoleManagement.Create)]
        public async Task<IActionResult> CreateRole(CreateAppRoleViewModel model)

        [HttpGet]
        [Permission(PermissionCatalog.RoleManagement.Update)]
        public IActionResult UpdateRole(string id)

        [HttpPost]
        [Permission(PermissionCatalog.RoleManagement.Update)]
        public async Task<IActionResult> UpdateRole(UpdateAppRoleViewModel model)

        [HttpGet]
        [Permission(PermissionCatalog.RoleManagement.AssignClaim)]
        public async Task<IActionResult> AddClaimToRole(string id)

        [HttpPost]
        [Permission(PermissionCatalog.RoleManagement.AssignClaim)]
        public async Task<IActionResult> AddClaimToRole(AddClaimToRoleViewModel model)

        [HttpPost]
        [Permission(PermissionCatalog.RoleManagement.Delete)]
        public async Task<JsonResult> DeleteRole(string id)
```

- [ ] **Step 4: UserController'a izinleri ekle**

[UserController.cs](../../../Koala.Yedpa.WebUI/Controllers/UserController.cs) — **dikkat:** `[AllowAnonymous]` olan action'lara (Login, ForgetPassword, ResetPassword, ForgetConfirm, AccessDenied) ve kullanıcının kendi profiline ait olanlara (`ChangePassword`, `UserProfile`, `Logout`) `[Permission]` **ekleme**; onlar giriş yapmış her kullanıcıya açık kalmalı.

```csharp
        [Permission(PermissionCatalog.UserManagement.List)]
        public async Task<IActionResult> Index()

        [HttpGet]
        [Permission(PermissionCatalog.UserManagement.Create)]
        public async Task<IActionResult> CreateUser()

        [HttpPost]
        [Permission(PermissionCatalog.UserManagement.Create)]
        public async Task<IActionResult> CreateUser(CreateAppUserViewModel model)

        [HttpGet]
        [Permission(PermissionCatalog.UserManagement.Update)]
        public async Task<IActionResult> UpdateUser(string id)

        [HttpPost]
        [Permission(PermissionCatalog.UserManagement.Update)]
        public async Task<IActionResult> UpdateUser(UpdateAppUserViewModel model)

        [HttpGet]
        [Permission(PermissionCatalog.UserManagement.AssignRole)]
        public async Task<IActionResult> AsignRoleToUser(string userId)

        [HttpPost]
        [Permission(PermissionCatalog.UserManagement.AssignRole)]
        public async Task<IActionResult> AsignRoleToUser(List<AsignRoleToUserViewModel> model, string userId)

        [HttpPost]
        [Permission(PermissionCatalog.UserManagement.ChangeStatus)]
        public async Task<JsonResult> UserChangeStatus(UpdateUserStatusViewModel model)
```

- [ ] **Step 5: Kalan controller'lara sınıf seviyesi izin ekle**

Her dosyada mevcut `[Authorize]` satırını `[Permission(...)]` ile **değiştir** (ikisi birden gerekmez, `PermissionAttribute` zaten `AuthorizeAttribute`):

- [DashboardController.cs:16](../../../Koala.Yedpa.WebUI/Controllers/DashboardController.cs) → `[Permission(PermissionCatalog.Dashboard.View)]`
- [BulkInvoiceController.cs:17](../../../Koala.Yedpa.WebUI/Controllers/BulkInvoiceController.cs) → `[Permission(PermissionCatalog.BulkInvoice.View)]`
- [BudgetOrderController.cs:11](../../../Koala.Yedpa.WebUI/Controllers/BudgetOrderController.cs) → `[Permission(PermissionCatalog.BudgetOrder.View)]`
- `SettingsController.cs` → sınıfın üstüne `[Permission(PermissionCatalog.Settings.View)]`; `[HttpPost]` olan action'ların üstüne ayrıca `[Permission(PermissionCatalog.Settings.Update)]`

Her dosyaya `using Koala.Yedpa.WebUI.Authorization;` ekle.

**Dikkat — Dashboard hata sayfası:** `app.UseExceptionHandler("/Dashboard/Error")` ([Program.cs:220](../../../Koala.Yedpa.WebUI/Program.cs)) Dashboard controller'ına gidiyor. `DashboardController` sınıfına `[Permission]` koyunca `Error` action'ı da kilitlenir ve `Dashboard.View` yetkisi olmayan kullanıcı hata aldığında sonsuz döngüye girebilir. `Error` action'ının üstüne `[AllowAnonymous]` ekle.

- [ ] **Step 6: Build**

```bash
dotnet build Koala.Yedpa.sln
```

Beklenen: 0 error.

- [ ] **Step 7: Katalogda olmayan izin adı kullanılmadığını doğrula**

Tüm `[Permission("...")]` kullanımları `PermissionCatalog` sabitleri üzerinden yapılmalı, düz string yazılmamalı:

```bash
grep -rn "Permission(\"" --include=*.cs Koala.Yedpa.WebUI/Controllers/
```

Beklenen: **hiç sonuç yok**. Sonuç varsa o satırı `PermissionCatalog.<Grup>.<Sabit>` ile değiştir.

- [ ] **Step 8: Süper Yönetici ile regresyon**

```bash
dotnet run --project Koala.Yedpa.WebUI
```

Süper Yönetici kullanıcınla giriş yap; `/Dashboard`, `/Module`, `/AppRole`, `/User`, `/BulkInvoice`, `/Settings/LogoSettings` sayfalarının **hepsi** açılmalı. Biri `/User/AccessDenied`'a gidiyorsa o izin katalogda var ama Süper Yönetici rolüne verilmemiştir — uygulamayı yeniden başlat (seeder tamamlar) ve çıkış/giriş yap.

- [ ] **Step 9: Yetkisiz kullanıcı ile doğrula**

1. `/AppRole/CreateRole` → `Name = SinirliRol`, `DisplayName = Sınırlı Rol`
2. `/AppRole` → Sınırlı Rol'e yetki ata: **sadece** `Kokpit - Kokpiti Görüntüle` ve `Kullanıcı Yönetimi - Kullanıcı Listesi`
3. `/User/CreateUser` → yeni test kullanıcısı oluştur, `SinirliRol` ata
4. Gizli pencerede test kullanıcısıyla giriş yap:

| URL | Beklenen |
|---|---|
| `/Dashboard` | Açılır |
| `/User` | Açılır |
| `/User/CreateUser` | `/User/AccessDenied` |
| `/Module` | `/User/AccessDenied` |
| `/AppRole` | `/User/AccessDenied` |
| `/BulkInvoice` | `/User/AccessDenied` |
| `/User/UserProfile` | Açılır (kendi profili, izin gerektirmiyor) |

- [ ] **Step 10: Commit**

```bash
git add Koala.Yedpa.WebUI/Controllers
git commit -m "feat(auth): yetki yonetimi ve ana modul controller'larina izin bazli koruma"
```

---

### Task 6: View/menü tarafında yetki kontrolü

**Files:**
- Create: `Koala.Yedpa.WebUI/Authorization/ClaimsPrincipalExtensions.cs`
- Modify: `Koala.Yedpa.WebUI/Views/_ViewImports.cshtml`
- Modify: `Koala.Yedpa.WebUI/Views/Shared/_MainManuPartial.cshtml:92-130`
- Modify: `Koala.Yedpa.WebUI/Views/Module/Index.cshtml:29`
- Test: `Koala.Yedpa.WebUI.Tests/Authorization/ClaimsPrincipalExtensionsTests.cs`

**Interfaces:**
- Consumes: `PermissionPolicyProvider.PermissionClaimType`, `PermissionCatalog` (Task 1)
- Produces: `ClaimsPrincipalExtensions.HasPermission(this ClaimsPrincipal? user, string permission)` → `bool`
- Produces: `ClaimsPrincipalExtensions.HasAnyPermission(this ClaimsPrincipal? user, params string[] permissions)` → `bool`

- [ ] **Step 1: Failing test yaz**

`Koala.Yedpa.WebUI.Tests/Authorization/ClaimsPrincipalExtensionsTests.cs`:

```csharp
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
```

- [ ] **Step 2: Testi çalıştır, derlenmediğini gör**

```bash
dotnet test Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj --filter "FullyQualifiedName~ClaimsPrincipalExtensionsTests"
```

Beklenen: derleme hatası — `HasPermission` extension'ı yok.

- [ ] **Step 3: Extension'ı yaz**

`Koala.Yedpa.WebUI/Authorization/ClaimsPrincipalExtensions.cs`:

```csharp
using System.Security.Claims;

namespace Koala.Yedpa.WebUI.Authorization;

/// <summary>
/// View'larda ve controller'larda izin kontrolü için.
/// Örnek: @if (User.HasPermission(PermissionCatalog.ModuleManagement.List)) { ... }
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal? user, string permission)
    {
        if (user?.Identity is null || !user.Identity.IsAuthenticated)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        return user.HasClaim(PermissionPolicyProvider.PermissionClaimType, permission);
    }

    /// <summary>Verilen izinlerden en az biri varsa true. Menü başlıklarını gizlemek için.</summary>
    public static bool HasAnyPermission(this ClaimsPrincipal? user, params string[] permissions)
    {
        if (permissions is null || permissions.Length == 0)
        {
            return false;
        }

        return permissions.Any(p => user.HasPermission(p));
    }
}
```

Not: `ClaimsPrincipal.HasClaim(type, value)` büyük/küçük harf duyarlı `Ordinal` karşılaştırma yapar — testteki beklenti bununla uyumlu.

- [ ] **Step 4: Testleri çalıştır, geçtiğini gör**

```bash
dotnet test Koala.Yedpa.WebUI.Tests/Koala.Yedpa.WebUI.Tests.csproj --filter "FullyQualifiedName~ClaimsPrincipalExtensionsTests"
```

Beklenen: 8 test PASS.

- [ ] **Step 5: _ViewImports'a using ekle**

`Koala.Yedpa.WebUI/Views/_ViewImports.cshtml` dosyasının sonuna ekle:

```cshtml
@using Koala.Yedpa.WebUI.Authorization
```

- [ ] **Step 6: Menüde "Kullanıcı İşlemleri" bloğunu izne bağla**

[_MainManuPartial.cshtml:92-106](../../../Koala.Yedpa.WebUI/Views/Shared/_MainManuPartial.cshtml) aralığındaki `<li>` bloğunun tamamını bununla değiştir:

```cshtml
                @if (User.HasAnyPermission(
                        PermissionCatalog.UserManagement.List,
                        PermissionCatalog.UserManagement.Create,
                        PermissionCatalog.RoleManagement.List,
                        PermissionCatalog.RoleManagement.Create))
                {
                    <li class="menu-item  menu-item-submenu @ManageNavPages.UserNavClass(ViewContext)"  aria-haspopup="true" data-menu-toggle="hover">
                        <a href="javascript:;" class="menu-link menu-toggle"><i class="menu-icon flaticon-users"></i><span class="menu-text">Kullanıcı İşlemleri</span><i class="menu-arrow"></i></a>
                        <div class="menu-submenu ">
                            <i class="menu-arrow"></i>
                            <ul class="menu-subnav">
                                @if (User.HasPermission(PermissionCatalog.UserManagement.List))
                                {
                                    <li class="menu-item @ManageNavPages.UserListNavClass(ViewContext)" aria-haspopup="true"><a class="menu-link " asp-action="Index" asp-controller="User"><i class="menu-bullet menu-bullet-dot"><span></span></i><span class="menu-text">Kullanıcı Listesi</span></a></li>
                                }
                                @if (User.HasPermission(PermissionCatalog.UserManagement.Create))
                                {
                                    <li class="menu-item @ManageNavPages.CreateUserNavClass(ViewContext)" aria-haspopup="true"><a class="menu-link " asp-action="CreateUser" asp-controller="User"><i class="menu-bullet menu-bullet-dot"><span></span></i><span class="menu-text">Kullanıcı Ekle</span></a></li>
                                }
                                @if (User.HasPermission(PermissionCatalog.RoleManagement.List))
                                {
                                    <li class="menu-item @ManageNavPages.RoleListNavClass(ViewContext)" aria-haspopup="true"><a class="menu-link " asp-action="Index" asp-controller="AppRole"><i class="menu-bullet menu-bullet-dot"><span></span></i><span class="menu-text">Roller</span></a></li>
                                }
                                @if (User.HasPermission(PermissionCatalog.RoleManagement.Create))
                                {
                                    <li class="menu-item @ManageNavPages.CreateRoleNavClass(ViewContext)" aria-haspopup="true"><a class="menu-link " asp-controller="AppRole" asp-action="CreateRole"><i class="menu-bullet menu-bullet-dot"><span></span></i><span class="menu-text">Rol Ekle</span></a></li>
                                }
                            </ul>
                        </div>
                    </li>
                }
```

- [ ] **Step 7: Menüde "Modül İşlemleri" bloğunu izne bağla**

[_MainManuPartial.cshtml:107-116](../../../Koala.Yedpa.WebUI/Views/Shared/_MainManuPartial.cshtml) aralığındaki `<li>` bloğunun tamamını bununla değiştir:

```cshtml
                @if (User.HasAnyPermission(
                        PermissionCatalog.ModuleManagement.List,
                        PermissionCatalog.ModuleManagement.Create))
                {
                    <li class="menu-item  menu-item-submenu @ManageNavPages.ModuleNavClass(ViewContext)" aria-haspopup="true" data-menu-toggle="hover">
                        <a href="javascript:;" class="menu-link menu-toggle"><i class="menu-icon flaticon-cogwheel-2"></i><span class="menu-text">Modül İşlemleri</span><i class="menu-arrow"></i></a>
                        <div class="menu-submenu ">
                            <i class="menu-arrow"></i>
                            <ul class="menu-subnav">
                                @if (User.HasPermission(PermissionCatalog.ModuleManagement.List))
                                {
                                    <li class="menu-item @ManageNavPages.ModuleListNavClass(ViewContext)" aria-haspopup="true"><a asp-controller="Module" class="menu-link "><i class="menu-bullet menu-bullet-dot"><span></span></i><span class="menu-text">Modül Listesi</span></a></li>
                                }
                                @if (User.HasPermission(PermissionCatalog.ModuleManagement.Create))
                                {
                                    <li class="menu-item  @ManageNavPages.CreateModuleNavClass(ViewContext)" aria-haspopup="true"><a asp-controller="Module" asp-action="CreateModule" class="menu-link "><i class="menu-bullet menu-bullet-dot"><span></span></i><span class="menu-text">Modül Ekle</span></a></li>
                                }
                            </ul>
                        </div>
                    </li>
                }
```

- [ ] **Step 8: Menüde "Ayarlar" bloğunu izne bağla**

[_MainManuPartial.cshtml:117](../../../Koala.Yedpa.WebUI/Views/Shared/_MainManuPartial.cshtml) satırında başlayan `Ayarlar` `<li>` bloğunun tamamını (kapanış `</li>`'sine kadar) `@if (User.HasPermission(PermissionCatalog.Settings.View)) { ... }` içine al. İç `<li>` öğelerine ayrıca kontrol ekleme — hepsi aynı izne bağlı.

- [ ] **Step 9: Modül listesindeki "Modül Tanımla" butonunu izne bağla**

[Module/Index.cshtml:29-40](../../../Koala.Yedpa.WebUI/Views/Module/Index.cshtml) — `<a asp-action="CreateModule" ...>` bağlantısının tamamını sarmala:

```cshtml
                            @if (User.HasPermission(PermissionCatalog.ModuleManagement.Create))
                            {
                                <!--begin::Button-->
                                <a asp-action="CreateModule" asp-controller="Module" class="btn btn-primary font-weight-bolder">
                                    <span class="svg-icon svg-icon-md">
                                        <svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="24px" height="24px" viewBox="0 0 24 24" version="1.1">
                                            <g stroke="none" stroke-width="1" fill="none" fill-rule="evenodd">
                                                <rect x="0" y="0" width="24" height="24" />
                                                <circle fill="#000000" cx="9" cy="15" r="6" />
                                                <path d="M8.8012943,7.00241953 C9.83837775,5.20768121 11.7781543,4 14,4 C17.3137085,4 20,6.6862915 20,10 C20,12.2218457 18.7923188,14.1616223 16.9975805,15.1987057 C16.9991904,15.1326658 17,15.0664274 17,15 C17,10.581722 13.418278,7 9,7 C8.93357256,7 8.86733422,7.00080962 8.8012943,7.00241953 Z" fill="#000000" opacity="0.3" />
                                            </g>
                                        </svg>
                                        <!--end::Svg Icon-->
                                    </span>Modül Tanımla
                                </a>
                                <!--end::Button-->
                            }
```

- [ ] **Step 10: Build + tüm testler**

```bash
dotnet build Koala.Yedpa.sln
```

Beklenen: 0 error. Razor derleme hatası alırsan `_ViewImports.cshtml`'deki `@using` satırını kontrol et.

```bash
dotnet test Koala.Yedpa.sln
```

Beklenen: tüm testler PASS.

- [ ] **Step 11: Elle doğrula**

```bash
dotnet run --project Koala.Yedpa.WebUI
```

1. Süper Yönetici ile gir → menüde "Kullanıcı İşlemleri", "Modül İşlemleri", "Ayarlar" **görünür**, altlarındaki tüm öğeler var
2. Task 5 Step 9'daki `SinirliRol` kullanıcısı ile gir → menüde "Modül İşlemleri" ve "Ayarlar" **hiç görünmez**; "Kullanıcı İşlemleri" görünür ama altında **sadece** "Kullanıcı Listesi" var
3. `SinirliRol` kullanıcısıyla `/Module` adresine elle git → `/User/AccessDenied` (menü gizlemesi tek başına yeterli değil; controller koruması da çalışıyor)

- [ ] **Step 12: Commit**

```bash
git add Koala.Yedpa.WebUI/Authorization/ClaimsPrincipalExtensions.cs Koala.Yedpa.WebUI/Views Koala.Yedpa.WebUI.Tests
git commit -m "feat(auth): view/menu tarafinda izin bazli gorunurluk"
```

---

### Task 7: Claims.Name unique index

Policy adı olarak kullanılan `Claims.Name` alanının benzersizliğini DB seviyesinde garanti eder. Aynı isimde iki claim, `AddClaimToRole` ekranında çift satır ve kafa karışıklığı üretir.

**Files:**
- Modify: `Koala.Yedpa.Repositories/Configurations/ClaimsConfiguration.cs`
- Create: `Koala.Yedpa.Repositories/Migrations/<timestamp>_ClaimsNameUniqueIndex.cs` (EF üretir)

- [ ] **Step 1: Mevcut veride çift kayıt var mı kontrol et**

```sql
SELECT Name, COUNT(*) AS Adet
FROM Claims
GROUP BY Name
HAVING COUNT(*) > 1;
```

Sonuç boş değilse **önce temizle**. Hangi kaydın tutulacağına karar vermek için:

```sql
SELECT c.Id, c.Name, c.DisplayName, m.Name AS Modul,
       (SELECT COUNT(*) FROM AspNetRoleClaims rc
        WHERE rc.ClaimType = 'Permission' AND rc.ClaimValue = c.Name) AS RolKullanimi
FROM Claims c JOIN Module m ON m.Id = c.ModuleId
WHERE c.Name IN (SELECT Name FROM Claims GROUP BY Name HAVING COUNT(*) > 1)
ORDER BY c.Name;
```

Her isim için bir kaydı bırak, diğerlerini sil. Silmeden önce yedek al.

- [ ] **Step 2: Boş/NULL Name var mı kontrol et**

```sql
SELECT Id, ModuleId, DisplayName FROM Claims WHERE Name IS NULL OR LTRIM(RTRIM(Name)) = '';
```

Sonuç varsa bu kayıtlar policy adı olarak kullanılamaz — sil veya anlamlı bir ad ver. (MSSQL'de unique index birden fazla NULL'a izin vermez.)

- [ ] **Step 3: ClaimsConfiguration'a index ekle**

[ClaimsConfiguration.cs](../../../Koala.Yedpa.Repositories/Configurations/ClaimsConfiguration.cs) — `Configure` metodunun içine ekle:

```csharp
            // Claims.Name doğrudan authorization policy adı olarak kullanılıyor;
            // benzersizliği DB seviyesinde garanti altına alıyoruz.
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("IX_Claims_Name_Unique");
```

- [ ] **Step 4: Migration üret**

```bash
dotnet ef migrations add ClaimsNameUniqueIndex --project Koala.Yedpa.Repositories --startup-project Koala.Yedpa.WebUI
```

Beklenen: `Migrations/` altında yeni dosya. İçinde `AlterColumn<string>(name: "Name", table: "Claims", ...)` ve `CreateIndex(..., unique: true)` olmalı.

- [ ] **Step 5: Migration'ı uygula**

```bash
dotnet ef database update --project Koala.Yedpa.Repositories --startup-project Koala.Yedpa.WebUI
```

`Cannot create unique index ... duplicate key` hatası alırsan Step 1/2'ye dön — temizlik eksik kalmış.

- [ ] **Step 6: Build + test + uygulama**

```bash
dotnet build Koala.Yedpa.sln
```

Beklenen: 0 error.

```bash
dotnet test Koala.Yedpa.sln
```

Beklenen: tüm testler PASS.

```bash
dotnet run --project Koala.Yedpa.WebUI
```

Log'da `Yetki kataloğu senkronize edildi. Yeni kayıt: 0` görmelisin (seeder idempotent, index'i ihlal etmiyor).

- [ ] **Step 7: Elle doğrula**

`/Module` → bir modül seç → yetki ekranı → `/Claims/CreateClaim` ile **zaten var olan** bir isimle (örn. `ModuleManagement.List`) claim eklemeyi dene.

Beklenen: kayıt oluşmaz; `ClaimsService.CreateClaim` hatayı yakalar ve form geri döner. Uygulama çökmemeli.

- [ ] **Step 8: Commit**

```bash
git add Koala.Yedpa.Repositories
git commit -m "feat(auth): Claims.Name uzerinde unique index"
```

---

## Kabul Kriterleri

Tüm task'lar bittiğinde şunlar doğru olmalı:

- [ ] `dotnet build Koala.Yedpa.sln` → 0 error
- [ ] `dotnet test Koala.Yedpa.sln` → tüm testler yeşil
- [ ] Giriş yapmamış kullanıcı hiçbir sayfaya giremiyor (Login/ResetPassword/AccessDenied hariç)
- [ ] `/Module` → modül tanımlanabiliyor
- [ ] `/Claims/CreateClaim` → modüle claim tanımlanabiliyor; aynı isim iki kez eklenemiyor
- [ ] `/AppRole/CreateRole` → rol tanımlanabiliyor
- [ ] `/AppRole/AddClaimToRole` → role claim atanabiliyor; hiçbir şey seçmeden kaydetmek tüm yetkileri kaldırıyor ve patlamıyor
- [ ] `/User/AsignRoleToUser` → rol atanabiliyor; `DisplayName != Name` olan rolde de çalışıyor
- [ ] `[Permission(...)]` ile korunan bir sayfaya yetkisiz kullanıcı girmeye çalışınca `/User/AccessDenied`
- [ ] Menüde kullanıcının yetkisi olmayan öğeler hiç görünmüyor
- [ ] Uygulama açılışında `PermissionCatalog`'daki tüm izinler DB'de mevcut ve Süper Yönetici rolünde

## Geri Alma (Rollback)

Kilitlenirsen — kimse yetki yönetimi ekranlarına giremiyorsa — SQL ile kendi rolüne tüm izinleri ver:

```sql
DECLARE @RoleId NVARCHAR(450) = (
    SELECT TOP 1 r.Id FROM AspNetRoles r
    JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
    JOIN AspNetUsers u ON u.Id = ur.UserId
    WHERE u.UserName = 'erkan.dirikcan@gmail.com');

INSERT INTO AspNetRoleClaims (RoleId, ClaimType, ClaimValue)
SELECT @RoleId, 'Permission', c.Name
FROM Claims c
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetRoleClaims rc
    WHERE rc.RoleId = @RoleId AND rc.ClaimType = 'Permission' AND rc.ClaimValue = c.Name);
```

Ardından çıkış/giriş yap (veya 120 sn bekle) — cookie yenilenir.

Acil durumda Task 4'ün commit'ini geri al: `git revert <commit>` → FallbackPolicy kalkar, sistem Task 3 durumuna döner.

## Görev Dağılımı (öneri)

| Task | Uygun teammate | Model |
|---|---|---|
| 1 — Katalog + provider + attribute | `olga` | opus (mimari çekirdek) |
| 2 — Seeder | `natasa` | sonnet |
| 3 — Bug'lar | `olga` | sonnet |
| 4 — FallbackPolicy + temizlik | `nastya` | sonnet |
| 5 — Controller `[Permission]` | `nastya` | sonnet |
| 6 — View/menü | `mahmut` | sonnet |
| 7 — Unique index + migration | `natasa` | sonnet |

Task 1 → 2 → 3 → 4 → 5 → 6 → 7 sırayla yürütülmeli. 3 ve 7 teknik olarak bağımsızdır ama 4'ten önce 3'ün bitmesi hata ayıklamayı kolaylaştırır.
