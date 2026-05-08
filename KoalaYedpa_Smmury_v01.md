# Koala.Yedpa - Proje Ozet Dokumani v01

> **Olusturulma Tarihi:** 02.05.2026
> **Proje Amaci:** YEDPA (Yenibosna Esnaf ve Sanatkarlar Dernegi / Yedpa Ticaret Merkezi) yonetim sistemi
> **Gelistirici:** Sistem Bilgisayar (Erkan Dirikcan)
> **Framework:** .NET 10.0
> **Toplam Commit:** 28

---

## 1. PROJE GENEL BAKIS

### 1.1 Cozum Adı
**Koala.Yedpa.sln** - Visual Studio 2022 (.NET 10.0) tabanli kurumsal yonetim uygulamasi. YEDPA Ticaret Merkezi'nin aidat/butce yonetimi, isyeri takibi, sozlesme/ariza/otopark yonetimi, QR kod uretimi ve Logo ERP entegrasyonu gibi islevleri kapsar.

### 1.2 Katmanli Mimari

```
Koala.Yedpa.sln
├── Koala.Yedpa.Core          (Sinif Kitapligi - Modeller, DTO'lar, Enum'lar, Helper'lar)
├── Koala.Yedpa.Repositories  (Sinif Kitapligi - Veri Erisim, DbContext, EF Core)
├── Koala.Yedpa.Service       (Sinif Kitapligi - Is Mantigi, Provider'lar, Arka Plan Islemleri)
├── Koala.Yedpa.WebApi        (Web Uygulamasi - REST API, JWT Bearer Kimlik Dogrulama)
├── Koala.Yedpa.WebUI         (Web Uygulamasi - MVC + API, Cookie Kimlik Dogrulama, Metronic 7 UI)
├── Koala.Yedpa.Repositories.Tests (xUnit - Repository birim testleri, 54 test)
└── Koala.Yedpa.Service.Tests      (xUnit - Service birim testleri, 51 test)
```

### 1.3 Solution Klasor Yapilandiirmasi
```
Koala.Yedpa.Core          (Solution Folder)
Koala.Yedpa.Service       (Solution Folder)
Koala.Yedpa.Repositories  (Solution Folder)
Koala.Yedpa.Web           (Solution Folder - WebApi + WebUI)
Solution Items            (Todo.md)
```

### 1.4 Bagimlilik Haritasi
```
WebApi ──> Service ──> Repositories ──> Core
WebUI  ──> Service, Repositories, Core
```

---

## 2. TEKNOLOJI YIGINI

### 2.1 Ana NuGet Paketleri

| Paket | Versiyon | Kullanim Amaci |
|---|---|---|
| .NET 10.0 | - | Hedef framework |
| EF Core | 10.0.1 | ORM (SQL Server + PostgreSQL saglayicilari) |
| Dapper | 2.1.66 | Raw SQL sorgulama (Logo ERP veritabani) |
| AutoMapper | 15.1.0 | Entity <-> ViewModel donusumleri (13 profil) |
| ASP.NET Identity | 10.0.1 | Kullanici/rol/claim yonetimi |
| JWT Bearer | 10.0.1/10.0.3 | Token tabanli kimlik dogrulama (WebApi) |
| Hangfire | 1.8.22 | Arka plan is planlayici (SQL Server depolama) |
| ClosedXML | 0.105.0 | Excel dosya olusturma |
| QRCoder | 1.4.1 | QR kod uretimi |
| NLog | 6.0.6 / 6.1.1 | Yapilandirilmis loglama (JSON formatinda) |
| Swashbuckle | 9.0.6 | Swagger / OpenAPI dokumantasyonu |
| Newtonsoft.Json | 13.0.4 | JSON islemleri |
| System.Drawing.Common | 8.0.0 | Gorsel isleme (QR kod) |

### 2.2 Dis Sistem Entegrasyonlari

| Sistem | Protokol | Amac |
|---|---|---|
| Logo ERP REST API | HTTPS REST (Bearer Token) | Siparis olusturma, cari bilgileri |
| Logo ERP SQL | ADO.NET / Dapper | Dogrudan SQL sorgulama (cari hesap, ekstre, fatura) |
| IdentityServer | OAuth2 / OIDC | JWT token uretimi (https://identity.sistem-koala.com:44982) |
| Message34 | HTTPS REST (Bearer Token) | E-posta gonderim servisi |
| Koala Crypto API | HTTPS REST | Veri sifreleme/cozme (https://GetDec.sistem-koala.com:44326) |
| Koala License API | HTTPS REST | Lisans dogrulama (https://GetDec.sistem-koala.com:44426) |

### 2.3 Veritabanlari

| Baglanti Adi | Sunucu | Veritabani | Amac |
|---|---|---|---|
| YedpaYonetim | 85.105.152.74 | Koala_Yedpa_Yonetim | Ana uygulama veritabani (EF Core) |
| YONETIM | 85.105.152.74 | YONETIM | Yonetim modulu tablolari |
| Logo ERP (dinamik) | Ayarlardan | LG_{Firma} | Logo ERP veritabani (Dapper/SqlProvider) |

---

## 3. CORE KATMANI (Koala.Yedpa.Core)

### 3.1 Modeller (Models/)

#### 3.1.1 Kimlik Modelleri
- **AppUser** (`IdentityUser<string>`): FirstName, MiddleName, LastName, Status, Avatar, Transactions ICollection. `ToString()` ile tam ad hesaplama.
- **AppRole** (`IdentityRole<string>`): Description, DisplayName, StatusEnum.

#### 3.1.2 Modul/Yetki Modelleri
- **Module**: Id, Name, DisplayName, Description, Status. Navigation: GeneratedIds, Claims, ExtendedProperties.
- **Claims**: Id, ModuleId, Name, DisplayName, Description. Navigation: Module.
- **GeneratedIds** (`CommonProperties`): Id, ModuleId, Name, Description, Prefix, StartNumber, LastNumber, Digit. Otomatik ID uretimi icin (ornegin "TXN-000001").
- **ExtendedProperties** (`CommonProperties`): Id, ModuleId, Name, DisplayName, Description, ShowOn (Flags: Insert/Update/List/Detail), InputType (Text/TextArea/Select/Radio/CheckBox/DateTime/File/Image). Navigation: Module, Values, RecordValues.
- **ExtendedPropertyValues** (`CommonProperties`): Id, ExtendedPropertyId, DisplayText, Value.
- **ExtendedPropertyRecordValues**: Id, ExtendedPropertyId, Value, RecordId.

#### 3.1.3 Islem Takip Modelleri
- **Transaction** (`CommonProperties`): Id, TransactionNumber, TransactionTypeId, UserId, Title, Description, IsComplated. Navigation: TransactionType, AppUser, TransactionItems.
- **TransactionItem** (`CommonProperties`): Id, TransactionId, Description, IsSuccess. Navigation: Transaction.
- **TransactionType**: Id, ColorClass, Icon, Description, Name, Status. Navigation: Transactions.

#### 3.1.4 Butce/Aidat Modelleri
- **BudgetRatio** (`CommonProperties`): Id, Code, Description, Year, Ratio, TotalBugget, BuggetRatioMounths (Flags enum), BuggetType. Unique: Code+Year.
- **DuesStatistic** (`CommonProperties`): Id, BuggetRatioId, Code, Year, DivCode, DivName, DocTrackingNr, ClientCode, ClientRef, BudgetType, January-December (decimal(18,2)), Total, TransferStatus. Tablo: [DuesStatistics].

#### 3.1.5 Isyeri Modeli
- **Workplace**: 40+ ozellik. Id, Code, Definition, LogicalRef (unique), LogRef (unique), CustomerType, ResidenceTypeRef, ParcelRef, BlockRef, IndDivNo, ResidenceNo, DimGross/DimField (metrekare), PersonCount, WaterMeterNo, CalMeterNo, HotWaterMeterNo, IdentityNr, ProfitingOwner, GasCoefficient. Butce metre+katsayilari (14 alan), Yakit metre+katsayilari (14 alan), Toplam degerler (3 alan). QR kod alanlari: QRCodeNumber, QRCodeImagePath, QRCodeGeneratedDate.

#### 3.1.6 QR Kod Modelleri
- **QRCodeBatch** (`CommonProperties`): Id (auto-increment), SqlQuery, QrCodeYear, QrCodePreCode, QRCodeCount, Description.
- **QRCode** (`CommonProperties`): Id (auto-increment), BatchId, PartnerNo, QRCodeNumber, QRImagePath, FolderPath, QrCodeYear. Navigation: Batch. Unique: PartnerNo+QrCodeYear.

#### 3.1.7 Diger Modelleri
- **EmailTemplate** (`CommonProperties`): Id, Name, Description, Content.
- **Settings** (`CommonProperties`): Id, Name, Description, Value, SettingValueType, SettingType.

#### 3.1.8 Yonetim Modulu Modelleri (Models/Yonetim/)

**Ortak.cs** - 3 sinif:
- **Mail** `[Table("mail")]`: MailID, Ad, Soyad, EPosta, GSM, Telefon.
- **Birim** `[Table("BIRIM")]`: BirimID, BirimAdi.
- **Durum** `[Table("DURUM")]`: DurumID, DurumAdi.

**Arsiv.cs** - 3 sinif hiyerarsisi (Raf > Bolme > Koli):
- **Raf** `[Table("RAF")]`: RafID, RafKod. Navigation: Bolumeler.
- **Bolme** `[Table("BOLME")]`: BolmeID, RafID (FK), BolmeNo. Navigation: Raf, Koliler.
- **Koli** `[Table("KOLI")]`: KoliID, BolmeID (FK), KoliNo, Detay. Navigation: Bolme.

**Sozlesme.cs** - 2 sinif:
- **Sozlesme** `[Table("SOZLESME")]`: SozlesmeID, Firma, Konu, Tur, Baslangic, Bitis, Birim, AzKalda, Bitti, Gizli, Arsiv, SonTarih, SonKisi, Pdf (byte[]). Navigation: IlgiliKisiler.
- **SozlesmeKisi** `[Table("sozlesmekisi")]`: SozlesmeKisiID, SozlesmeID (FK), MailID (FK).

**Ariza.cs** - 3 sinif:
- **Ariza** `[Table("ARIZA")]`: ArizaID, FirmaAdres, Konu, Tarih, Birim, Durum, SonTarih, SonKisi, Gizli. Navigation: Hareketler, IlgiliKisiler. `[NotMapped] bool Bitti`.
- **ArizaHareket** `[Table("ARIZAHAREKET")]`: HareketID, ArizaID, Aciklama, Tarih, Kisi.
- **ArizaKisi** `[Table("arizakisi")]`: ArizaKisiID, ArizaID (FK), MailID (FK).

**Otopark.cs**:
- **OtoparkKayit** `[Table("kayit")]`: KayitID, Plaka, GirisTarih, CikisTarih, AboneAd, Telefon. `[NotMapped] bool Aktif`.

#### 3.1.9 Logo JSON Modelleri (Models/LogoJsonModels/)
- **SalesOrderJsonViewModel**: Logo REST API POST /api/v1/salesOrders icin tam model. NUMBER, DOC_TRACK_NR, DATE, DOC_NUMBER, AUTH_CODE, ARP_CODE, NOTES1/2, ORDER_STATUS, TRANSACTIONS (Items listesi). Her item: TYPE, MASTER_CODE, QUANTITY, PRICE, VAT_RATE, TRANS_DESCRIPTION, UNIT_CODE vb.

### 3.2 ViewModel'ler (Models/ViewModels/)

| ViewModel Grubu | Sinif Sayisi | Onemli Siniflar |
|---|---|---|
| AppUserViewModels | 11 | LoginViewModel, CreateAppUserViewModel, UserProfileViewModel, UserListViewModel |
| AppRoleViewModel | 6 | CreateAppRoleViewModel, AsignRoleToUserViewModel, AddClaimToRoleViewModel |
| ModuleViewModels | 5 | CreateModuleViewModel, SearchModuleViewModel |
| ClaimsViewModel | 6 | CreateClaimsViewModel, ClaimListForRoleViewModels |
| TransactionViewModels | 6+ | CreateTransactionViewModel, TransactionSearchViewModel |
| BudgetRatioViewModels | 6 | CreateBudgetRatioViewModel, BudgetRatioDetailViewModel |
| BudgetOrderViewModels | 14+ | CreateBudgetOrderViewModel, BudgetCalculationRequestViewModel, BudgetOrderResultViewModel, OrderResultViewModel, PreviewUpdateResultViewModel, TransferDuesStatisticsViewModel |
| DuesStatisticViewModels | 6 | DuesStatisticSummaryViewModel, MonthlyBudgetSummaryViewModel |
| WorkplaceViewModels | 4 | WorkplaceListViewModel, WorkplaceDetailViewModel, WorkplaceCurrentAccounts |
| SettingViewModels | 8 | EmailSettingViewModel, LogoRestServiceSettingViewModel, LogoSqlSettingViewModel, Message34SettingsViewModel, QRCodeSettingsViewModel |
| DataTableRequest | 5 | DataTableRequest, DataTableSearch, DataTableOrder, DataTableColumn |
| ApiLogoViewModels | 6 | ClCardInfoViewModel (30+ alan), StatementSummeryViewModel, ClCardStatementViewModel, PendingInvoiceViewModel |

### 3.3 DTO'lar (Dtos/)

| Dosya | Icerik |
|---|---|
| **EnumDto.cs** | 14 enum tanimi (StatusEnum, PriorityEnum, BuggetRatioMounthEnum [Flags], TransferStatusEnum, InputTypeEnum, CompanyTypeEnum [Flags], SettingsTypeEnum, BuggetTypeEnum vb.) |
| **ResponseDto.cs** | ResponseDto, ResponseDto<T>, ResponseListDto<T> - Standart API yanit yapisi. Success/Fail statik fabrika metotlari. |
| **ErrorDto.cs** | ErrorDto - Hata mesajlari listesi + IsShow bayragi |
| **EmailDto.cs** | EmailDto, CustomEmailDto, ResetPasswordEmailDto, EmailAttachmentDto |
| **QRCodeDto.cs** | QRCodeDto (Text, Width, Height, LogoFilePath, IncludeLogo) |
| **CryptResponse.cs** | DecryptResponse, CryptoApiResponse |
| **SelectListDto.cs** | SelectListDto<T> - Generic secim listesi |
| **MailSettingsDto.cs** | SmtpMailSettingsDto |
| **Message34Models.cs** | 10+ sinif: Authentication, SendTransaction/Bulk/Transfer request/response, CampaignDetail |
| **Yonetim/ArsivDto.cs** | ArsivDto, ArsivDetayDto, RafDto, KoliCreateDto |
| **Yonetim/SozlesmeDto.cs** | SozlesmeListDto (KalanGun, Yaklasan hesaplanan), SozlesmeDto, SozlesmeCreateDto, SozlesmeDurumUpdateDto |
| **Yonetim/ArizaDto.cs** | ArizaListDto, ArizaDto, ArizaCreateDto, ArizaDurumUpdateDto, ArizaHareketEkleDto |
| **Yonetim/OtoparkDto.cs** | OtoparkListDto (Aktif, Durum hesaplanan), OtoparkGirisDto, OtoparkCikisDto, OtoparkAboneDto |

### 3.4 Enum'lar (EnumDto.cs icinde)

| Enum | Degerler | Aciklama |
|---|---|---|
| StatusEnum | Active(0x01), Passive(0x02), Deleted(0x03), Locked(0x04), Unlocked(0x05), Pending(0x06) | Kayit durumu |
| BuggetRatioMounthEnum | [Flags] January-December (0x0001-0x0800) | Hangi aylar secili (bitwise) |
| TransferStatusEnum | Pending(0x01), Completed(0x02), Failed(0x03), Canceled(0x04), FromLogo(0x10) | Logo aktarim durumu |
| InputTypeEnum | Text, TextArea, Select, Radio, CheckBox, DateTime, File, Image | Form alan tipi |
| SettingsTypeEnum | LogoSql, Email, Sms, PushNotification, Application, LogoRestService, LogoUser, Hangfire, Message34, KoalaApi | Ayar kategorisi |
| BuggetTypeEnum | Budget(0x01), ExtraBudget(0x02) | Butce tipi |
| ExtendedPropertyShowOnEnum | [Flags] Insert, Update, List, Detail | Ekranda gosterim yeri |

### 3.5 Helper'lar (Helpers/)

| Sinif | Amac |
|---|---|
| **Tools** (static) | CreateGuidStr() (UuidCreateSequential), MessageReplace() (Turkce karakter donusumu), TcCheck() (TC kimlik dogrulama), DecimalParse(), CreateConnectionString(), SqlQueryCreator(), IsValidEmail(), ConvertToLogoTime() (DateTime -> int), InternetControl(), ServerControl() |
| **PredicateBuilder** (static) | True<T>(), False<T>(), Or(), And() - Dinamik LINQ predicate olusturma |
| **DataTableExtensions** (static) | AsList<T>() - DataTable -> List<T> donusumu |
| **LicenseFileHelper** (static) | ReadLicensePayload() - RSA imza dogrulama + OAEP sifre cozme ile lisans dosyasi okuma. Public key: wwwroot/Licenses/Koala.Yedpa.Yonetim_public.pem |

### 3.6 Exception'lar
- **CryptoLicenseException**: Lisans/crypto hatalari icin ozel exception.

---

## 4. REPOSITORY KATMANI (Koala.Yedpa.Repositories)

### 4.1 AppDbContext

**Kalitim:** `IdentityDbContext<AppUser, AppRole, string>`

**DbSet'ler (17 ana + 6 Yonetim = 23 toplam):**

| DbSet | Entity | Aciklama |
|---|---|---|
| BudgetRatio | BudgetRatio | Butce oranlari |
| Claims | Claims | Yetki talepleri |
| DuesStatistics | DuesStatistic | Aidat istatistikleri |
| QRCodeBatches | QRCodeBatch | QR kod toplu islemler |
| QRCodes | QRCode | QR kod kayitlari |
| Workplace | Workplace | Isyerleri |
| EmailTemplate | EmailTemplate | E-posta sablonlari |
| ExtendedProperties | ExtendedProperties | Genisletilmis ozellikler |
| ExtendedPropertyRecordValues | ExtendedPropertyRecordValues | Ozellik kayit degerleri |
| ExtendedPropertyValues | ExtendedPropertyValues | Ozellik degerleri |
| GeneratedIds | GeneratedIds | ID uretecleri |
| Module | Module | Moduller |
| Settings | Settings | Sistem ayarlari |
| Transaction | Transaction | Islemler |
| TransactionItem | TransactionItem | Islem kalemleri |
| TransactionType | TransactionType | Islem tipleri |
| Raflar | Raf | Arsiv raflari |
| Bolumeler | Bolme | Raf bolumleri |
| Koliler | Koli | Arsiv kolileri |
| Sozlesmeler | Sozlesme | Sozlesmeler |
| SozlesmeKisiler | SozlesmeKisi | Sozlesme ilgili kisiler |
| Arizalar | Ariza | Ariza kayitlari |
| ArizaHareketleri | ArizaHareket | Ariza hareketleri |
| ArizaKisiler | ArizaKisi | Ariza ilgili kisiler |
| OtoparkKayitlari | OtoparkKayit | Otopark kayitlari |
| MailAdresleri | Mail | Mail adresleri |
| Birimler | Birim | Birimler |
| Durumlar | Durum | Durumlar |

### 4.2 Entity Configuration'lari (14 dosya)

Tum configuration'lar `IEntityTypeConfiguration<T>` pattern'i ile tanimli. Onemli yapilandirmalar:

- **BudgetRatio**: Code+Year unique index, Year ve BugGetType uzerinde index.
- **Workplace**: LogicalRef ve LogRef unique index, Code uzerinde index.
- **QRCode**: PartnerNo+QrCodeYear unique index, BatchId FK (Cascade delete).
- **Transaction**: TransactionItems (Cascade), TransactionType (Restrict), AppUser (Restrict).
- **ExtendedProperties**: Values (NoAction), RecordValues (Restrict), Module (Restrict).

### 4.3 UnitOfWork

`UnitOfWork<TContext>` generic sinifi. Lazy-loaded repository ornekleri: TransactionRepository, TransactionItemRepository, TransactionTypeRepository, BudgetRatioRepository, DuesStatisticRepository, EmailTemplateRepository. `Commit()` ve `CommitAsync()` metotlari.

### 4.4 Repository Siniflari (17 ana + 5 Yonetim = 22 toplam)

#### Ana Repository'ler

| Repository | Temel CRUD | Ozel Metotlar |
|---|---|---|
| TransactionRepository | Tam | GetByUserIdAsync, GetCompletedTransactionsAsync, GetPendingTransactionsAsync, GetTransactionsByDateRangeAsync, CountAsync |
| TransactionItemRepository | Tam | GetByTransactionIdAsync, GetItemsByDateRangeAsync |
| TransactionTypeRepository | Tam | GetActiveTransactionTypesAsync, GetByNameAsync (case-insensitive), GetByStatusAsync |
| BudgetRatioRepository | Tam | GetByYearAsync, GetByCodeAsync, GetByBudgetTypeAsync, ExistsAsync(code+year) |
| DuesStatisticRepository | Tam | GetByIdsAsync (coklu ID), GetByClientReferenceAsync, BulkInsertAsync, DeleteByYearAsync |
| EmailTemplateRepository | Tam | GetByNameAsyc (yazim hatasi), IsExistAsync, WhereAsync |
| ModuleRepository | Tam | GetModuleWithExtentedProperty, WhereModule |
| ClaimsRepository | Tam | GetClaimsByModuleIdAsync, WhereClaimsAsync |
| GeneratedIdsRepository | Tam | Where (IQueryable) |
| ExtendedPropertiesRepository | Tam | GetModuleExtendedPropertiesAsync, GetExtendedPropertiesByNameAsync |
| ExtendedPropertyValuesRepository | Bos | (Arayuzde metot yok) |
| ExtendedPropertyRecordValuesRepository | NotImplementedException | (Tum metotlar implement edilmemis) |
| SettingsRepository | Tam | GetSettings(type), GetSettingByName, GetSettingsByNames |
| WorkplaceRepository | Tam | GetByCodeAsync, GetByLogicalRefAsync, GetByLogRefAsync, GetPagedAsync, CountAsync |
| QRCodeBatchRepository | Soft delete | GetByYearAsync, CountAsync |
| QRCodeRepository | Soft + Hard delete | GetByPartnerNoAsync, GetByYearAsync, DeleteAllAsync (hard), DeleteByYearAsync (hard) |
| AppUserRepository | Tam | GetUserStatusById/ByEmail/ByUserName, GetUserClaimsById (claimType parse), GetUserRolesById (Join), UpdateUserStatus, RemoveUserLockout |
| AppRoleRepository | Kismen | GetUsersInRoleById/ByName (Join). **3 metot NotImplementedException:** GetRoleClaimById, GetRoleClaimByName, GetAllRoleClaims |

#### Yonetim Repository'leri

| Repository | Aciklama | Soft Delete |
|---|---|---|
| ArsivRepository | Raf/Bolme/Koli hiyerarsisi + GetArsivListesiAsync (LINQ Join) + GetKoliDetayAsync | Evet |
| SozlesmeRepository | GetExpiringContractsAsync(gun), GetContractPdfAsync, AddIlgiliKisiAsync/RemoveIlgiliKisiAsync, UpdateContractStatusAsync | Evet |
| ArizaRepository | GetActiveFaultsAsync, AddHareketAsync, GetHareketlerAsync, AddIlgiliKisiAsync/RemoveIlgiliKisiAsync, UpdateDurumAsync | Evet |
| OtoparkRepository | GetByPlakaAsync (son kayit), GetActiveSubscriptionsAsync, GirisYapAsync, CikisYapAsync, AboneEkleAsync/GuncelleAsync/SilAsync | Evet (AboneSil) |
| OrtakRepository | GetAllBirimlerAsync, GetAllMailAdresleriAsync, GetMailByIdAsync, AddMailAsync, UpdateMailAsync, DeleteMailAsync (**hard delete**) | Hayir |

### 4.5 Migration'lar (10 dosya)

| Migration | Tarih | Aciklama |
|---|---|---|
| Init | 30.07.2025 | Ilk veritabani |
| 001-005 | 13.11-24.12.2025 | Kademeli guncellemeler |
| AddWorkplaceTable | 06.01.2026 | Workplace tablosu |
| RemoveAuditFieldsFromWorkplace | 06.01.2026 | Workplace audit alanlari kaldirma |
| AddQRCodeBatchAndQRCodes | 06.02.2026 | QR kod tablolari |

---

## 5. SERVICE KATMANI (Koala.Yedpa.Service)

### 5.1 Ana Servisler (Services/)

| Servis | Bagimliliklar | Temel Is Mantigi |
|---|---|---|
| **CurrentUserService** | IHttpContextAccessor | JWT claim'lerinden UserId ve IsAuthenticated okuma |
| **SeedHostedService** | IServiceProvider | Uygulama baslangicinda veritabani seed (roller, admin kullanicisi, varsayilan ayarlar) |
| **SeedService** | AppDbContext | Baslangic verileri: "SistemKoala" rolu, admin kullanici (erkan@sistem-bilgisayar.com.tr), varsayilan ayarlar |
| **LicenseReader** | LicenseFileHelper | Lisans dosyasindan CustomerCode, ApplicationId, LogoClientId/Secret okuma (cache'li) |
| **LicenseValidator** | - | RSA+SHA256 imza dogrulama, son kullanma tarihi kontrolu |
| **CryptoService** | HttpClient, IConfiguration, ILicenseReader | Dis crypto API'si ile Encrypt/Decrypt. Lisans dogrulama ile X-SKey header'i. |
| **SettingsService** | ISettingsRepository, ICryptoService, IMapper | Tum ayarlarin sifreli saklanmasi/okunmasi (Reflection ile generic pattern). CryptoLicenseException ile lisans kontrolu. |
| **EmailService** | ISettingsService, IEmailTemplateService | SMTP ile e-posta gonderimi. Template sistemi ([[Title]], [[Body]], [[Name]], [[Date]] placeholder'leri). Attachment destegi. |
| **EmailSenderAdapter** | IEmailService | ASP.NET Identity IEmailSender arayuzunun uygulamaya uyarlanmasi (Adapter pattern) |
| **EmailTemplateService** | IEmailTemplateRepository, IMapper | Sablon CRUD + ChangeStatus |
| **Message34EmailService** | HttpClient, ISettingsService | Message34 REST API ile e-posta gonderimi (transactional, bulk, transfer). Bearer token (50dk cache). Sabit alici listesi. |
| **ModuleService** | IMapper, IModuleRepository | Modul CRUD + arama/sayfalama |
| **ClaimsService** | IClaimsRepository, IMapper | Claim CRUD + modul bazli sorgulama |
| **TransactionService** | IUnitOfWork, ITransactionRepository | Islem takibi. TransactionNumber: "TXN-{tarih}-{guid}". Filtreleme + sayfalama. |
| **TransactionItemService** | IUnitOfWork | Islem kalemleri CRUD |
| **TransactionTypeService** | IUnitOfWork | Islem turu CRUD. Isim birligi kontrolu. |
| **BudgetRatioService** | IUnitOfWork, IBudgetRatioRepository, ITransactionService | Butce orani CRUD. Her islem icin Transaction audit kaydi. |
| **DuesStatisticService** | IUnitOfWork, IDuesStatisticRepository, ISqlProvider, IDapperProvider | **Uygulamanin kalbi.** Logo ERP'den aidat verisi cekme, senkronize etme, istatistik hesaplama. Aylik/yillik ozet, reflection ile aylik property okuma. |
| **BudgetOrderService** | IDuesStatisticService, IBudgetRatioService, ILogoRestServiceProvider, IEmailService, IApiLogoSqlDataService | **Ana is mantigi servisi.** Butce olusturma, oran uygulama (bitwise flag ile ay secimi), Logo'ya siparis gonderme, kilitleme, on izleme, Excel raporu + e-posta. |
| **BudgetOrderTransferService** | IUnitOfWork, ILogoRestServiceProvider, IEmailService | DuesStatistic -> Logo SalesOrder donusumu. Basarili/basarisiz her kayit icin TransferStatus guncelleme. Debug modunda ilk 3 kayit siniri. |
| **QRCodeService** | IQRCodeRepository, IQRCodeBatchRepository, IDapperProvider, ISettingsService | QR kod olusturma (QRCoder), toplu uretim (Logo SQL sorgusu + batch), dosya sistemi yonetimi. |
| **AppUserService** | AppDbContext | Kullanici bilgi sorgulama (ID, Email) |
| **ApiLogoSqlDataService** | ISqlProvider, AppDbContext, ISettingsService | Logo ERP SQL sorgulari. Cari hesap listesi, ekstre, bekleyen faturalar. Karmasik SQL (CTE, window fonksiyonlari, Join'ler). Dinamik Firma/Donem bilgisi. |
| **WorkplaceService** | IWorkplaceRepository, IApiLogoSqlDataService, IMessage34EmailService, IDuesStatisticService, IQRCodeService | Isyeri yonetimi + Logo entegrasyonu. QR kod, toplu butce e-postasi, Excel raporu. |

### 5.2 Yonetim Servisleri (Services/Yonetim/)

| Servis | Aciklama |
|---|---|
| **SozlesmeService** | Sozlesme CRUD, PDF alma, durum guncelleme, ilgili kisi atama, yaklasan sozlesme listesi |
| **ArizaService** | Ariza CRUD, durum guncelleme, hareket (log) kaydi, ilgili kisi atama |
| **OrtakService** | Birim listesi, mail adresi CRUD |
| **ArsivService** | Raf/Bolme/Koli hiyerarsisi yonetimi |
| **OtoparkService** | Girisi/cikis, abonelik yonetimi |

### 5.3 Provider'lar (Providers/)

| Provider | Amac |
|---|---|
| **SqlProvider** | Raw SQL calistirma (DataTable, INSERT/UPDATE/DELETE). Dinamik sorgu olusturma. SQL injection riski (parametresiz). |
| **DapperProvider** | Dapper ORM ile type-safe veri erisimi. Generic CRUD (Reflection), BulkInsert, StoredProcedure, Transaction destegi. |
| **LogoRestServiceProvider** | Logo ERP REST API istemcisi. Her istekte token alinir, islem yapilir, token revoke edilir. |
| **RestServiceProvider** | Generic HTTP istemcisi (obsolesce isaretli). |
| **EmailProvider** | Eski SMTP provider (EmailService ile degistirildi). |

### 5.4 Arka Plan Islemleri

| Bilesen | Aciklama |
|---|---|
| **DuesStatisticTransferBackgroundService** | BackgroundService + Channel<T> pattern ile kuyruk tabanli Logo aktarim isi. Singleton. |
| **DuesStatisticTransferQueue** | Thread-safe singleton kuyruk (Channel.CreateUnbounded). Enqueue/Dequeue metotlari. |
| **BackgroundServices (bos)** | Gelecekteki Hangfire job tanimlari icin ayrilmis. |

### 5.5 Hangfire Yapilandirmasi (HangfireDashboard/)

- **HangfireAuthorizationFilter**: "Hangfire.Access" claim'i ile dashboard erisimi, "Hangfire.Trigger" claim'i ile job tetikleme.
- **HangfireDashboardConfiguration**: `/hangfire` endpoint'inde dashboard sunma.

---

## 6. WEBAPI PROJESI (Koala.Yedpa.WebApi)

### 6.1 Yapilandirma
- **Kimlik Dogrulama:** JWT Bearer (IdentityServer: https://identity.sistem-koala.com:44982, Audience: Rs-19001)
- **Yetkilendirme Politikalari:** "CurrentAccuant" (sc-190101 scope), "Sistem" (sc-030100 scope)
- **CORS:** AllowAll (tum origin/metod/baslik)
- **Swagger:** Her ortamda aktif
- **Veritabani:** SQL Server (YedpaYonetim baglanti dizisi)
- **Middleware Siralamasi:** Swagger -> CORS -> HTTPS -> Auth -> Controllers

### 6.2 Controller'lar (2 aktif controller)

#### LogoClCardApiController
- **Route:** `api/LogoClCardApi/[action]`
- **Yetki:** CurrentAccuant politikasi
- **Swagger:** Acik (SwaggerTag)
- **Endpoint'ler:**
  - `GET ClCardInfoAll` - Sayfali tum dukkan cari bilgileri
  - `POST ClCardInfoSearch` - Filtreli arama
  - `GET PendingInvoices` - Bekleyen faturalar (sayfali)
  - `POST PendingInvoicesSearch` - Filtreli bekleyen fatura arama
  - `GET test` - Token gecerlilik testi

#### HealthCheckApiController
- **Route:** `api/HealthCheckApi`
- **Endpoint'ler:** GET (basit), GET detailed, GET token, GET detailed/token

### 6.3 Yonetim API Controller'lari
- **SozlesmeApiController**: CRUD + PDF + durum guncelleme
- **OtoparkApiController**: CRUD + giris/cikis + abonelik
- **ArsivApiController**: Raf/Bolme/Koli CRUD
- **ArizaApiController**: CRUD + durum + hareket ekleme

---

## 7. WEBUI PROJESI (Koala.Yedpa.WebUI)

### 7.1 Yapilandirma
- **Kimlik Dogrulama:** ASP.NET Identity Cookie (30 gun, sliding expiration)
- **Cookie Adi:** "KoalaYedpa"
- **LoginPath:** /User/Login
- **Sifre Gereksinimleri:** Min 8 karakter, rakam, kucuk/buyuk harf, ozel karakter, 3 benzersiz
- **Kilitlenme:** 3 basarisiz giris -> 2 saat kilitleme
- **SecurityStamp:** 120 saniyede bir dogrulama
- **UI Framework:** Metronic 7 (Kecap CSS/JS framework)
- **Lisans Kontrolu:** Baslangicta zorunlu. Gecersiz lisans -> uygulama baslamaz.
- **AutoMapper:** 13 profil kayitli

### 7.2 Dinamik Yetkilendirme
- **AuthorizationRulesInitializer** (IHostedService): Baslangicta veritabanindaki claim'leri okuyup dinamik politika olusturma.
- **DynamicAuthorizationPolicyProvider**: Veritabanindan IAuthorizationPolicyProvider.

### 7.3 MVC Controller'lar (Sayfa donduren)

| Controller | Sayfalar | Yetki |
|---|---|---|
| **DashboardController** | Index, Privacy, Error | [Authorize] |
| **UserController** | Login, Logout, CreateUser, UpdateUser, ResetPassword, ChangePassword, UserProfile, AsignRoleToUser, AccessDenied | Karisik (AllowAnonymous/Login/Profile) |
| **AppRoleController** | Index, CreateRole, UpdateRole, AddClaimToRole, DeleteRole | - |
| **ModuleController** | Index, CreateModule, UpdateModule, ChangeStatus | - |
| **ClaimsController** | ModuleClaims, CreateClaim, UpdateClaim, DeleteClaim | - |
| **SettingsController** | EmailSettings, LogoSettings, LogoSqlSettings, LogoRestServiceSettings, Message34Settings, KoalaApiSettings, QRCodeSettings | - |
| **BudgetOrderController** | Index, Create, Update, Details, Transfer, Review | [Authorize] |
| **WorkplaceController** | Index, Detail, Update, SendBulkBudgetEmails, GenerateBudgetExcel | - |
| **QRCodeController** | Index, Create, Refresh, Delete, ViewBatch, CurrentAccountDetail | - |
| **ArsivController** (Yonetim) | Index, Detay, KoliEkle | [Authorize] |
| **SozlesmeController** (Yonetim) | Index, Detay, Yeni, Duzenle, Yazdir | [Authorize] |
| **ArizaController** (Yonetim) | Index, Detay, Yeni, Atama | [Authorize] |
| **OtoparkController** (Yonetim) | Index, Giris, Cikis, Abonelik | [Authorize] |

### 7.4 API Controller'lar (WebUI icinde)

| Controller | Route | Yetki | Endpoint'ler |
|---|---|---|---|
| **BudgetOrderApiController** | api/BudgetOrderApi | [Authorize] | CreateBudgetAndOrders, CreateBudget, CreateOrdersForExistingBudget, SaveNewBudget, CalculateBudget, PreviewUpdate, Transfer (BackgroundService kuyrugu) |
| **BudgetRatioApiController** | api/BudgetRatioApi | - | GetById, GetAll, GetByYear, Create, Update, Delete, CheckExists |
| **DuesStatisticApiController** | api/DuesStatisticApi | [Authorize] | GetDistinctYears, GetMonthlyBudgetSummary, GetByYearAndType, GetPagedList (DataTable uyumlu) |
| **QRCodeApiController** | api/QRCodeApi | [Authorize] | Generate, GenerateWithLogo, GenerateForWorkplace, GetImage (AllowAnonymous) |
| **LogoClCardApiController** | api/LogoClCardApi/[action] | - | ClCardInfoAll, Search (WebUI icin duplikasyon) |
| **ConnectionTestController** | api/ConnectionTest | - | Get, GetDetails |
| **KoalaApiController** | api/KoalaApi | - | GetSettings, GetBaseUrl |
| **SettingsApiController** | api/SettingsApi | - | GetSettings (placeholder) |
| **TestEmailController** | api/TestEmail | - | SendTestEmail, SendPaymentPlanTestEmail |
| **LogoSyncController** | api/LogoSync/[action] | - | SyncDuesStatisticYearData |
| **FinancialStatementController** | api/FinancialStatement/[action] | - | GetClsStatementsSummert, GetClCardStatement |

### 7.5 Views (59 .cshtml dosyasi)

**Layout:** `Views/Shared/_Layout.cshtml` (Metronic 7 tabanli)
**Partial View'ler:** _HeaderPartial, _HeaderMobilePartial, _FooterPartial, _UserPanelPartial, _MainManuPartial, _ValidationScriptsPartial, _ScrollTopPartial, _LoaderParital

**Sayfa Kategorileri:**
- Dashboard (2 sayfa)
- User (10 sayfa: Login, List, Create, Update, Profile, ResetPassword, vb.)
- AppRole (4 sayfa)
- Module (3 sayfa)
- Claims (3 sayfa)
- Settings (7 sayfa: Email, Logo, LogoSql, LogoRest, Message34, KoalaApi, QRCode)
- QRCode (5 sayfa: Index, Create, ViewBatch, CurrentAccountDetail, CreatePdf-deprecated)
- BudgetOrder (6 sayfa: Index, Create, Update, Details, Transfer, Review)
- Workplace (3 sayfa: Index, Detail, Update)
- Yonetim/Arsiv (2 sayfa)
- Yonetim/Sozlesme (1 sayfa)
- Yonetim/Ariza (1 sayfa)
- Yonetim/Otopark (1 sayfa)

### 7.6 wwwroot Icerigi
- `assets/css_/` - Metronic 7 CSS (minified)
- `assets/js/` - Metronic JS + custom scriptler
- `assets/media/users/` - Kullanici avatarlari (`/avatars` path ile eslestirilmis)
- `Licenses/` - Lisans dosyalari (license.lic, public.pem)

---

## 8. TEST PROJELERI

### 8.1 Koala.Yedpa.Repositories.Tests (54 test)

**Framework:** xUnit + Entity Framework InMemoryDatabase
**Fixture:** YonetimTestFixture (IClassFixture - her test sinifi icin bir kez)

| Test Sinifi | Test Sayisi | Kapsam |
|---|---|---|
| ArsivRepositoryTests | 12 | Raf/Bolme/Koli hiyerarsisi CRUD + soft delete |
| OtoparkRepositoryTests | 11 | Giris/cikis, abonelik CRUD |
| OrtakRepositoryTests | 9 | Birim/Mail CRUD |
| SozlesmeRepositoryTests | 10 | CRUD + PDF + durum + ilgili kisi |
| ArizaRepositoryTests | 12 | CRUD + durum + hareket + ilgili kisi |

### 8.2 Koala.Yedpa.Service.Tests (51 test)

**Framework:** xUnit + Moq + FluentAssertions
**Base:** ServiceTestBase (abstract) + TestDtoHelper (static)

| Test Sinifi | Test Sayisi | Kapsam |
|---|---|---|
| OtoparkServiceTests | 13 | GetAll, GetActive, GetByPlaka, GirisYap (icinde var kontrolu), CikisYap, Abone CRUD |
| ArsivServiceTests | 8 | GetArsivListesi, GetKoliDetay, AddRaf/Bolme/Koli, UpdateKoli, DeleteKoli |
| OrtakServiceTests | 8 | GetAllBirimler, GetAllMailAdresleri, GetMailById, AddMail, UpdateMail, DeleteMail |
| SozlesmeServiceTests | 10 | GetAll, GetById, GetExpiring, Create (PDF'li dahil), Update, Delete, GetPdf, UpdateDurum |
| ArizaServiceTests | 12 | GetAll, GetById, GetByBirim, GetActiveFaults, Create (aciklama bos/ozel kisili), UpdateDurum, AddHareket, Delete |

**Not:** Ana is servisleri (BudgetOrderService, DuesStatisticService, TransactionService vb.) icin test bulunmamaktadir.

---

## 9. IS AKISLARI

### 9.1 Butce Olusturma ve Logo'ya Aktarim
```
1. Kullanici -> BudgetOrder/Create sayfasi
2. Kaynak yil, hedef yil, butce tipi, oran, secili aylar (bitwise flag) girilir
3. BudgetOrderService.CalculateBudgetAsync() -> On izleme
4. BudgetOrderService.CreateBudgetAndOrdersAsync()
   ├─ DuesStatisticService.GetByYearAsync(kaynakYil) -> Logo'dan mevcut aidat verileri
   ├─ BudgetRatio orani uygulanir (her ay icin secili aylara)
   ├─ ApiLogoSqlDataService.GetClientInfoByWorkplaceCodeAsync() -> Logo'dan cari bilgisi
   ├─ Yeni DuesStatistic kayitlari olusturulur (hedef yil)
   ├─ Isteniyorsa -> BudgetOrderTransferService.TransferDuesStatisticsToLogoAsync()
   │   ├─ Her kayit icin LogoRestServiceProvider.PostSalesOrderAsync()
   │   ├─ Basarili -> TransferStatus = Completed
   │   └─ Basarisiz -> TransferStatus = Failed
   └─ Basarisiz siparisler -> ClosedXML ile Excel raporu + EmailService ile e-posta
5. BudgetRatio kilitlenir (StatusEnum.Locked)
```

### 9.2 Aidat Verisi Senkronizasyonu
```
1. DuesStatisticService.ImportFromSourceDatabaseAsync(yil, butceTipi)
2. Ayarlardan Logo SQL baglanti bilgileri okunur (sifreli)
3. SqlProvider/DapperProvider ile Logo veritabanina baglanilir
4. [YEDPA].[dbo].[AL_AIDAT_RAKAMLARI_PERFORMANS] tablosu sorgulanir
5. Turkce ay adlari (OCAK-SUBAT-...) Ingilizce property adlarina map edilir
6. Mevcut yilin kayitlari silinir (DeleteByYearAsync)
7. BulkInsertAsync ile yeni kayitlar eklenir
```

### 9.3 QR Kod Olusturma
```
1. Ayarlardaki SQL sorgusu calistirilir (Logo veritabani -> PARTNERNO listesi)
2. Her PARTNERNO icin QRCodeService.GenerateAndSaveQRCodeAsync()
   ├─ QRCoder ile QR kod olusturulur (PNG)
   ├─ Dosya: wwwroot/Uploads/Qr/{Yil}/{Prefix}-{PartnerNo}.jpg
   └─ Veritabanina QRCode kaydi eklenir
3. QRCodeBatch kaydi olusturulur (toplu islem kaydi)
```

### 9.4 Kullanici Kimlik Dogrulama
```
WebUI: Cookie Authentication
  Login -> UserManager.PasswordSignInAsync() -> Cookie olusturulur (30 gun)
  Her istek -> Cookie ile kimlik dogrulama -> Dinamik yetkilendirme (DB claim'ler)

WebApi: JWT Bearer Authentication
  Client -> IdentityServer'dan token alir
  Her istek -> Bearer token ile -> IdentityServer dogrulama -> Politika kontrolu
```

### 9.5 Ayar Yonetimi ve Sifreleme
```
1. SettingsController -> SettingsService.UpdateEmailSettingsAsync(model)
2. Reflection ile tum property'ler gezilir
3. Her deger CryptoService.EncryptAsync() ile sifrelenir
4. Dis crypto API'sine gonderilir (ApplicationId + X-SKey header)
5. Sifrelenmis deger Settings tablosuna kaydedilir
6. Okuma sirasinda CryptoService.DecryptAsync() ile cozulur
```

---

## 10. ONEMLI TASARIM KARARLARI VE PATTERLERI

### 10.1 Kullanilan Desenler
| Pattern | Kullanim Yeri |
|---|---|
| Repository Pattern | Tum veri erisimi |
| Unit of Work | Transaction yonetimi |
| Adapter Pattern | EmailSenderAdapter (Identity -> IEmailService) |
| Provider Pattern | SqlProvider, DapperProvider, LogoRestServiceProvider |
| Template Method | E-posta sablon sistemi ([[Placeholder]]) |
| Background Service + Channel | DuesStatisticTransferBackgroundService |
| Dynamic Authorization | AuthorizationRulesInitializer + DynamicAuthorizationPolicyProvider |
| Flags Enum | BuggetRatioMounthEnum (ay secimi), ExtendedPropertyShowOnEnum |
| Soft Delete | StatusEnum.Deleted ile isaretlenen kayitlar |
| Audit Trail | Transaction + TransactionItem ile CRUD islemlerinin izlenmesi |

### 10.2 ResponseDto Pattern
Tum servisler `ResponseDto<T>` veya `ResponseListDto<T>` doner:
```csharp
ResponseDto.Success(statusCode, message)
ResponseDto<T>.SuccessData(data, statusCode, message)
ResponseListDto<T>.SuccessData(data, recordsTotal, recordsFiltered, statusCode, message)
```

### 10.3 Lisans Yonetimi
- Lisans dosyasi: `wwwroot/Licenses/license.lic`
- Public key: `wwwroot/Licenses/Koala.Yedpa.Yonetim_public.pem`
- RSA + SHA256 imza dogrulama + OAEP sifre cozme
- CustomerCode, ApplicationId, ExpirationDate, LogoClientId, LogoClientSecret bilgileri
- Gecersiz lisans -> uygulama baslamaz (WebUI)
- CryptoLicenseException ile runtime lisans kontrolu

---

## 11. BILINEN SORUNLAR VE TEKNIK BORC

### 11.1 Kritik Sorunlar
1. **SQL Injection Riski:** SqlProvider parametresiz sorgu kullaniyor. ApiLogoSqlDataService'te `Replace("'", "''")` ile onleme yapilmaya calisiliyor.
2. **CORS AllowAll:** Her iki projede de `AllowAnyOrigin()` aktif.
3. **appsettings.json'da acik metin:** Sifreler ve baglanti dizileri sifresiz.
4. **Production'da Swagger acik:** Her iki projede de her ortamda aktif.
5. **UseDeveloperExceptionPage mantik hatasi:** WebUI'de sadece `!IsDevelopment` kosulunda aktif.

### 11.2 Implementasyon Eksiklikleri
1. **ExtendedPropertyRecordValuesRepository:** Tum metotlar `NotImplementedException` firlatiyor.
2. **ExtendedPropertyValuesRepository:** Tamamen bos.
3. **AppRoleRepository:** 3 metot (GetRoleClaimById, GetRoleClaimByName, GetAllRoleClaims) implement edilmemis.
4. **BackgroundServices (bos):** IBackgroundServices arayuzu tanimli ama implement edilmemis.

### 11.3 Tutarsizliklar
1. **UnitOfWork vs Dogrudan SaveChanges:** Yonetim repository'leri dogrudan `SaveChangesAsync()` cagiriyor, ana repository'ler UnitOfWork'e birakiyor.
2. **Soft Delete vs Hard Delete:** Mail ve bazi QRCode metotlari hard delete yapiyor.
3. **Veritabani saglayicisi celiskisi:** csproj'da PostgreSQL saglayicisi var ama migration'larda SQL Server fonksiyonlari (GETUTCDATE, NEWID) kullanilmis.
4. **LogoClCardApiController duplikasyonu:** Hem WebApi'de hem WebUI'de mevcut.

### 11.4 Test Kapsami
- **Test Edilen:** Yonetim modulu (5 repository + 5 service = 105 test)
- **Test Edilmeyen:** Ana is servisleri (BudgetOrder, DuesStatistic, Transaction, QRCode, Workplace, Email, Crypto, Settings vb.)

### 11.5 Kod Kalitesi
1. **EmailTemplateRepository.GetByNameAsyc:** "Async" yerine "Asyc" yazim hatasi.
2. **EmailTemplateRepository.DeleteAsync:** void donduruyor ama "Async" ile bitiyor.
3. **WorkplaceRepository.UpdateAsync:** Task.Run ile sarilmis senkron islem.
4. **Console.WriteLine loglama:** LicenseValidator'da production'da uygun olmayan loglama.

---

## 12. PROJE DOSYA ISTATISTIKLERI

| Proje | .cs Dosyasi | .cshtml Dosyasi | Toplam |
|---|---|---|---|
| Koala.Yedpa.Core | ~50 | 0 | ~50 |
| Koala.Yedpa.Repositories | ~35 | 0 | ~35 |
| Koala.Yedpa.Service | ~30 | 0 | ~30 |
| Koala.Yedpa.WebApi | ~10 | 0 | ~10 |
| Koala.Yedpa.WebUI | ~30 | 59 | ~89 |
| Test Projeleri | ~12 | 0 | ~12 |
| **Toplam** | **~167** | **59** | **~226** |

---

## 13. GIT GECMISI (Son 20 Commit)

| Hash | Mesaj |
|---|---|
| 6fbf132 | feat: Add PendingInvoices endpoints to LogoClCardApiController |
| d49932c | docs: Add PendingInvoices implementation plan |
| d2d4896 | docs: Fix PendingInvoices spec - align SQL aliases with property names |
| d146cd3 | docs: Add PendingInvoices endpoint design spec |
| b345fd3 | feat: Add Yonetim module and refactor project structure |
| bd269a7 | test: Add unit test projects for YONETIM modules |
| 8c795b4 | feat: Add comprehensive logging with NLog and improve QR Code functionality |
| 20f627d | feat: Add WebAPI project and configure Swagger documentation |
| 5325e15 | fix: Make budgetType parameter optional in GetByYearAndType API |
| 7d69f2d | fix: Load source year records for additional budget type |
| dac3927 | style: Add compact table styles to reduce row height |
| 2b6a67f | fix: Update Workplace menu to use WorkplaceIndexNavClass |
| fd81438 | feat: Add Workplace navigation pages to ManageNavPages |
| 6dd0e0b | chore: Remove unused BudgetRatio and Site controllers/views |
| 1f71126 | feat: Add workplace current accounts and update functionality |
| 824e19f | fix: Restore missing Site controller and views |
| 6ac8d7d | feat: BudgetOrder transfer improvements and email notifications |
| 9bc30da | fix: Budget Order Create Page UI improvements and bug fixes |
| 72dd2d0 | fix: EmailService and LogoSyncJobService improvements |
| 46a7f7c | fix: Remove invalid Authorization header access in Message34EmailService |

---

## 14. DEGISKEN/TERMINOLOJI SOZLUGU

| Terim | Aciklama |
|---|---|
| **BudgetRatio** | Yildan yila butce artis orani (ornegin %10) |
| **DuesStatistic** | Isyeri bazli aylik aidat tutarlari (12 ay) |
| **BuggetType** | Budget (aidat) veya ExtraBudget (ek butce) |
| **BuggetRatioMounth** | Hangi aylar icin butce hesaplanacagi (bitwise flag) |
| **TransferStatus** | Logo'ya aktarim durumu (Pending/Completed/Failed/Canceled) |
| **Workplace** | Isyeri (dukkan/buro) kaydi |
| **ClCard** | Logo ERP cari kart tablosu |
| **LogicalRef/LogRef** | Logo ERP kayit referans numaralari |
| **DocTrackingNr** | Logo belge takip numarasi |
| **PartnerNo** | Logo CLCARD PARTNERNO (QR kod icin kullanilir) |
| **ExtendedProperties** | Modullere ozel dinamik alan sistemi |
| **Message34** | Dis e-posta gonderim servisi saglayicisi |
| **IdentityServer** | Merkezi kimlik dogrulama sunucusu (sistem-koala.com) |
| **CryptoService** | Dis API ile sifreleme/cozme servisi |
| **SeedService** | Baslangic verilerini olusturan servis |

---

> **Not:** Bu dokuman 02.05.2026 tarihinde projenin master branch'i uzerinden olusturulmustur. Guncel durum icin kaynak kodu inceleyiniz.
