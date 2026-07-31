# Toplu Faturalandırma — Tetik Mimarisi: Hangfire → N8N + RabbitMQ

**Tarih:** 2026-07-01
**Durum:** Tasarım onaylandı (Hangfire kaldırıldı, RabbitMQ/N8N entegrasyonu yazılacak)

## Amaç
Aylık AIDAT toplu faturalandırma tetiğini uygulama-içi Hangfire'dan çıkarıp, dış (Coolify) N8N + RabbitMQ altyapısına taşımak. Hedef: altyapı sadeleştirme + tetiğin uygulama ömründen bağımsızlaşması.

## Topoloji ve firewall
- **Ağ 1 (iç):** Sunucu 1 = Logo + LogoSql + LogoRest (`:32001`, IP ile). Sunucu 2 = Uygulamamız.
- **VPS (Coolify):** N8N (`yedpa-n8n.koalasupport.com`), RabbitMQ (`yedpa-amq.koalasupport.com`), PostgreSQL (`164.68.98.193:5432`).
- **Firewall:** N8N → uygulama (inbound) KAPALI. Uygulama → dışarı (VPS) AÇIK.
- Sonuç: Uygulamanın VPS ile tüm teması **outbound** — RabbitMQ consumer (kalıcı bağlantı, mesaj bu kanaldan iner) + PostgreSQL yazımı. İnbound port gerekmez.

## Güvenlik kuralı (değişmez)
VPS'e **iş verisi taşınmaz.** RabbitMQ/PG yalnızca **kontrol verisi** taşır (aktarım tarihi, "başlat" tetiği). Cari/fatura/Excel/crosstable iç ağda kalır; mail'i uygulama Sunucu 2'den gönderir.

## Akış
```
Tarih seçimi (Dashboard, ayın 15'inden sonra alert → modal → kaydet)
  → App: MSSQL'e session yazar (alert + Manage sayfası — app'in kendi doğruluğu)
       + VPS PostgreSQL'e tek satır UPSERT (transfer_date) — N8N'in okuyacağı kopya
N8N (TEK workflow, günlük 00:01 schedule)
  → PG'den transfer_date oku → bugünse → RabbitMQ'ya "run" mesajı publish
App (RabbitMQ consumer, outbound)
  → mesaj → RunTransferAsync → aktar + DB'ye yaz + Excel(crosstable) + mail → ack → biter
Eksik/başarısız satırlar → Manage sayfası "Eksik Kalanları Yeniden Aktar"
```

**Kaynak-doğruluk:** MSSQL = uygulamanın kendi doğruluğu (VPS düşse de app çalışır). PG = N8N için "sıradaki tarih" projeksiyonu. Tarih seçilince ikisine de yazılır.

## Mail
> ⚠️ **GEÇERSİZ (2026-07-28'de değişti).** Aşağıdaki "tek mail" kararı iptal edildi; müşteri T-1
> bilgilendirme maili istedi. Güncel hâli: **iki mail** — (1) T-1 günü **12:01** bilgilendirme maili
> (Excel + toplam), (2) aktarım sonrası sonuç maili. Tetikleyici: N8N workflow'una eklenen ikinci
> zamanlayıcı + RabbitMQ mesajındaki `kind: "info" | "transfer"` alanı. Bkz. `handoff.md` §2 ve §5b.

~~Tek mail: **aktarım sonrası sonuç maili** (crosstable → Excel). Ayrı T-1 08:00 ön-bilgi maili YOK — kesilecekler Manage sayfasından görülebiliyor.~~

## Bileşenler (uygulama tarafı)
- **`IScheduleStore` + `PgScheduleStore` (Npgsql):** `UpsertTransferDateAsync(DateOnly)`. Tek satır (sabit id). Tarih seçiminde çağrılır. Bağlantı `appsettings`/secret'ten.
- **RabbitMQ consumer (`HostedService`):** Outbound kalıcı bağlantı (auto-recovery). "run" mesajı → yeni DI scope → `BulkInvoiceJobs.RunTransferAsync`. İş bitince ack. Bağlantı bilgisi config'ten.
- **Değişmeden kalan çekirdek:** `BulkInvoiceJobs.RunTransferAsync / SendInfoMailAsync / RetryFailedAsync`, `BulkInvoiceService`, Manage sayfası + "Eksik Kalanları Yeniden Aktar" (Failed+Pending kapsamı düzeltildi).
- **Kaldırıldı:** Hangfire (server, dashboard, BackgroundJob, 7 paket). `CreateSessionAsync` artık zamanlamaz, sadece session kaydeder + (yeni) PG upsert tetikler.

## İdempotency / çift tetik
N8N tek publish. Ayrıca aktarım yalnızca Transferred-olmayan satırları işler ve başarılıları Logo'da TRGFLAG=1 yapar → olası yeniden-teslimatta bile çift fatura olmaz. Ek guard gereksiz.

## Konfigürasyon
- `ConnectionStrings:N8nScheduleDb` (Npgsql, PG) — **secret**, commit edilmez.
- `RabbitMq:*` (host=yedpa-amq, port, vhost, user, pass, queue adı) — **secret**.
- Parolalar Coolify env / user-secrets / gitignore'lu config'te.

## Kullanıcı sorumluluğu / dış işler
- Coolify: proje + N8N + RabbitMQ + PostgreSQL (yapılıyor).
- N8N workflow: PG oku → tarih eşleşince RabbitMQ publish.
- **LogoRest ayarı `localhost` → Sunucu1 IP** (dünkü token fail'inin muhtemel sebebi; kullanıcı güncelleyecek).
- Firewall: Sunucu2 → VPS PostgreSQL(5432) + RabbitMQ(5671/5672) outbound açık.

## Uygulama sırası
1. Coolify infra (kullanıcı)
2. RabbitMQ ↔ uygulama: consumer + `PgScheduleStore` (asistan) — infra'dan bağımsız yazılabilir
3. N8N ↔ RabbitMQ (kullanıcı)
4. N8N workflow (kullanıcı)
5. Test (birlikte)
