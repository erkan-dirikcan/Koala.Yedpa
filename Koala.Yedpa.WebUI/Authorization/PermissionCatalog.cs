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
    }

    public static class Settings
    {
        public const string View = "Settings.View";
        public const string Update = "Settings.Update";
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

        new PermissionModule("BudgetOrder", "Bütçe Emri",
            "Bütçe emri ekranları",
        [
            new(BudgetOrder.View, "Bütçe Emrini Görüntüle", "Bütçe emri ekranını açar")
        ]),

        new PermissionModule("Settings", "Ayarlar",
            "Logo bağlantı ve sistem ayarları",
        [
            new(Settings.View, "Ayarları Görüntüle", "Ayar ekranlarını açar"),
            new(Settings.Update, "Ayarları Güncelle", "Ayarları kaydeder")
        ])
    ];

    /// <summary>Katalogdaki tüm izin adları (düz liste).</summary>
    public static readonly IReadOnlyList<string> AllPermissionNames =
        Modules.SelectMany(m => m.Permissions).Select(p => p.Name).ToList();
}
