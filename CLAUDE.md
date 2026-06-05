# Koala.Yedpa Proje Talimatları

## Otomatik Takım Koordinasyonu

Bu projede 6 kisilik bir agent team tanimlidir. Kullanici bir gorev verdiginde asagidaki akisi izle:

### Akis

1. **Gorev analiz et** — Kullanicinin istegini anlamlandir ve kapsamini belirle
2. **Katya'ya (team-lead) delegasyon** — `Agent` araci ile `katya` subagent_type'ini kullanarak gorevi ilet
3. **Katya gorevi dagitir** — Katya uygun teammate'(lere) SendMessage ile ulasir
4. **Sonucu raporla** — Katya'dan gelen sonucu kullaniciya ozetle

### Takim Uyeleri

| Teammate | Subagent Type | Uzmanlik Alani |
|----------|--------------|----------------|
| **katya** | `katya` | Takim lideri — gorev dagitimi, koordinasyon |
| **natasa** | `natasa` | Veritabani, EF Core, migration, MSSQL |
| **olga** | `olga` | Service layer, DTO, is mantigi, hesaplamalar |
| **nastya** | `nastya` | Logo REST API, Message34, Hangfire, WebAPI |
| **mahmut** | `mahmut` | Metronic tema, Razor Views, DataTables, JS |
| **gonca** | `gonca` | Unit test, entegrasyon testi, coverage |

### Gorev Turune Gore Yonlendirme

| Gorev Turu | Teammate |
|-------------|----------|
| Entity, migration, repository, DB sorgu | natasa |
| Service, DTO, is mantigi, hesaplama | olga |
| API endpoint, Logo, Message34, Hangfire | nastya |
| View, JS, CSS, DataTables, Chart.js | mahmut |
| Test yazma, test coverage, bug tespiti | gonca |
| Birden fazla katman veya belirsiz | katya (o dagitir) |

### Onemli Kurallar

- **Basit sorular** (tek satirlik aciklama, dosya okuma) icin takimi kullanma — direkt yanitla
- **Kod yazma gerektiren gorevlerde** mutlaka katya'ya delege et
- **Tek dosyalik basit duzeltmeler** icin takimi kullanma — direkt yap
- Katya'ya gorev iletirken Turkce ve aciklayici bir prompt ver

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
- **Theme:** Metronic 7
- **JS:** jQuery, DataTables, Chart.js
