# Koala.Yedpa Proje Talimatları

## Takım Koordinasyonu

Bu projede uzman agent'lardan oluşan bir takım tanımlıdır. Görev geldiğinde ilgili uzman teammate'e **DOĞRUDAN** delege edilir; ara koordinasyon katmanı yoktur. Orkestrasyonu (delegasyon, bekleme, doğrulama, raporlama) ana asistan yürütür.

### Akış

1. **Görev analiz et** — Kullanıcının isteğini anlamlandır ve kapsamını belirle
2. **Doğrudan delege et** — `Agent` aracı ile ilgili teammate'in `subagent_type`'ini kullanarak görevi ilet (Türkçe, açıklayıcı, dosya yollarını içeren prompt). Bağımsız işler için birden fazla teammate'i paralel başlat.
3. **Bekle ve doğrula** — Teammate(ler) bitene kadar bekle; sonucu `dotnet build` / `dotnet test` / `git` ile DOĞRULA (teammate'in "bitti" demesine güvenme).
4. **Raporla** — Yalnızca build/test yeşil olduğunda sonucu kullanıcıya özetle.

### Takım Üyeleri

| Teammate | Subagent Type | Uzmanlık Alanı |
|----------|--------------|----------------|
| **natasa** | `natasa` | Veritabanı, EF Core, migration, MSSQL |
| **olga** | `olga` | Service layer, DTO, iş mantığı, hesaplamalar |
| **nastya** | `nastya` | Logo REST API, Message34, Hangfire, WebAPI |
| **mahmut** | `mahmut` | Metronic tema, Razor Views, DataTables, JS |
| **gonca** | `gonca` | Unit test, entegrasyon testi, coverage |
| **katya** | `katya` | (Opsiyonel) Koordinatör — yalnızca çok-teammate'li, uzun süren büyük paralel işlerde |

### Görev Türüne Göre Yönlendirme

| Görev Türü | Teammate |
|-------------|----------|
| Entity, migration, repository, DB sorgu | natasa |
| Service, DTO, iş mantığı, hesaplama | olga |
| API endpoint, Logo, Message34, Hangfire | nastya |
| View, JS, CSS, DataTables, Chart.js | mahmut |
| Test yazma, test coverage, bug tespiti | gonca |
| Birden fazla katman | Ana asistan koordine eder; ilgili teammate'lere paralel delege eder |

### Önemli Kurallar

- **Basit sorular** (tek satırlık açıklama, dosya okuma) için takımı kullanma — direkt yanıtla
- **Tek/iki dosyalık basit düzeltmeler** için takımı kullanma — direkt yap
- **Kod yazma gerektiren çok katmanlı görevlerde** ilgili uzman teammate'lere **doğrudan** delege et; orkestrasyonu (bekleme + build/test doğrulama + raporlama) ana asistan yürütür
- **Katya opsiyoneldir**: çok sayıda teammate'in uzun süre paralel çalıştığı, bağlam tamponlaması gereken büyük işlerde koordinatör olarak çağrılabilir — varsayılan değildir
- "Tamamlandı" demeden önce mutlaka `dotnet build` (0 error) + ilgili `dotnet test` (yeşil) çalıştır
- Delegasyon prompt'unu Türkçe, açıklayıcı ve dosya yollarını içerecek şekilde yaz

## Proje Yapisi

```
Koala.Yedpa.Core/       → Entity, DTO, Interface
Koala.Yedpa.Repositories→ EF Core, AppDbContext, Migrations
Koala.Yedpa.Service/    → Service implementasyonlari
Koala.Yedpa.WebApi/     → REST API endpoints
Koala.Yedpa.WebUI/      → ASP.NET MVC + Metronic Theme
```

## Teknik Bilgiler

- **Framework:** ASP.NET Core 10.0
- **ORM:** EF Core (Code-First)
- **DB:** MSSQL
- **Theme:** Metronic 7 → **Bootstrap 4.6** (`wwwroot/assets/css/style.bundle.css`)
- **JS:** jQuery, DataTables, Chart.js

### ⚠️ Bootstrap 4 kuralı (Metronic 7)

Bootstrap 5 / Metronic 8 sözdizimi bu temada **sessizce çalışmaz** — hata vermez, sadece bozuk görünür
veya kapanmayan modal/alert üretir. Yeni view/JS yazarken **daima BS4 karşılığını** kullan:

| Yazma (BS5/M8) ❌ | Kullan (BS4/M7) ✅ |
|---|---|
| `data-bs-dismiss` / `data-bs-toggle` / `data-bs-target` / `data-bs-backdrop` | `data-dismiss` / `data-toggle` / `data-target` / `data-backdrop` |
| `<button class="btn-close">` | `<button class="close"><span aria-hidden="true">&times;</span></button>` |
| `text-end` / `text-start` | `text-right` / `text-left` |
| `me-2` / `ms-2` | `mr-2` / `ml-2` |
| `fw-bold` / `fw-bolder` | `font-weight-bold` / `font-weight-bolder` |
| `fs-6` / `fs-5` / `fs-4` / `fs-3` | `font-size-base` / `font-size-h5` / `font-size-h3` / `font-size-h2` |
| `gap-2` | üst öğeye değil, çocuklara `mr-2` |
| `badge badge-light-success` | `label label-light-success label-inline` |
| `text-gray-800` | `text-dark` |

Şüphedeyken sınıfın gerçekten var olduğunu doğrula:
`grep -c "font-size-h3" Koala.Yedpa.WebUI/wwwroot/assets/css/style.bundle.css`
