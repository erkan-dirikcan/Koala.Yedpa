# Dashboard Design Spec

Drag-and-drop ozellestirilebilir dashboard. GridStack.js ile widget grid sistemi, Chart.js ile grafikler, claim bazli yetkilendirme ile widget gorunurlugu.

## Mimari

**GridStack.js** (drag-and-drop grid) + **Chart.js** (grafikler) + **AJAX** (veri yukleme).

Her widget:
1. Bir partial view (`_WidgetXxx.cshtml`)
2. Bir claim gereksinimi (sunucu tarafinda kontrol edilir)
3. Kendi API endpoint'ine AJAX ile baglanir
4. Kullanici pozisyonunu DB'ye kaydeder

Kullanici tercihleri `DashboardWidgetPreference` tablosunda saklanir. Ilk yuklemede default layout kullanilir.

### Veri Akisi

```
[DashboardController.Index]
  → Claim bazli widget listesini olusturur
  → Kullanici tercihlerini DB'den okur (varsa)
  → GridStack container'i partial view'lar ile render eder
  → Her widget sayfa yuklendikten sonra AJAX ile kendi API'sine baglanir

[Kullanici pozisyon degistirir]
  → GridStack `change` eventi → AJAX POST /Dashboard/SaveLayout
  → DB'ye yeni pozisyonlar kaydedilir
```

## Widget Envantteri

### Finansal Widget'lar (Claim: `CurrentAccuant`)

| Id | Widget | Aciklama | Veri Kaynagi | Gorsellastirme |
|----|--------|----------|-------------|----------------|
| W1 | Toplam Bakiye Ozeti | Toplam alacak/borç/net bakiye KPI kartlari | `GET /api/LogoClCardApi/CustomerListWithBalance` | 3 KPI kart |
| W2 | Bekleyen Faturalar | Son 10 bekleyen fatura + toplam tutar | `GET /api/LogoClCardApi/PendingInvoices` | Mini tablo + KPI |
| W3 | Vadesi Gecen Odemeler | Vadesi gecmis faturalar, kirmizi uyari | `GET /api/LogoClCardApi/PendingInvoicesSearch` | Uyari kart + tutar |
| W4 | Cari Bakiye Dagilimi | En cok borclu 10 cari | `GET /api/LogoClCardApi/CustomerListWithBalance` | Bar chart |
| W5 | Aylik Tahsilat Trendi | Son 12 ay tahsilat grafigi | `GET /api/LogoClCardApi/ClCardStatementDetailed` | Line chart |
| W6 | Son Islemler | Son 10 cari hareket | `GET /api/LogoClCardApi/ClCardStatementDetailed` | Liste |

### Butce/Aidat Widget'lari (Claim: `BudgetManagement`)

| Id | Widget | Aciklama | Veri Kaynagi | Gorsellastirme |
|----|--------|----------|-------------|----------------|
| W7 | Aidat Tahsilat Orani | Odendi/acik aidat orani | `GET /api/DuesStatisticApi/GetMonthlyBudgetSummary` | Doughnut chart |
| W8 | Aylik Butce Ozeti | Aylik butce KPI'lari | `GET /api/DuesStatisticApi/GetMonthlyBudgetSummary` | KPI kartlari |
| W9 | Yillik Butce Karsilastirma | Yillara gore butce karsilastirmasi | `GET /api/DuesStatisticApi/GetByYearAndType` | Grouped bar chart |

### Operasyonel Widget'lar (Claim: `Management`)

| Id | Widget | Aciklama | Veri Kaynagi | Gorsellastirme |
|----|--------|----------|-------------|----------------|
| W10 | Aktif Dukkan Sayisi | Toplam aktif dukkan sayisi | `GET /api/LogoClCardApi/ClCardInfoAll` | KPI kart |

## Default Layout

```
[W1: KPI Kartlari - tam genislik (12 sutun)]
[W4: Bakiye Dagilimi (6 sutun)] [W7: Aidat Orani (6 sutun)]
[W2: Bekleyen Faturalar (6 sutun)] [W5: Tahsilat Trendi (6 sutun)]
[W8: Butce Ozeti - tam genislik (12 sutun)]
```

Gizli widget'lar (kullanici geri getirebilir): W3, W6, W9, W10.

## Veritabani

### DashboardWidgetPreference Tablosu

| Kolon | Tip | Aciklama |
|-------|-----|----------|
| Id | int (PK) | Auto-increment |
| UserId | string (FK) | AppUser Id |
| WidgetId | string | Widget tanimlayici (W1, W2...) |
| GridX | int | Grid sutun pozisyonu |
| GridY | int | Grid satir pozisyonu |
| Width | int | Grid genisligi (1-12) |
| Height | int | Grid yuksekligi |
| Visible | bit | Widget gorunur mu |

Entity: `DashboardWidgetPreference` in `Koala.Yedpa.Core/Entities/`
DbSet: `AppDbContext.DashboardWidgetPreferences`

## Dosya Yapisi

```
Views/Dashboard/
  Index.cshtml                         → GridStack container + widget slot'lari
  _WidgetBalanceSummary.cshtml         → W1
  _WidgetPendingInvoices.cshtml        → W2
  _WidgetOverduePayments.cshtml        → W3
  _WidgetBalanceDistribution.cshtml    → W4
  _WidgetMonthlyTrend.cshtml           → W5
  _WidgetRecentTransactions.cshtml     → W6
  _WidgetDuesCollection.cshtml         → W7
  _WidgetMonthlyBudget.cshtml          → W8
  _WidgetYearlyBudget.cshtml           → W9
  _WidgetShopCount.cshtml              → W10

wwwroot/js/dashboard/
  dashboard.js                         → GridStack init, layout save/load, widget sidebar
  widgets.js                           → Widget AJAX loader, Chart.js init

wwwroot/css/dashboard/
  dashboard.css                        → Dashboard-specific styles

Controllers/DashboardController.cs     → Index (claim-filtered widgets), SaveLayout, ResetLayout

Koala.Yedpa.Core/Entities/DashboardWidgetPreference.cs
Koala.Yedpa.Repositories/DashboardWidgetPreferenceRepository.cs (if needed)
```

## Claim Bazli Yetkilendirme

`DashboardController.Index` action'unda:
1. Tum kayitli widget tanimlarini al
2. Her widget'in gerekli claim'ini kontrol et (`User.HasClaim()`)
3. Kullaniciya sadece yetkili oldugu widget'lari gonder
4. Claim'e sahip olmayan widget'lar hic render edilmez

Widget claim eslemesi (hardcoded dictionary veya enum):
```csharp
new Dictionary<string, string>
{
    { "W1", "CurrentAccuant" },
    { "W2", "CurrentAccuant" },
    { "W3", "CurrentAccuant" },
    { "W4", "CurrentAccuant" },
    { "W5", "CurrentAccuant" },
    { "W6", "CurrentAccuant" },
    { "W7", "BudgetManagement" },
    { "W8", "BudgetManagement" },
    { "W9", "BudgetManagement" },
    { "W10", "Management" }
};
```

## GridStack Yapilandirmasi

- 12 sutunlu grid
- Min yukseklik: 2 birim
- Min genislik: 3 birim (KPI kartlari icin 12)
- Drag-and-drop aktif
- Resize aktif
- `change` event'inde pozisyonlari otomatik kaydet (debounce ile)

## Chart.js Yapilandirmasi

- Responsive (container'a uyum saglar)
- Animasyonlar acik
- Renk paleti: Tema renkleri (Primary #3699FF, Success #1BC5BD, Warning #FFA800, Danger #F64E60)
- Legend pozisyonu: alt
- Tooltip: aciklama ile

## Kullanici Islevleri

1. **Widget surukle-birak**: Pozisyon degistirme
2. **Widget boyutlandirma**: Grid uzerinden resize
3. **Widget gizle/goster**: Sidebar'dan widget secimi (toggle)
4. **Layout sifirla**: Varsayilan layout'a donme butonu
5. **Otomatik kaydetme**: Her degisiklikte DB'ye kaydetme

## Hata Yonetimi

- Widget AJAX hatasi: Widget icinde hata mesaji goster, diger widget'lari etkileme
- API timeout: 10 saniye timeout, loading spinner
- DB偏好 hatasi: Default layout'a fallback
- Token suresi dolma: 401 → login sayfasina yonlendirme

## Guvenlik

- Tum dashboard endpoint'leri `[Authorize]`
- Widget claim kontrolu sunucu tarafinda (client-side bypass'a karsi)
- API token yenileme mekanizmasi
