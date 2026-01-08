using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koala.Yedpa.Core.Models;

[Table("Workplace")]
public class Workplace
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // LG_211_CLCARD tablosundan gelen alanlar
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Definition { get; set; } = string.Empty;

    public int LogicalRef { get; set; }

    // LG_XT001_211 tablosundan gelen alanlar
    public int LogRef { get; set; }

    public short? CustomerType { get; set; }

    public int? ResidenceTypeRef { get; set; }

    public int? ResidenceGroupRef { get; set; }

    public int? ParcelRef { get; set; }

    public int? PhaseRef { get; set; }

    public int? CauldronRef { get; set; }

    public int? ShareNo { get; set; }

    public DateTime? BegDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? BlockRef { get; set; }

    [MaxLength(50)]
    public string? IndDivNo { get; set; }

    [MaxLength(50)]
    public string? ResidenceNo { get; set; }

    public double? DimGross { get; set; }

    public double? DimField { get; set; }

    public short? PersonCount { get; set; }

    [MaxLength(50)]
    public string? WaterMeterNo { get; set; }

    [MaxLength(50)]
    public string? CalMeterNo { get; set; }

    [MaxLength(50)]
    public string? HotWaterMeterNo { get; set; }

    public short? ChiefReg { get; set; }

    public short? TaxPayer { get; set; }

    [MaxLength(50)]
    public string? IdentityNr { get; set; }

    public short? DeedInfo { get; set; }

    [MaxLength(200)]
    public string? ProfitingOwner { get; set; }

    public double? GasCoefficient { get; set; }

    public DateTime? ActiveResDate { get; set; }

    // Bölütle ilgili metre ve katsayılar
    public double? BudgetDepotMetre1 { get; set; }

    public double? BudgetDepotMetre2 { get; set; }

    public double? BudgetGroundMetre { get; set; }

    public double? BudgetHungMetre { get; set; }

    public double? BudgetFloorMetre { get; set; }

    public double? BudgetPassageMetre1 { get; set; }

    public double? BudgetPassageMetre2 { get; set; }

    public double? BudgetDepotCoefficient1 { get; set; }

    public double? BudgetDepotCoefficient2 { get; set; }

    public double? BudgetGroundCoefficient { get; set; }

    public double? BudgetHungCoefficient { get; set; }

    public double? BudgetFloorCoefficient { get; set; }

    public double? BudgetPassageCoefficient1 { get; set; }

    public double? BudgetPassageCoefficient2 { get; set; }

    // Yakıt ile ilgili metre ve katsayılar
    public double? FuelDepotMetre1 { get; set; }

    public double? FuelDepotMetre2 { get; set; }

    public double? FuelGroundMetre { get; set; }

    public double? FuelHungMetre { get; set; }

    public double? FuelFloorMetre { get; set; }

    public double? FuelPassageMetre1 { get; set; }

    public double? FuelPassageMetre2 { get; set; }

    public double? FuelDepotCoefficient1 { get; set; }

    public double? FuelDepotCoefficient2 { get; set; }

    public double? FuelGroundCoefficient { get; set; }

    public double? FuelHungCoefficient { get; set; }

    public double? FuelFloorCoefficient { get; set; }

    public double? FuelPassageCoefficient1 { get; set; }

    public double? FuelPassageCoefficient2 { get; set; }

    // Toplam değerler
    public double? TotalBrutCoefficientMetre { get; set; }

    public double? TotalNetMetre { get; set; }

    public double? TotalFuelMetre { get; set; }
}
