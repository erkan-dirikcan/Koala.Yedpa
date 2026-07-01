# Toplu Faturalandırma (Bulk Invoice) Yeniden Yazım Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Seçilen bir tarihte (gece 00:01) bekleyen AIDAT sipariş satırlarını Logo REST ile otomatik faturaya dönüştürmek; T-1 gün 08:00'de Excel'li bilgi maili, aktarım sonrası rapor maili göndermek.

**Architecture:** Kullanıcı dashboard'dan bir oturum (`BulkInvoiceSession`) oluşturur ve fatura tarihini seçer. Oturum oluşturulduğunda 2 Hangfire job zamanlanır: (1) T-1 08:00 bilgi maili+Excel, (2) T 00:01 aktarım. Aktarım job'ı bekleyen AIDAT satırlarını çeker, her cari için Logo REST `salesInvoices`'a kanıtlanmış payload'la POST eder, başarılı satırların `ORFLINE.TRGFLAG`'ını toplu T-SQL ile 1 yapar, sonuçları `BulkInvoiceItem`'a yazar ve rapor maili gönderir.

**Tech Stack:** ASP.NET Core 10, EF Core (MSSQL), Hangfire (zamanlama), ClosedXML 0.105.0 (Excel), Logo Tiger REST API, System.Text.Json.

## Global Constraints

- Logo Firma=211, Dönem=16 değildir varsayma — daima `ISettingsService.GetLogoSettingsAsync()`'ten `Firm`/`Period` oku.
- Logo REST payload yapısı KANITLANMIŞ (repo kök `test-aidat-fatura-temmuz.json`): `TRANSACTIONS` düz dizi DEĞİL → `{ "items": [ ... ] }`. `PAYMENT_CODE="10-3"` (tüm AIDAT carileri için SABİT). Toplamlar (TOTAL/VAT_AMOUNT/VAT_BASE/TOTAL_NET) GÖNDERİLMEZ — REST hesaplar. GL kodları `DataObjectParameter.FillAccCodesOnPreSave=true` ile otomatik.
- Ay eşleşmesi `ORL.LINEEXP` (büyük harf ASCII ay adı) ile yapılır, `LINENO_` ile DEĞİL. Tutar `ORL.TOTAL`'dır (`ORL.AMOUNT`=miktar).
- İdempotency: Logo, REST fatura kesince `TRGFLAG`'ı otomatik 1 YAPMAZ (canlı veriyle doğrulandı). Başarılı satırlar hem `BulkInvoiceItem`'a yazılır hem `ORFLINE.TRGFLAG=1` yapılır.
- Teammate/agent delegasyonu YOK — ana asistan yazar. Her task sonunda `dotnet build` (0 error) + ilgili `dotnet test` yeşil olmadan "tamam" denmez.
- Fatura tipi `TYPE=7`, e-fatura `EINVOICE=1`/`PROFILE_ID=1` (kanıtlandı).
- **KALICILIK (kritik):** Aktarım sonuçları cache/bellekte BEKLETİLMEZ — elektrik kesintisi/sistem kapanması riski. Her satırın durumu anında DB'ye (`BulkInvoiceItem`) yazılır. Önce tüm satırlar "gönderilmedi" (Pending) olarak yazılır, sonra her deneme sonucu o satırda güncellenir (per-item `SaveChanges`).
- **Mail alıcıları (şimdilik SABİT):** `erkan@sistem-bilgisayar.com.tr`, `adegimli@yedpa.com.tr`, `muhasebe@yedpa.com.tr`. (İleride yetki bazlı — ayardan okunacak şekilde sabit liste bir const/config'te tutulur.)

---

## ADDENDUM (2026-06-30): Kalıcı Crosstable + Retry + Token Politikası

"Crosstable" = mevcut **`BulkInvoiceItem`** tablosu (session × ORFLINE satırı). Ayrı tablo açılmaz; bu tablo şu kolonlarla **genişletilir** (Task 5 migration'a dahil):

```csharp
// BulkInvoiceItem.cs'e eklenecek alanlar
public int RetryCount { get; set; } = 0;                  // kaç kez denendi
public bool CanRetry { get; set; } = false;               // "Tekrar dene" — token/geçici hatada true
[StringLength(500)]  public string? Note { get; set; }     // neden aktarılmadı (iş açıklaması)
[StringLength(2000)] public string? RestError { get; set; } // REST'ten dönen HAM hata
```
(Mevcut `ErrorMessage` kolonu `RestError` ile aynı amaçlı — yeni kodda `RestError` kullanılır; `ErrorMessage` geriye dönük bırakılır, set edilmez.)

**Hata sınıflandırması — `TransferLineResult` genişler:**
```csharp
public record TransferLineResult(bool Success, int Orflineref, string ClientCode,
    int? LogoInvoiceRef, string? InvoiceNumber, string? Note, string? RestError, bool IsTransient);
// IsTransient=true  → token/auth/geçici hata → CanRetry=true (kuyruk sonrası tekrar denenir)
// IsTransient=false → iş/kalıcı hata (örn. cari bulunamadı) → CanRetry=false (tekrar denenmez)
```
Token hatası tespiti: `HttpPost` yanıtı `StatusCode==401` VEYA `Message`/`Errors` "token" içeriyorsa transient kabul edilir.

**İki katmanlı retry:**
1. **Anlık token-retry (transfer anında):** Bir POST token hatasıyla dönerse, `Task.Delay(3000)` ile birkaç saniye bekle ve POST'u (token yeniden alınarak) en fazla **2 kez** tekrar dene. Hâlâ token hatasıysa → IsTransient=true döner (kuyruk sonrası retry'a kalır).
2. **Kuyruk-sonrası retry (3 tur):** Ana geçiş bitince, `Status=Failed AND CanRetry=true AND RetryCount<3` satırlar tekrar denenir. Her turdan önce kısa bekleme. `RetryCount` artırılır. **3 denemeden sonra** `CanRetry=false`, `Note="3 deneme sonrası aktarılamadı"` set edilir.

**Aktarım job akışı (kesin sıra):**
1. Bekleyen siparişleri çek (`GetPendingLinesAsync(ay)`).
2. **Tümünü `BulkInvoiceItem`'a `Status=Pending` (gönderilmedi) olarak yaz + `SaveChanges`** (transferden ÖNCE — kalıcılık).
3. Her item: `TransferLineAsync` → sonucu o satırda güncelle (`Transferred`+`LogoInvoiceRef` VEYA `Failed`+`Note`+`RestError`+`CanRetry`) → **per-item `SaveChanges`**.
4. Tüm satırlar bitince: `CanRetry` olanları **3 tura kadar** tekrar dene (her başarıda satırı `Transferred` yap).
5. Başarılı tüm satırların `Orflineref`'lerini topla → `MarkLinesAsTransferredAsync` (TRGFLAG=1).
6. Rapor maili hazırla/gönder (sabit 3 alıcı; başarılı/başarısız sayısı + başarısız tablo: ClientCode, fiş no/LogoInvoiceRef, Note, RestError).

---

## Mevcut / Yeniden Kullanılan Varlıklar (DEĞİŞTİRİLMEZ, referans)

- `Koala.Yedpa.Core/Models/BulkInvoiceSession.cs` — Id, InvoiceDate, Month, Year, Status, CreatedBy, CreatedAt, CompletedAt, Items
- `Koala.Yedpa.Core/Models/BulkInvoiceItem.cs` — Id, SessionId, OrficheRef, Orflineref, ClientCode, ClientName, Amount, MonthName, Status, LogoInvoiceRef, ErrorMessage
- Migration `20260513074638_AddBulkInvoiceTables`
- `Koala.Yedpa.Service/Services/BulkInvoiceService.cs` — `GetPendingLinesAsync` (sorgu DÜZELTİLDİ ve doğrulandı), `CheckAlertAsync`, `CreateSessionAsync`, `GetSessionStatusAsync`
- `Koala.Yedpa.WebUI/Controllers/BulkInvoiceController.cs` (MVC) + dashboard modal + `wwwroot/js/dashboard/bulk-invoice.js`
- `Koala.Yedpa.Service/Providers/LogoRestServiceProvider.cs` — `HttpPost(url, json)`
- `Koala.Yedpa.Core/Helpers/LogoJsonHelper.cs` — `InjectDataObjectParameter`
- `Koala.Yedpa.Core/Helpers/Tools.cs` — `ConvertToLogoTime`
- `Koala.Yedpa.Service/Services/EmailService.cs` (IEmailService) — mevcut mail altyapısı

## Yeniden Yazılan / Silinen (GLM artıkları)

- `Koala.Yedpa.Service/Services/BackgroundServices/BulkInvoiceTransferBackgroundService.cs` — SİL, yerine Hangfire job (Task 6)
- `Koala.Yedpa.Service/Services/BulkInvoiceEmailService.cs` + `Koala.Yedpa.Core/Services/IBulkInvoiceEmailService.cs` — gözden geçir, Task 7'de yeniden yaz

## File Structure (yeni/değişen)

- Create `Koala.Yedpa.Core/Dtos/BulkInvoice/AidatInvoicePayload.cs` — Logo `salesInvoices` payload modeli (items sarmalı, sadece girdi alanları)
- Create `Koala.Yedpa.Core/Services/IBulkInvoiceTransferService.cs` + `Koala.Yedpa.Service/Services/BulkInvoiceTransferService.cs` — payload kur, POST et, sonuç dön
- Modify `Koala.Yedpa.Core/Services/IBulkInvoiceService.cs` + `BulkInvoiceService.cs` — `MarkLinesAsTransferredAsync(orflinerefs)` (toplu TRGFLAG T-SQL), `GetPendingLinesAsync(string monthName)` (parametreli)
- Create `Koala.Yedpa.Core/Services/IBulkInvoiceExcelService.cs` + `Koala.Yedpa.Service/Services/BulkInvoiceExcelService.cs` — ClosedXML ile önizleme Excel
- Rewrite `Koala.Yedpa.Core/Services/IBulkInvoiceEmailService.cs` + `BulkInvoiceEmailService.cs` — bilgi maili + rapor maili
- Create `Koala.Yedpa.Service/Services/BulkInvoiceJobs.cs` — Hangfire job metotları (info-email, transfer)
- Modify `BulkInvoiceService.CreateSessionAsync` — Hangfire `BackgroundJob.Schedule` ile 2 job zamanla
- Modify `Koala.Yedpa.Service/Extentions/ServiceCollectionExtensions.cs` — yeni servis kayıtları
- Create migration: `BulkInvoiceSession`'a `InfoJobId`/`TransferJobId` (string?) ekle (job iptali için)
- Settings: bilgi/rapor maili alıcı listesi (`SettingsTypeEnum.BulkInvoiceMailRecipients`)

---

## Task 1: AidatInvoicePayload modeli (kanıtlanmış Logo yapısı)

**Files:**
- Create: `Koala.Yedpa.Core/Dtos/BulkInvoice/AidatInvoicePayload.cs`
- Test: `Koala.Yedpa.Service.Tests/BulkInvoice/AidatInvoicePayloadTests.cs`

**Interfaces:**
- Produces: `AidatInvoicePayload` (header alanları + `Transactions.Items`), `AidatInvoiceTransaction`. System.Text.Json `[JsonPropertyName]` ile Logo anahtarlarına map'lenir.

- [ ] **Step 1: Failing test — payload doğru JSON anahtarlarına serialize oluyor**

```csharp
using System.Text.Json;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Xunit;

public class AidatInvoicePayloadTests
{
    [Fact]
    public void Serialize_ProducesProvenLogoStructure()
    {
        var p = new AidatInvoicePayload
        {
            ArpCode = "1.F000.090.00.11",
            Date = "2026-07-01",
            Time = 66048,
            Notes1 = "Temmuz AIDAT TAHAKKUKU",
            Transactions = new AidatInvoiceTransactions
            {
                Items = { new AidatInvoiceTransaction { Price = "5016.7", Description = "Temmuz AIDAT" } }
            }
        };

        var json = JsonSerializer.Serialize(p);

        Assert.Contains("\"ARP_CODE\":\"1.F000.090.00.11\"", json);
        Assert.Contains("\"PAYMENT_CODE\":\"10-3\"", json);
        Assert.Contains("\"TYPE\":7", json);
        Assert.Contains("\"TRANSACTIONS\":{\"items\":[", json); // items sarmalı
        Assert.Contains("\"MASTER_CODE\":\"600.11.0001\"", json);
        Assert.DoesNotContain("VAT_AMOUNT", json);  // toplamlar gönderilmez
        Assert.DoesNotContain("TOTAL_NET", json);
    }
}
```

- [ ] **Step 2: Testi çalıştır, FAIL gör**

Run: `dotnet test Koala.Yedpa.Service.Tests --filter AidatInvoicePayloadTests`
Expected: FAIL — `AidatInvoicePayload` tipi yok (derlenmez).

- [ ] **Step 3: Modeli yaz**

```csharp
using System.Text.Json.Serialization;

namespace Koala.Yedpa.Core.Dtos.BulkInvoice;

/// <summary>
/// Logo REST `salesInvoices` AIDAT fatura payload'ı.
/// Yapı repo kök test-aidat-fatura-temmuz.json ile manuel doğrulandı.
/// Toplamlar/VAT kırılımı GÖNDERİLMEZ — REST hesaplar.
/// </summary>
public class AidatInvoicePayload
{
    [JsonPropertyName("GRP_CODE")] public int GrpCode { get; set; } = 2;
    [JsonPropertyName("TYPE")] public int Type { get; set; } = 7;
    [JsonPropertyName("NUMBER")] public string Number { get; set; } = "~"; // Logo otomatik no
    [JsonPropertyName("DATE")] public string Date { get; set; } = string.Empty; // "yyyy-MM-dd"
    [JsonPropertyName("TIME")] public int Time { get; set; }
    [JsonPropertyName("DOC_NUMBER")] public string DocNumber { get; set; } = "AIDAT";
    [JsonPropertyName("AUTH_CODE")] public string AuthCode { get; set; } = "AIDAT";
    [JsonPropertyName("ARP_CODE")] public string ArpCode { get; set; } = string.Empty;
    [JsonPropertyName("NOTES1")] public string Notes1 { get; set; } = string.Empty;
    [JsonPropertyName("PAYMENT_CODE")] public string PaymentCode { get; set; } = "10-3"; // tüm AIDAT için sabit
    [JsonPropertyName("EINVOICE")] public int EInvoice { get; set; } = 1;
    [JsonPropertyName("PROFILE_ID")] public int ProfileId { get; set; } = 1;
    [JsonPropertyName("EINSTEAD_OF_DISPATCH")] public int EInsteadOfDispatch { get; set; } = 1;
    [JsonPropertyName("EDTCURR_GLOBAL_CODE")] public string EdtCurrGlobalCode { get; set; } = "TL";
    [JsonPropertyName("TRANSACTIONS")] public AidatInvoiceTransactions Transactions { get; set; } = new();
}

public class AidatInvoiceTransactions
{
    [JsonPropertyName("items")] public List<AidatInvoiceTransaction> Items { get; set; } = new();
}

public class AidatInvoiceTransaction
{
    [JsonPropertyName("TYPE")] public int Type { get; set; } = 4;
    [JsonPropertyName("MASTER_CODE")] public string MasterCode { get; set; } = "600.11.0001"; // sabit AIDAT hizmet kartı
    [JsonPropertyName("QUANTITY")] public int Quantity { get; set; } = 1;
    [JsonPropertyName("UNIT_CONV1")] public string UnitConv1 { get; set; } = "1";
    [JsonPropertyName("UNIT_CONV2")] public string UnitConv2 { get; set; } = "1";
    [JsonPropertyName("PRICE")] public string Price { get; set; } = string.Empty; // KDV dahil tutar (string)
    [JsonPropertyName("VAT_RATE")] public int VatRate { get; set; } = 20;
    [JsonPropertyName("VAT_INCLUDED")] public int VatIncluded { get; set; } = 1;
    [JsonPropertyName("UNIT_CODE")] public string UnitCode { get; set; } = "ADET";
    [JsonPropertyName("DESCRIPTION")] public string Description { get; set; } = string.Empty;
}
```

- [ ] **Step 4: Testi çalıştır, PASS gör**

Run: `dotnet test Koala.Yedpa.Service.Tests --filter AidatInvoicePayloadTests`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add Koala.Yedpa.Core/Dtos/BulkInvoice/AidatInvoicePayload.cs Koala.Yedpa.Service.Tests/BulkInvoice/AidatInvoicePayloadTests.cs
git commit -m "feat(bulk-invoice): add proven Logo salesInvoices AIDAT payload model"
```

---

## Task 2: GetPendingLinesAsync — ay parametresi

**Files:**
- Modify: `Koala.Yedpa.Core/Services/IBulkInvoiceService.cs`
- Modify: `Koala.Yedpa.Service/Services/BulkInvoiceService.cs:92-167`
- Test: `Koala.Yedpa.Service.Tests/BulkInvoice/PendingLinesMonthTests.cs`

**Interfaces:**
- Produces: `Task<ResponseDto<List<PendingInvoiceLineDto>>> GetPendingLinesAsync(string logoMonthName)` — verilen LINEEXP ay adıyla filtreler. Parametresiz overload (mevcut davranış: gelecek ay) korunur ve yeni overload'a delege eder.
- Consumes (helper): `BulkInvoiceMonths.ToLogoName(int month)` → "OCAK".."ARALIK".

- [ ] **Step 1: Failing test — ay adı yardımcısı doğru üretiyor**

```csharp
using Koala.Yedpa.Core.Helpers;
using Xunit;

public class PendingLinesMonthTests
{
    [Theory]
    [InlineData(1, "OCAK")]
    [InlineData(7, "TEMMUZ")]
    [InlineData(8, "AGUSTOS")]
    [InlineData(9, "EYLUL")]
    [InlineData(12, "ARALIK")]
    public void ToLogoName_ReturnsUppercaseAsciiMonth(int month, string expected)
        => Assert.Equal(expected, BulkInvoiceMonths.ToLogoName(month));
}
```

- [ ] **Step 2: Çalıştır, FAIL gör** — `BulkInvoiceMonths` yok.

Run: `dotnet test Koala.Yedpa.Service.Tests --filter PendingLinesMonthTests`
Expected: FAIL (derlenmez)

- [ ] **Step 3: Helper + parametreli overload yaz**

`Koala.Yedpa.Core/Helpers/BulkInvoiceMonths.cs`:
```csharp
namespace Koala.Yedpa.Core.Helpers;

public static class BulkInvoiceMonths
{
    private static readonly string[] Names =
        { "OCAK","SUBAT","MART","NISAN","MAYIS","HAZIRAN","TEMMUZ","AGUSTOS","EYLUL","EKIM","KASIM","ARALIK" };

    public static string ToLogoName(int month) => Names[month - 1];
}
```

`BulkInvoiceService.cs` — `GetPendingLinesAsync` gövdesini parametreli overload'a taşı; `hedefAyAdi` artık parametreden gelir:
```csharp
public Task<ResponseDto<List<PendingInvoiceLineDto>>> GetPendingLinesAsync()
    => GetPendingLinesAsync(BulkInvoiceMonths.ToLogoName(DateTime.Now.AddMonths(1).Month));

public async Task<ResponseDto<List<PendingInvoiceLineDto>>> GetPendingLinesAsync(string logoMonthName)
{
    // ... mevcut gövde, sadece: var hedefAyAdi = logoMonthName; satırı kullan,
    // WHERE ORL.LINEEXP = '{hedefAyAdi}' korunur.
}
```
`IBulkInvoiceService.cs`'e overload imzasını ekle.

- [ ] **Step 4: Çalıştır, PASS gör**

Run: `dotnet test Koala.Yedpa.Service.Tests --filter PendingLinesMonthTests`
Expected: PASS

- [ ] **Step 5: Build + Commit**

```bash
dotnet build Koala.Yedpa.Service/Koala.Yedpa.Service.csproj
git add -A && git commit -m "feat(bulk-invoice): parametrize GetPendingLines by Logo month name"
```

---

## Task 3: MarkLinesAsTransferredAsync — toplu TRGFLAG=1 (idempotency)

**Files:**
- Modify: `Koala.Yedpa.Core/Services/IBulkInvoiceService.cs`
- Modify: `Koala.Yedpa.Service/Services/BulkInvoiceService.cs`
- Test: `Koala.Yedpa.Service.Tests/BulkInvoice/MarkTransferredTests.cs`

**Interfaces:**
- Produces: `Task<ResponseDto<int>> MarkLinesAsTransferredAsync(IReadOnlyList<int> orflinerefs)` — `UPDATE LG_{firm}_{period}_ORFLINE SET TRGFLAG=1 WHERE LOGICALREF IN (...)`. Etkilenen satır sayısını döner. Boş liste → 0, sorgu çalıştırmaz.
- Consumes: `ISqlProvider.SqlNonQuery(string)` (yoksa Task 3a'da eklenir — bkz. not).

> **NOT:** `ISqlProvider`'da non-query metodu yoksa önce onu ekle (`int SqlNonQuery(string sql)` → `ExecuteNonQuery`). Mevcut provider'ı oku; `SqlReader` paterni neyse onu izle.

- [ ] **Step 1: Failing test — boş liste 0 döner, sorgu üretmez**

```csharp
[Fact]
public async Task MarkTransferred_EmptyList_ReturnsZero_NoSql()
{
    var sqlProvider = new Mock<ISqlProvider>();
    var svc = BulkInvoiceServiceFactory.Create(sqlProvider.Object); // test fixture
    var res = await svc.MarkLinesAsTransferredAsync(new List<int>());
    Assert.True(res.IsSuccess);
    Assert.Equal(0, res.Data);
    sqlProvider.Verify(p => p.SqlNonQuery(It.IsAny<string>()), Times.Never);
}

[Fact]
public async Task MarkTransferred_BuildsInClause_FromLogoSettings()
{
    var sqlProvider = new Mock<ISqlProvider>();
    sqlProvider.Setup(p => p.SqlNonQuery(It.IsAny<string>()))
               .Returns(ResponseDto<int>.SuccessData(200, "ok", 2));
    var svc = BulkInvoiceServiceFactory.Create(sqlProvider.Object); // Firm=211, Period=16 stub
    var res = await svc.MarkLinesAsTransferredAsync(new List<int> { 100, 200 });
    sqlProvider.Verify(p => p.SqlNonQuery(
        It.Is<string>(s => s.Contains("LG_211_16_ORFLINE")
                        && s.Contains("SET TRGFLAG=1")
                        && s.Contains("100,200"))), Times.Once);
    Assert.Equal(2, res.Data);
}
```

- [ ] **Step 2: Çalıştır, FAIL gör**

Run: `dotnet test Koala.Yedpa.Service.Tests --filter MarkTransferredTests`
Expected: FAIL

- [ ] **Step 3: Implementasyon**

```csharp
public async Task<ResponseDto<int>> MarkLinesAsTransferredAsync(IReadOnlyList<int> orflinerefs)
{
    if (orflinerefs == null || orflinerefs.Count == 0)
        return ResponseDto<int>.SuccessData(200, "Güncellenecek satır yok", 0);

    var logo = await _settingsService.GetLogoSettingsAsync();
    if (!logo.IsSuccess || logo.Data == null)
        return ResponseDto<int>.FailData(500, "Logo ayarları alınamadı", "settings null", true);

    var inClause = string.Join(",", orflinerefs); // int listesi → SQL injection riski yok
    var sql = $"UPDATE LG_{logo.Data.Firm}_{logo.Data.Period}_ORFLINE SET TRGFLAG=1 WHERE LOGICALREF IN ({inClause})";

    var res = _sqlProvider.SqlNonQuery(sql);
    if (!res.IsSuccess)
        return ResponseDto<int>.FailData(500, "TRGFLAG güncellenemedi", res.Message, true);

    return ResponseDto<int>.SuccessData(200, $"{res.Data} satır transferli işaretlendi", res.Data);
}
```

- [ ] **Step 4: Çalıştır, PASS gör**; **Step 5: Build + Commit**

```bash
dotnet build Koala.Yedpa.Service/Koala.Yedpa.Service.csproj
git add -A && git commit -m "feat(bulk-invoice): bulk TRGFLAG=1 update for transferred order lines"
```

> **DOĞRULAMA (manuel, kod dışı):** Kullanıcının bekleyen TRGFLAG re-testi sonucu "Logo otomatik 1 yapıyor" çıkarsa bu task gereksiz değildir — yine de BulkInvoiceItem asıl idempotency guard'ıdır ve TRGFLAG=1 (zaten 1'se) zararsızdır.

---

## Task 3B: LogoRestServiceProvider — token alma retry (Task 4'TEN ÖNCE)

**Neden:** LogoRestService, token (`api/v1/token`) talebine bazen geçici "kullanıcı bulunamadı" benzeri hatalarla dönüyor. Birkaç saniye bekleyip tekrar denemek gerekiyor. Bu, POST seviyesinde değil **token alma seviyesinde** çözülmeli — tüm REST çağrıları faydalanır.

**Files:**
- Modify: `Koala.Yedpa.Service/Providers/LogoRestServiceProvider.cs` (`GetAccessTokenAsync` retry sarmalı + `RequestTokenOnceAsync` ayrıştırması; test edilebilir `RetryTokenAsync` helper)
- Test: `Koala.Yedpa.Service.Tests/Providers/TokenRetryTests.cs`

**Interfaces:**
- Produces (internal, test için): `static Task<ResponseDto<string>> RetryTokenAsync(Func<Task<ResponseDto<string>>> request, int maxAttempts, Func<Task> delay)` — `request` başarılı olana kadar (veya `maxAttempts` dolana kadar) çağırır; her başarısızlıkta `delay()` bekler.

- [ ] **Step 1: Failing test — 2 başarısız sonra 1 başarılı → başarılı döner, 2 kez beklenir**

```csharp
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Service.Providers;
using Xunit;

public class TokenRetryTests
{
    [Fact]
    public async Task RetryToken_RetriesUntilSuccess()
    {
        var calls = 0; var delays = 0;
        var res = await LogoRestServiceProvider.RetryTokenAsync(() =>
        {
            calls++;
            return Task.FromResult(calls < 3
                ? ResponseDto<string>.FailData(401, "Token alınamadı", "kullanıcı bulunamadı", true)
                : ResponseDto<string>.SuccessData(200, "ok", "TOKEN123"));
        }, maxAttempts: 3, delay: () => { delays++; return Task.CompletedTask; });

        Assert.True(res.IsSuccess);
        Assert.Equal("TOKEN123", res.Data);
        Assert.Equal(3, calls);
        Assert.Equal(2, delays); // son denemeden sonra beklemez
    }

    [Fact]
    public async Task RetryToken_AllFail_ReturnsLastFailure()
    {
        var res = await LogoRestServiceProvider.RetryTokenAsync(
            () => Task.FromResult(ResponseDto<string>.FailData(401, "Token alınamadı", "kullanıcı bulunamadı", true)),
            maxAttempts: 3, delay: () => Task.CompletedTask);
        Assert.False(res.IsSuccess);
        Assert.Contains("kullanıcı bulunamadı", res.Errors?.ToString() ?? "");
    }
}
```

- [ ] **Step 2: Çalıştır, FAIL gör** — `RetryTokenAsync` yok.

Run: `dotnet test Koala.Yedpa.Service.Tests --filter TokenRetryTests`
Expected: FAIL

- [ ] **Step 3: Helper + GetAccessTokenAsync refactor**

`LogoRestServiceProvider`'a ekle:
```csharp
public static async Task<ResponseDto<string>> RetryTokenAsync(
    Func<Task<ResponseDto<string>>> request, int maxAttempts, Func<Task> delay)
{
    ResponseDto<string> last = ResponseDto<string>.FailData(500, "Token alınamadı", "deneme yok", true);
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        last = await request();
        if (last.IsSuccess) return last;
        if (attempt < maxAttempts) await delay();
    }
    return last;
}
```

`GetAccessTokenAsync` gövdesini (settings/license/http token POST) `RequestTokenOnceAsync()` adlı private metoda taşı (mevcut kod aynen). Sonra:
```csharp
private async Task<ResponseDto<string>> GetAccessTokenAsync()
    => await RetryTokenAsync(
        RequestTokenOnceAsync,
        maxAttempts: 3,
        delay: () => Task.Delay(3000));   // geçici token hatasında ~3 sn bekle
```

- [ ] **Step 4: Çalıştır, PASS gör**

Run: `dotnet test Koala.Yedpa.Service.Tests --filter TokenRetryTests`
Expected: PASS

- [ ] **Step 5: Build + Commit**

```bash
dotnet build Koala.Yedpa.Service/Koala.Yedpa.Service.csproj
git add -A && git commit -m "feat(logo-rest): retry token acquisition with backoff on transient failures"
```

> **NOT:** Bu task tamamlanınca Task 4'teki anlık POST-token-retry ikincil güvenlik katmanı olarak kalır (token alma artık provider'da retry'lı). Task 4'ün transient sınıflandırması (CanRetry) yine kuyruk-sonrası 3-tur retry için gereklidir.

---

## Task 4: BulkInvoiceTransferService — tek satır → Logo fatura

**Files:**
- Create: `Koala.Yedpa.Core/Services/IBulkInvoiceTransferService.cs`
- Create: `Koala.Yedpa.Service/Services/BulkInvoiceTransferService.cs`
- Test: `Koala.Yedpa.Service.Tests/BulkInvoice/TransferServiceTests.cs`

**Interfaces:**
- Consumes: `ILogoRestServiceProvider.HttpPost(string url, string json)`, `LogoJsonHelper.InjectDataObjectParameter`, `Tools.ConvertToLogoTime`, `AidatInvoicePayload` (Task 1).
- Produces:
  - `Task<TransferLineResult> TransferLineAsync(PendingInvoiceLineDto line, DateTime invoiceDate)`
  - `record TransferLineResult(bool Success, int Orflineref, string ClientCode, int? LogoInvoiceRef, string? InvoiceNumber, string? Error)`

- [ ] **Step 1: Failing test — başarılı POST sonucu parse edilir**

```csharp
[Fact]
public async Task TransferLine_PostsItemsWrapper_AndParsesRef()
{
    var rest = new Mock<ILogoRestServiceProvider>();
    string? sent = null;
    rest.Setup(r => r.HttpPost("salesInvoices", It.IsAny<string>()))
        .Callback<string,string>((_, j) => sent = j)
        .ReturnsAsync(ResponseDto<string>.SuccessData(200, "ok",
            "{\"INTERNAL_REFERENCE\":23828,\"NUMBER\":\"YED2026000014227\"}"));

    var svc = new BulkInvoiceTransferService(rest.Object, NullLogger<BulkInvoiceTransferService>.Instance);
    var line = new PendingInvoiceLineDto { Orflineref = 28868, ClientCode = "1.F000.090.00.11",
        Amount = 5016.70m, MonthName = "TEMMUZ" };

    var res = await svc.TransferLineAsync(line, new DateTime(2026,7,1));

    Assert.True(res.Success);
    Assert.Equal(23828, res.LogoInvoiceRef);
    Assert.Contains("\"TRANSACTIONS\":{\"items\":[", sent);
    Assert.Contains("\"DATE\":\"2026-07-01\"", sent);
    Assert.Contains("\"NOTES1\":\"Temmuz AIDAT TAHAKKUKU\"", sent);
    Assert.Contains("DataObjectParameter", sent);
}

[Fact]
public async Task TransferLine_OnRestFailure_ReturnsError()
{
    var rest = new Mock<ILogoRestServiceProvider>();
    rest.Setup(r => r.HttpPost("salesInvoices", It.IsAny<string>()))
        .ReturnsAsync(ResponseDto<string>.FailData(500, "Logo hata", "ARP_CODE bulunamadı", true));
    var svc = new BulkInvoiceTransferService(rest.Object, NullLogger<BulkInvoiceTransferService>.Instance);
    var res = await svc.TransferLineAsync(
        new PendingInvoiceLineDto { Orflineref = 1, ClientCode = "X", Amount = 1m, MonthName = "TEMMUZ" },
        new DateTime(2026,7,1));
    Assert.False(res.Success);
    Assert.Contains("ARP_CODE", res.RestError);
}
```

- [ ] **Step 2: Çalıştır, FAIL gör**

Run: `dotnet test Koala.Yedpa.Service.Tests --filter TransferServiceTests`
Expected: FAIL

- [ ] **Step 3: Implementasyon**

`IBulkInvoiceTransferService.cs`:
```csharp
using Koala.Yedpa.Core.Dtos.BulkInvoice;
namespace Koala.Yedpa.Core.Services;

public record TransferLineResult(bool Success, int Orflineref, string ClientCode,
    int? LogoInvoiceRef, string? InvoiceNumber, string? Note, string? RestError, bool IsTransient);

public interface IBulkInvoiceTransferService
{
    Task<TransferLineResult> TransferLineAsync(
        Koala.Yedpa.Core.Dtos.BulkInvoice.PendingInvoiceLineDto line, DateTime invoiceDate);
}
```

`BulkInvoiceTransferService.cs`:
```csharp
using System.Globalization;
using System.Text.Json;
using Koala.Yedpa.Core.Dtos.BulkInvoice;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services;

public class BulkInvoiceTransferService : IBulkInvoiceTransferService
{
    private readonly ILogoRestServiceProvider _rest;
    private readonly ILogger<BulkInvoiceTransferService> _logger;

    public BulkInvoiceTransferService(ILogoRestServiceProvider rest,
        ILogger<BulkInvoiceTransferService> logger)
    { _rest = rest; _logger = logger; }

    public async Task<TransferLineResult> TransferLineAsync(PendingInvoiceLineDto line, DateTime invoiceDate)
    {
        try
        {
            // Ay adının baş harfi büyük: "Temmuz AIDAT TAHAKKUKU"
            var ayTitle = CultureInfo.GetCultureInfo("tr-TR").TextInfo
                .ToTitleCase(line.MonthName.ToLower(CultureInfo.GetCultureInfo("tr-TR")));

            var payload = new AidatInvoicePayload
            {
                ArpCode = line.ClientCode,
                Date = invoiceDate.ToString("yyyy-MM-dd"),
                Time = invoiceDate.ConvertToLogoTime(),
                Notes1 = $"{ayTitle} AIDAT TAHAKKUKU",
                Transactions = new AidatInvoiceTransactions
                {
                    Items = { new AidatInvoiceTransaction
                    {
                        Price = line.Amount.ToString(CultureInfo.InvariantCulture),
                        Description = $"{ayTitle} AIDAT"
                    } }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            json = LogoJsonHelper.InjectDataObjectParameter(json);

            // Anlık token-retry: token hatasında birkaç saniye bekle, POST'u tekrar dene (en fazla 2 retry).
            ResponseDto<string>? resp = null;
            for (var attempt = 0; attempt < 3; attempt++)
            {
                resp = await _rest.HttpPost("salesInvoices", json);
                if (resp.IsSuccess || !IsTokenError(resp)) break;
                await Task.Delay(3000);
            }

            if (resp == null || !resp.IsSuccess)
            {
                var transient = resp != null && IsTokenError(resp);
                var raw = resp?.Errors?.ToString() ?? resp?.Message ?? "Bilinmeyen hata";
                return new TransferLineResult(false, line.Orflineref, line.ClientCode, null, null,
                    transient ? "Token alınamadı (geçici)" : "REST iş hatası", raw, transient);
            }

            using var doc = JsonDocument.Parse(resp.Data);
            int? refId = doc.RootElement.TryGetProperty("INTERNAL_REFERENCE", out var r) ? r.GetInt32() : null;
            string? no = doc.RootElement.TryGetProperty("NUMBER", out var n) ? n.GetString() : null;
            return new TransferLineResult(true, line.Orflineref, line.ClientCode, refId, no, null, null, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatura aktarım hatası. Cari {Code}", line.ClientCode);
            return new TransferLineResult(false, line.Orflineref, line.ClientCode, null, null,
                "İstisna", ex.Message, true);
        }
    }

    private static bool IsTokenError(ResponseDto<string> r)
        => r.StatusCode == 401
        || (r.Message?.Contains("token", StringComparison.OrdinalIgnoreCase) ?? false)
        || (r.Errors?.ToString()?.Contains("token", StringComparison.OrdinalIgnoreCase) ?? false);
}
```

- [ ] **Step 4: Çalıştır, PASS gör**; **Step 5: Build + Commit**

```bash
dotnet test Koala.Yedpa.Service.Tests --filter TransferServiceTests
git add -A && git commit -m "feat(bulk-invoice): per-line Logo REST invoice transfer service"
```

---

## Task 5: Session job id + Crosstable (BulkInvoiceItem) genişletme + migration

**Files:**
- Modify: `Koala.Yedpa.Core/Models/BulkInvoiceSession.cs` (InfoJobId, TransferJobId string?)
- Modify: `Koala.Yedpa.Core/Models/BulkInvoiceItem.cs` (RetryCount, CanRetry, Note, RestError)
- Create: migration `AddBulkInvoiceJobIdsAndRetry`
- Test: yok (şema değişikliği); doğrulama `dotnet ef migrations` + build.

- [ ] **Step 1: Session'a alan ekle**

```csharp
[StringLength(100)] public string? InfoJobId { get; set; }
[StringLength(100)] public string? TransferJobId { get; set; }
```

- [ ] **Step 1b: BulkInvoiceItem'a takip/retry alanları ekle**

```csharp
public int RetryCount { get; set; } = 0;                    // kaç kez denendi
public bool CanRetry { get; set; } = false;                 // "Tekrar dene" (token/geçici hatada true)
[StringLength(500)]  public string? Note { get; set; }       // neden aktarılmadı
[StringLength(2000)] public string? RestError { get; set; }  // REST'ten dönen ham hata
```

- [ ] **Step 2: Migration üret**

Run: `dotnet ef migrations add AddBulkInvoiceJobIdsAndRetry -p Koala.Yedpa.Repositories -s Koala.Yedpa.WebUI`
Expected: yeni migration; `InfoJobId`, `TransferJobId`, `RetryCount`, `CanRetry`, `Note`, `RestError` AddColumn içerir.

- [ ] **Step 3: Build + (gerekirse) DB güncelle**

Run: `dotnet build Koala.Yedpa.WebUI/Koala.Yedpa.WebUI.csproj`
Expected: 0 error. (DB update prod'da kontrollü uygulanır.)

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat(bulk-invoice): persist Hangfire job ids on session"
```

---

## Task 6: Hangfire job'ları + session create'te zamanlama

**Files:**
- Create: `Koala.Yedpa.Service/Services/BulkInvoiceJobs.cs`
- Modify: `BulkInvoiceService.CreateSessionAsync` (job'ları zamanla, id'leri kaydet)
- Delete: `Koala.Yedpa.Service/Services/BackgroundServices/BulkInvoiceTransferBackgroundService.cs`
- Test: `Koala.Yedpa.Service.Tests/BulkInvoice/TransferJobTests.cs`

**Interfaces:**
- Produces: `BulkInvoiceJobs.RunTransferAsync(int sessionId)`, `BulkInvoiceJobs.SendInfoMailAsync(int sessionId)`.
- `RunTransferAsync` akışı: session'ı yükle → `GetPendingLinesAsync(session.MonthName)` → her satır `TransferLineAsync` → başarılıları `BulkInvoiceItem` (Transferred) + `MarkLinesAsTransferredAsync` → başarısızları (Failed, ErrorMessage) → session.Status=Completed → rapor maili.

> **NOT:** Session'a `MonthName` türetimi: `BulkInvoiceMonths.ToLogoName(session.Month)`.

- [ ] **Step 1: Failing test — transfer job başarılı/başarısızı ayırır ve TRGFLAG günceller**

```csharp
[Fact]
public async Task RunTransfer_MarksSuccessTransferred_AndUpdatesTrgflag()
{
    // pending: 2 satır; transfer: 1 başarılı (orfline=100), 1 başarısız (orfline=200)
    var bulk = new Mock<IBulkInvoiceService>();
    bulk.Setup(b => b.GetPendingLinesAsync("TEMMUZ")).ReturnsAsync(
        ResponseDto<List<PendingInvoiceLineDto>>.SuccessData(200, "ok", new()
        {
            new() { Orflineref = 100, ClientCode = "A", Amount = 10m, MonthName = "TEMMUZ" },
            new() { Orflineref = 200, ClientCode = "B", Amount = 20m, MonthName = "TEMMUZ" },
        }));
    var transfer = new Mock<IBulkInvoiceTransferService>();
    transfer.Setup(t => t.TransferLineAsync(It.Is<PendingInvoiceLineDto>(l => l.Orflineref==100), It.IsAny<DateTime>()))
            .ReturnsAsync(new TransferLineResult(true, 100, "A", 999, "YED1", null, null, false));
    transfer.Setup(t => t.TransferLineAsync(It.Is<PendingInvoiceLineDto>(l => l.Orflineref==200), It.IsAny<DateTime>()))
            .ReturnsAsync(new TransferLineResult(false, 200, "B", null, null, "REST iş hatası", "Logo hata", false));

    // ... in-memory AppDbContext + session(Month=7) fixture ...
    var jobs = TransferJobFactory.Create(bulk.Object, transfer.Object, /*markVerifier*/ out var bulkMock);
    await jobs.RunTransferAsync(sessionId);

    bulk.Verify(b => b.MarkLinesAsTransferredAsync(
        It.Is<IReadOnlyList<int>>(ids => ids.Count==1 && ids[0]==100)), Times.Once);
    // session.Status == Completed; 1 Transferred + 1 Failed item
}
```

- [ ] **Step 2: Çalıştır, FAIL gör**

Run: `dotnet test Koala.Yedpa.Service.Tests --filter TransferJobTests`
Expected: FAIL

- [ ] **Step 3: BulkInvoiceJobs implementasyonu**

```csharp
public class BulkInvoiceJobs
{
    private readonly AppDbContext _db;
    private readonly IBulkInvoiceService _bulk;
    private readonly IBulkInvoiceTransferService _transfer;
    private readonly IBulkInvoiceEmailService _email;
    private readonly ILogger<BulkInvoiceJobs> _logger;
    // ctor inject

    public async Task RunTransferAsync(int sessionId)
    {
        var session = await _db.BulkInvoiceSessions.FindAsync(sessionId);
        if (session == null) { _logger.LogWarning("Session yok: {Id}", sessionId); return; }
        session.Status = BulkInvoiceSessionStatus.Processing;
        await _db.SaveChangesAsync();

        // 1) Bekleyenleri çek
        var monthName = BulkInvoiceMonths.ToLogoName(session.Month);
        var pending = await _bulk.GetPendingLinesAsync(monthName);
        var lines = pending.Data ?? new();

        // 2) Tümünü ÖNCE "gönderilmedi" yaz (kalıcılık — elektrik kesilse bile kayıt durur)
        foreach (var line in lines)
        {
            _db.BulkInvoiceItems.Add(new BulkInvoiceItem
            {
                SessionId = session.Id, OrficheRef = line.OrficheRef, Orflineref = line.Orflineref,
                ClientCode = line.ClientCode, ClientName = line.ClientName, Amount = line.Amount,
                MonthName = line.MonthName, Status = BulkInvoiceItemStatus.Pending
            });
        }
        await _db.SaveChangesAsync();
        var items = await _db.BulkInvoiceItems.Where(i => i.SessionId == session.Id).ToListAsync();

        // 3) İlk geçiş — her sonucu ANINDA kaydet
        foreach (var item in items)
            await TryTransferItemAsync(item, session.InvoiceDate, lines);

        // 4) Kuyruk sonrası retry — CanRetry & RetryCount<3 olanlar, 3 tura kadar
        for (var round = 0; round < 3; round++)
        {
            var retryables = items.Where(i =>
                i.Status == BulkInvoiceItemStatus.Failed && i.CanRetry && i.RetryCount < 3).ToList();
            if (retryables.Count == 0) break;
            await Task.Delay(5000);
            foreach (var item in retryables)
                await TryTransferItemAsync(item, session.InvoiceDate, lines);
        }

        // 3 deneme sonrası hâlâ başarısız → artık denenmez
        foreach (var item in items.Where(i =>
            i.Status == BulkInvoiceItemStatus.Failed && i.RetryCount >= 3))
        {
            item.CanRetry = false;
            item.Note = "3 deneme sonrası aktarılamadı";
        }
        await _db.SaveChangesAsync();

        // 5) Başarılı satırlar → TRGFLAG=1
        var transferredRefs = items.Where(i => i.Status == BulkInvoiceItemStatus.Transferred)
                                   .Select(i => i.Orflineref).ToList();
        await _bulk.MarkLinesAsTransferredAsync(transferredRefs);

        session.Status = BulkInvoiceSessionStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // 6) Rapor maili
        await _email.SendReportMailAsync(session.Id);
    }

    // Tek item'ı dener, sonucu o satıra yazar ve ANINDA persist eder (kalıcılık).
    private async Task TryTransferItemAsync(BulkInvoiceItem item, DateTime invoiceDate,
        List<PendingInvoiceLineDto> lines)
    {
        var line = lines.First(l => l.Orflineref == item.Orflineref);
        var r = await _transfer.TransferLineAsync(line, invoiceDate);
        item.RetryCount++;
        if (r.Success)
        {
            item.Status = BulkInvoiceItemStatus.Transferred;
            item.LogoInvoiceRef = r.LogoInvoiceRef;
            item.Note = null; item.RestError = null; item.CanRetry = false;
        }
        else
        {
            item.Status = BulkInvoiceItemStatus.Failed;
            item.Note = r.Note; item.RestError = r.RestError; item.CanRetry = r.IsTransient;
        }
        await _db.SaveChangesAsync(); // per-item kalıcılık (cache'te bekletme YOK)
    }

    public Task SendInfoMailAsync(int sessionId) => _email.SendInfoMailAsync(sessionId); // Task 7
}
// NOT: ToListAsync için `using Microsoft.EntityFrameworkCore;` gerekir.
```

`CreateSessionAsync` sonuna (session kaydedildikten sonra) zamanlama:
```csharp
var infoAt = session.InvoiceDate.Date.AddDays(-1).AddHours(8);    // T-1 08:00
var transferAt = session.InvoiceDate.Date.AddMinutes(1);          // T 00:01
session.InfoJobId = BackgroundJob.Schedule<BulkInvoiceJobs>(j => j.SendInfoMailAsync(session.Id), infoAt);
session.TransferJobId = BackgroundJob.Schedule<BulkInvoiceJobs>(j => j.RunTransferAsync(session.Id), transferAt);
await _context.SaveChangesAsync();
```
`BulkInvoiceTransferBackgroundService.cs` dosyasını sil.

- [ ] **Step 4: Çalıştır, PASS gör**; **Step 5: Build + Commit**

```bash
dotnet test Koala.Yedpa.Service.Tests --filter TransferJobTests
dotnet build Koala.Yedpa.Service/Koala.Yedpa.Service.csproj
git add -A && git commit -m "feat(bulk-invoice): Hangfire transfer/info jobs + scheduling on session create"
```

---

## Task 7: Excel + mailler (bilgi + rapor)

**Files:**
- Create: `Koala.Yedpa.Core/Services/IBulkInvoiceExcelService.cs` + `Koala.Yedpa.Service/Services/BulkInvoiceExcelService.cs`
- Rewrite: `Koala.Yedpa.Core/Services/IBulkInvoiceEmailService.cs` + `Koala.Yedpa.Service/Services/BulkInvoiceEmailService.cs`
- Settings: `SettingsTypeEnum.BulkInvoiceMailRecipients` (virgülle ayrılmış adresler)
- Test: `Koala.Yedpa.Service.Tests/BulkInvoice/ExcelServiceTests.cs`

**Interfaces:**
- Produces:
  - `byte[] BuildPreviewExcel(IReadOnlyList<PendingInvoiceLineDto> lines)` — kolonlar: Cari Kod, Cari Ad, Ay, Tutar.
  - `Task SendInfoMailAsync(int sessionId)` — session ayına ait bekleyen satırların Excel'ini ekler, ayar listesine gönderir.
  - `Task SendReportMailAsync(int sessionId)` — BulkInvoiceItem'lardan başarılı/başarısız özeti; başarısızlar tablo (ClientCode + LogoInvoiceRef/fiş no + ErrorMessage).

- [ ] **Step 1: Failing test — Excel başlık + satır sayısı**

```csharp
[Fact]
public void BuildPreviewExcel_HasHeaderAndRows()
{
    var svc = new BulkInvoiceExcelService();
    var bytes = svc.BuildPreviewExcel(new List<PendingInvoiceLineDto>
    {
        new() { ClientCode="A", ClientName="Cari A", MonthName="TEMMUZ", Amount=100m },
    });
    using var wb = new ClosedXML.Excel.XLWorkbook(new MemoryStream(bytes));
    var ws = wb.Worksheet(1);
    Assert.Equal("Cari Kod", ws.Cell(1,1).GetString());
    Assert.Equal("A", ws.Cell(2,1).GetString());
    Assert.Equal(100d, ws.Cell(2,4).GetDouble());
}
```

- [ ] **Step 2: Çalıştır, FAIL gör**

Run: `dotnet test Koala.Yedpa.Service.Tests --filter ExcelServiceTests`
Expected: FAIL

- [ ] **Step 3: Excel servisi**

```csharp
using ClosedXML.Excel;
using Koala.Yedpa.Core.Dtos.BulkInvoice;

public class BulkInvoiceExcelService : IBulkInvoiceExcelService
{
    public byte[] BuildPreviewExcel(IReadOnlyList<PendingInvoiceLineDto> lines)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("AIDAT Faturalar");
        ws.Cell(1,1).Value = "Cari Kod"; ws.Cell(1,2).Value = "Cari Ad";
        ws.Cell(1,3).Value = "Ay"; ws.Cell(1,4).Value = "Tutar";
        ws.Range(1,1,1,4).Style.Font.Bold = true;
        var row = 2;
        foreach (var l in lines)
        {
            ws.Cell(row,1).Value = l.ClientCode; ws.Cell(row,2).Value = l.ClientName;
            ws.Cell(row,3).Value = l.MonthName;  ws.Cell(row,4).Value = l.Amount;
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream(); wb.SaveAs(ms); return ms.ToArray();
    }
}
```

- [ ] **Step 4: Çalıştır, PASS gör**

- [ ] **Step 5: Email servisi (bilgi + rapor)** — `IEmailService` ile gönder. Alıcılar ŞİMDİLİK sabit (ileride ayardan):

```csharp
// BulkInvoiceEmailService içinde sabit alıcı listesi (ileride ISettingsService'e taşınacak)
private static readonly string[] Recipients =
    { "erkan@sistem-bilgisayar.com.tr", "adegimli@yedpa.com.tr", "muhasebe@yedpa.com.tr" };
```
`SendInfoMailAsync`: `GetPendingLinesAsync(month)` → `BuildPreviewExcel` → Excel ek olarak `Recipients`'a gönder. `SendReportMailAsync`: `_db.BulkInvoiceItems.Where(i=>i.SessionId==id)` → başarılı/başarısız say, başarısızları HTML tablo (**ClientCode, fiş no/LogoInvoiceRef, Note, RestError**) → `Recipients`'a gönder. (Mevcut `EmailService` imzasını oku, attachment API'sine göre yaz.)

- [ ] **Step 6: Build + Commit**

```bash
dotnet test Koala.Yedpa.Service.Tests --filter ExcelServiceTests
dotnet build Koala.Yedpa.Service/Koala.Yedpa.Service.csproj
git add -A && git commit -m "feat(bulk-invoice): preview Excel + info/report mails"
```

---

## Task 8: DI kayıtları + UI tarih seçimi doğrulaması

**Files:**
- Modify: `Koala.Yedpa.Service/Extentions/ServiceCollectionExtensions.cs`
- Modify (gerekirse): `wwwroot/js/dashboard/bulk-invoice.js`, modal (tarih zorunlu, gelecek ay default)
- Test: manuel + build

- [ ] **Step 1: DI kayıtları**

```csharp
services.AddScoped<IBulkInvoiceTransferService, BulkInvoiceTransferService>();
services.AddScoped<IBulkInvoiceExcelService, BulkInvoiceExcelService>();
services.AddScoped<IBulkInvoiceEmailService, BulkInvoiceEmailService>();
services.AddScoped<BulkInvoiceJobs>();
```

- [ ] **Step 2: Build (tüm çözüm)**

Run: `dotnet build Koala.Yedpa.sln`
Expected: 0 error.

- [ ] **Step 3: Tüm testler**

Run: `dotnet test Koala.Yedpa.Service.Tests`
Expected: tüm testler PASS.

- [ ] **Step 4: Manuel doğrulama (verify skill)** — WebUI çalıştır, dashboard modalında tarih seç → session oluştur → Hangfire dashboard'da 2 scheduled job göründüğünü doğrula (`/hangfire`).

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(bulk-invoice): wire DI + finalize scheduling UI"
```

---

## Self-Review Notları (plan yazarı tarafından)

- **Spec coverage:** T-1 08:00 bilgi+Excel (Task 6 schedule + Task 7 mail) ✓; T 00:01 aktarım (Task 6) ✓; rapor maili başarılı/başarısız+cari+fiş no (Task 7) ✓; idempotency TRGFLAG (Task 3) ✓; her cari ayrı fatura (Task 4 satır-bazlı) ✓; LINEEXP ay (Task 2) ✓; kanıtlı payload items+PAYMENT_CODE (Task 1) ✓.
- **Açık bağımlılık:** `ISqlProvider.SqlNonQuery` mevcut değilse Task 3'ten önce eklenmeli (provider'ı oku). `IEmailService` attachment imzası Task 7'de okunup uyarlanmalı. `Mock`/`AppDbContext in-memory` test fixture'ları test projesinin mevcut paternine göre yazılmalı (önce bir mevcut test dosyasını incele).
- **Doğrulanacak (kod dışı, kullanıcı):** TRGFLAG re-test sonucu; `PAYMENT_CODE="10-3"` ve `MASTER_CODE="600.11.0001"` tüm carilerde geçerli kabul edildi.
