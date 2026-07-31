# Cari Değişim Karşılaştırması — Tasarım

**Tarih:** 2026-07-31
**Durum:** Tasarım onaylandı, uygulama bekliyor (01.08 aktarımından sonra)
**İlgili modül:** Toplu Faturalandırma (`BulkInvoice*`)

---

## 1. Amaç

Aylık aidat bilgilendirme mailine, **bir önceki aya göre cari listesindeki değişimi** gösteren bir
bölüm eklemek. Amaç iki soruyu cevaplamak:

1. Hangi dükkânda sakin değişti, kim kimin yerine geldi?
2. **Çıkan var ama yerine fiş açılmamış bir dükkân kaldı mı?**

İkincisi asıl değerli olan. İş kuralı: kiracı çıkar ve dükkân boş kalırsa, fişin **mal sahibine**
açılması gerekir. Bu gözden kaçarsa o dükkân için o ay hiç fatura kesilmez.

## 2. Kapsam dışı (bilinçli)

- Excel ekine dokunulmuyor — değişim **yalnızca mail gövdesinde**.
- Tutar kolonu yok. Dört kolon yeterli (kullanıcı kararı).
- Ana cari listesiyle (CLCARD / Workplace) karşılaştırma yok. Kıyas noktası yalnızca **bir önceki
  aidat oturumu**.
- Sonuç mailine eklenmiyor; sadece T-1 bilgilendirme mailine.

## 3. Veri kaynağı — yeni tablo YOK

`BulkInvoiceItems` zaten oturum başına cari listesini tutuyor (cari başına bir satır).
31.07.2026 ölçümü: Oturum #1 (TEMMUZ) 2093 satır / 2093 tekil cari, Oturum #2 (AĞUSTOS) 2094 / 2094.

Karşılaştırma için ek bir snapshot tablosuna gerek yok.

**Önceki oturum tanımı:** `InvoiceDate`'i mevcut oturumunkinden küçük olan, en büyük `InvoiceDate`'e
sahip oturum.

**Bilinen sınır:** `SyncSessionItemsAsync`, artık bekleyen listesinde olmayan **Pending** satırları
siliyor ([BulkInvoiceService.cs:330](../../../Koala.Yedpa.Service/Services/BulkInvoiceService.cs)).
Aktarılmış (`Transferred`) satırlar hiç silinmez, bu yüzden tamamlanmış oturumların listesi sağlamdır.
Yarım kalmış bir oturum kıyas noktası olursa liste eksik olabilir — §7'deki koruma bunu ele alıyor.

## 4. Eşleştirme kuralı

Cari kod **16 karakter sabit** (2094 kaydın tamamında doğrulandı). Yapısı:

```
1.B000.063.01.M3
└──── 14 karakter ────┘└┘
      dükkân kimliği   sakin
```

Son iki hane sakini gösterir: `K1..K8`, `KR` = kiracı; `M1`, `M2`, `MS` = mal sahibi.
İlk 14 karakter dükkânı tanımlar ve sakin değişse de sabit kalır.

**Algoritma:**

1. Önceki oturumda olup bu oturumda olmayan cariler → `Eski`
2. Bu oturumda olup önceki oturumda olmayan cariler → `Yeni`
3. `LEFT(ClientCode, 14)` üzerinden `FULL OUTER JOIN`

**Doğrulama (31.07.2026, gerçek veri):** Ham karşılaştırma 63 eklenen / 62 çıkan veriyordu.
Dükkân bazında eşleştirince: **62 eşleşme + 1 eşleşmeyen yeni + 0 eşleşmeyen eski**.
Asimetri tam olarak açıklandı.

## 5. Satır tipleri

| Tip | Eski kod/ad | Yeni kod/ad | Anlamı | Vurgu |
|---|---|---|---|---|
| Değişim | dolu | dolu | Dükkânın sakini değişti | — |
| **Yerine gelen yok** | dolu | **boş** | Çıkan var, fiş açılmamış | **Aksiyon gerektirir** |
| Öncesi yok | boş | dolu | Yeni dükkân/sakin | Bilgi |

## 6. Mail çıktısı

Bilgilendirme mailinin gövdesinde, mevcut özet metninin altında:

**Özet cümlesi:** *"Geçen aya göre 62 dükkânda sakin değişti, 1 yeni cari eklendi, yerine fiş
açılmamış cari yok."*

**Tablo — 4 kolon:** Eski Cari Kod | Eski Cari Adı | Yeni Cari Kod | Yeni Cari Adı

**Sıralama:** Eşleşmeyenler en üstte (aksiyon gerektirenler önce), sonra eşleşenler dükkân koduna göre.

**Vurgu:** "Yerine gelen yok" satırları görsel olarak ayrışmalı (arka plan rengi), çünkü tek aksiyon
kalemi onlar.

## 7. Sınır durumları

| Durum | Davranış |
|---|---|
| Önceki oturum yok (ilk koşu) | Bölüm mailde hiç görünmez |
| Önceki oturumun satırı yok | Karşılaştırma yapılmaz; yerine tek satır not. (Aksi halde 2094 cari "yeni" görünür — anlamsız gürültü) |
| Değişim yok | "Geçen aya göre cari listesinde değişiklik yok." tek satırı |
| Karşılaştırma hatası | Mail yine de gönderilir, sadece bu bölüm atlanır ve loglanır. Bilgilendirme maili bu özellik yüzünden kaybedilmemeli |

**Kıyas kapsamı:** Önceki oturumun **tüm** satırları kullanılır, sadece `Transferred` olanlar değil.
Soru "kim listedeydi", "kime fatura kesilebildi" değil.

## 8. Kod yerleşimi

`BulkInvoiceEmailService` şu an mail kurgusu yapıyor; karşılaştırma mantığı oraya girmemeli.

**Yeni sınıf:** iki oturum ID'si alır, satır listesi döner (`EskiKod, EskiAd, YeniKod, YeniAd` +
satır tipi). Mailden bağımsız, saf veri dönüştürme — kolay test edilir.

`BulkInvoiceEmailService.SendInfoMailAsync` bu sınıfı çağırır ve sonucu HTML tabloya çevirir.

İleride Manage sayfasına da konabilir (aynı sınıf, farklı sunum).

## 9. Test

- İki oturum, sakin değişmiş → 1 eşleşme satırı
- Çıkan var, yerine gelen yok → eşleşmeyen eski satırı
- Öncesi olmayan yeni cari → eşleşmeyen yeni satırı
- Önceki oturum yok → boş sonuç
- Önceki oturumun satırı yok → boş sonuç (2094 "yeni" ÜRETMEMELİ)
- Değişim yok → boş sonuç
- Gerçek veri regresyonu: Oturum #1 → #2 karşılaştırması 62 / 1 / 0 vermeli

## 10. Referans

31.07.2026'da üretilen kontrol listesi: `Cari-Degisim-Temmuz-Agustos-2026.xlsx`
(62 değişim + 1 yeni). Uygulama sonrası mail çıktısı bununla karşılaştırılabilir.
