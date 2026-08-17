namespace Koala.Yedpa.WebUI.Authorization;

/// <summary>Katalogdaki tek bir izin tanımı. Name → Claims.Name → policy adı.</summary>
public sealed record PermissionDefinition(string Name, string DisplayName, string Description);

/// <summary>Katalogdaki bir modül ve altındaki izinler.</summary>
public sealed record PermissionModule(
    string Name,
    string DisplayName,
    string Description,
    IReadOnlyList<PermissionDefinition> Permissions);

/// <summary>
/// Kodda kullanılan tüm izinlerin TEK doğruluk kaynağı.
/// Buraya eklenen her izin, PermissionSeeder tarafından uygulama açılışında
/// Module + Claims tablolarına idempotent şekilde yazılır ve
/// AppRole/AddClaimToRole ekranında otomatik görünür.
///
/// Yeni izin eklerken:
/// 1. İlgili gruba bir sabit ekle
/// 2. Modules listesindeki ilgili modülün Permissions dizisine PermissionDefinition ekle
/// 3. Controller/action üzerine [Permission(...)] koy
/// </summary>
public static class PermissionCatalog
{
    public static class UserManagement
    {
        public const string List = "UserManagement.List";
        public const string Create = "UserManagement.Create";
        public const string Update = "UserManagement.Update";
        public const string ChangeStatus = "UserManagement.ChangeStatus";
        public const string AssignRole = "UserManagement.AssignRole";
    }

    public static class RoleManagement
    {
        public const string List = "RoleManagement.List";
        public const string Create = "RoleManagement.Create";
        public const string Update = "RoleManagement.Update";
        public const string Delete = "RoleManagement.Delete";
        public const string AssignClaim = "RoleManagement.AssignClaim";
    }

    public static class ModuleManagement
    {
        public const string List = "ModuleManagement.List";
        public const string Create = "ModuleManagement.Create";
        public const string Update = "ModuleManagement.Update";
        public const string ChangeStatus = "ModuleManagement.ChangeStatus";
        public const string ClaimList = "ModuleManagement.ClaimList";
        public const string ClaimCreate = "ModuleManagement.ClaimCreate";
        public const string ClaimUpdate = "ModuleManagement.ClaimUpdate";
        public const string ClaimDelete = "ModuleManagement.ClaimDelete";
    }

    public static class Dashboard
    {
        public const string View = "Dashboard.View";
    }

    public static class BulkInvoice
    {
        public const string View = "BulkInvoice.View";
        public const string Transfer = "BulkInvoice.Transfer";
    }

    public static class BudgetOrder
    {
        public const string View = "BudgetOrder.View";
        public const string Create = "BudgetOrder.Create";
        public const string Calculate = "BudgetOrder.Calculate";
        public const string Transfer = "BudgetOrder.Transfer";
        public const string RatioView = "BudgetOrder.RatioView";
        public const string RatioManage = "BudgetOrder.RatioManage";
    }

    /// <summary>
    /// Aidat istatistiği okuma izni. DİKKAT: bu uçları HEM bütçe emri ekranı
    /// HEM DE işyerleri ekranı çağırıyor (yıl listesi açılır kutusu).
    /// Bu yüzden BudgetOrder altına değil, ayrı modül olarak konuldu;
    /// Workplace.View verilen role bu izin de verilmezse işyerleri ekranındaki
    /// yıl listesi sessizce boş kalır.
    /// </summary>
    public static class DuesStatistic
    {
        public const string View = "DuesStatistic.View";
    }

    public static class Workplace
    {
        public const string View = "Workplace.View";
        public const string Update = "Workplace.Update";
        public const string SendBudgetEmail = "Workplace.SendBudgetEmail";
        public const string ExportBudgetExcel = "Workplace.ExportBudgetExcel";
    }

    public static class QRCode
    {
        public const string View = "QRCode.View";
        public const string Create = "QRCode.Create";
        public const string Delete = "QRCode.Delete";
    }

    public static class CurrentAccount
    {
        public const string View = "CurrentAccount.View";
        public const string StatementView = "CurrentAccount.StatementView";
    }

    /// <summary>
    /// Controllers/Yonetim altındaki arıza, arşiv, otopark ve sözleşme ekranları.
    /// Menüsü şu an gizli ama controller'lar URL'den erişilebilir durumda.
    /// </summary>
    public static class Management
    {
        public const string ArizaView = "Management.ArizaView";
        public const string ArizaManage = "Management.ArizaManage";
        public const string ArsivView = "Management.ArsivView";
        public const string ArsivManage = "Management.ArsivManage";
        public const string OtoparkView = "Management.OtoparkView";
        public const string OtoparkManage = "Management.OtoparkManage";
        public const string SozlesmeView = "Management.SozlesmeView";
        public const string SozlesmeManage = "Management.SozlesmeManage";
    }

    public static class Settings
    {
        public const string View = "Settings.View";
        public const string Update = "Settings.Update";
    }

    /// <summary>
    /// Bakım/teşhis uçları. Normal kullanıcıya verilmez; sadece sistem yöneticisi rolüne.
    /// </summary>
    public static class SystemMaintenance
    {
        public const string LogoSync = "SystemMaintenance.LogoSync";
        public const string ConnectionTest = "SystemMaintenance.ConnectionTest";
        public const string TestEmail = "SystemMaintenance.TestEmail";
    }

    public static readonly IReadOnlyList<PermissionModule> Modules =
    [
        new PermissionModule("UserManagement", "Kullanıcı Yönetimi",
            "Kullanıcı listeleme, oluşturma, güncelleme ve rol atama işlemleri",
        [
            new(UserManagement.List, "Kullanıcı Listesi", "Kullanıcı listesini görüntüler"),
            new(UserManagement.Create, "Kullanıcı Ekle", "Yeni kullanıcı oluşturur"),
            new(UserManagement.Update, "Kullanıcı Güncelle", "Mevcut kullanıcıyı günceller"),
            new(UserManagement.ChangeStatus, "Kullanıcı Durumu Değiştir", "Kullanıcıyı aktif/pasif yapar"),
            new(UserManagement.AssignRole, "Kullanıcıya Rol Ata", "Kullanıcının rollerini düzenler")
        ]),

        new PermissionModule("RoleManagement", "Rol Yönetimi",
            "Rol tanımlama ve rollere yetki (claim) atama işlemleri",
        [
            new(RoleManagement.List, "Rol Listesi", "Rol listesini görüntüler"),
            new(RoleManagement.Create, "Rol Ekle", "Yeni rol oluşturur"),
            new(RoleManagement.Update, "Rol Güncelle", "Mevcut rolü günceller"),
            new(RoleManagement.Delete, "Rol Sil", "Rolü siler"),
            new(RoleManagement.AssignClaim, "Role Yetki Ata", "Rolün yetkilerini düzenler")
        ]),

        new PermissionModule("ModuleManagement", "Modül Yönetimi",
            "Modül ve modül altındaki yetki (claim) tanımlama işlemleri",
        [
            new(ModuleManagement.List, "Modül Listesi", "Modül listesini görüntüler"),
            new(ModuleManagement.Create, "Modül Ekle", "Yeni modül oluşturur"),
            new(ModuleManagement.Update, "Modül Güncelle", "Mevcut modülü günceller"),
            new(ModuleManagement.ChangeStatus, "Modül Durumu Değiştir", "Modülü aktif/pasif yapar"),
            new(ModuleManagement.ClaimList, "Yetki Listesi", "Modüle ait yetkileri görüntüler"),
            new(ModuleManagement.ClaimCreate, "Yetki Ekle", "Modüle yeni yetki tanımlar"),
            new(ModuleManagement.ClaimUpdate, "Yetki Güncelle", "Mevcut yetkiyi günceller"),
            new(ModuleManagement.ClaimDelete, "Yetki Sil", "Yetkiyi siler")
        ]),

        new PermissionModule("Dashboard", "Kokpit",
            "Ana kokpit ekranı",
        [
            new(Dashboard.View, "Kokpiti Görüntüle", "Ana kokpit ekranını açar")
        ]),

        new PermissionModule("BulkInvoice", "Toplu Faturalandırma",
            "Aidat siparişlerinden toplu fatura oluşturma işlemleri",
        [
            new(BulkInvoice.View, "Toplu Faturalandırmayı Görüntüle", "Toplu faturalandırma ekranını açar"),
            new(BulkInvoice.Transfer, "Fatura Aktarımı Başlat", "Logo'ya fatura aktarımını tetikler")
        ]),

        new PermissionModule("BudgetOrder", "Bütçe & Sipariş",
            "Bütçe emri oluşturma, hesaplama, oran tanımlama ve Logo'ya aktarma işlemleri",
        [
            new(BudgetOrder.View, "Bütçe Emrini Görüntüle", "Bütçe emri ekranını açar"),
            new(BudgetOrder.Create, "Bütçe/Sipariş Oluştur", "Yeni bütçe ve bütçeye bağlı siparişleri oluşturur"),
            new(BudgetOrder.Calculate, "Bütçe Hesapla", "Bütçe hesaplama ve güncelleme önizlemesi yapar"),
            new(BudgetOrder.Transfer, "Aidat İstatistiği Aktar", "Aidat istatistiklerini aktarır"),
            new(BudgetOrder.RatioView, "Bütçe Oranlarını Görüntüle", "Tanımlı bütçe oranlarını listeler"),
            new(BudgetOrder.RatioManage, "Bütçe Oranlarını Yönet", "Bütçe oranı ekler, günceller ve siler")
        ]),

        new PermissionModule("DuesStatistic", "Aidat İstatistiği",
            "Aidat istatistiği okuma. Bütçe emri ve işyerleri ekranlarının ikisi de bu izni ister",
        [
            new(DuesStatistic.View, "Aidat İstatistiğini Görüntüle",
                "Aidat istatistiği listelerini ve yıl seçeneklerini okur. Bütçe emri ve işyerleri ekranları için gereklidir")
        ]),

        new PermissionModule("Workplace", "İşyerleri",
            "İşyeri listeleme, güncelleme ve bütçe bilgilendirme işlemleri",
        [
            new(Workplace.View, "İşyerlerini Görüntüle", "İşyeri listesini ve detayını açar"),
            new(Workplace.Update, "İşyeri Güncelle", "İşyeri bilgilerini günceller"),
            new(Workplace.SendBudgetEmail, "Bütçe E-postası Gönder", "İşyerlerine toplu bütçe bilgilendirme e-postası gönderir"),
            new(Workplace.ExportBudgetExcel, "Bütçe Excel'i İndir", "İşyeri bütçe listesini Excel olarak üretir")
        ]),

        new PermissionModule("QRCode", "QR Kod",
            "QR kod üretme, listeleme ve silme işlemleri",
        [
            new(QRCode.View, "QR Kodları Görüntüle", "QR kod listesini ve parti detaylarını açar"),
            new(QRCode.Create, "QR Kod Üret", "Yeni QR kod ve QR kod partisi üretir, PDF çıktısı alır"),
            new(QRCode.Delete, "QR Kod Sil", "QR kodu veya QR kod partisini siler")
        ]),

        new PermissionModule("CurrentAccount", "Cari Hesap",
            "Cari kart listeleme ve cari ekstre görüntüleme",
        [
            new(CurrentAccount.View, "Cari Kartları Görüntüle", "Logo cari kart listesini ve aramasını açar"),
            new(CurrentAccount.StatementView, "Cari Ekstre Görüntüle", "Cari hesap ekstresini ve özetini görüntüler")
        ]),

        new PermissionModule("Management", "Yönetim",
            "Arıza, arşiv, otopark ve sözleşme yönetim ekranları",
        [
            new(Management.ArizaView, "Arızaları Görüntüle", "Arıza listesini ve detayını açar"),
            new(Management.ArizaManage, "Arıza Yönet", "Yeni arıza oluşturur ve personel atar"),
            new(Management.ArsivView, "Arşivi Görüntüle", "Arşiv listesini ve detayını açar"),
            new(Management.ArsivManage, "Arşiv Yönet", "Arşive koli ekler"),
            new(Management.OtoparkView, "Otoparkı Görüntüle", "Otopark listesini açar"),
            new(Management.OtoparkManage, "Otopark Yönet", "Araç giriş/çıkış ve abonelik işlemlerini yapar"),
            new(Management.SozlesmeView, "Sözleşmeleri Görüntüle", "Sözleşme listesini, detayını ve çıktısını açar"),
            new(Management.SozlesmeManage, "Sözleşme Yönet", "Yeni sözleşme oluşturur ve düzenler")
        ]),

        new PermissionModule("Settings", "Ayarlar",
            "Logo bağlantı ve sistem ayarları",
        [
            new(Settings.View, "Ayarları Görüntüle", "Ayar ekranlarını açar"),
            new(Settings.Update, "Ayarları Güncelle", "Ayarları kaydeder")
        ]),

        new PermissionModule("SystemMaintenance", "Sistem Bakımı",
            "Bakım ve teşhis işlemleri. Yalnızca sistem yöneticisine verilmelidir",
        [
            new(SystemMaintenance.LogoSync, "Logo Senkronizasyonu Çalıştır", "Logo verilerini elle senkronize eder"),
            new(SystemMaintenance.ConnectionTest, "Bağlantı Testi", "Veritabanı ve servis bağlantılarını test eder"),
            new(SystemMaintenance.TestEmail, "Test E-postası Gönder", "SMTP ayarlarını doğrulamak için test e-postası gönderir")
        ])
    ];

    /// <summary>Katalogdaki tüm izin adları (düz liste).</summary>
    public static readonly IReadOnlyList<string> AllPermissionNames =
        Modules.SelectMany(m => m.Permissions).Select(p => p.Name).ToList();
}
