# Takım İlerleme Raporu

> Bu dosyayı sadece Katya (team-lead) günceller. Teammate'ler buraya yazamaz.

## Son Güncelleme: 2026-06-05 — Feedback Madde 1 ve 4 TAMAMLANDI

### Feedback Madde 1: PendingInvoices.customerCode Düzeltmesi

**Tarih:** 2026-06-05
**Build:** Service 0 hata, WebApi 0 hata
**Commit:** bekliyor (kullanıcı topluca atacak)

#### Kök Neden Analizi
Logo Tiger CLCARD hiyerarşisinde iki kart tipi vardır:
- Alt kart (çocuk): `1.A000.002.00.MS` formatında gerçek cari kart kodu
- Üst kart / muhasebe grubu: `679.01.x` veya `320.02.x` formatında muhasebe hesabı kodu

`PAYTRANS.CARDREF` → `CLCARD.LOGICALREF` join'i bazen muhasebe grubu kartını işaret edebilir. Oysa `INVOICE.CLIENTREF` daima faturanın kesildiği gerçek cari kartı gösterir. Bu nedenle `ClCardInfoAll`'daki `CARI_KODU` (`1.A000.x` formatı) ile PendingInvoices'taki `customerCode` çakışmıyordu.

#### Uygulanan Değişiklikler

**Dosya:** `Koala.Yedpa.Service/Services/ApiLogoSqlDataService.cs`

1. Parametresiz `BuildBasePendingInvoiceQuery()` (~529. satır): Artık parametreli versiyona delegate ediyor (`remainingFilter: "open"`). Eski inline SQL kaldırıldı — tek kaynak korundu, geriye dönük uyumluluk tam.

2. Parametreli `BuildBasePendingInvoiceQuery(...)` (~984. satır):
   - `CustomerCode`: `CLNTC.CODE` → `ISNULL(INVCL.CODE, CLNTC.CODE)` (gerçek cari kodu öncelikli)
   - `CustomerName`: `CLNTC.DEFINITION_` → `ISNULL(INVCL.DEFINITION_, CLNTC.DEFINITION_)`
   - FROM bloğuna yeni LEFT JOIN eklendi: `LG_xxx_CLCARD AS INVCL ON INVCL.LOGICALREF = INVFC.CLIENTREF`

3. `SearchPendingInvoicesAsync` filtre koşulları güncellendi:
   - `CLNTC.CODE LIKE ...` → `ISNULL(INVCL.CODE, CLNTC.CODE) LIKE ...`
   - `CLNTC.DEFINITION_ LIKE ...` → `ISNULL(INVCL.DEFINITION_, CLNTC.DEFINITION_) LIKE ...`

#### Müşteri Sorularına Cevaplar
- **customerReference (13951) nedir?** `PAYTRANS.CARDREF → CLCARD.LOGICALREF` — cari kartın benzersiz ID'si.
- **Gerçek cari kodu nasıl bulunur?** `INVOICE.CLIENTREF → CLCARD.CODE` — fatura tablosundaki `CLIENTREF` alanı her zaman gerçek cari kartı gösterir.
- **679.01.x / 1.A000.x farkı:** Aynı firmada birden fazla CLCARD kaydı olabilir; `PAYTRANS.CARDREF` muhasebe entegrasyon kartını gösterirken `INVOICE.CLIENTREF` işletim kartını gösterir. Düzeltme sonrası `ClCardInfoAll` ile aynı kod gelecek.

---

### Feedback Madde 4: ClCardStatementDetailed.DueDate Eklenmesi

**Tarih:** 2026-06-05
**Build:** Service 0 hata, WebApi 0 hata

#### Analiz
`CLFLINE` tablosunda vade tarihi alanı bulunmamaktadır. Gerçek vade tarihi `PAYTRANS.DATE_` alanındadır ve bu tablo `GetClCardStatementDetailedAsync` sorgusuna dahil edilmemektedir. Bu nedenle `DueDate` şimdilik `NULL` olarak eklendi — aging hesabı için `PendingInvoices.DueDate` kullanılmalıdır.

#### Uygulanan Değişiklikler

**Dosya 1:** `Koala.Yedpa.Service/Services/ApiLogoSqlDataService.cs`
- UNION ALL içindeki 7 blokun tamamına `CAST(NULL AS DATETIME)` sütunu eklendi (Blok 1 `DUE_DATE` alias'lı, diğerleri pozisyon uyumu için isimsiz)
- `StatementDetailedGroupViewModel` parse kodu: `DueDate = header["DUE_DATE"] != DBNull.Value ? ... : null`

**Dosya 2:** `Koala.Yedpa.Core/Models/ViewModels/ApiLogoViewModels.cs`
- `StatementDetailedGroupViewModel`'e `public DateTime? DueDate { get; set; }` eklendi (nullable, geriye dönük uyumlu)

#### clCode Tutarlılık Doğrulaması
`ClCardStatementDetailed`'daki `clCode` = `CLC.CODE` (CLFLINE → CLIENTREF → CLCARD.CODE).
`ClCardInfoAll`'daki `CARI_KODU` = `CLC.CODE` (CLCARD self-join, alt kart).
Her ikisi de aynı `LG_xxx_CLCARD.CODE` alanından geliyor. Kullanım şekli: `ClCardStatementDetailed` doğrudan `CLC.CODE` ile filtreliyor (`WHERE CL_CODE = '{safeCode}'`), `ClCardInfoAll` ise `CLC.CODE AS CARI_KODU` döndürüyor. **TUTARLI — aynı kodu kabul edebilirsiniz.**

---

## Son Güncelleme: 2026-06-05 — PendingInvoices Faz 2 (Status Filtresi) TAMAMLANDI

### Faz 2: PendingInvoicesSearch'e Status Filtresi Eklendi

**Tarih:** 2026-06-05
**Build:** Service 0 hata, WebApi 0 hata
**Commit:** bekliyor (kullanıcı topluca atacak)

#### Yapılan Değişiklikler

**Dosya 1:** `Koala.Yedpa.Core/Models/ViewModels/ApiLogoViewModels.cs`
- `PendingInvoiceSearchViewModel`'e `public string? Status { get; set; }` eklendi
  - Kabul edilen değerler: `"open"` (varsayılan), `"closed"`, `"all"`

**Dosya 2:** `Koala.Yedpa.Service/Services/ApiLogoSqlDataService.cs`
- `BuildBasePendingInvoiceQuery` imzası genişletildi: `(string? specodeWhereClause = null, string remainingFilter = "open")`
  - `remainingFilter` whitelist switch ile normalize ediliyor (SQL injection koruması)
  - `open`  → `AND (PTRNS.TOTAL - ISNULL(...,0)) > 0`
  - `closed` → `AND (PTRNS.TOTAL - ISNULL(...,0)) = 0`
  - `all`    → koşul yok
  - Çekirdek filtreler (`MODULENR=4, SIGN=0, CROSSREF=0, CANCELLED=0, TRCODE IN(7,8,9,11,14), INVFC.CANCELLED=0`) HER durumda koruyor
- `SearchPendingInvoicesAsync`: `searchModel.Status` whitelist ile normalize edildikten sonra `BuildBasePendingInvoiceQuery(remainingFilter: statusFilter)` çağrısına iletiliyor
- `GetPendingInvoicesAsync` (parametresiz GET): `BuildBasePendingInvoiceQuery()` çağrısı değişmedi — varsayılan `"open"` → geriye dönük uyumluluk korundu

#### Gonca Dogrulama Kontrol Noktaları
- `Status` null/bos/"open" → sadece `remaining>0` satırlar (eski davranış)
- `Status="closed"` → sadece `remaining=0` satırlar
- `Status="all"` → remaining koşulu yok, tüm TRCODE(7,8,9,11,14) faturalar
- Bilinmeyen Status değeri ("xyz" vb.) → "open" olarak davranır, SQL'e gömülmez
- `GetPendingInvoicesAsync` (GET, parametresiz) → hala sadece açık faturalar

---

## Son Güncelleme (önceki): 2026-06-05 — PendingInvoices TRCODE Kapsam Genişletme TAMAMLANDI

### Ek Rötuş: TRCODE 11 (Vade Farkı) + 14 (Satış Fiyat Farkı) Kapsama Alındı

**Tarih:** 2026-06-05
**Dosya:** `Koala.Yedpa.Service/Services/ApiLogoSqlDataService.cs`
**Build:** Service 0 hata, WebApi 0 hata

**Değişiklikler (5 nokta):**
1. `BuildBasePendingInvoiceQuery` WHERE: `TRCODE IN (7,8,9)` → `TRCODE IN (7,8,9,11,14)`
2. `BuildBasePendingInvoiceQuery` InvoiceType CASE: `WHEN 11 THEN 'Verilen Vade Farkı Faturası'` eklendi
3. `BuildBasePendingInvoiceQuery` InvoiceCategory CASE: `WHEN 11 THEN 'Satış'` eklendi
4. `GetPendingInvoicesAsync` bölgesindeki ikincil CASE bloğu: aynı TRCODE 11 eşlemeleri eklendi
5. `ClCardStatementDetailed` FICHE_TYPE CASE: `WHEN 11 THEN 'VERİLEN VADE FARKI FATURASI'` (2 ayrı CASE bloğu) eklendi

**Etki:** 1.732 adet TRCODE=11 (DOCODE "VADE FARKI") faturası artık PendingInvoices'ta görünür.

---

## Son Güncelleme (önceki): 2026-06-05 — PendingInvoices Kritik Bug Fix TAMAMLANDI

### Bug Fix: PendingInvoices SQL Sorgu Hatası — TAMAMLANDI

**Tarih:** 2026-06-05
**Kapsam:** Koala.Yedpa.Service + Koala.Yedpa.Core
**Build Durumu:** 0 hata (Service + WebApi projeleri)

#### Kök Neden (3 hata, canlı DB'de kanıtlandı)
1. PAYTRANS'ta MODULENR filtresi yoktu — dekont/banka/kasa satırları da geliyordu
2. SIGN=1 yanlış yöndü — satış faturaları SIGN=0; eski WHERE sıfır gerçek fatura döndürüyordu
3. LEFT JOIN ile FICHEREF eşleşmesi başka modüllerin ref'lerini rastgele faturalara bağlıyordu

#### Uygulanan Değişiklikler

**Dosya 1:** `Koala.Yedpa.Service/Services/ApiLogoSqlDataService.cs`

- `BuildBasePendingInvoiceQuery`: LEFT OUTER JOIN INVOICE -> INNER JOIN; WHERE PTRNS.SIGN=1 -> SIGN=0, MODULENR=4 eklendi, INVFC.TRCODE IN (7,8,9) eklendi, INVFC.CANCELLED=0 eklendi, FROMKASA filtresi kaldırıldı (gereksiz)
- `BuildBasePendingInvoiceQuery`: SELECT'e InvoiceNetTotal ve TotalPayTransForInvoice kolonları eklendi (kuruş farkı analizi)
- `GetPendingInvoicesAsync`: 14 satırlık inline totalQuery kaldırıldı, yerine tek satır: BuildBasePendingInvoiceQuery() saran COUNT sorgusu
- `SearchPendingInvoicesAsync`: Aynı inline totalQuery temizlendi, tek kaynaktan besleniyor

**Dosya 2:** `Koala.Yedpa.Core/Models/ViewModels/ApiLogoViewModels.cs`

- `PendingInvoiceViewModel`'e iki yeni nullable property eklendi: `InvoiceNetTotal` ve `TotalPayTransForInvoice`

#### Beklenen Etki (canlı SQL testinden)
- Eski: 1762 satır, büyük çoğunluğu çöp (dekont/banka/kasa)
- Yeni: ~3852 gerçek açık satış faturası, doğru üye cari kodlarıyla

---

## Son Güncelleme (önceki): 2026-06-03 (15:45) - Banka Fişi & Kasa Fişi DTO'ları

### Güncel Görevler

#### Olga (backend-dev) - Banka Fişi & Kasa Fişi DTO'ları
- ⏳ **Task #3 PENDING**: CreateArpSlipRequestDto oluştur (Cari Hesap Fişi)
  - Referans: CreditFicheJsonModel (Orionpos)
  - Endpoint: ArpSlips
  - 3 nested class: Ana DTO + Transactions + Item
  - Default değerler: TYPE=70, MODULENR=5, BANKACCREF=1
- ⏳ **Task #4 PENDING**: CreateSafeDepositSlipRequestDto oluştur (Kasa Fişi)
  - Referans: SafeDepositSlipsJsonModel (Orionpos)
  - Endpoint: safeDepositSlips
  - 5 nested class: Ana DTO + AttachmentArp + AttachmentArpItem + PaymentList + PaymentItem
  - Default değerler: TYPE="11", SD_CODE="100.03", MODULENR="10"
- ⏳ **Task #5 PENDING**: CreateSalesInvoiceRequestDto'yu güncelle
  - PaymentType alanını iyileştir ("CreditCard", "Cash" vb.)
  - XML comment'leri zenginleştir

**Not:** Bu görevler SADECE DTO oluşturma — service implementation YAZILMAYACAK!

#### Olga (backend-dev) - SalesInvoice Service Katmanı
- ✅ **Task #2 COMPLETED**: SalesInvoice Service katmanı implementasyonu TAMAMLANDI
  - DTO'lara JsonPropertyName attribute'ları eklendi (ALL_CAPS formatı)
  - `ISalesInvoiceService` interface oluşturuldu
  - `SalesInvoiceService` service class'ı oluşturuldu
  - Logo Tiger REST API entegrasyonu yapıldı (`ILogoRestServiceProvider` kullanılarak)
  - Service registration eklendi (`ServiceCollectionExtensions.cs`)
  - **Kullanılan Pattern:** BudgetOrderService referans alındı
  - **Endpoint:** `salesInvoices` (Logo Tiger REST API)
  - **JSON Serializasyon:** Newtonsoft.Json (mevcut proje standardı)
  - **Error Handling:** ResponseDto<T> wrapper ile tutarlı hata yönetimi
  - **Loglama:** ILogger ile detaylı loglama

**Oluşturulan Dosyalar:**
1. `Koala.Yedpa.Core/Services/ISalesInvoiceService.cs` (Interface)
2. `Koala.Yedpa.Service/Services/SalesInvoiceService.cs` (Implementation)
3. `Koala.Yedpa.Core/Dtos/SalesInvoice/CreateSalesInvoiceRequestDto.cs` (Güncellendi)
4. `Koala.Yedpa.Core/Dtos/SalesInvoice/CreateSalesInvoiceResponseDto.cs` (Güncellendi)

**Değiştirilen Dosyalar:**
1. `Koala.Yedpa.Service/Extentions/ServiceCollectionExtensions.cs` (DI registration eklendi)

---

### Önceki Görevler (Arşiv)

#### Olga (backend-dev) - SalesInvoice Service Katmanı
- ✅ **Task #2 COMPLETED**: SalesInvoice Service katmanı implementasyonu TAMAMLANDI
  - DTO'lara JsonPropertyName attribute'ları eklendi (ALL_CAPS formatı)
  - `ISalesInvoiceService` interface oluşturuldu
  - `SalesInvoiceService` service class'ı oluşturuldu
  - Logo Tiger REST API entegrasyonu yapıldı (`ILogoRestServiceProvider` kullanılarak)
  - Service registration eklendi (`ServiceCollectionExtensions.cs`)
  - **Kullanılan Pattern:** BudgetOrderService referans alındı
  - **Endpoint:** `salesInvoices` (Logo Tiger REST API)
  - **JSON Serializasyon:** Newtonsoft.Json (mevcut proje standardı)
  - **Error Handling:** ResponseDto<T> wrapper ile tutarlı hata yönetimi
  - **Loglama:** ILogger ile detaylı loglama

**Oluşturulan Dosyalar:**
1. `Koala.Yedpa.Core/Services/ISalesInvoiceService.cs` (Interface)
2. `Koala.Yedpa.Service/Services/SalesInvoiceService.cs` (Implementation)
3. `Koala.Yedpa.Core/Dtos/SalesInvoice/CreateSalesInvoiceRequestDto.cs` (Güncellendi)
4. `Koala.Yedpa.Core/Dtos/SalesInvoice/CreateSalesInvoiceResponseDto.cs` (Güncellendi)

**Değiştirilen Dosyalar:**
1. `Koala.Yedpa.Service/Extentions/ServiceCollectionExtensions.cs` (DI registration eklendi)

**Sonraki Adım:** API endpoint entegrasyonu (Nastya'ya devredilecek)

## Önceki Görevler (Arşiv)

### Bulk Invoicing Faz 1 - TAMAMLANDI ✅
**Tarih:** 2026-05-13 (12:35)

#### Katya (team-lead)
- ✅ Toplu Faturalandırma (Bulk Invoicing) design spec'ini inceledi
- ✅ BudgetOrder pattern referansını analiz etti
- ✅ Faz 1 için 5 task oluşturdu (Task #1-5)
- ✅ Nataşa'ya Task #3 atadı: Entity + Migration oluştur
- ✅ Task #6: Bulk Invoice 500 Internal Server Error - Debug ve Fix (CRITICAL - ÇÖZÜLDÜ)
- ✅ Task #7: Bulk Invoice 500 Hatası - DI ve Service Kaytlarını İncele (COMPLETED)
- ✅ Task #8: BulkInvoiceService Method Implementasyonunu İncele (COMPLETED)
- ✅ Task #9: BulkInvoiceService Constructor Injection Hatasını Düzelt (CRITICAL - COMPLETED)
- ✅ Task #4: Dependency Injection ve Program.cs konfigürasyonu (zaten mevcuttu)

#### Nataşa (db-specialist)
- ✅ Task #12: PendingInvoiceViewModel ve PendingInvoiceSearchViewModel (mevcut)
- ✅ Task #9: IApiLogoSqlDataService interface güncellemeleri (mevcut)
- ✅ Task #3: Entity + Migration oluşturuldu (BulkInvoiceSession, BulkInvoiceItem)
- ✅ Migration veritabanına uygulandı (20260513074638_AddBulkInvoiceTables)

#### Olga (backend-dev)
- ✅ Task #11: BuildBasePendingInvoiceQuery SQL builder (mevcut)
- ✅ Task #14: GetPendingInvoicesAsync method (mevcut)
- ✅ Task #8: SearchPendingInvoicesAsync method (mevcut)
- ✅ Task #5: Service Layer implementasyonu (IBulkInvoiceService, BulkInvoiceService)

#### Nastya (api-dev)
- ✅ Task #16: PendingInvoices endpoints (mevcut)
- ✅ Task #15: Solution build verification ve duplicate cleanup (commit: 23ab020)
- ✅ Task #17: Final deployment (commit: 3462df3)
- ✅ Task #1: API Endpoints oluşturuldu (BulkInvoiceApiController)
- ✅ Task #7-9: BulkInvoice 500 hatasının KALICI ÇÖZÜMÜ (Constructor Injection düzeltildi)

**BULK INVOICING FAZ 1 DURUMU: TAMAMLANDI ✅**
- 5 task oluşturuldu
- 5/5 task tamamlandı
- Backend + API hazır, UI implementation bekliyor

## KRİTİK BUG FIX: 2026-05-13 (12:35)
**Sorun:** Bulk Invoice endpoint'leri 500 Internal Server Error veriyordu
**Kök Neden:** `BulkInvoiceService.GetPendingLinesAsync()` method'unda constructor injection ihlali
**Hatalı Kod:** `var sqlProvider = new Koala.Yedpa.Service.Providers.SqlProvider(_settingsService);`
**Düzeltme:** `ISqlProvider` interface'ini constructor'da inject et ve kullan
**Dosya:** `Koala.Yedpa.Service/Services/BulkInvoiceService.cs`
**Değişiklik:**
- Constructor'a `ISqlProvider sqlProvider` parametresi eklendi
- Field olarak `_sqlProvider` eklendi
- `GetPendingLinesAsync()` method'unda `new SqlProvider(...)` çağrısı `_sqlProvider` ile değiştirildi
- `using Koala.Yedpa.Core.Providers;` namespace'i eklendi
**Test Sonucu:** ✅ Endpoint artık 401 Unauthorized dönüyor (beklenen), 500 yok

### Mahmut (frontend-dev)
- ✅ Task #2: Dashboard UI implementasyonu tamamlandı (Alert + Modal)

### Gonca (qa-engineer)
- ⏳ Bulk Invoice özelliği için test bekleniyor

## Önceki Görev: Aidat Tahsilat Durumu Widget (Arşiv)
**Tarih:** 2026-05-13 (17:45)

**Amaç:** Dashboard'a 3 KPI'li widget ekle (Toplam Alacak, Ödenen, Bekleyen)
**Veri Kaynağı:** Logo DB, DOCODE='AIDAT' filtreli, current month

### Oluşturulan Task'lar:
- **Task #2** (Nataşa): Logo DB sorgusu araştırması - PENDING
  - DOCODE='AIDAT' sorgu yapısını belirle
  - DTO/entity hazırla
  - Repository pattern yaklaşımı
- **Task #1** (Olga): Aidat tahsilat servisi - PENDING (bloklu: Task #2)
  - IAidatCollectionService interface
  - 3 KPI hesaplama mantığı
- **Task #3** (Nastya): Dashboard API endpoint - PENDING (bloklu: Task #1)
  - GET /api/Dashboard/aidat-tahsilat
- **Task #4** (Mahmut): Dashboard widget UI - PENDING (bloklu: Task #3)
  - 3 KPI kartı (Metronic theme)
  - Dashboard entegrasyonu

**Durum:** Nataşa'ya görev atandı, çalışma başlıyor...
