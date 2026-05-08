# Sistem Koala YEDPA API Dokümantasyonu

**Sürüm:** v1.0
**Tarih:** Mayıs 2026
**İletişim:** info@sistem-bilgi.com
**Sağlayıcı:** Sistem Bilgisayar

---

## İçindekiler

1. [Genel Bakış](#1-genel-bakiş)
2. [Kimlik Doğrulama](#2-kimlik-doğrulama)
3. [Standart Yanıt Yapısı](#3-standart-yani̇t-yapisi)
4. [Hata Kodları](#4-hata-kodlari)
5. [Sağlık Kontrolü (Health Check)](#5-sağlik-kontrolü-health-check)
6. [Cari Kart (Müşteri Bilgileri)](#6-cari-kart-müşteri-bilgileri)
7. [Veri Modelleri](#7-veri-modelleri̇)

---

## 1. Genel Bakış

Sistem Koala YEDPA API, Yedpa Ticaret Merkezi yönetim sistemi için geliştirilmiş RESTful bir web servistir. Logo muhasebe entegrasyonu ile müşteri bilgileri, ekstre ve fatura sorgulama hizmetlerini sunar.

**Temel URL:** `https://<sunucu-adresi>/api`

**Protokol:** HTTPS (Tüm istekler SSL üzerinden yapılmalıdır)

---

## 2. Kimlik Doğrulama

API, **JWT Bearer Token** tabanlı kimlik doğrulama kullanır.

### 2.1 Token Kullanımı

Tüm korumalı endpoint'lere istek yaparken, HTTP header'a token eklenmelidir:

```
Authorization: Bearer <token>
```

### 2.2 Yetki Kapsamları (Scopes)

| Scope Kodu | Açıklama |
|-----------|----------|
| `sc-190101` | **CurrentAccount** — Cari kart ve genel API erişimi 

### 2.3 Yetkilendirme Kuralları

- Health Check endpoint'lerinin bir kısmı **token gerektirmez**
- Tüm Cari Kart endpoint'leri **en az `CurrentAccount` yetkisi** gerektirir

---

## 3. Standart Yanıt Yapısı

### 3.1 Başarılı Yanıt (Tek Kayıt)

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "İşlem başarılı",
  "data": { }
}
```

### 3.2 Başarılı Yanıt (Liste)

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "İşlem başarılı",
  "data": [ ],
  "recordsTotal": 150,
  "recordsFiltered": 95,
  "recordsShow": 50
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `recordsTotal` | int | Toplam kayıt sayısı |
| `recordsFiltered` | int | Filtrelenmiş kayıt sayısı |
| `recordsShow` | int | Bu sayfada gösterilen kayıt sayısı |

### 3.3 Hata Yanıtı

```json
{
  "isSuccess": false,
  "statusCode": 400,
  "message": "Hata açıklaması",
  "errors": {
    "errors": ["Detaylı hata mesajı"],
    "isShow": true
  }
}
```

---

## 4. Hata Kodları

| HTTP Kodu | Anlamı | Açıklama |
|-----------|--------|----------|
| `200` | OK | İstek başarılı |
| `201` | Created | Kayıt başarıyla oluşturuldu |
| `204` | No Content | Kayıt bulunamadı |
| `400` | Bad Request | Geçersiz istek veya parametre |
| `401` | Unauthorized | Token eksik veya geçersiz |
| `403` | Forbidden | Yetkisiz erişim |
| `404` | Not Found | Kaynak bulunamadı |
| `500` | Internal Server Error | Sunucu hatası |

---

## 5. Sağlık Kontrolü (Health Check)

API'nin erişilebilirliğini ve sistem durumunu kontrol etmek için kullanılır.

### 5.1 Temel Sağlık Kontrolü (Token Gerektirmez)

```
GET /api/HealthCheckApiController
```

**Yanıt:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "API aktif"
}
```

### 5.2 Detaylı Sistem Bilgisi (Token Gerektirmez)

```
GET /api/HealthCheckApiController/detailed
```

**Yanıt:** Sistem durumu, versiyon ve ortam bilgilerini döner.

### 5.3 Token ile Sağlık Kontrolü

```
GET /api/HealthCheckApiController/token
GET /api/HealthCheckApiController/detailed/token
```

**Header:** `Authorization: Bearer <token>`

Kimlik doğrulamanın çalışıp çalışmadığını test etmek için kullanılır.

---

## 6. Cari Kart (Müşteri Bilgileri)

Logo muhasebe sistemi ile entegre çalışan cari kart modülüdür. Müşteri bilgileri sorgulama, fatura takibi ve ekstre görüntüleme işlemlerini kapsar.

### 6.1 Tüm Müşterileri Listele

```
GET /api/LogoClCardApiController/ClCardInfoAll?perPage=50&pageNo=1
```

**Query Parametreleri:**

| Parametre | Tip | Varsayılan | Açıklama |
|-----------|-----|------------|----------|
| `perPage` | int | 50 | Sayfa başına kayıt sayısı |
| `pageNo` | int | 1 | Sayfa numarası |

**Yanıt:** `List<ClCardInfoViewModel>` — Tüm müşteri listesi (sayfalanmış)

**Örnek İstek:**
```http
GET /api/LogoClCardApiController/ClCardInfoAll?perPage=25&pageNo=2
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

**Örnek Yanıt:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Cari bilgileri başarıyla getirildi",
  "data": [
    {
      "CARI_KODU": "120.01",
      "CARI_UNVAN": "ABC Ticaret A.Ş.",
      "IL": "İstanbul",
      "TELEFON_1": "0216 123 4567"
    }
  ],
  "recordsTotal": 350,
  "recordsFiltered": 350,
  "recordsShow": 25
}
```

---

### 6.2 Müşteri Arama

Birden fazla kriter ile müşteri araması yapar. Tüm alanlar opsiyoneldir; boş bırakılan alanlar filtreye dahil edilmez. Birden fazla alan ile eşleşme (AND) yapılır.

```
POST /api/LogoClCardApiController/ClCardInfoSearch
Content-Type: application/json
```

**İstek Gövdesi (ClCardInfoSearchViewModel):**

```json
{
  "CariKodu": "120.01",
  "CariUnvan": "Ahmet",
  "VergiDairesi": "",
  "VergiNumarasi": "",
  "Tckn": "",
  "Il": "İstanbul",
  "Ilce": "",
  "Mahalle": "",
  "Adres1": "",
  "Adres2": "",
  "OzelKod": "",
  "OzelKod2": "",
  "OzelKod3": "",
  "OzelKod4": "",
  "OzelKod5": "",
  "DukkanCariKodu": "",
  "DukkanAdresOrjinal": "",
  "Cadde": "",
  "No": "",
  "PasajNo": "",
  "YeniNo": "",
  "Kat": "",
  "Yetkili1AdSoyad": "",
  "Yetkili2AdSoyad": ""
}
```

**Yanıt:** `List<ClCardInfoViewModel>`

---

### 6.3 Bekleyen Faturalar

Ödenmemiş fatura listesini sayfalı olarak getirir.

```
GET /api/LogoClCardApiController/PendingInvoices?perPage=50&pageNo=1
```

**Query Parametreleri:**

| Parametre | Tip | Varsayılan | Açıklama |
|-----------|-----|------------|----------|
| `perPage` | int | 50 | Sayfa başına kayıt sayısı |
| `pageNo` | int | 1 | Sayfa numarası |

**Yanıt:** `List<PendingInvoiceViewModel>` — Ödenmemiş fatura listesi

**Örnek Yanıt:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "data": [
    {
      "CustomerCode": "120.01",
      "CustomerName": "ABC Ticaret A.Ş.",
      "InvoiceNumber": "INV-2026-001",
      "InvoiceDate": "2026-04-15T00:00:00",
      "InvoiceDueAmount": 15000.00,
      "PaidAmount": 5000.00,
      "RemainingAmount": 10000.00,
      "DueDate": "2026-05-15T00:00:00",
      "RemainingDays": 8,
      "Status": "Açık"
    }
  ]
}
```

---

### 6.4 Bekleyen Fatura Arama

Fatura numarası, müşteri bilgisi veya vade aralığı ile fatura araması yapar.

```
POST /api/LogoClCardApiController/PendingInvoicesSearch
Content-Type: application/json
```

**İstek Gövdesi (PendingInvoiceSearchViewModel):**

```json
{
  "CustomerCode": "120.01",
  "CustomerName": "",
  "DueDateStart": "2026-01-01",
  "DueDateEnd": "2026-12-31",
  "InvoiceNumber": ""
}
```

> Tüm alanlar opsiyoneldir.

**Yanıt:** `List<PendingInvoiceViewModel>`

---

### 6.5 Detaylı Cari Ekstre

Belirli bir cari hesabın gruplandırılmış detaylı ekstresini getirir.

```
GET /api/LogoClCardApiController/ClCardStatementDetailed?clCardCode={cariKodu}&startDate={baslangicTarihi}&endDate={bitisTarihi}
```

**Query Parametreleri:**

| Parametre | Tip | Zorunlu | Açıklama |
|-----------|-----|---------|----------|
| `clCardCode` | string | Evet | Cari kodu |
| `startDate` | string (yyyy-MM-dd) | Hayır | Başlangıç tarihi |
| `endDate` | string (yyyy-MM-dd) | Hayır | Bitiş tarihi |

**Örnek İstek:**
```http
GET /api/LogoClCardApiController/ClCardStatementDetailed?clCardCode=120.01&startDate=2026-01-01&endDate=2026-12-31
```

**Yanıt:** `ClCardStatementDetailedViewModel` — Gruplandırılmış detaylı ekstre (satır detayları dahil)

---

### 6.6 Bakiyeli Müşteri Listesi

Güncel bakiye, son satış ve son ödeme bilgileri ile müşteri listesini getirir.

```
GET /api/LogoClCardApiController/CustomerListWithBalance?perPage=50&pageNo=1
```

**Query Parametreleri:**

| Parametre | Tip | Varsayılan | Açıklama |
|-----------|-----|------------|----------|
| `perPage` | int | 50 | Sayfa başına kayıt sayısı |
| `pageNo` | int | 1 | Sayfa numarası |

**Yanıt:** `List<CustomerListWithBalanceViewModel>` — Bakiye bilgisi ile müşteri listesi

**Örnek Yanıt:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "data": [
    {
      "Code": "120.01",
      "Definition": "ABC Ticaret A.Ş.",
      "City": "İstanbul",
      "Balance": 25000.00,
      "LastSaleDate": "2026-04-28T00:00:00",
      "LastPaymentDate": "2026-04-15T00:00:00"
    }
  ]
}
```

---

## 7. Veri Modelleri

### 7.1 ClCardInfoViewModel (Müşteri Bilgileri)

| Alan | Tip | Açıklama |
|------|-----|----------|
| `CARI_KODU` | string | Cari kodu (benzersiz) |
| `CARI_UNVAN` | string | Cari ünvan / firma adı |
| `VERGI_DAIRESI` | string | Vergi dairesi |
| `VERGI_NUMARASI` | string | Vergi numarası |
| `TCKN` | string | T.C. kimlik numarası |
| `IL` | string | İl |
| `ILCE` | string | İlçe |
| `MAHALLE` | string | Mahalle |
| `ADRES_1` | string | Adres satırı 1 |
| `ADRES_2` | string | Adres satırı 2 |
| `EMAIL_3` | string | E-posta |
| `TELEFON_1` | string | Telefon 1 |
| `TELEFON_2` | string | Telefon 2 |
| `FAX` | string | Faks |
| `OZEL_KOD` | string | Özel kod 1 |
| `OZEL_KOD2` | string | Özel kod 2 |
| `OZEL_KOD3` | string | Özel kod 3 |
| `OZEL_KOD4` | string | Özel kod 4 |
| `OZEL_KOD5` | string | Özel kod 5 |
| `DUKKAN_CARI_KODU` | string | Dükkan cari kodu |
| `DUKKAN_ADRES_ORJINAL` | string | Dükkan orijinal adres |
| `CADDE` | string | Cadde adı |
| `NO` | string | Bina numarası |
| `PASAJ_NO` | string | Pasaj numarası |
| `YENI_NO` | string | Yeni numara |
| `KAT` | string | Kat |
| `YETKILI1_AD_SOYAD` | string | 1. Yetkili ad soyad |
| `YETKILI1_EMAIL` | string | 1. Yetkili e-posta |
| `YETKILI1_TELEFON` | string | 1. Yetkili telefon |
| `YETKILI2_AD_SOYAD` | string | 2. Yetkili ad soyad |
| `YETKILI2_EMAIL` | string | 2. Yetkili e-posta |
| `YETKILI2_TELEFON` | string | 2. Yetkili telefon |

### 7.2 PendingInvoiceViewModel (Bekleyen Fatura)

| Alan | Tip | Açıklama |
|------|-----|----------|
| `CustomerReference` | int | Müşteri referans ID |
| `CustomerCode` | string | Müşteri kodu |
| `CustomerName` | string | Müşteri adı |
| `InvoiceLogicalRef` | int | Fatura referans ID |
| `InvoiceNumber` | string | Fatura numarası |
| `InvoiceDate` | datetime | Fatura tarihi |
| `InvoiceType` | string | Fatura türü |
| `InvoiceDescription1` | string | Fatura açıklaması 1 |
| `InvoiceDescription2` | string | Fatura açıklaması 2 |
| `InvoiceDueAmount` | decimal | Fatura toplam tutarı |
| `PaidAmount` | decimal | Ödenen tutar |
| `RemainingAmount` | decimal | Kalan tutar |
| `DueDate` | datetime | Vade tarihi |
| `Month` | int | Ay |
| `Week` | int | Hafta |
| `DueDays` | int | Vade gün sayısı |
| `RemainingDays` | int | Kalan gün sayısı |
| `CurrencyType` | string | Para birimi |
| `Status` | string | Durum |

### 7.3 ClCardStatementDetailedViewModel (Detaylı Ekstre)

| Alan | Tip | Açıklama |
|------|-----|----------|
| `LogicalRef` | int | Cari referans ID |
| `ClCode` | string | Cari kodu |
| `ClTitle` | string | Cari ünvanı |
| `Balance` | decimal | Güncel bakiye |
| `StatementGroups` | list | Ekstre grupları |

**StatementDetailedGroupViewModel (Ekstre Grubu):**

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Date` | datetime | Tarih |
| `FicheNo` | string | Fiş numarası |
| `FicheType` | string | Fiş türü |
| `Description` | string | Açıklama |
| `Donumber` | string | İrsaliye/DO numarası |
| `Debit` | decimal | Borç (Borçlandırıcı) |
| `Credit` | decimal | Alacak (Alacaklandırıcı) |
| `Discount` | decimal | İndirim |
| `TaxTotal` | decimal | Vergi toplamı |
| `Balance` | decimal | Bakiye |
| `ExpType` | string | Açıklama türü |
| `Lines` | list | Ekstre satırları |

**StatementDetailedLineViewModel (Ekstre Satırı):**

| Alan | Tip | Açıklama |
|------|-----|----------|
| `ItemCode` | string | Stok kodu |
| `ItemName` | string | Stok adı |
| `Amount` | decimal | Miktar |
| `Unit` | string | Birim |
| `UnitPrice` | decimal | Birim fiyat |
| `LineNet` | decimal | Satır net tutar |
| `LineTotal` | decimal | Satır toplam |
| `Vat` | decimal | KDV |
| `LineType` | string | Satır türü |

### 7.4 CustomerListWithBalanceViewModel (Bakiyeli Müşteri)

| Alan | Tip | Açıklama |
|------|-----|----------|
| `LogicalRef` | int | Referans ID |
| `Code` | string | Cari kodu |
| `Definition` | string | Cari ünvanı |
| `City` | string | İl |
| `Town` | string | İlçe |
| `District` | string | Semt/Mahalle |
| `Addr1` | string | Adres satırı 1 |
| `Addr2` | string | Adres satırı 2 |
| `Balance` | decimal | Güncel bakiye |
| `LastSaleDate` | datetime? | Son satış tarihi |
| `LastPaymentDate` | datetime? | Son ödeme tarihi |

### 7.5 ClCardInfoSearchViewModel (Müşteri Arama Kriterleri)

Tüm alanlar opsiyoneldir. Boş bırakılan alanlar filtreye dahil edilmez.

| Alan | Tip | Açıklama |
|------|-----|----------|
| `CariKodu` | string? | Cari kodu ile arama |
| `CariUnvan` | string? | Cari ünvan ile arama |
| `VergiDairesi` | string? | Vergi dairesi ile arama |
| `VergiNumarasi` | string? | Vergi numarası ile arama |
| `Tckn` | string? | T.C. kimlik no ile arama |
| `Il` | string? | İl ile arama |
| `Ilce` | string? | İlçe ile arama |
| `Mahalle` | string? | Mahalle ile arama |
| `Adres1` | string? | Adres satırı 1 ile arama |
| `Adres2` | string? | Adres satırı 2 ile arama |
| `OzelKod` - `OzelKod5` | string? | Özel kodlar ile arama |
| `DukkanCariKodu` | string? | Dükkan cari kodu ile arama |
| `Cadde` | string? | Cadde ile arama |
| `No` | string? | Bina no ile arama |
| `PasajNo` | string? | Pasaj no ile arama |
| `Kat` | string? | Kat ile arama |
| `Yetkili1AdSoyad` | string? | 1. yetkili ad soyad ile arama |
| `Yetkili2AdSoyad` | string? | 2. yetkili ad soyad ile arama |

### 7.6 PendingInvoiceSearchViewModel (Fatura Arama Kriterleri)

Tüm alanlar opsiyoneldir.

| Alan | Tip | Açıklama |
|------|-----|----------|
| `CustomerCode` | string? | Müşteri kodu |
| `CustomerName` | string? | Müşteri adı |
| `DueDateStart` | datetime? | Vade başlangıç tarihi |
| `DueDateEnd` | datetime? | Vade bitiş tarihi |
| `InvoiceNumber` | string? | Fatura numarası |

---

## Destek

Teknik destek ve bilgi için:

- **E-posta:** info@sistem-bilgi.com
- **Sağlayıcı:** Sistem Bilgisayar
- **Swagger UI:** `https://<sunucu-adresi>/swagger`
