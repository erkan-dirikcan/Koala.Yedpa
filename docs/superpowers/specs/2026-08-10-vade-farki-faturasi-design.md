# Vade Farkı Faturası Endpoint'i — Tasarım

**Tarih:** 2026-08-10
**Durum:** ⏸ Keşif tamamlandı ve kanıtlandı. **Tasarım kullanıcı onayı bekliyor** — bölüm 5 okunup
onaylanmadan kod yazılmayacak.
**Branch:** feature/bulk-invoice-rewrite

---

## 1. Amaç

WebApi'ye, dışarıdan gelen istekle Logo Tiger'da **Verilen Vade Farkı Faturası** oluşturan bir
endpoint eklemek.

**Vade farkını biz hesaplamıyoruz.** Ayrı bir uygulama, açık faturalar endpoint'inden aldığı
veriyle (son güncellenme tarihi, kalan/ödenen tutar) gecikme farkını hesaplayacak ve bize hazır
tutarla fatura oluşturma talebi gönderecek. Bizim sorumluluğumuz: talebi doğru Logo payload'ına
çevirip göndermek, sonucu kalıcı kaydetmek, çift faturayı engellemek.

Kapsam: **tek istek = tek fatura.** Toplu/otomatik tetikleme bu spec'in dışında.

---

## 2. Keşif — nasıl kanıtlandı

Bu bölüm, aynı yanlış yollara tekrar girilmemesi için tutuluyor. İki hipotez ölçümle çürütüldü.

### 2.1 Elenen aday: `paymentDifferenceInvoices`

İsim birebir "vade farkı faturası" gibi durduğu için ilk aday buydu. **Yanlış.**

- Swagger tag'i: `Satış ve Dağıtım | Hareketler | **Fiyat Farkı** Faturaları`. Vade farkı değil.
- `GET /paymentDifferenceInvoices?withCount=true` → **62.013** kayıt. Bu bir fatura listesi değil;
  `PAYTRANS` (borç takip) satırları + ilişkili faturanın alanları.
  Kanıt: `withcount.json`, `withreferance.json` (repo kökü).
- Dönen kayıtların hiçbirinde `INVOICE_TRCODE = 11` yok, hepsinde `INVOICE_INTERESTAPP = 0`.

**Ders:** Logo REST kaynaklarının İngilizce adları yanıltıcı olabiliyor. Tek güvenilir kaynak,
`swaggerFiles/v1/swagger.json` içindeki **tag'lerin Logo menü yolu**.

### 2.2 Elenen aday: `ArpSlips`

Logo ekranının başlığındaki **(41)** ve formda satır grid'i olmaması, belgeyi cari hesap fişi
gibi gösterdi. **Yanlış.**

İki ayrı numaralandırma var ve karıştırılıyor:

```
ReceiptTypeNo = (MODULENR * 100) + TRCODE
441 → MODULENR=4 (Fatura modülü) / TRCODE=41   ← ekrandaki "(41)"
aynı belgenin INVOICE.TRCODE'u ise 11
```

Sabit +30 kayması: 7→37, 8→38, 9→39, **11→41**, 14→44.
Eşleme kodda zaten var: `ApiLogoSqlDataService.cs:366`.

`MODULENR=4` olduğu için belge **Fatura modülünde**. `ArpSlips` MODULENR=5'tir → yanlış kapı.

### 2.3 Doğrulanan hedef: `salesInvoices` + `TYPE=11`

Logo arayüzünde 2026-08-10'da elle kesilmiş iki gerçek fatura REST'ten okundu:

| Dosya | Fiş No | Tip |
|---|---|---|
| `VADEFARKI001.json` | YED2026000018594 | e-Fatura |
| `VADEFARKI002.json` | YDA2026000008696 | e-Arşiv |

İkisi de `GET /api/v1/salesInvoices/{ref}` ile geldi ve **`TYPE: 11`** taşıyor. Hedef kesin.

---

## 3. Kanıtlanmış Logo payload'ı

### 3.1 Gönderilecek alanlar

```json
{
  "GRPCODE": 2,
  "TYPE": 11,
  "NUMBER": "~",
  "DATE": "2026-08-10",
  "TIME": 168432128,
  "DOC_NUMBER": "VADE FARKI",
  "ARP_CODE": "1.H1P1.049.00.K2",
  "VAT_RATE": 20,
  "TOTAL_GROSS": 11.28,
  "NOTES1": "H1-P1/49 VADE FARKI",
  "NOTES2": "YED2026000010690 nolu faturadan",
  "EINVOICE": 1,
  "PROFILE_ID": 1
}
```

e-Arşiv ise `EINVOICE: 2` ve ek olarak — iki kayıt arasındaki tek fark bu:

```json
"EARCHIVEDETR_SENDMOD": 2,
"EARCHIVEDETR_INTPAYMENTTYPE": 4,
"EARCHIVEDETR_INTPAYMENTDATE": "10.08.2026"
```

### 3.2 Kritik detaylar

- **`TRANSACTIONS` BOŞ.** Vade farkı faturasının satırı yoktur; hizmet kartı gerekmez.
  AIDAT payload'ından (`{"TRANSACTIONS": {"items": [...]}}`) tamamen farklı bir şekil.
- **`GL_CODE` GÖNDERİLMEZ.** `LogoJsonHelper.InjectDataObjectParameter` zaten
  `DataObjectParameter.FillAccCodesOnPreSave = true` ekliyor; Logo muhasebe kodunu kendi
  *Muhasebe Bağlantı Kodları → Fatura → Vade Farkı* ayarından doldurur.
  ⚠️ AIDAT'ta kod **hizmet kartından** türüyordu; burada kart yok. Test POST'unda dönen kayıtta
  `GL_CODE = "600.03.0001"` geldiği **doğrulanmalı** — boş gelirse muhasebe bağlantısı tanımsız.
- **`TOTAL_GROSS` = KDV HARİÇ matrah.** Doğrulama: 11.28 + %20 → `TOTAL_VAT` 2.26, `TOTAL_NET` 13.54.
  `TOTAL_VAT` / `TOTAL_NET` / `TOTAL_NET_STR` Logo'nun hesabı, gönderilmez.
- **Ödeme planı / borç takip alanı gönderilmez:** `PAYDEFREF=0`, `PAYMENT_LIST` boş, `AFFECT_RISK=0`.
  Ayrı bir "borç takibe yazma" adımı gerekmiyor.
- **Fiş no serisini Logo atıyor** ve e-fatura tipine göre ayrışıyor: e-Fatura `YED…`, e-Arşiv `YDA…`.
- `TIME` doğrulaması: kayıttaki `168432128` = `ConvertToLogoTime(10,10,18)`
  (`16777216·saat + 65536·dakika + 256·saniye`). `Tools.cs:127` formülü doğru.

### 3.3 Yan bulgu (bu spec'in kapsamı dışında)

`salesInvoices` şemasında **`GRP_CODE` diye bir alan yok**, doğrusu `GRPCODE`.
`AidatInvoicePayload.cs:13` `GRP_CODE` gönderiyor → Logo sessizce yok sayıyor. Varsayılan zaten 2
olduğu için zararsız, ama düzeltilmeli.

### 3.4 Kaynak fatura bağı

Logo'da vade farkı faturasını kaynak faturaya bağlayan **bir alan yok**. Elle kesilen iki örnekte de
`NOTES1` cari koduna dayalı (`"H1-P1/49 VADE FARKI"`), fatura numarası hiçbir yerde geçmiyor.

**Karar (kullanıcı):** kaynak fatura numarası `NOTES2`'ye yazılacak.

---

## 4. Açık kalan / doğrulanacak

| # | Konu | Durum |
|---|---|---|
| A1 | `GL_CODE` `FillAccCodesOnPreSave` ile gerçekten doluyor mu (satırsız faturada) | Test POST'u ile |
| A2 | `TOTAL_VAT`/`TOTAL_NET`, `TOTAL_GROSS`+`VAT_RATE`'ten hesaplanıyor mu | Test POST'u ile |
| A3 | e-Arşiv varyantı (üç ek alan) POST'ta kabul ediliyor mu | Test POST'u ile |
| A4 | KDV oranı sabit 20 mi, ayardan mı, istekten mi | **Kullanıcı cevaplamadı.** Öneri: servis içinde sabit 20 |

A1–A3 için **tek bir manuel test POST'u** yeterli. Kod yazımı bundan sonra başlar — AIDAT'ta
izlenen ve işe yarayan yol budur.

---

## 5. Tasarım (ONAY BEKLİYOR)

### 5.1 İstek sözleşmesi

`POST /api/VadeFarkiFatura`

```json
{
  "customerCode": "1.H1P1.049.00.K2",
  "customerReference": 20773,
  "sourceInvoiceRef": 17867,
  "sourceInvoiceNumber": "YED2026000010690",
  "amount": 11.28,
  "invoiceDate": "2026-08-10",
  "description": "H1-P1/49 VADE FARKI"
}
```

- Alanların tamamı açık faturalar endpoint'inin **aynı satırından** geliyor
  (`CustomerCode`, `CustomerReference`, `InvoiceLogicalRef`, `InvoiceNumber`) — dış uygulama
  zaten elinde tutuyor. `customerReference`'ı istemek, her istekte cari kodu→ref SQL sorgusu
  atmaktan ucuz ve tek kod yolu bırakıyor.
- `amount` = **KDV hariç matrah**, `> 0` zorunlu.
- `invoiceDate` boşsa bugün. `description` boşsa `"{customerCode} VADE FARKI"`.

### 5.2 Akış

1. Model validation → `400`
2. **Idempotency:** `sourceInvoiceRef` için başarılı kayıt varsa → `409` + mevcut fatura numarası
3. Kayıt `Pending` olarak DB'ye yazılır *(AIDAT dersi: sonuç cache'te bekletilmez)*
4. `GET methods/ARPEInvoiceCheck/{customerReference}` → `EINVOICE` 1 mi 2 mi
   (kural: `docs/superpowers/specs/2026-08-01-efatura-earsiv-ayrimi-design.md`)
5. Payload kurulur (bölüm 3.1), `InjectDataObjectParameter` uygulanır
6. `POST salesInvoices`, token-retry: 3 deneme × 3 sn (`BulkInvoiceTransferService` deseni)
7. Sonuç **anında** DB'ye: `Transferred` + `INTERNAL_REFERENCE`/`NUMBER`, ya da `Failed` + `RestError`
8. Response

### 5.3 Dosyalar

| Katman | Dosya |
|---|---|
| Core | `Dtos/VadeFarkiFatura/CreateVadeFarkiFaturaRequestDto.cs` |
| Core | `Dtos/VadeFarkiFatura/VadeFarkiFaturaLogoDto.cs` |
| Core | `Dtos/VadeFarkiFatura/CreateVadeFarkiFaturaResponseDto.cs` |
| Core | `Services/IVadeFarkiFaturaService.cs` |
| Repositories | `VadeFarkiFatura` entity + migration (idempotency tablosu) |
| Service | `Services/VadeFarkiFaturaService.cs` |
| WebApi | `Controllers/VadeFarkiFaturaApiController.cs` — `[Authorize(Policy = "CurrentAccuant")]` |

Desen olarak `KrediKartTahsilat` üçlüsü (DTO → Logo DTO → Service → Controller) birebir izlenir.

### 5.4 Hata yönetimi

Logo hatası **`result.Errors.Errors`**'tan okunur, `result.Message`'tan **değil** — bu kod tabanında
gerçek REST/SQL hatası orada saklı, `Message` genel metin döndürüyor. Token hatası geçici sayılır ve
retry'lanır; diğerleri kalıcı kabul edilip `Failed` yazılır.

### 5.5 Test

- Payload şekli: `TYPE=11`, `GRPCODE`, `TRANSACTIONS` **yok**, `GL_CODE` **yok**,
  `TOTAL_GROSS`=matrah; e-Fatura ve e-Arşiv iki varyant
- `TIME` dönüşümü, `NOTES2` formatı
- Idempotency: ikinci istek `409`
- `KrediKartTahsilatServiceTests` desenini izler

---

## 6. Sonraki adım

1. Bölüm 5 kullanıcı onayı
2. A4 (KDV oranı) kararı
3. Manuel test POST'u → A1/A2/A3 kanıtı
4. `superpowers:writing-plans` ile implementasyon planı

---

## Kanıt dosyaları

| Dosya | İçerik |
|---|---|
| `VADEFARKI001.json` | Gerçek vade farkı faturası, e-Fatura (`TYPE=11`) |
| `VADEFARKI002.json` | Gerçek vade farkı faturası, e-Arşiv |
| `withcount.json` | `paymentDifferenceInvoices` toplam sayısı — yanlış aday kanıtı |
| `withreferance.json` | `paymentDifferenceInvoices` son 2 kayıt — PAYTRANS olduğunun kanıtı |

Logo REST swagger tanımları: `Downloads/RESTServis_2.98.00.00.rar` → `RESTServis/swaggerFiles/v1/*.json`
(UnRAR ile açılır; `swagger.json` tag'leri her kaynağın Logo menü yolunu verir).
