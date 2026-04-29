# Logo Cari & Dükkan Bilgileri API Dökümantasyonu

## Genel Bilgiler

**API Adı:** LogoClCard API  
**Temel URL:** `https://api.domain.com/api/LogoClCardApi`  
**Kimlik Doğrulama:** Bearer Token (JWT)  
**Response Format:** JSON

---

## Autentikasyon

Tüm API istekleri için geçerli bir JWT token gereklidir. Token'ı request header'ında aşağıdaki formatta gönderin:

```
Authorization: Bearer <YOUR_JWT_TOKEN>
```

---

## Endpoint'ler

### 1. Tüm Dükkan Cari Bilgilerini Al (Sayfalı)

**Endpoint:** `GET /api/LogoClCardApi/ClCardInfoAll`

**Açıklama:** Defter sisteminde kayıtlı tüm dükkan cari bilgilerini sayfalı şekilde döndürür.

#### Request Parameters

| Parameter | Tip   | Zorunlu | Varsayılan | Açıklama                    |
|-----------|-------|---------|------------|-----------------------------|
| perPage   | int   | Hayır   | 50         | Sayfa başına kaç kayıt döndürülecek (max: 100) |
| pageNo    | int   | Hayır   | 1          | Sayfa numarası (1'den başlar) |

#### Request Örneği

```bash
curl -X GET "https://api.domain.com/api/LogoClCardApi/ClCardInfoAll?perPage=20&pageNo=1" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json"
```

#### Response (200 OK)

```json
{
  "data": [
    {
      "CARI_KODU": "120.001",
      "CARI_UNVAN": "ÖRNEK ŞİRKETİ A.Ş.",
      "VERGI_DAIRESI": "İstanbul",
      "VERGI_NUMARASI": "1234567890",
      "TCKN": "12345678901",
      "IL": "İstanbul",
      "ILCE": "Beşiktaş",
      "MAHALLE": "Ortaköy",
      "ADRES_1": "Cankiri Cad. No: 45",
      "ADRES_2": "",
      "EMAIL_3": "muhasebe@example.com",
      "TELEFON_1": "0212 2563000",
      "TELEFON_2": "0212 2563001",
      "FAX": "0212 2563099",
      "OZEL_KOD": "KOD1",
      "OZEL_KOD2": "KOD2",
      "OZEL_KOD3": "KOD3",
      "OZEL_KOD4": "KOD4",
      "OZEL_KOD5": "KOD5",
      "DUKKAN_CARI_KODU": "120.001.IS",
      "DUKKAN_ADRES_ORJINAL": "Cankiri Cad. No: 45 Ortaköy BESIKTAS/ISTANBUL",
      "CADDE": "Cankiri Cad.",
      "NO": "45",
      "PASAJ_NO": "",
      "YENI_NO": "45A",
      "KAT": "ZEMIN KAT",
      "YETKILI1_AD_SOYAD": "Ahmet Yılmaz",
      "YETKILI1_EMAIL": "ahmet@example.com",
      "YETKILI1_TELEFON": "0533 5555555",
      "YETKILI2_AD_SOYAD": "Fatma Kaya",
      "YETKILI2_EMAIL": "fatma@example.com",
      "YETKILI2_TELEFON": "0533 5555556"
    }
  ],
  "message": "Başarılı",
  "isSuccess": true,
  "statusCode": 200,
  "totalRecordCount": 150,
  "pageSize": 20,
  "pageNo": 1
}
```

#### Hata Yanıtları

**401 Unauthorized** - Token geçersiz veya süresi dolmuş
```json
{
  "message": "Yetkisiz erişim",
  "isSuccess": false,
  "statusCode": 401
}
```

**500 Internal Server Error** - Sunucu hatası
```json
{
  "message": "Sunucu hatası",
  "isSuccess": false,
  "statusCode": 500,
  "errors": "Hata ayrıntısı..."
}
```

---

### 2. Gelişmiş Arama (Filtreleme)

**Endpoint:** `POST /api/LogoClCardApi/ClCardInfoSearch`

**Açıklama:** Belirtilen kriterlere göre dükkan cari bilgilerini arar. Arama kriterleri opsiyoneldir; boş gönderilen alanlar göz ardı edilir.

#### Request Body Parameters

| Alan | Tip | Açıklama |
|------|-----|----------|
| CariKodu | string | Ana cari kart kodu (örn: "120.001") |
| CariUnvan | string | Cari firma adı / unvanı (kısmi eşleşme) |
| VergiDairesi | string | Vergi dairesi adı |
| VergiNumarasi | string | Vergi numarası |
| Tckn | string | T.C. Kimlik numarası |
| Il | string | İl adı |
| Ilce | string | İlçe adı |
| Mahalle | string | Mahalle / semt |
| Adres1 | string | Adres satırı 1 (kısmi eşleşme) |
| Adres2 | string | Adres satırı 2 |
| OzelKod | string | Özel kod 1 |
| OzelKod2 | string | Özel kod 2 |
| OzelKod3 | string | Özel kod 3 |
| OzelKod4 | string | Özel kod 4 |
| OzelKod5 | string | Özel kod 5 |
| DukkanCariKodu | string | Dükkan cari kodu (örn: "120.001.IS") |
| DukkanAdresOrjinal | string | Dükkan adresi (kısmi eşleşme) |
| Cadde | string | Cadde / sokak adı |
| No | string | Eski kapı numarası |
| PasajNo | string | Pasaj numarası |
| YeniNo | string | Yeni belediye kapı numarası |
| Kat | string | Kat bilgisi (ZEMIN KAT, ASMAKAT, vb.) |
| Yetkili1AdSoyad | string | 1. yetkili ad soyad |
| Yetkili2AdSoyad | string | 2. yetkili ad soyad |

#### Query Parameters

| Parameter | Tip   | Zorunlu | Varsayılan | Açıklama                    |
|-----------|-------|---------|------------|-----------------------------|
| perPage   | int   | Hayır   | 50         | Sayfa başına kaç kayıt döndürülecek |
| pageNo    | int   | Hayır   | 1          | Sayfa numarası              |

#### Request Örneği 1: Şehir ve İlçeye Göre Arama

```bash
curl -X POST "https://api.domain.com/api/LogoClCardApi/ClCardInfoSearch?perPage=25&pageNo=1" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "Il": "İstanbul",
    "Ilce": "Beşiktaş"
  }'
```

#### Request Örneği 2: Firma Adına Göre Arama

```bash
curl -X POST "https://api.domain.com/api/LogoClCardApi/ClCardInfoSearch?perPage=10&pageNo=1" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "CariUnvan": "ÖRNEK"
  }'
```

#### Request Örneği 3: Vergi Numarasına Göre Arama

```bash
curl -X POST "https://api.domain.com/api/LogoClCardApi/ClCardInfoSearch?perPage=50&pageNo=1" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "VergiNumarasi": "1234567890"
  }'
```

#### Response (200 OK)

```json
{
  "data": [
    {
      "CARI_KODU": "120.001",
      "CARI_UNVAN": "ÖRNEK ŞİRKETİ A.Ş.",
      "VERGI_DAIRESI": "İstanbul",
      "VERGI_NUMARASI": "1234567890",
      "TCKN": "12345678901",
      "IL": "İstanbul",
      "ILCE": "Beşiktaş",
      "MAHALLE": "Ortaköy",
      "ADRES_1": "Cankiri Cad. No: 45",
      "ADRES_2": "",
      "EMAIL_3": "muhasebe@example.com",
      "TELEFON_1": "0212 2563000",
      "TELEFON_2": "0212 2563001",
      "FAX": "0212 2563099",
      "OZEL_KOD": "KOD1",
      "OZEL_KOD2": "KOD2",
      "OZEL_KOD3": "KOD3",
      "OZEL_KOD4": "KOD4",
      "OZEL_KOD5": "KOD5",
      "DUKKAN_CARI_KODU": "120.001.IS",
      "DUKKAN_ADRES_ORJINAL": "Cankiri Cad. No: 45 Ortaköy BESIKTAS/ISTANBUL",
      "CADDE": "Cankiri Cad.",
      "NO": "45",
      "PASAJ_NO": "",
      "YENI_NO": "45A",
      "KAT": "ZEMIN KAT",
      "YETKILI1_AD_SOYAD": "Ahmet Yılmaz",
      "YETKILI1_EMAIL": "ahmet@example.com",
      "YETKILI1_TELEFON": "0533 5555555",
      "YETKILI2_AD_SOYAD": "Fatma Kaya",
      "YETKILI2_EMAIL": "fatma@example.com",
      "YETKILI2_TELEFON": "0533 5555556"
    },
    {
      "CARI_KODU": "120.002",
      "CARI_UNVAN": "ÖRNEK 2 LTD. ŞTİ.",
      "VERGI_DAIRESI": "İstanbul",
      "VERGI_NUMARASI": "1234567891",
      "TCKN": "98765432109",
      "IL": "İstanbul",
      "ILCE": "Beşiktaş",
      "MAHALLE": "Akaretler",
      "ADRES_1": "Muallim Naci Cad. No: 12",
      "ADRES_2": "Daire 3",
      "EMAIL_3": "info@example2.com",
      "TELEFON_1": "0212 3456789",
      "TELEFON_2": "0212 3456790",
      "FAX": "0212 3456799",
      "OZEL_KOD": "KOD1",
      "OZEL_KOD2": "",
      "OZEL_KOD3": "",
      "OZEL_KOD4": "",
      "OZEL_KOD5": "",
      "DUKKAN_CARI_KODU": "120.002.IS",
      "DUKKAN_ADRES_ORJINAL": "Muallim Naci Cad. No: 12/3 Akaretler BESIKTAS/ISTANBUL",
      "CADDE": "Muallim Naci Cad.",
      "NO": "12",
      "PASAJ_NO": "",
      "YENI_NO": "12B",
      "KAT": "3. KAT",
      "YETKILI1_AD_SOYAD": "Mehmet Demir",
      "YETKILI1_EMAIL": "mehmet@example2.com",
      "YETKILI1_TELEFON": "0533 6666666",
      "YETKILI2_AD_SOYAD": "",
      "YETKILI2_EMAIL": "",
      "YETKILI2_TELEFON": ""
    }
  ],
  "message": "Başarılı",
  "isSuccess": true,
  "statusCode": 200,
  "totalRecordCount": 2,
  "pageSize": 25,
  "pageNo": 1
}
```

#### Hata Yanıtları

**400 Bad Request** - Geçersiz istek
```json
{
  "message": "Arama modeli boş olamaz",
  "isSuccess": false,
  "statusCode": 400,
  "errors": "Model null"
}
```

**401 Unauthorized** - Token geçersiz veya süresi dolmuş
```json
{
  "message": "Yetkisiz erişim",
  "isSuccess": false,
  "statusCode": 401
}
```

**500 Internal Server Error** - Sunucu hatası
```json
{
  "message": "Sunucu hatası",
  "isSuccess": false,
  "statusCode": 500
}
```

---

## Response Yapısı

Tüm başarılı yanıtlar aşağıdaki genel yapıyı takip eder:

```json
{
  "data": [ /* Array of ClCardInfoViewModel */ ],
  "message": "Başarılı",
  "isSuccess": true,
  "statusCode": 200,
  "totalRecordCount": 150,
  "pageSize": 20,
  "pageNo": 1
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| data | Array | Dükkan cari bilgileri dizisi |
| message | string | İşlem sonucu mesajı |
| isSuccess | boolean | İşlemin başarılı olup olmadığını gösterir |
| statusCode | int | HTTP durum kodu |
| totalRecordCount | int | Toplam kayıt sayısı |
| pageSize | int | Sayfa başına kayıt sayısı |
| pageNo | int | Sayfa numarası |

---

## HTTP Durum Kodları

| Kod | Açıklama |
|-----|----------|
| 200 | OK - İstek başarılı |
| 400 | Bad Request - Geçersiz istek parametreleri |
| 401 | Unauthorized - Kimlik doğrulama başarısız |
| 500 | Internal Server Error - Sunucu hatası |

---

## Paging (Sayfalama)

API, büyük veri setleri için sayfalama desteği sağlar.

**Örnek:** 50 kayıt başına 3. sayfayı almak
```
GET /api/LogoClCardApi/ClCardInfoAll?perPage=50&pageNo=3
```

Response'ta dönen `totalRecordCount`, `pageSize` ve `pageNo` değerleri kullanarak toplam sayfa sayısını hesaplayabilirsiniz:

```
Toplam Sayfa Sayısı = Math.Ceiling(totalRecordCount / pageSize)
```

---

## Alan Açıklamaları

### Ana Cari Bilgileri
- **CARI_KODU**: Defter sisteminde tanımlı ana cari kart kodu (örn: "120.001")
- **CARI_UNVAN**: Cari kartın firma adı veya unvanı
- **VERGI_DAIRESI**: Vergi dairesinin bulunduğu şehir/semt
- **VERGI_NUMARASI**: Kurumsal vergi numarası
- **TCKN**: Şahıs firmalar için T.C. Kimlik numarası

### Adres Bilgileri
- **IL**: İl adı
- **ILCE**: İlçe adı
- **MAHALLE**: Mahalle veya semt adı
- **ADRES_1**: Birincil adres satırı
- **ADRES_2**: İkincil adres satırı (Daire no, Binaarka No vb.)
- **EMAIL_3**: Muhasebe veya hesaplarla ilgili e-posta adresi

### Dükkan Adresi Bilgileri (Logo Defter Sisteminden)
- **DUKKAN_CARI_KODU**: ".IS" ile biten dükkan cari kodu (örn: "120.001.IS")
- **DUKKAN_ADRES_ORJINAL**: Logo defterindeki ham dükkan adresi tanımı
- **CADDE**: Dükkan'ın bulunduğu cadde veya sokak adı
- **NO**: Eski kapı numarası
- **PASAJ_NO**: Pasaj numarası (örn: "1. PASAJ")
- **YENI_NO**: Yeni belediye tarafından atanan kapı numarası
- **KAT**: Dükkan'ın bulunduğu kat bilgisi (ZEMIN KAT, ASMAKAT, vb.)

### İletişim Bilgileri
- **TELEFON_1**: Ana telefon numarası
- **TELEFON_2**: Alternatif telefon numarası
- **FAX**: Faks numarası

### Özel Kodlar
- **OZEL_KOD** - **OZEL_KOD5**: Müşteri tarafından tanımlanmış özel kategoriler ve kodlar

### Yetkili Kişiler
- **YETKILI1_AD_SOYAD**: 1. Yetkili kişinin adı soyadı
- **YETKILI1_EMAIL**: 1. Yetkili e-posta adresi
- **YETKILI1_TELEFON**: 1. Yetkili telefon numarası
- **YETKILI2_AD_SOYAD**: 2. Yetkili kişinin adı soyadı
- **YETKILI2_EMAIL**: 2. Yetkili e-posta adresi
- **YETKILI2_TELEFON**: 2. Yetkili telefon numarası

---

## Başarılı Entegrasyonun Kontrol Listesi

- [ ] API token'ı doğru şekilde alınmış ve kullanılıyor
- [ ] Authorization header'ı her request'te gönderiliyor
- [ ] Request parametreleri doğru formatta gönderiliyor
- [ ] Response'taki `isSuccess` alanı kontrol ediliyor
- [ ] Hata durumlarında `statusCode` ve `message` kontrol ediliyor
- [ ] Sayfalama için `totalRecordCount` ve `pageSize` kullanılıyor

---

## Destek

API ile ilgili sorularınız veya sorunlarınız için lütfen sistem yöneticisine veya geliştirme ekibine başvurunuz.

**Versiyon:** 1.0  
**Güncelleme Tarihi:** 2026-04-21
