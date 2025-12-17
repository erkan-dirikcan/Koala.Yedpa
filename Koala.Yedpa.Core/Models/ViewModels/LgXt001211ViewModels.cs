using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models.ViewModels
{
    public class LgXt001211ListViewModel
    {
        /// <summary>
        /// Satır ID Bilgisi
        /// </summary>
        public string Id { get; set; } = Tools.CreateGuidStr();

        // ---------- LOGO'DAN GELEN VERİLER ----------
        /// <summary>
        /// Logo Referans Bilgisi
        /// </summary>
        public int LogRef { get; set; } // Sadece veri, PK değil, Identity değil
        /// <summary>
        /// Logo Üst Referans Bilgisi
        /// </summary>
        public int? ParLogRef { get; set; }
        /// <summary>
        /// Dükkan/Depo Kodu
        /// </summary>
        public string? GroupCode { get; set; }
        /// <summary>
        /// Dükkan/Depo Tam Adresi
        /// </summary>
        public string? GroupName { get; set; }
        /// <summary>
        /// Cari Kodu
        /// </summary>
        public string? ClientCode { get; set; }
        /// <summary>
        /// Cari Ünvanı
        /// </summary>
        public string? ClientName { get; set; }

        /// <summary>
        /// Müşteri Tipi 1:Mal Sahibi 2:Kiracı,4:Diğer
        /// </summary>
        public short? CustomerType { get; set; }

        /// <summary>
        /// Başlangıç Tarihi
        /// </summary>
        public DateTime? BegDate { get; set; }

        /// <summary>
        /// Bitiş Tarihi
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Katsalı Bürüt Metre Kare
        /// </summary>
        public double? TotalBrutCoefficientMetre { get; set; }
        /// <summary>
        /// Net Metre Kare
        /// </summary>
        public double? TotalNetMetre { get; set; }
        /// <summary>
        /// Yakıt Metre Kare
        /// </summary>
        public double? TotalFuelMetre { get; set; }
    }

    
    public class LgXt001211UpdateViewModel
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; } = Tools.CreateGuidStr();

        // ---------- LOGO'DAN GELEN VERİLER ----------
        /// <summary>
        /// Logo Referans Bilgisi
        /// </summary>
        public int LogRef { get; set; } // Sadece veri, PK değil, Identity değil
        /// <summary>
        /// Logo Üst Referans Bilgisi
        /// </summary>
        public int? ParLogRef { get; set; }
        /// <summary>
        /// Dükkan/Depo Kodu
        /// </summary>
        public string? GroupCode { get; set; }

        /// <summary>
        /// Dükkan/Depo Tam Adresi
        /// </summary>
        public string? GroupName { get; set; }
        /// <summary>
        /// Cari Kodu
        /// </summary>
        public string? ClientCode { get; set; }

        /// <summary>
        /// Cari Ünvanı
        /// </summary>
        public string? ClientName { get; set; }
        /// <summary>
        /// Müşteri Tipi 1:Mal Sahibi 2:Kiracı,3:İşyeri,4:Diğer
        /// </summary>
        public short? CustomerType { get; set; }

        /// <summary>
        /// Kişi Sayısı
        /// </summary>
        public short? PersonCount { get; set; }
        /// <summary>
        /// Muhtarlık Kaydı 0:Var, 1:Yok
        /// </summary>
        public short? ChiefReg { get; set; }
        /// <summary>
        /// Vergi Mükellefliği 0:Muaf,1:Gelir Vergisi,2:Kurumsal,3:Diğer
        /// </summary>
        public short? TaxPayer { get; set; }
        /// <summary>
        /// Kimlik Numarası
        /// </summary>
        public string? IdentityNr { get; set; }
        /// <summary>
        /// Tapu Bilgisi Hisseli:0, Müstakil/Tam Mülkiyet :1
        /// </summary>
        public short? DeedInfo { get; set; }
        /// <summary>
        /// İntifa Sahibi
        /// </summary>
        public string? ProfitingOwner { get; set; }
        /// <summary>
        /// Giriş Tarihi
        /// </summary>
        public DateTime? BegDate { get; set; }

        /// <summary>
        /// Çıkış Tarihi
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Resmi Giriş Tarihi
        /// </summary>
        public DateTime? OfficialBegDate { get; set; }
        /// <summary>
        /// Resmi Çıkış Tarihi
        /// </summary>
        public DateTime? OfficialEndDate { get; set; }
        /// <summary>
        /// Gaz Katsayısı
        /// </summary>
        public double? GasCoefficient { get; set; }
        /// <summary>
        /// Konut Aktif Kayıt Tarihi
        /// </summary>
        public DateTime? ActiveResDate { get; set; }
        /// <summary>
        /// 1.Depo Bütçe Metre Kare
        /// </summary>
        public double? BudgetDepotMetre1 { get; set; }
        /// <summary>
        /// 2.Depo Bütçe Metre Kare
        /// </summary>
        public double? BudgetDepotMetre2 { get; set; }
        /// <summary>
        /// Zemin Bütçe Metre Kare
        /// </summary>
        public double? BudgetGroundMetre { get; set; }
        /// <summary>
        /// Asma Kat Bütçe Metre Kare
        /// </summary>
        public double? BudgetHungMetre { get; set; }
        /// <summary>
        /// Zemin Kat Bütçe Metre Kare
        /// </summary>
        public double? BudgetFloorMetre { get; set; }
        /// <summary>
        /// 1.Pasaj Bütçe Metre Kare
        /// </summary>
        public double? BudgetPassageMetre1 { get; set; }
        /// <summary>
        /// 2.Pasaj Bütçe Metre Kare
        /// </summary>
        public double? BudgetPassageMetre2 { get; set; }
        /// <summary>
        /// 1.Depo Bütçe Katsayısı
        /// </summary>
        public double? BudgetDepotCoefficient1 { get; set; }
        /// <summary>
        /// 2.Depo Bütçe Katsayısı
        /// </summary>
        public double? BudgetDepotCoefficient2 { get; set; }
        /// <summary>
        /// Zemin Bütçe Katsayısı
        /// </summary>
        public double? BudgetGroundCoefficient { get; set; }
        /// <summary>
        /// Asma Kat Bütçe Katsayısı
        /// </summary>
        public double? BudgetHungCoefficient { get; set; }
        /// <summary>
        /// Zemin Kat Bütçe Katsayısı
        /// </summary>
        public double? BudgetFloorCoefficient { get; set; }
        /// <summary>
        /// 1.Pasaj Bütçe Katsayısı
        /// </summary>
        public double? BudgetPassageCoefficient1 { get; set; }
        /// <summary>
        /// 2.Pasaj Bütçe Katsayı
        /// </summary>
        public double? BudgetPassageCoefficient2 { get; set; }
        /// <summary>
        /// 1.Depo Yakıt Metre Kare
        /// </summary>
        public double? FuelDepotMetre1 { get; set; }
        /// <summary>
        /// 2.Depo Yakıt Metre Kare
        /// </summary>
        public double? FuelDepotMetre2 { get; set; }
        /// <summary>
        /// Zemin Yakıt Metre Kare
        /// </summary>
        public double? FuelGroundMetre { get; set; }
        /// <summary>
        /// Asma Kat Yakıt Metre Kare
        /// </summary>
        public double? FuelHungMetre { get; set; }
        /// <summary>
        /// 1.Kat Yakıt Metre Kare
        /// </summary>
        public double? FuelFloorMetre { get; set; }
        /// <summary>
        /// 1.Pasaj Yakıt Metre Kare
        /// </summary>
        public double? FuelPassageMetre1 { get; set; }
        /// <summary>
        /// 2.Pasaj Yakıt Metre Kare
        /// </summary>
        public double? FuelPassageMetre2 { get; set; }
        /// <summary>
        /// 1.Depo Yakıt Katsayısı
        /// </summary>
        public double? FuelDepotCoefficient1 { get; set; }
        /// <summary>
        /// 2.Depo Yakıt Katsayısı
        /// </summary>
        public double? FuelDepotCoefficient2 { get; set; }
        /// <summary>
        /// Zemin Yakıt Katsayısı
        /// </summary>
        public double? FuelGroundCoefficient { get; set; }
        /// <summary>
        /// Asma Kat Yakıt Katsayısı
        /// </summary>
        public double? FuelHungCoefficient { get; set; }
        /// <summary>
        /// 1.Kat Yakıt Katsayısı
        /// </summary>
        public double? FuelFloorCoefficient { get; set; }
        /// <summary>
        /// 1.Pasaj Yakıt Katsayısı
        /// </summary>
        public double? FuelPassageCoefficient1 { get; set; }
        /// <summary>
        /// 2.Pasaj Yakıt Katsayısı
        /// </summary>
        public double? FuelPassageCoefficient2 { get; set; }
        /// <summary>
        /// Kayıtlı Bürüt Metrekare
        /// </summary>
        public double? TotalBrutCoefficientMetre { get; set; }
        /// <summary>
        /// Met Metre Kare
        /// </summary>
        public double? TotalNetMetre { get; set; }
        /// <summary>
        /// Yakıt Metrekare
        /// </summary>
        public double? TotalFuelMetre { get; set; }
    }
}
