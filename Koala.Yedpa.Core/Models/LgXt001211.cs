using Koala.Yedpa.Core.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("LG_XT001_211")]
    public class LgXt001211
    {
        [Key] // Senin anahtar alanın
        [Required]
        [StringLength(36)]
        public string Id { get; set; } = Tools.CreateGuidStr();

        // ---------- LOGO'DAN GELEN VERİLER ----------
        [Column("LOGREF")]
        public int LogRef { get; set; } // Sadece veri, PK değil, Identity değil

        [Column("PARLOGREF")]
        public int? ParLogRef { get; set; }
        [Column("GROUPCODE")]
        public string? GroupCode { get; set; }

        [Column("GROUPNAME")]
        public string? GroupName { get; set; }
        [Column("CLIENTCODE")]
        public string? ClientCode { get; set; }

        [Column("CLIENTNAME")]
        public string? ClientName { get; set; }

        [Column("CUSTOMER_TYPE")]
        public short? CustomerType { get; set; }

        [Column("RESIDENCETYPEREF")]
        public int? ResidenceTypeRef { get; set; }

        [Column("RESIDENCEGROUPREF")]
        public int? ResidenceGroupRef { get; set; }

        [Column("PARCELREF")]
        public int? ParcelRef { get; set; }

        [Column("PHASEREF")]
        public int? PhaseRef { get; set; }

        [Column("CAULDRONREF")]
        public int? CauldronRef { get; set; }

        [Column("SHARENO")]
        public int? ShareNo { get; set; }

        [Column("BEGDATE", TypeName = "datetime")]
        public DateTime? BegDate { get; set; }

        [Column("ENDDATE", TypeName = "datetime")]
        public DateTime? EndDate { get; set; }

        [Column("BLOCKREF")]
        public int? BlockRef { get; set; }

        [Column("INDDIVNO")]
        [StringLength(15)]
        public string? IndDivNo { get; set; }

        [Column("RESIDENCENO")]
        [StringLength(5)]
        public string? ResidenceNo { get; set; }

        [Column("DIMGROSS", TypeName = "float")]
        public double? DimGross { get; set; }

        [Column("DIMFIELD", TypeName = "float")]
        public double? DimField { get; set; }

        [Column("PERSONCOUNT")]
        public short? PersonCount { get; set; }

        [Column("WATERMETERNO")]
        [StringLength(21)]
        public string? WaterMeterNo { get; set; }

        [Column("CALMETERNO")]
        [StringLength(21)]
        public string? CalMeterNo { get; set; }

        [Column("HOTWATERMETERNO")]
        [StringLength(21)]
        public string? HotWaterMeterNo { get; set; }

        [Column("CHIEFREG")]
        public short? ChiefReg { get; set; }

        [Column("TAXPAYER")]
        public short? TaxPayer { get; set; }

        [Column("IDENTITYNR")]
        [StringLength(21)]
        public string? IdentityNr { get; set; }

        [Column("DEEDINFO")]
        public short? DeedInfo { get; set; }

        [Column("PROFITINGOWNER")]
        [StringLength(31)]
        public string? ProfitingOwner { get; set; }

        [Column("OFFICIALBEGDATE", TypeName = "datetime")]
        public DateTime? OfficialBegDate { get; set; }

        [Column("OFFICIALENDDATE", TypeName = "datetime")]
        public DateTime? OfficialEndDate { get; set; }

        [Column("GASCOEFFICIENT", TypeName = "float")]
        public double? GasCoefficient { get; set; }

        [Column("ACTIVERESDATE", TypeName = "datetime")]
        public DateTime? ActiveResDate { get; set; }

        [Column("BUDGETDEPOTMETRE1", TypeName = "float")]
        public double? BudgetDepotMetre1 { get; set; }

        [Column("BUDGETDEPOTMETRE2", TypeName = "float")]
        public double? BudgetDepotMetre2 { get; set; }

        [Column("BUDGETGROUNDMETRE", TypeName = "float")]
        public double? BudgetGroundMetre { get; set; }

        [Column("BUDGETHUNGMETRE", TypeName = "float")]
        public double? BudgetHungMetre { get; set; }

        [Column("BUDGETFLOORMETRE", TypeName = "float")]
        public double? BudgetFloorMetre { get; set; }

        [Column("BUDGETPASSAGEMETRE1", TypeName = "float")]
        public double? BudgetPassageMetre1 { get; set; }

        [Column("BUDGETPASSAGEMETRE2", TypeName = "float")]
        public double? BudgetPassageMetre2 { get; set; }

        [Column("BUDGETDEPOTCOEFFICIENT1", TypeName = "float")]
        public double? BudgetDepotCoefficient1 { get; set; }

        [Column("BUDGETDEPOTCOEFFICIENT2", TypeName = "float")]
        public double? BudgetDepotCoefficient2 { get; set; }

        [Column("BUDGETGROUNDCOEFFICIENT", TypeName = "float")]
        public double? BudgetGroundCoefficient { get; set; }

        [Column("BUDGETHUNGCOEFFICIENT", TypeName = "float")]
        public double? BudgetHungCoefficient { get; set; }

        [Column("BUDGETFLOORCOEFFICIENT", TypeName = "float")]
        public double? BudgetFloorCoefficient { get; set; }

        [Column("BUDGETPASSAGECOEFFICIENT1", TypeName = "float")]
        public double? BudgetPassageCoefficient1 { get; set; }

        [Column("BUDGETPASSAGECOEFFICIENT2", TypeName = "float")]
        public double? BudgetPassageCoefficient2 { get; set; }

        [Column("FUELDEPOTMETRE1", TypeName = "float")]
        public double? FuelDepotMetre1 { get; set; }

        [Column("FUELDEPOTMETRE2", TypeName = "float")]
        public double? FuelDepotMetre2 { get; set; }

        [Column("FUELGROUNDMETRE", TypeName = "float")]
        public double? FuelGroundMetre { get; set; }

        [Column("FUELHUNGMETRE", TypeName = "float")]
        public double? FuelHungMetre { get; set; }

        [Column("FUELFLOORMETRE", TypeName = "float")]
        public double? FuelFloorMetre { get; set; }

        [Column("FUELPASSAGEMETRE1", TypeName = "float")]
        public double? FuelPassageMetre1 { get; set; }

        [Column("FUELPASSAGEMETRE2", TypeName = "float")]
        public double? FuelPassageMetre2 { get; set; }

        [Column("FUELDEPOTCOEFFICIENT1", TypeName = "float")]
        public double? FuelDepotCoefficient1 { get; set; }

        [Column("FUELDEPOTCOEFFICIENT2", TypeName = "float")]
        public double? FuelDepotCoefficient2 { get; set; }

        [Column("FUELGROUNDCOEFFICIENT", TypeName = "float")]
        public double? FuelGroundCoefficient { get; set; }

        [Column("FUELHUNGCOEFFICIENT", TypeName = "float")]
        public double? FuelHungCoefficient { get; set; }

        [Column("FUELFLOORCOEFFICIENT", TypeName = "float")]
        public double? FuelFloorCoefficient { get; set; }

        [Column("FUELPASSAGECOEFFICIENT1", TypeName = "float")]
        public double? FuelPassageCoefficient1 { get; set; }

        [Column("FUELPASSAGECOEFFICIENT2", TypeName = "float")]
        public double? FuelPassageCoefficient2 { get; set; }

        [Column("TOTALBRUTCOEFFICIENTMETRE", TypeName = "float")]
        public double? TotalBrutCoefficientMetre { get; set; }

        [Column("TOTALNETMETRE", TypeName = "float")]
        public double? TotalNetMetre { get; set; }

        [Column("TOTALFUELMETRE", TypeName = "float")]
        public double? TotalFuelMetre { get; set; }
    }
}