namespace Koala.Yedpa.Core.Models.ViewModels;

public class WorkplaceListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string ClCode { get; set; } = string.Empty;
    public string ClDefinition { get; set; } = string.Empty;
    public int LogicalRef { get; set; }
    public int LogRef { get; set; }
    public short? CustomerType { get; set; }
    public DateTime? BegDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double? TotalNetMetre { get; set; }
    public double? TotalFuelMetre { get; set; }
    public List<WorkplaceCurrentAccounts> CurrentAccounts { get; set; }
}

public class WorkplaceDetailViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string ClCode { get; set; } = string.Empty;
    public string ClDefinition { get; set; } = string.Empty;
    public int LogicalRef { get; set; }
    public int LogRef { get; set; }

    // Müşteri tipi bilgileri
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

    // Konut bilgileri
    public string? IndDivNo { get; set; }
    public string? ResidenceNo { get; set; }
    public double? DimGross { get; set; }
    public double? DimField { get; set; }
    public short? PersonCount { get; set; }

    // Sayaç bilgileri
    public string? WaterMeterNo { get; set; }
    public string? CalMeterNo { get; set; }
    public string? HotWaterMeterNo { get; set; }

    // Yasal bilgiler
    public short? ChiefReg { get; set; }
    public short? TaxPayer { get; set; }
    public string? IdentityNr { get; set; }
    public short? DeedInfo { get; set; }
    public string? ProfitingOwner { get; set; }

    // Katsayılar
    public double? GasCoefficient { get; set; }
    public DateTime? ActiveResDate { get; set; }

    // Bölütle ilgili metreler
    public double? BudgetDepotMetre1 { get; set; }
    public double? BudgetDepotMetre2 { get; set; }
    public double? BudgetGroundMetre { get; set; }
    public double? BudgetHungMetre { get; set; }
    public double? BudgetFloorMetre { get; set; }
    public double? BudgetPassageMetre1 { get; set; }
    public double? BudgetPassageMetre2 { get; set; }

    // Bölütle ilgili katsayılar
    public double? BudgetDepotCoefficient1 { get; set; }
    public double? BudgetDepotCoefficient2 { get; set; }
    public double? BudgetGroundCoefficient { get; set; }
    public double? BudgetHungCoefficient { get; set; }
    public double? BudgetFloorCoefficient { get; set; }
    public double? BudgetPassageCoefficient1 { get; set; }
    public double? BudgetPassageCoefficient2 { get; set; }

    // Yakıt ile ilgili metreler
    public double? FuelDepotMetre1 { get; set; }
    public double? FuelDepotMetre2 { get; set; }
    public double? FuelGroundMetre { get; set; }
    public double? FuelHungMetre { get; set; }
    public double? FuelFloorMetre { get; set; }
    public double? FuelPassageMetre1 { get; set; }
    public double? FuelPassageMetre2 { get; set; }

    // Yakıt ile ilgili katsayılar
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

    // Cari bilgileri
    public string? CariCode { get; set; }
    public string? CariDefinition { get; set; }

    public List<WorkplaceCurrentAccounts> CurrentAccounts { get; set; }
}

public class WorkplaceCurrentAccounts
{
    public int LogicalRef { get; set; }
    public string Code { get; set; }
    public string Definition { get; set; }
    public string EmailAddress { get; set; }
    
}
