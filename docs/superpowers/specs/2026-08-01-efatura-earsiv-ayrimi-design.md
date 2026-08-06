# E-Fatura / E-Arşiv Ayrımı — Tasarım

**Tarih:** 2026-08-01
**Durum:** Keşif tamamlandı, kural kesinleşti. Tasarım kararları onay bekliyor.
**Aciliyet:** Eylül aktarımından önce. 01.08 koşusunda 2.093 faturanın tamamı yanlış tipte kesildi
ve iptal edilmek zorunda kalındı.

---

## 1. Problem

`AidatInvoicePayload.EInvoice` sabit `1` (e-Fatura). Cari mükellefiyetine bakılmıyor, bu yüzden
e-Arşiv kesilmesi gereken cariler için de e-Fatura üretiliyor.

## 2. Kural — kesinleşti

Referans: **FazlaGida** projesi (`D:\Source\FazlaGida`), aynı Logo REST altyapısıyla çalışan
kanıtlanmış uygulama.

**Karar Logo'nun kendi uç noktasından alınıyor, CLCARD alanından DEĞİL:**

```
GET methods/ARPEInvoiceCheck/{clientRef}
```

| Sonuç | Karar |
|---|---|
| Başarılı → cari e-fatura mükellefi | `EINVOICE = 1` (e-Fatura) |
| Hata + mesajda "TC Kimlik" veya "vergi" geçiyor | `EINVOICE = 2` (e-Arşiv) |

Kaynak: `FazlaGida/Services/InvoiceService.cs:27` ve `FazlaGida/Services/ClCardService.cs:1389`.
Değer anlamı `FazlaGida/Models/InvoiceModel.cs`'te belgelenmiş: *"E-Fatura mı? (1-E-Fatura 2E-Archive)"*.

### Neden CLCARD alanı değil

31.07 gecesi 2.094 cari üzerinde ölçüldü:

| Alan | Durum |
|---|---|
| `EINVOICETYP` | **Herkeste 0** — kullanılmıyor |
| `EINVOICEID` | **Herkeste boş** — kullanılmıyor |
| `PROFILEID` | 1→1472, 0→364, 2→258 |
| `POSTLABELCODE` dolu | 1550 var / 544 yok |

`PROFILEID` ve `POSTLABELCODE` birbirini tutmuyor (~78 kayıt farklı) ve ikisi de gerçek kaynak değil.
Mükellefiyet bilgisi kartta tutulmuyor, çalışma anında sorgulanıyor.

## 3. E-Arşiv için ek alanlar

E-Arşiv payload'ı, e-Fatura'da bulunmayan üç alan taşımalı:

```
EARCHIVEDETR_SENDMOD        = "2"
EARCHIVEDETR_INSTEADOFDESP  = "1"
EARCHIVEDETR_INTPAYMENTTYPE = 0
```

Yalnızca `EINVOICE`'ı 2 yapmak **yetmez**.

## 4. Açık tasarım kararları (onay bekliyor)

### K1 — Mükellefiyet kontrolü ne zaman çalışsın?

**a) `SyncSessionItemsAsync` sırasında, sonuç satıra yazılsın** ✅ önerilen
- Aktarım döngüsü yavaşlamaz (şu an ~3,4 sn/fatura; 2.094 satır ≈ 1 sa 52 dk)
- T-1 bilgilendirme mailinde "kaç e-fatura / kaç e-arşiv" raporlanabilir
- Risk: kontrol ile aktarım arasında mükellefiyet değişirse bayat kalır (nadir)

**b) Aktarım döngüsünde, satır satır**
- Her zaman güncel, ama fatura başına bir REST round-trip daha (+2.094 çağrı)

### K2 — Kontrol başarısız olursa (ağ hatası, belirsiz cevap)?

**Öneri: satırı `Failed` işaretle, aktarma.** Gerekçe: **yanlış tipte fatura kesmek,
fatura kesmemekten daha kötü.** 01.08'de tam olarak bu yaşandı — 2.093 fatura iptal edildi.
Eksik kalan satır Manage sayfasından tek tıkla tamamlanabilir; yanlış kesilmiş fatura ise
GİB tarafında iptal + yeniden kesme demek.

Varsayılana düşmek (bugünkü davranış) bu hatayı üreten şeydir; tekrarlanmamalı.

### K3 — Sonuç nerede saklansın?

`BulkInvoiceItem`'a alan eklenmeli (ör. `EInvoiceType` — 1/2, nullable = henüz kontrol edilmedi).
Migration gerekir. Böylece Manage sayfasında da tip görünür ve rapor mailinde kırılım verilebilir.

## 5. Uygulama adımları

1. `PendingInvoiceLineDto`'ya **`ClientRef`** ekle — `ARPEInvoiceCheck` cari referansı istiyor.
   Sorgu zaten `ORF.CLIENTREF = CLC.LOGICALREF` join'i yapıyor
   ([BulkInvoiceService.cs:125](../../../Koala.Yedpa.Service/Services/BulkInvoiceService.cs)), alan eklemek yeterli.
2. `LogoRestServiceProvider`'a `ArpEInvoiceCheckAsync(int clientRef)` ekle.
   FazlaGida'daki gibi mesaj eşleşmesi yerine **açık bir sonuç tipi** döndür
   (`EFatura` / `EArsiv` / `Belirsiz`) — string arama kırılgan.
3. `BulkInvoiceItem.EInvoiceType` alanı + migration.
4. `SyncSessionItemsAsync` içinde kontrolü çalıştır, sonucu yaz (K1-a onaylanırsa).
5. `AidatInvoicePayload`'ı tipe göre kur: e-Arşiv'de §3'teki üç alan eklensin.
6. `BulkInvoiceTransferService` payload'ı satırdaki tipe göre üretsin.

## 6. Test

- Mükellef cari → `EINVOICE=1`, `EARCHIVEDETR_*` alanları **yok**
- Mükellef olmayan cari → `EINVOICE=2` + üç ek alan dolu
- Kontrol hata verirse → satır `Failed`, `Note` açıklayıcı, **fatura kesilmez**
- `ClientRef` boş/0 ise → satır `Failed`, aktarılmaz
- Rapor/bilgi mailinde e-fatura/e-arşiv kırılımı doğru

## 7. Doğrulama önerisi (uygulamadan önce)

`ARPEInvoiceCheck` salt okuma. 20-30 carilik bir örneklemde çalıştırılıp sonuç `PROFILEID` ile
karşılaştırılmalı. Eğer birebir örtüşüyorsa `PROFILEID` ucuz bir yerel gösterge olarak kullanılabilir
ve 2.094 REST çağrısından kurtulunur. Örtüşmüyorsa REST kontrolü zorunlu.

Bu ölçüm aynı zamanda 01.08'de kaç faturanın yanlış tipte kesildiğini de sayısal olarak verir.
