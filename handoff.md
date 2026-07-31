# Toplu Faturalandırma (Bulk Invoice) — Handoff

**Son güncelleme:** 2026-07-28
**Branch:** `feature/bulk-invoice-rewrite`
**Yerel commit:** `da90f9c` (push BEKLİYOR — bkz. §9)

---

## 1. Proje Hedefi

Her ay, bekleyen **AIDAT sipariş satırlarını** otomatik olarak **Logo REST** ile faturaya dönüştürmek.

- Müşteri, ayın **15'inden sonra** çıkan bir uyarıdan **gelecek ayın aktarım gününü** (ilk iş günü) seçer.
- Seçilen tarihte, **gece 00:01'de** aktarım otomatik çalışır: o ayın tüm bekleyen AIDAT satırları Logo'da faturaya dönüşür.
- Aktarım bitince **sonuç maili** (başarılı/başarısız + Excel) gider.
- Bir **yönetim sayfası** ile aktarılacak veriler gün içi görülebilir ve **eksik/başarısız satırlar yeniden aktarılabilir**.

**Önemli iş kuralları (canlı veriyle doğrulandı):**
- Firma/Dönem: **Firm=211, Period=16** → `LG_211_16_ORFICHE/ORFLINE`, `LG_211_CLCARD`.
- `ORF.DOCODE='AIDAT'`, `ORL.TRGFLAG=0` (0=faturalanmadı, 1=faturalandı).
- Ay eşleşmesi **`ORL.LINEEXP`** (ay adı, ör. 'TEMMUZ') ile — `LINENO_` GÜVENİLİR DEĞİL (kiracı değişiminde kayar).
- Tutar **`ORL.TOTAL`** (KDV dahil) — `ORL.AMOUNT` miktardır (hep 1).
- Cari kod/ad **CLCARD join**: `ORF.CLIENTREF=CLC.LOGICALREF`, `CLC.CODE`/`CLC.DEFINITION_`.
- Kırmızı/pasif cari dışlanır: `ISNULL(CLC.ACTIVE,0)=0` (BLOCKED kullanılmıyor).
- İdempotency: Logo REST bağımsız fatura kesince **TRGFLAG'ı OTOMATİK YAPMAZ** → başarılı satırların LOGICALREF'leri toplu T-SQL `UPDATE ... SET TRGFLAG=1` ile işaretlenir.
- Logo payload kanıtı: `test-aidat-fatura-temmuz.json` (repo kök, gitignore'lu). `TRANSACTIONS:{items:[...]}` sarmalı, `PAYMENT_CODE="10-3"`, `TYPE=7`, `MASTER_CODE="600.11.0001"`, `EINVOICE=1`, `VAT_INCLUDED=1`, toplamlar GÖNDERİLMEZ (REST hesaplar), GL kodları `FillAccCodesOnPreSave` ile otomatik.

---

## 2. Mimari — Hangfire ÇIKTI, yerine N8N + RabbitMQ + PostgreSQL

**Sebep:** Altyapı sadeleştirme; kullanıcı Coolify kullanıyor (N8N + RabbitMQ + PostgreSQL mevcut). Ayrıca tetiğin uygulama ömründen bağımsız olması.

**Ağ topolojisi:**
```
Ağ 1 (iç):  Sunucu 1 [Logo + LogoSql + LogoRest (:32001, IP ile)]
            Sunucu 2 [Bizim Uygulamamız]
VPS:        Coolify [N8N (yedpa-n8n.koalasupport.com) + RabbitMQ (yedpa-amq...) + PostgreSQL (164.68.98.193:5432)]
```
**Firewall:** N8N → uygulama (inbound) KAPALI. Uygulama → dışarı (VPS) AÇIK.
→ Uygulamanın VPS ile teması yalnızca **outbound**: RabbitMQ consumer (mesaj bu kanaldan iner) + PG'ye tarih yazımı. İnbound port GEREKMEZ.

**GÜVENLİK KURALI (değişmez):** VPS'e (N8N/RabbitMQ/PG) **iş verisi taşınmaz** — yalnızca kontrol verisi (aktarım tarihi, "başlat" tetiği). Cari/fatura/Excel/crosstable iç ağda kalır; mail'i uygulama Sunucu2'den gönderir.

**Akış:**
```
Tarih seçimi (Dashboard, 15'inden sonra alert → modal → SADECE tarih → kaydet)
  → App: MSSQL'e oturum (BulkInvoiceSession) + Coolify PG'ye tek satır UPSERT (bulk_invoice_schedule.transfer_date)
  → Alert kalkar, yerine "Aktarım Yapılacak Firmaları Görüntüle" paneli gelir (→ Manage?sessionId=N)

N8N (TEK workflow, İKİ zamanlayıcı, Europe/Istanbul):
  a) günlük 12:01 → PG'de transfer_date == YARIN mı? → 'bulk_invoice.run' { "date":"...", "kind":"info" }
  b) günlük 00:01 → PG'de transfer_date == BUGÜN mü? → 'bulk_invoice.run' { "date":"...", "kind":"transfer" }

App (BulkInvoiceTriggerConsumer, outbound AMQP consumer)
  → kind=info     → Sync + SendInfoMailAsync (T-1 bilgilendirme maili + Excel) → ack
  → kind=transfer → RunTransferAsync → aktar + DB + sonuç maili → ack
Eksik/başarısız satırlar → Manage sayfası "Eksik Kalanları Yeniden Aktar"
```
**Kaynak-doğruluk:** MSSQL = uygulamanın kendi doğruluğu (alert/Manage bundan çalışır). PG = N8N'in okuyacağı "sıradaki tarih" projeksiyonu.

**Mail — İKİ ADET:**
1. **T-1, 12:01 — bilgilendirme maili:** aktarılacak firmaların Excel'i + toplam tutar. Öncesinde `SyncSessionItemsAsync` çalışır → mailde giden liste ile Manage sayfasındaki liste aynıdır.
2. **Aktarım sonrası — sonuç maili:** başarılı/eksik sayıları, faturalanan toplam tutar, başarısız satır tablosu (ClientCode / Ref / Not / REST hata).

Alıcılar (ŞİMDİLİK sabit, `BulkInvoiceEmailService.Recipients`): erkan@sistem-bilgisayar.com.tr,
adegimli@yedpa.com.tr, **tahsilat@yedpa.com.tr** (31.07.2026'da muhasebe@ yerine bu kullanılmaya başlandı).

---

## 3. Şu Anki Durum

### ✅ Bitti ve KANITLANDI
- Hangfire tamamen kaldırıldı (4 proje: server, dashboard, BackgroundJob, 7 NuGet paketi).
- RabbitMQ consumer + PG tarih-yazımı yazıldı, **canlı altyapıya karşı test edildi**.
- **Uçtan uca plumbing kanıtlandı (2026-07-01):** N8N → RabbitMQ (`bulk_invoice.run`) → app consumer → oturum arama. Log: `consumer dinliyor` + `Tetik geldi ama uygun bekleyen oturum yok` (bugün için oturum olmadığından güvenle ack — gerçek aktarım tetiklenmedi).
- Logo payload (29.06 manuel test → gerçek fatura oluştu) ve `RunTransferAsync` (01.07 00:01 koşusunda çalıştı; tek takılma token) daha önce kanıtlandı.
- Alert (15-sonrası) mantığı **gelecek-ay** kontrolüne göre düzeltildi.
- Manage sayfası: önizleme + "Eksik Kalanları Yeniden Aktar" (Failed+Pending).
- **(2026-07-28)** Tam UI akışı + T-1 12:01 maili + sonuç durumu/raporu — bkz. §5b. Build 0 hata, **182/182 test yeşil**.

### ⏳ Bekleyen (üretime hazırlık — hiçbiri canlı Logo'ya fatura oluşturmaz)
- **LogoRest ayarı `localhost` → Sunucu1 IP** (01.07 token fail'inin sebebi). Kullanıcı yapacak.
- **N8N workflow yeniden import + Active** (12:01 kolu eklendi).
- Sunucu2 deploy config (RabbitMq/PG **env değişkeni** olarak).
- Uygulamanın 00:01'de ayakta olması (IIS app pool idle/recycle).
- Yeni UI + T-1 mail kodu için restart.
- `da90f9c` sonrası değişikliklerin commit + push'u (yetki sorunu).

### 🚫 Yapılmayacak
- **Canlı Logo'ya test amaçlı fatura oluşturmak YOK** (veriler canlı). İlk gerçek aktarım = müşterinin gerçek üretim koşusu, **~1 ay sonra** (bir sonraki aidat döngüsü).

---

## 4. Çalışılan Dosyalar

**Core**
- `Core/Dtos/BulkInvoice/AidatInvoicePayload.cs` — kanıtlı Logo payload (JsonPropertyName, TRANSACTIONS.items).
- `Core/Helpers/BulkInvoiceMonths.cs` — `ToLogoName(month)` → OCAK..ARALIK.
- `Core/Services/IScheduleStore.cs` — tarih deposu arayüzü (YENİ).
- `Core/Configuration/RabbitMqSettings.cs` — RabbitMQ ayar POCO (YENİ).
- `Core/Models/BulkInvoiceSession.cs` (InfoJobId/TransferJobId artık kullanılmıyor), `BulkInvoiceItem.cs` (RetryCount/CanRetry/Note/RestError).
- `Core/Dtos/EnumDto.cs` — `BulkInvoiceSessionStatus`, `BulkInvoiceItemStatus` (DİKKAT: Dtos'ta, Models'ta değil).

**Service**
- `Service/Services/BulkInvoiceService.cs` — sorgu (LINEEXP/TOTAL/CLCARD/[LineNo]/ACTIVE), `CheckAlertAsync` (gelecek-ay), `CreateSessionAsync` (artık zamanlamaz, oturum kaydeder), `SyncSessionItemsAsync`, `MarkLinesAsTransferredAsync`, `GetSessions/Items`.
- `Service/Services/BulkInvoiceTransferService.cs` — tek satır → Logo REST POST (token-retry).
- `Service/Services/BulkInvoiceJobs.cs` — `RunTransferAsync` (Sync→dene→retry→TRGFLAG→durum→rapor maili), `RetryFailedAsync` (Failed+Pending), `SendInfoMailAsync` (Sync + T-1 maili).
- `Service/Services/BulkInvoiceExcelService.cs`, `BulkInvoiceEmailService.cs` (info + sonuç maili).
- `Service/Services/PgScheduleStore.cs` — Npgsql tek-satır upsert (YENİ).
- `Service/Services/BulkInvoiceTriggerConsumer.cs` — RabbitMQ consumer HostedService. `kind` (info|transfer) ayrımı burada.
- `Service/Providers/SqlProvider.cs` — `SqlReader` gerçek hatayı `result.Errors.Errors[0]`'a koyar (Message'a değil!).
- `Service/Providers/LogoRestServiceProvider.cs` — token; artık red sebebini loglar.

**WebUI (DİKKAT: DI burada!)**
- `WebUI/Extentions/StartupExtention.cs` — **WebUI'nin GERÇEKTE kullandığı `AddApplicationServices`/`AddApplicationProviders`**. (Service projesindeki `ServiceCollectionExtensions.cs` WebUI tarafından KULLANILMAZ — sadece WebApi kullanır. Yeni servis buraya eklenir.)
- `WebUI/Program.cs` — config binding (`Configure<RabbitMqSettings>`), `AddUserSecrets<Program>` (geçici).
- `WebUI/Controllers/BulkInvoiceController.cs` — CheckAlert/GetPendingLines/CreateSession(+PG upsert)/`Manage(int? sessionId)`/Sessions/Items/PrepareItems/RetryFailed.
- `WebUI/Views/Dashboard/Index.cshtml` — alert bandı + planlanan aktarım paneli + modal (checkbox YOK).
- `WebUI/Views/BulkInvoice/Manage.cshtml` — inline JS yok; `window.__bulkInvoiceSessionId` ile derin bağlantı.
- `WebUI/wwwroot/assets/js/custom/bulk-invoice-dashboard.js` — `KLBulkInvoiceDashboard` (alert/panel/modal/tarih kaydı).
- `WebUI/wwwroot/assets/js/custom/bulk-invoice-manage-page.js` — `KLBulkInvoiceManage` (oturumlar/satırlar/retry/deep-link).
- ~~`WebUI/wwwroot/js/dashboard/bulk-invoice.js`~~ — **SİLİNDİ** (yerine yukarıdaki iki dosya).
- `WebUI/appsettings.json` (boş placeholder'lar), `WebUI/appsettings.Development.json` (**GERÇEK secret'lar burada — gitignore'lu**).

**Docs / N8N**
- `docs/n8n/koala-bulk-invoice-trigger.workflow.json` — N8N workflow. ⚠️ 12:01 bilgilendirme kolu eklendi → **yeniden import edilmeli**.
- `docs/superpowers/specs/2026-07-01-bulk-invoice-n8n-rabbitmq-design.md` — tasarım/spec.
- `docs/superpowers/plans/2026-06-30-bulk-invoice-rewrite.md` — eski implementasyon planı.

---

## 5b. Neler Değişti (2026-07-28 oturumu — UI akışı + T-1 maili)

**Amaç:** "15'inden sonra alert → sadece tarih → alert kalkar, yerine firmaları görüntüle butonu →
T-1 12:01 mail → aktarım günü 00:01 → sonuç raporlanır/kaydedilir/maillenir → eksikler yeniden denenebilir."

1. **Checkbox'lar KALDIRILDI.** Dashboard modalinde satır seçimi yok; liste salt önizleme, aktarımda
   o ana kadar biriken TÜM bekleyen AIDAT satırları işlenir. "Seçilen Toplam" → "Aktarılacak Toplam"
   (+ satır sayısı). Buton: "Aktarım Tarihini Kaydet".
2. **Planlanan aktarım paneli.** `AlertCheckResultDto`'ya `ShowPlannedPanel` / `SessionId` /
   `TransferDate` eklendi. `CheckAlertAsync` artık "yaklaşan oturum" (InvoiceDate >= bugün, en yakın)
   döner. Tarih seçilince alert kalkar → yeşil panel + **"Aktarım Yapılacak Firmaları Görüntüle"**
   butonu (`/BulkInvoice/Manage?sessionId=N`). Aktarım günü boyunca panel açık kalır.
3. **Manage derin bağlantı.** `Manage(int? sessionId)` → o oturum otomatik açılır; satır listesi boşsa
   bir kez otomatik senkronlanır (kullanıcı butona basmadan firmaları görür).
4. **T-1 12:01 bilgilendirme maili GERİ GELDİ.** Hangfire kalkınca `SendInfoMailAsync` çağrısız
   (ölü kod) kalmıştı. Consumer'a `kind` alanı eklendi (`info` | `transfer`, yoksa transfer);
   N8N workflow'una ikinci zamanlayıcı (12:01, transfer_date = yarın) eklendi.
5. **Sonuç kaydı sağlamlaştırıldı.** Aktarım/yeniden aktarım sonunda eksik kalan varsa oturum
   `Failed` (Hatalı), yoksa `Completed`. Sonuç maili artık aktarım tarihi + faturalanan toplam tutarı
   ve "Eksik Kalanları Yeniden Aktar" yönlendirmesini içerir.
6. **Çift oturum koruması.** Aynı ay için Pending oturum varsa yenisi açılmaz, tarihi güncellenir.
7. **Metronic skill kuralı:** inline JS kaldırıldı. `wwwroot/js/dashboard/bulk-invoice.js` **silindi**;
   yerine `wwwroot/assets/js/custom/bulk-invoice-dashboard.js` (KLBulkInvoiceDashboard) ve
   Manage sayfasının inline script'i → `bulk-invoice-manage-page.js` (KLBulkInvoiceManage).
8. **Sessiz arıza görünürlüğü:** RabbitMq:HostName boşsa artık `LogWarning` değil **`LogError`** —
   "aktarım otomatik tetiklenmeyecek" mesajıyla.
9. **Testler:** 114 → 182 (Service 122 + Repositories 60). Yeni: `AlertCheckTests` (5),
   `TransferJobTests` +3 (tam başarı → Completed, retry akışı, info mail sync'i).
10. **Bootstrap 4 uyum düzeltmesi (tüm proje):** tema Metronic 7 = **BS 4.6**, ama bazı sayfalar
    BS5/Metronic 8 sözdizimiyle yazılmıştı → `data-bs-*` ve `btn-close` çalışmıyor, modal/sekmeler
    kapanmıyordu. 9 dosyada dönüştürüldü (`data-bs-*`→`data-*`, `btn-close`→`close`+`&times;`,
    `text-end`→`text-right`, `me-*`→`mr-*`, `fw-*`→`font-weight-*`, `fs-*`→`font-size-*`,
    `badge-light-*`→`label-light-* label-inline`, `gap-*`→çocuklara `mr-*`).
    Dönüşüm tablosu **CLAUDE.md**'ye kural olarak eklendi.
    ⚠️ Kalan: Ariza/Arsiv/Otopark/Sozlesme sayfalarında Metronic **8 yerleşim** iskeleti
    (`flex-stack`, `symbol-50px`, `card-xl-stretch`, `table-row-dashed`, `g-4` — hepsi v7'de YOK,
    ~40 kullanım). İşlevi bozmuyor, yalnızca düzen tutarsız. Markup yeniden kurulmalı.
11. **Manage sayfası çıktı butonları:** "Aktarım Yapılacak Firmalar" tablosuna **Excel + Yazdır**
    eklendi (DataTables Buttons — `datatables.bundle.js` içinde Buttons 1.6.5 + JSZip 3.5.0 hazır,
    CDN gerekmez). Yazdırma ayrı pencerede yalnız tabloyu basar (menü/kart/buton çıkmaz), yatay
    sayfa. Tutar sütunu para simgesinden arındırılıp aktarılır → Excel'de sayı olarak toplanabilir.
    Butonlar `initComplete`'te kart araç çubuğuna taşınır (`language.url` asenkron olduğu için).

⚠️ **N8N workflow'u YENİDEN İMPORT EDİLMELİ** — 12:01 kolu eklendi (`docs/n8n/koala-bulk-invoice-trigger.workflow.json`).

---

## 5. Neler Değişti (önceki oturum)

1. **GetPendingLines sorgu fix'leri:** `ORL.LINENO_ AS [LineNo]` (LINENO rezerve kelime!), `ISNULL(CLC.ACTIVE,0)=0` (pasif cari dışla). Daha önce: CLCARD join, ORL.TOTAL, LINEEXP.
2. **Yutulan hatalar açığa çıkarıldı:** `GetPendingLinesAsync` artık `result.Errors.Errors[0]` (gerçek SQL hatası) + firm/period loglar. `LogoRestServiceProvider` token red sebebini loglar + üst katmana taşır.
3. **Manage "Eksik Kalanları Yeniden Aktar":** `RetryFailedAsync` artık Failed **+** Pending kapsar (yarım koşuyu tamamlar), buton/onay metni güncellendi.
4. **Hangfire kaldırıldı** (4 proje). `CreateSessionAsync` zamanlamayı bıraktı, oturum kaydeder + PG'ye tarih upsert eder (controller'da).
5. **RabbitMQ/PG entegrasyonu:** IScheduleStore/PgScheduleStore, RabbitMqSettings, BulkInvoiceTriggerConsumer, DI (StartupExtention), config binding (Program), appsettings placeholder'ları.
6. **Alert fix:** `CheckAlertAsync` gelecek-ay oturumunu kontrol eder (mevcut ay değil) → tarih seçilince kalkar.
7. **N8N workflow JSON** + `bulk_invoice_schedule` tablosu (PG'de canlı oluşturuldu).

---

## 6. Neler Denendi / Kritik Öğrenilenler (tekrar tuzağa düşmemek için)

- **DERS: Önce ölç, sonra düzelt.** Bu oturumun ilk yarısında teori üretildi ("stale binary", "hot reload"); asıl sebep her seferinde YUTULAN hataydı. Bir işlem "başarısız" derse DAİMA `result.Errors.Errors` oku, `result.Message` DEĞİL. Gerekirse önce loglama ekle.
- **İKİ `AddApplicationServices` tuzağı:** WebUI, `StartupExtention.cs`'teki metodu çağırır (`using Koala.Yedpa.WebUI.Extentions`); Service projesinin `ServiceCollectionExtensions.cs`'ini DEĞİL. Yeni servis yanlış dosyaya eklenince "Unable to resolve IBulkInvoiceService" 500 alınır.
- **`LINENO` rezerve kelime:** `AS LineNo` → "Incorrect syntax near 'LineNo'". `AS [LineNo]` kullan.
- **user-secrets YÜKLENMEDİ (çözülemeyen gizem):** Env=Development, UserSecretsId attribute üretilmiş, secrets.json doğru, `AddUserSecrets<Program>` eklendi — yine de `rawHost=''`. ÇÖZÜM: değerler **`appsettings.Development.json`**'a kondu (gitignore'lu, Development'ta kesin yüklenir — Logging ayarı oradan geliyordu, kanıtlı). Sunucuda **env değişkeni** kullanılacak.
- **Canlı bağlantı gerçekleri:** RabbitMQ **AMQP 5672 düz (TLS yok)** — `yedpa-amq...:15672` yönetim UI'dir, uygulama onu KULLANMAZ (AMQP, HTTP proxy'den geçmez). PostgreSQL **SSL DESTEKLEMİYOR** → Npgsql `SSL Mode=Disable` (Require/Prefer → "SSL handshake" hatası). ⚠️ İkisi de plaintext/internete açık → TLS veya firewall IP-kısıtı SONRA eklenecek (kullanıcı erteledi).
- **Dünkü ilk gerçek koşu (01.07 00:01) neden fail:** Logo **0 token** aldı. Muhtemel sebep: LogoRest base URL `localhost:32001` (yanlış) — Sunucu1 IP olmalı. Kod uçtan uca çalıştı, tek engel buydu.
- **Test araçları (scratchpad, geçici):** `.NET 10 file-based app` ile canlı bağlantı testi — `#:package RabbitMQ.Client@7.2.1` / `Npgsql@10.0.3`, `dotnet run file.cs`, creds env değişkeninden.

---

## 7. Konfigürasyon (secret'lar)

**Config anahtarları** (`RabbitMqSettings` section "RabbitMq" + `ConnectionStrings:N8nScheduleDb`):
- `RabbitMq:HostName / Port / UserName / Password / VirtualHost / UseTls / TriggerQueue`
- `ConnectionStrings:N8nScheduleDb` (Npgsql, `SSL Mode=Disable`)

**Değerler nerede:**
- **Dev makinesi:** `WebUI/appsettings.Development.json` (gitignore'lu). (user-secrets'ta da var ama yüklenmiyor.)
- **Sunucu2 (prod):** **env değişkeni** olarak verilecek — `RabbitMq__HostName`, `RabbitMq__Password`, `ConnectionStrings__N8nScheduleDb` vb.
- **N8N credential'ları (N8N UI'de):** Postgres (Host 164.68.98.193, DB `postgres`, User `postgres`, **SSL Disable**) + RabbitMQ (Host yedpa-amq..., Port 5672, Vhost `/`, SSL kapalı).
- **Kuyruk:** `bulk_invoice.run` (durable). **PG tablo:** `bulk_invoice_schedule` (id=1 tek satır).
- ⚠️ Parolalar bu dokümanda YOK (güvenlik). appsettings.Development.json / user-secrets / N8N credential'larında.

---

## 8. Sırada Yapılacaklar

**Kod tarafı bitti (build 0 hata, 182/182 test yeşil). Kalanların hepsi OPERASYON.**
Bu maddelerden biri eksikse aktarım **sessizce hiç çalışmaz** — hata da vermez.

1. **LogoRest `localhost` → Sunucu1 IP** düzelt (Logo ayarları — şifreli Settings). Sonra manuel token isteğiyle doğrula (fatura OLUŞTURMADAN). *01.07 koşusunun 0 fatura kesme sebebi buydu.*
2. **N8N workflow'unu YENİDEN İMPORT et** (12:01 bilgilendirme kolu eklendi) ve **Active** yap. PG/RabbitMQ credential ID'lerini bağla.
3. **Sunucu2 deploy config:** `RabbitMq__HostName`, `RabbitMq__Password`, `ConnectionStrings__N8nScheduleDb` vb. **env değişkeni** olarak ver. Boşsa log'da `LogError` çıkar — deploy sonrası bu satırı kontrol et.
4. **Uygulamanın 00:01'de ayakta olduğunu garantile:** consumer bir `BackgroundService`. IIS app pool idle-timeout / gece recycle varsa tetik boşa gider. (Hangfire kalktığı için tek dinleyici budur.)
5. **Restart:** yeni UI + T-1 mail kodunun canlıya geçmesi için Stop→Rebuild→F5.
6. **Push:** push `jello-smart` hesabı yetkisiz (403). Doğru GitHub hesabıyla auth (`gh auth switch` / Credential Manager) → `git push -u origin feature/bulk-invoice-rewrite`.
7. **Ağustos döngüsü:** alert çıkıyor (bugün 28.07, Ağustos oturumu yok) → tarih seçilecek → T-1 12:01 bilgi maili → aktarım günü 00:01 gerçek Logo aktarımı (izlenerek).

**Bilinen sınır (bilinçli):** Tarih seçildikten sonra dashboard'dan tarih değiştirme bağlantısı yok.
Servis tarafı hazır (aynı ay Pending oturum varsa tarihi günceller), sadece UI girişi eklenmedi.

---

## 10. Boş catch blokları (31.07.2026'da tespit — aktarım sonrası doldurulacak)

Kullanıcı debug sırasında fark etti. **Şu an hiçbiri gerçek bir hatayı gizlemiyor** — hepsi
temizlik veya geri-düşüş yolu. Yani bu bir bug değil, hijyen borcu. Ama bu kod tabanı yutulan
hatalar yüzünden daha önce bir gün kaybettirdi (bkz. §6), o yüzden kapatılmalı.

**Kesinleşen 6 konum:**

| Dosya:satır | Bağlam | Değerlendirme |
|---|---|---|
| `LogoRestServiceProvider.cs:434` | Hata yolunda token revoke denemesi | Savunulabilir — zaten hata yolundayız, başarısız revoke asıl hatayı maskelememeli. **Debug log eklenmeli.** |
| `LogoRestServiceProvider.cs:465` | Aynı | Aynı |
| `LogoRestServiceProvider.cs:484` | Aynı | Aynı |
| `BulkInvoiceTriggerConsumer.cs:173` | `catch (JsonException)` → düz metin ayrıştırmaya düş | **Kasıtlı ve doğru.** Olduğu gibi kalabilir, yorumu netleştirilebilir. |
| `BulkInvoiceTriggerConsumer.cs:218` | `SafeCloseAsync` — kanal kapatma | Kapanış temizliği. **Debug log eklenmeli.** |
| `BulkInvoiceTriggerConsumer.cs:223` | `SafeCloseAsync` — bağlantı kapatma | Aynı |

⚠️ Daha gevşek bir tarama Service'te 7, Core'da 1 sonucu verdi; yukarıdaki 6'sı satır satır
doğrulandı. Kalan 1-2 tanesi Pazartesi taramayla netleşecek (`Koala.Yedpa.Core/Helpers/` altında).

**Önerilen kural:** sessiz `catch` yok. En azından `LogDebug` ile "ne yutuldu" yazılsın; gerçekten
önemsizse yorumda **neden** önemsiz olduğu açıklansın ("zaten hata yolundayız" gibi).

**Kapsam dışı ama ilgili:** Asıl tehlike boş catch'ler değil, hatayı `result.Message`'a gömüp
`result.Errors.Errors`'ı okutmayan desen (§6). Boş catch temizliği sırasında ona da bakılabilir.

---

## 9. Git / Deploy Durumu

- **Branch:** `feature/bulk-invoice-rewrite`
- **Yerel commit:** `da90f9c` — "feat(bulk-invoice): Hangfire yerine N8N/RabbitMQ tetik + fix'ler" (KrediKartTahsilat vb. önceki işleri de içeren checkpoint).
- **Push:** BAŞARISIZ — `remote: Permission to erkan-dirikcan/Koala.Yedpa.git denied to jello-smart. (403)`. Commit author doğru (Erkan) ama HTTPS push kimliği `jello-smart` (yetkisiz). Doğru hesapla auth gerekiyor.
- **Build:** 0 hata. **Test:** 114/114 yeşil.
- **.gitignore:** TestResults/, CoverageReport/, XML dump'ları, appsettings.Development.json, secrets.json, rabbitinfo.txt vb. hariç.
