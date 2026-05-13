# Toplu Faturalandırma (Bulk Invoicing) Design

## Goal
Her ayın 15'inden itibaren dashboard'ta alert göstererek, faturalandırılmamış aidat sipariş satırlarının (ORFLINE) toplu faturalandırılmasını sağlamak. BudgetOrder modülünün pattern'ini takip eder.

## Architecture
BudgetOrder pattern: Queue-based processing, per-item status tracking, email raporlama ile Excel ekli.

## Scope
- Dashboard alert + modal UI (Metronic Bootstrap)
- ORFLINE sorgu endpoint'leri
- Background transfer service (Logo REST API - JSON data gelecek)
- Email raporlama + Excel ek
- Ilk faz: sadece listeleme ve queue'ya alma. Aktarım logic JSON data geldikten sonra eklenecek.

## Users
Tüm kullanıcılar alert'i gorecek ve faturalandırma baslatabilecek.

---

## Data Model

### BulkInvoiceSessions
Her ayin faturalandirma oturumunu tutar.

| Field | Type | Description |
|-------|------|-------------|
| Id | int (PK) | Auto increment |
| InvoiceDate | DateTime | Kullanicinin sectigi fatura tarihi (hem islem hem vade) |
| Month | int | Hangi ay (1-12) |
| Year | int | Hangi yil |
| Status | int | 0=Pending, 1=Processing, 2=Completed, 3=Failed |
| CreatedBy | string | Islemi baslatan kullanici |
| CreatedAt | DateTime | Olusturma tarihi |
| CompletedAt | DateTime? | Tamamlanma tarihi |

### BulkInvoiceItems
Her fatura satirinin durumu.

| Field | Type | Description |
|-------|------|-------------|
| Id | int (PK) | Auto increment |
| SessionId | int (FK) | BulkInvoiceSessions.Id |
| OrficheRef | int | Logo ORFICHE.LOGICALREF |
| Orflineref | int | Logo ORFLINE.LOGICALREF |
| ClientCode | string | Cari kod |
| ClientName | string | Cari adı |
| Amount | decimal | Tutar |
| MonthName | string | Ay adi (HAZIRAN gibi) |
| Status | int | 0=Pending, 1=Transferred, 2=Failed |
| LogoInvoiceRef | int? | Logo'da olusturulan fatura ref |
| ErrorMessage | string? | Hata mesaji |

### Alert Kontrolu
Ayin 15'inden sonra `BulkInvoiceSessions` tablosunda o ay icin kayit varsa alert gizlenir. Kayit yoksa her dashboard yuklemesinde gosterilir.

---

## UI Flow

### Dashboard Alert
- Dashboard'a her yuklemede AJAX ile `GET /api/BulkInvoice/check-alert` cagrilir
- Ayin 15'inden sonra ve o ay icin session yoksa alert gosterilir
- Alert'e tiklaninca modal acilir

### Modal (Metronic Bootstrap Pattern)
- Ust kisim: tarih secici (datepicker) - tek tarih, hem islem hem vade olarak kullanilir
- Alt kisim: faturalandirilmamis ORFLINE satirlari DataTable olarak listelenir
- Her satirda checkbox ile secim, "Tumunu Sec" checkbox'i
- Alt kisimda toplam tutar ve "Faturalandir" butonu
- Kaydet sonrasi session olusturulur, alert bu ay icin bir daha gosterilmez

---

## Backend Flow

### API Endpoints

1. **GET /api/BulkInvoice/check-alert**
   - Ayin 15'i gecmis mi + o ay icin session var mi kontrol eder
   - Return: `{ showAlert: true/false }`

2. **GET /api/BulkInvoice/pending-lines**
   - ORFLINE'dan faturalandirilmamis satirlari ceker
   - Sorgu: `ORF.DOCODE='AIDAT' AND ORL.TRGFLAG=0 AND ORL.LINENO_={currentMonth} AND ORF.CANCELLED=0`
   - CLOSED alanina gore odenen/odenmeyen durumu gosterilir
   - Return: Pending invoice line listesi

3. **POST /api/BulkInvoice/create-session**
   - Secili satirlar + tarih alir
   - BulkInvoiceSession + BulkInvoiceItems olusturur
   - Queue'ya ekler
   - Return: Session ID

4. **GET /api/BulkInvoice/session-status/{id}**
   - Session durumunu dondurur (progress tracking icin)

### Background Processing (BudgetOrder Pattern)

1. Queue-based architecture: `BulkInvoiceTransferQueue`
2. Background service: `BulkInvoiceTransferBackgroundService`
3. Akis:
   - Queue'dan session alir
   - Session status = Processing
   - Her item icin Logo REST API ile fatura olusturur (JSON data - TODO)
   - Per-item status gunceller (Transferred/Failed)
   - Session status = Completed
   - Email raporu gonderir

### Email Reporting
- Tamamlandiktan sonra otomatik email gonderilir
- Icerik: basari/hata sayilari
- Excel ek: Sheet 1 basarili, Sheet 2 hatali kayitlar

---

## ORFLINE Query

```sql
SELECT
    ORF.LOGICALREF AS OrficheRef,
    ORL.LOGICALREF AS Orflineref,
    ORF.CLIENTREF AS ClientRef,
    ORF.CODE AS ClientCode,
    ORF.CLIENTREFNAME AS ClientName,
    ORL.AMOUNT AS Amount,
    ORL.LINEEXP AS MonthName,
    ORL.CLOSED AS ClosedStatus,
    ORL.LINENO_ AS LineNo
FROM LG_{firm}_{period}_ORFICHE ORF
INNER JOIN LG_{firm}_{period}_ORFLINE AS ORL ON ORL.ORDFICHEREF=ORF.LOGICALREF
WHERE ORF.DOCODE='AIDAT'
  AND ORL.TRGFLAG=0
  AND ORL.LINENO_ = {currentMonth}
  AND ORF.CANCELLED=0
```

`{firm}` ve `{period}` LogoSetting'den alinir.

---

## Implementation Phases

### Faz 1: Listeleme ve UI (simdilik)
- Entity + Migration (BulkInvoiceSessions, BulkInvoiceItems)
- API endpoints (check-alert, pending-lines, create-session, session-status)
- Dashboard alert + modal UI
- ORFLINE sorgu servisi

### Faz 2: Aktarim (JSON data geldikten sonra)
- Logo REST API fatura olusturma logic
- Background transfer service
- Email raporlama + Excel

---

## Constraints
- Turkish language UI
- Metronic Bootstrap pattern for modals
- BudgetOrder pattern for queue/transfer
- LogoSetting.Firm ve LogoSetting.Period kullanilacak
- LINENO_ = bulundugumuz ay (DATEPART(mm, GETDATE()))
- Tek tarih secimi: hem PROCDATE hem DATE_ icin
