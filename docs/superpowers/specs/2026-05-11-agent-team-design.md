# Agent Team Design Spec — Koala.Yedpa

**Tarih:** 2026-05-11
**Durum:** Onaylandı
**Yaklaşım:** Odaklı Takım (Yaklaşım B)

---

## Proje Özeti

Koala.Yedpa, ASP.NET Core 10.0 tabanlı kurumsal bir yönetim uygulamasıdır. Clean Architecture ile 7 projeden oluşur.

| Metrik | Değer |
|--------|-------|
| Proje sayısı | 7 |
| C# dosyası | 348 |
| Entity | 48 |
| MVC Controller | 24 |
| API Controller | 6 |
| Service | 32 |
| Repository | 22 |
| View | 71 |
| JavaScript dosyası | 475 |
| CSS dosyası | 226 |
| Migration | 21 |

**Teknoloji Yığını:**
- .NET 10.0, Entity Framework Core, Dapper
- MSSQL + PostgreSQL çift veritabanı desteği
- Logo REST API, Message34 Email API
- Hangfire background jobs, NLog logging
- Metronic teması, DataTables, Chart.js, QRCode

---

## Takım Amacı

Hem yeni özellik geliştirme hem mevcut kodun refactoring/iyileştirmesi. Tüm katmanlar öncelikli: veritabanı, API entegrasyonları, matematiksel iş mantığı, frontend.

---

## Takım Kompozisyonu (6 Uzman)

### 1. team-lead — Takım Lideri (Tech Lead PM)

**Rol:** Koordinasyon, görev dağıtımı, mimari kararlar, kullanıcı iletişimi

**Sorumluluklar:**
- Kullanıcıdan gelen istekleri analiz eder ve görevlere böler
- Görevleri teammate'lere atar (TaskCreate/TaskUpdate)
- İlerlemeyi takip eder, blokajları çözer
- Kullanıcıya düzenli rapor verir
- Mimari kararları yönetir
- Kod review yapar
- Takım içi iletişimi koordine eder

**Kullanacağı araçlar:** TaskCreate, TaskUpdate, TaskList, SendMessage, Agent (teammate spawn), Read, Write, Edit
**Dokunacağı alanlar:** Tüm proje (koordinasyon amaçlı), docs/team/

---

### 2. db-specialist — Veritabanı & EF Core Uzmanı

**Rol:** MSSQL, EF Core, Dapper uzmanı

**Sorumluluklar:**
- Entity yapıları ve ilişkileri tasarımı
- Migration yönetimi ve veritabanı şema değişiklikleri
- MSSQL sorgu optimizasyonu ve index stratejisi
- Dapper ile yüksek performanslı sorgular
- Repository pattern implementasyonu
- StoredProcedure yazımı
- Veritabanı performans izleme

**Dokunacağı alanlar:**
- `Koala.Yedpa.Core/Models/` (entity/domain sınıfları ve ViewModel'ler)
- `Koala.Yedpa.Core/Repositories/` (repository interface'leri)
- `Koala.Yedpa.Repositories/` (repository implementasyonları, AppDbContext, Migrations/)

---

### 3. backend-dev — Backend Service Developer

**Rol:** İş mantığı ve service layer geliştirici

**Sorumluluklar:**
- Service layer geliştirme ve iyileştirme
- Matematiksel hesaplamalar (bütçe, aidat, otopark ücreti)
- Unit of Work pattern yönetimi
- AutoMapper konfigürasyonları
- İş kuralları ve validasyonlar
- Domain model zenginleştirme
- DTO tasarımı

**Dokunacağı alanlar:**
- `Koala.Yedpa.Service/` (service implementasyonları, iş mantığı)
- `Koala.Yedpa.Core/Dtos/` (DTO tasarımı ve dönüşümleri)
- `Koala.Yedpa.Core/Services/` (service interface'leri)

---

### 4. api-dev — API & Entegrasyon Geliştirici

**Rol:** External API, WebAPI, background jobs uzmanı

**Sorumluluklar:**
- Logo REST API entegrasyonu
- Message34 Email Service entegrasyonu
- Hangfire background job yönetimi ve yeni job'lar
- WebAPI projesi endpoint geliştirme
- Swagger dokümantasyonu
- HTTP client yapılandırması ve retry mekanizmaları
- API güvenliği, rate limiting
- WebHook/callback mekanizmaları

**Dokunacağı alanlar:**
- `Koala.Yedpa.WebApi/`
- `Koala.Yedpa.Service/Services/` (Message34EmailService gibi external servis implementasyonları)
- `Koala.Yedpa.Service/Providers/` (LogoRestServiceProvider, RestServiceProvider gibi provider implementasyonları)
- `Koala.Yedpa.Core/Providers/` (provider interface'leri)
- Hangfire konfigürasyonu

---

### 5. frontend-dev — Frontend Developer

**Rol:** Metronic tema, JavaScript, Razor Views uzmanı

**Sorumluluklar:**
- Metronic tema özelleştirme ve yeni sayfa geliştirme
- Razor Views ve Partial View'lar
- JavaScript mimarisi ve modüler yapı
- DataTables yapılandırması ve özelleştirme
- Chart.js dashboard widget'ları (W1-W10)
- AJAX call'lar, form validasyonları
- Responsive tasarım
- CSS/SCSS organizasyonu

**Dokunacağı alanlar:**
- `Koala.Yedpa.WebUI/Views/`
- `Koala.Yedpa.WebUI/wwwroot/`
- `Koala.Yedpa.WebUI/Controllers/` (sadece view-related logic)

---

### 6. qa-engineer — QA & Test Otomasyon Uzmanı

**Rol:** Test geliştirme ve kalite güvencesi

**Sorumluluklar:**
- Mevcut test projelerini genişletme
- Unit test yazımı (Repository ve Service katmanları)
- Entegrasyon testi (API endpoint'leri)
- Regression testi
- Test coverage raporlama
- Bug tespiti ve raporlama
- Test verisi hazırlama

**Dokunacağı alanlar:**
- `Koala.Yedpa.Repositories.Tests/`
- `Koala.Yedpa.Service.Tests/`
- Gerekirse yeni test projeleri

---

## Dosya Yapısı

```
docs/team/
├── team-lead.md          # Lead'in talimatları ve koordinasyon notları
├── db-specialist.md       # DB uzmanının bağlam dosyası
├── backend-dev.md         # Backend dev'in bağlam dosyası
├── api-dev.md             # API dev'in bağlam dosyası
├── frontend-dev.md        # Frontend dev'in bağlam dosyası
├── qa-engineer.md         # QA engineer'ın bağlam dosyası
└── progress.md            # Ortak ilerleme raporu (sadece team-lead yazar)
```

Her `.md` dosyası teammate'in:
- Sorumluluk alanını
- Dokunması gereken dosya/dizinleri
- Kullanması gereken desen ve prensipleri
- İletişim kurallarını
- Son yaptığı işlerin özetini içerir

### Başlatma (Initialization)

Takım oluşturulmadan önce team-lead şu adımları gerçekleştirir:
1. `docs/team/` dizinini oluşturur
2. Her teammate için `.md` bağlam dosyasını yazar
3. `progress.md` dosyasını boş şablonla oluşturur
4. Ardından teammate'leri spawn eder

### `.claude/agents/` İlişkisi

Her teammate için `.claude/agents/` dizininde bir agent tanım dosyası oluşturulur. Bu dosyalar:
- Agent'ın hangi araçları kullanabileceğini belirler
- Agent'ın system prompt'unu tanımlar
- `docs/team/*.md` dosyalarını referans gösterir

Örnek: `.claude/agents/db-specialist.md` → `docs/team/db-specialist.md` dosyasını okuyarak bağlamını yükler

---

## İletişim ve Koordinasyon

### Görev Akışı

```
Kullanıcı → Team Lead → Görev analizi ve bölme
                       ├── db-specialist    (DB/EF/Core)
                       ├── backend-dev      (Service/Domain)
                       ├── api-dev          (WebAPI/External)
                       ├── frontend-dev     (WebUI/Views/JS)
                       └── qa-engineer      (Test/Tüm katmanlar)
                       ← Sonuç sentezi ←
           ← Rapor ←
```

### İletişim Kuralları

1. Kullanıcı sadece team-lead ile iletişim kurar
2. Team-lead teammate'lere görev atar ve takip eder
3. Teammate'ler birbirleriyle SendMessage ile iletişim kurabilir
4. Her teammate işini bitirdiğinde team-lead'e mesaj gönderir, team-lead progress.md'yi günceller
5. Team-lead düzenli aralıklarla kullanıcıya durum raporu verir

### Dosya Çakışma Önleme ve Çözüm

Her teammate'in birincil dosya alanları tanımlıdır. Aynı dosyayı iki teammate düzenlemez.

**Ortak alanlar ve sahiplik kuralları:**
- `Koala.Yedpa.Core/Models/` → db-specialist sahiplikte (entity tanımları), backend-dev sadece okur
- `Koala.Yedpa.Service/Services/` → backend-dev sahiplikte (iş mantığı), api-dev sadece external servis dosyalarına dokunur
- `Koala.Yedpa.Core/Providers/` → api-dev sahiplikte (provider interface'leri)

**Çakışma çözüm protokolü:**
1. Teammate çakışma tespit ettiğinde team-lead'e bildirir
2. Team-lead hangi teammate'in öncelikli olduğuna karar verir
3. Öncelik sırası: db-specialist → backend-dev → api-dev → frontend-dev → qa-engineer
4. Alt katman üst katmanı bekler, üst katman tamamlandığında devam eder

---

## Görev Bağımlılıkları

Tipik görev bağımlılık zinciri:

```
1. db-specialist: Yeni entity/migration oluşturur
2. backend-dev: Yeni service'i yazar (DB'ye bağlı)
3. api-dev: Yeni API endpoint ekler (Service'e bağlı)
4. frontend-dev: Yeni view/JS ekler (API'ye bağlı)
5. qa-engineer: Tüm zinciri test eder (tamamlandıktan sonra)
```

---

## Başarı Kriterleri

- Her teammate kendi sorumluluk alanında bağımsız çalışabilir
- Dosya çakışması yaşanmaz
- Test coverage artar
- Migration'lar sorunsuz uygulanır
- API entegrasyonları stabil çalışır
- Frontend Metronic standartlarına uygun
- Kullanıcı düzenli rapor alır
