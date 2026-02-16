# QR Code Entegrasyon TODO Listesi

## ✅ Tamamlanan İşlemler
- [x] NuGet paketleri eklendi (QRCoder, System.Drawing.Common)
- [x] QRCodeSettingsViewModel oluşturuldu
- [x] QR Code Settings view ve controller oluşturuldu
- [x] ISettingsService ve SettingsService'e QRCode metodları eklendi
- [x] QRCodeService temel yapısı oluşturuldu
- [x] CreatePdf ve CurrentAccountDetail view'leri oluşturuldu
- [x] Menü entegrasyonu yapıldı

## 🚧 Yeni Yapılacak İşlemler (Geliştirme Fazı)

### 1. Veritabanı Tablosu Oluşturma ⭐
- [x] QRCode entity'si oluştur (Koala.Yedpa.Core/Models/)
- [x] IQRCodeRepository interface'i ekle
- [x] QRCodeRepository implementasyonu ekle
- [x] QRCodeConfiguration oluştur
- [x] AppDbContext'e DbSet<QRCode> ekle
- [x] Migration oluştur (AddQRCodeTable)
- [ ] Migration'ı uygula
  - `dotnet ef database update --project Koala.Yedpa.Repositories`

### 2. Model ve Repository Güncellemeleri
- [ ] IQRCodeRepository interface'i ekle (Koala.Yedpa.Core/Repositories/)
  - GetAllAsync()
  - GetByPartnerNoAsync(string partnerNo)
  - AddAsync(QRCode entity)
  - UpdateAsync(QRCode entity)
  - DeleteAsync(int id)
  - DeleteAllAsync()

- [ ] QRCodeRepository implementasyonu ekle (Koala.Yedpa.Repositories/)

### 3. Controller Güncellemeleri
- [x] QRCodeController/Index action - QR kod listesini göster (GET)
- [x] QRCodeController/List action - AJAX ile listeyi döndür (GET)
- [x] QRCodeController/Create action - Yeni QR kodları oluştur (POST)
- [x] QRCodeController/Refresh action - Mevcut kodları yeniden oluştur (POST)
- [x] QRCodeController/Delete action - Tüm QR kodları sil (POST)
- [x] QRCodeController/ViewBatch action - Oluşturulan görselleri göster (GET)

### 4. View Oluşturmalar
- [x] Index.cshtml - QR kod liste sayfası (/Views/QRCode/Index.cshtml)
  - Tablo sütunları: PartnerNo, QRCodeNumber, CreatedDate, Status, İşlemler
  - Her satırda Görüntüle butonu
  - "Yeni QR Kod Oluştur", "Yeniden Oluştur", "Tümünü Sil" butonları
  - Progress bar ve durum mesajları
  - DataTables ile listeleme
  - SweetAlert2 ile onay dialogları

- [x] ViewBatch.cshtml - Oluşturulan QR görsellerini göster (/Views/QRCode/ViewBatch.cshtml)
  - Grid layout ile QR kodlar
  - Her karte: QR görsel + PartnerNo + bilgiler
  - Yazdırma desteği (CSS @media print)
  - Detay, Yeni Sekme, İndir butonları

### 5. Service Güncellemeleri (QRCodeService)
- [x] GenerateBulkQRCodesAsync - Veritabanına kayıt ekle
- [x] DeleteQRCodesAsync - Veritabanı ve dosya silme
  - Tüm kayıtları soft delete
  - Dosyaları fiziksel olarak sil
- [x] RefreshQRCodesAsync - Mevcut kayıtları silip yeniden oluştur
  - Önce mevcut kayıtları sil (DB + dosya)
  - SQL sorgusunu tekrar çalıştır
  - Yeni QR kodları oluştur
- [x] GetQRCodesAsync - Veritabanından liste getir
- [x] GetQRCodeByPartnerNoAsync - PartnerNo'ya göre QR kod getir

### 6. JavaScript ve UI
- [x] Index.cshtml içindeki JavaScript
  - QR kod listesi DataTables ile göster
  - AJAX işlemleri (Create, Refresh, Delete)
  - Loading state'ler ve progress bar
  - SweetAlert2 ile onay dialogları (Sil işlemi için)

- [x] Print view için CSS optimizasyonu (ViewBatch.cshtml)
  - @media print rules
  - Grid layout'da QR kodlar

### 7. İş Akışı (User Flow)
```
1. Menü > QR Kod > Tıkla
   └─ Index.cshtml açılır
   └─ Önce oluşturulmuş QR kod listesi gösterilir

2. Liste Sayfası
   ├─ Tablo: PartnerNo, QR Kod No, Tarih, Durum
   ├─ Her satırda: [Görüntüle] [Yeniden Oluştur] [Sil]
   └─ Üstte: [Yeni QR Kod Oluştur] butonu

3. Yeni QR Kod Oluştur
   └─ Ayarlardaki SQL sorgusunu çalıştır
   └─ Her PARTNERNO için QR kod oluştur
   └─ Veritabanına kaydet
   └─ Dosyayı wwwroot/Uploads/Qr/{Yıl}/ klasörüne kaydet

4. Görüntüle
   └─ ViewBatch.cshtml sayfasına yönlendir
   └─ Oluşturulan tüm QR görsellerini grid'de göster
   └─ Yazdır butonu ile PDF çıktı al

5. Yeniden Oluştur
   └─ Mevcut QR kodlarını sil (DB + dosya)
   └─ SQL sorgusunu tekrar çalıştır
   └─ Yeni QR kodları oluştur

6. Sil
   └─ SweetAlert2 onay dialog
   └─ Tüm QR kodlarını sil (DB + dosya)
   └─ Listeyi yenile
```

### 8. Test ve Dokümantasyon
- [ ] Migration'ı uygula (dotnet ef database update)
- [ ] SQL sorgusu test et (Logo veritabanı)
- [ ] QR kod oluşturma test et
- [ ] Dosya kaydetme test et
- [ ] Veritabanı kayıtlarını kontrol et
- [ ] Delete/Refresh işlemlerini test et
- [ ] PDF/Yazdırma test et

## 📝 Teknik Notlar

### Veritabanı Tablo Yapısı
```sql
CREATE TABLE QRCodes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    PartnerNo NVARCHAR(50) NOT NULL,
    QRCodeNumber NVARCHAR(100) NOT NULL,
    QRImagePath NVARCHAR(500),
    FolderPath NVARCHAR(500),
    QrCodeYear NVARCHAR(10),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedSqlQuery NVARCHAR(MAX),
    Status INT DEFAULT 1, -- 1=Active, 2=Passive, 3=Deleted
    CONSTRAINT UQ_QRCode_PartnerNo UNIQUE(PartnerNo, QrCodeYear)
);
```

### Dosya Yapısı
- Klasör: `wwwroot/Uploads/Qr/{Yıl}/`
- Dosya Adı: `{QrCodePreCode}-{PartnerNo}.jpg`
- Format: JPEG, 10px piksel boyutu

### Ayarlar
- QrCodeYear: 2025
- QrCodePreCode: G11522-Yd
- QrSqlQuery: SELECT PARTNERNO FROM LG_XXX_CLCARD WHERE ACTIVE = 0

## Eski TODO Listesi (Arşiv)
> Faz 1-6已完成，见"✅ Tamamlanan İşlemler"部分

---

# Eksik Loglama İyileştirme Planı

> **Tüm Fazlar Tamamlandı ✅** - Tüm Service, Provider ve Controller sınıflarına ILogger eklendi ve kullanıma sunuldu.

## ✅ Tamamlanan Fazlar

### ✅ Faz 1: Kritik Service Sınıfları
- ✅ **SettingsService** - Logger eklendi, tüm metodlarda loglama aktif
- ✅ **CryptoService** - Logger eklendi, tüm metodlarda loglama aktif
- ✅ **EmailTemplateService** - Logger eklendi, tüm metodlarda loglama aktif
- ✅ **ModuleService** - Logger eklendi, tüm metodlarda loglama aktif
- ✅ **ClaimsService** - Logger tipi düzeltildi (`ILogger<ClaimsService>`), tüm metodlarda loglama aktif
- ✅ **AppUserService** - Logger tipi düzeltildi (`ILogger<AppUserService>`), tüm metodlarda loglama aktif

### ✅ Faz 2: Provider Sınıfları
- ✅ **RestServiceProvider** - Logger eklendi, tüm HTTP metodlarında loglama aktif
  - `HttpPost`, `HttpPut`, `HttpPatch`, `HttpGet` tümünde loglama mevcut
  - HTTP hataları, timeout'lar loglanıyor

### ✅ Faz 3: API Controllers
- ✅ **BudgetRatioApiController** - Logger eklendi, tüm endpoint'lerde loglama aktif
- ✅ **LogoClCardApiController** - Logger eklendi, tüm endpoint'lerde loglama aktif
- ✅ **QRCodeController (WebUI)** - Logger eklendi, tüm action'larda loglama aktif

### ✅ Faz 4: Kısmi Loglama İyileştirmeleri
- ✅ **BudgetOrderApiController** - Logger eklendi, tüm metodlarda loglama aktif
- ✅ **DuesStatisticApiController** - Logger eklendi, tüm endpoint'lerde giriş/çıkış logları aktif
- ✅ **QRCodeApiController** - Logger eklendi, tüm endpoint'lerde giriş/çıkış logları aktif

---

## ✅ NLog Entegrasyonu Tamamlandı (16.02.2026)

**Yapılan Değişiklikler:**

### Paketler
- ✅ `NLog.Extensions.Logging` (6.1.1) eklendi
- ✅ `NLog.Web.AspNetCore` (6.1.1) eklendi

### Konfigürasyon Dosyaları (Güncellenmiş - 16.02.2026)
- ✅ `Koala.Yedpa.WebApi/nlog.config` - API için JSON loglama yapılandırması
  - **appLog.json** (Trace/Debug) - Debug logları
  - **infoLog.json** (Info/Warn) - Bilgi logları
  - **errLog.json** (Error/Fatal) - Hata logları
  - Console target (sadece Development)
  - Microsoft.* logları filtreleniyor
  - 5MB arşiv boyutu, 50/100 dosya, aylık döngü

- ✅ `Koala.Yedpa.WebUI/nlog.config` - WebUI için JSON loglama yapılandırması
  - **appLog.json** (Trace/Debug) - Debug logları
  - **infoLog.json** (Info/Warn) - Bilgi logları
  - **errLog.json** (Error/Fatal) - Hata logları
  - Console target (sadece Development)
  - ASP.NET Context bilgileri (Controller, Action, UserIdentity, IP, URL)
  - 5MB arşiv boyutu, 50/100 dosya, aylık döngü

### Program.cs Güncellemeleri
- ✅ WebApi Program.cs - NLog yapılandırması eklendi
- ✅ WebUI Program.cs - NLog yapılandırması eklendi
- ✅ `using NLog.Extensions.Logging;` eklendi
- ✅ `builder.Host.ConfigureLogging()` ile NLog provider eklendi

### Ek Düzeltmeler
- ✅ QRCodeDto özellikleri güncellendi (Text, Width, Height)
- ✅ QRCodeService Content/PixelSize → Text/Width olarak güncellendi
- ✅ CryptoService dynamic tip hatası düzeltildi
- ✅ .csproj dosyalarına nlog.config CopyToOutputDirectory eklendi

### nlog.config İyileştirmeleri (16.02.2026)
- ✅ `throwExceptions="false"` - Production güvenliği
- ✅ `internalLogLevel="Debug"` - Detaylı internal loglama
- ✅ `archiveAboveSize="5MB"` - Optimum arşiv boyutu
- ✅ `concurrentWrites="true"` - Performans iyileştirmesi
- ✅ `includeExceptionProperty="true"` - JSON'da exception özelliği
- ✅ `Data=maxInnerExceptionDepth=10` - İç içe exception detayı
- ✅ `StackTrace` attribute - Hata loglarında stack trace
- ✅ Microsoft.* filtresi - Framework logları engellendi
- ✅ Console target - Development için ekran çıktısı
- ✅ Fazladan boşluklar temizlendi
