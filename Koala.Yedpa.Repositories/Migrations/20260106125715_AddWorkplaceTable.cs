using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkplaceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LG_XT001_211");

            migrationBuilder.CreateTable(
                name: "Workplace",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "(NEWID())"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LogicalRef = table.Column<int>(type: "int", nullable: false),
                    LogRef = table.Column<int>(type: "int", nullable: false),
                    CustomerType = table.Column<short>(type: "smallint", nullable: true),
                    ResidenceTypeRef = table.Column<int>(type: "int", nullable: true),
                    ResidenceGroupRef = table.Column<int>(type: "int", nullable: true),
                    ParcelRef = table.Column<int>(type: "int", nullable: true),
                    PhaseRef = table.Column<int>(type: "int", nullable: true),
                    CauldronRef = table.Column<int>(type: "int", nullable: true),
                    ShareNo = table.Column<int>(type: "int", nullable: true),
                    BegDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockRef = table.Column<int>(type: "int", nullable: true),
                    IndDivNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResidenceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DimGross = table.Column<double>(type: "float", nullable: true),
                    DimField = table.Column<double>(type: "float", nullable: true),
                    PersonCount = table.Column<short>(type: "smallint", nullable: true),
                    WaterMeterNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CalMeterNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HotWaterMeterNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ChiefReg = table.Column<short>(type: "smallint", nullable: true),
                    TaxPayer = table.Column<short>(type: "smallint", nullable: true),
                    IdentityNr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeedInfo = table.Column<short>(type: "smallint", nullable: true),
                    ProfitingOwner = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GasCoefficient = table.Column<double>(type: "float", nullable: true),
                    ActiveResDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BudgetDepotMetre1 = table.Column<double>(type: "float", nullable: true),
                    BudgetDepotMetre2 = table.Column<double>(type: "float", nullable: true),
                    BudgetGroundMetre = table.Column<double>(type: "float", nullable: true),
                    BudgetHungMetre = table.Column<double>(type: "float", nullable: true),
                    BudgetFloorMetre = table.Column<double>(type: "float", nullable: true),
                    BudgetPassageMetre1 = table.Column<double>(type: "float", nullable: true),
                    BudgetPassageMetre2 = table.Column<double>(type: "float", nullable: true),
                    BudgetDepotCoefficient1 = table.Column<double>(type: "float", nullable: true),
                    BudgetDepotCoefficient2 = table.Column<double>(type: "float", nullable: true),
                    BudgetGroundCoefficient = table.Column<double>(type: "float", nullable: true),
                    BudgetHungCoefficient = table.Column<double>(type: "float", nullable: true),
                    BudgetFloorCoefficient = table.Column<double>(type: "float", nullable: true),
                    BudgetPassageCoefficient1 = table.Column<double>(type: "float", nullable: true),
                    BudgetPassageCoefficient2 = table.Column<double>(type: "float", nullable: true),
                    FuelDepotMetre1 = table.Column<double>(type: "float", nullable: true),
                    FuelDepotMetre2 = table.Column<double>(type: "float", nullable: true),
                    FuelGroundMetre = table.Column<double>(type: "float", nullable: true),
                    FuelHungMetre = table.Column<double>(type: "float", nullable: true),
                    FuelFloorMetre = table.Column<double>(type: "float", nullable: true),
                    FuelPassageMetre1 = table.Column<double>(type: "float", nullable: true),
                    FuelPassageMetre2 = table.Column<double>(type: "float", nullable: true),
                    FuelDepotCoefficient1 = table.Column<double>(type: "float", nullable: true),
                    FuelDepotCoefficient2 = table.Column<double>(type: "float", nullable: true),
                    FuelGroundCoefficient = table.Column<double>(type: "float", nullable: true),
                    FuelHungCoefficient = table.Column<double>(type: "float", nullable: true),
                    FuelFloorCoefficient = table.Column<double>(type: "float", nullable: true),
                    FuelPassageCoefficient1 = table.Column<double>(type: "float", nullable: true),
                    FuelPassageCoefficient2 = table.Column<double>(type: "float", nullable: true),
                    TotalBrutCoefficientMetre = table.Column<double>(type: "float", nullable: true),
                    TotalNetMetre = table.Column<double>(type: "float", nullable: true),
                    TotalFuelMetre = table.Column<double>(type: "float", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreateUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workplace", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workplace_Code",
                table: "Workplace",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Workplace_LogicalRef",
                table: "Workplace",
                column: "LogicalRef",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workplace_LogRef",
                table: "Workplace",
                column: "LogRef",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Workplace");

            migrationBuilder.CreateTable(
                name: "LG_XT001_211",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ACTIVE = table.Column<int>(type: "int", nullable: true),
                    ACTIVERESDATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    BEGDATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    BLOCKREF = table.Column<int>(type: "int", nullable: true),
                    BUDGETDEPOTCOEFFICIENT1 = table.Column<double>(type: "float", nullable: true),
                    BUDGETDEPOTCOEFFICIENT2 = table.Column<double>(type: "float", nullable: true),
                    BUDGETDEPOTMETRE1 = table.Column<double>(type: "float", nullable: true),
                    BUDGETDEPOTMETRE2 = table.Column<double>(type: "float", nullable: true),
                    BUDGETFLOORCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    BUDGETFLOORMETRE = table.Column<double>(type: "float", nullable: true),
                    BUDGETGROUNDCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    BUDGETGROUNDMETRE = table.Column<double>(type: "float", nullable: true),
                    BUDGETHUNGCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    BUDGETHUNGMETRE = table.Column<double>(type: "float", nullable: true),
                    BUDGETPASSAGECOEFFICIENT1 = table.Column<double>(type: "float", nullable: true),
                    BUDGETPASSAGECOEFFICIENT2 = table.Column<double>(type: "float", nullable: true),
                    BUDGETPASSAGEMETRE1 = table.Column<double>(type: "float", nullable: true),
                    BUDGETPASSAGEMETRE2 = table.Column<double>(type: "float", nullable: true),
                    CALMETERNO = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: true),
                    CAULDRONREF = table.Column<int>(type: "int", nullable: true),
                    CHIEFREG = table.Column<short>(type: "smallint", nullable: true),
                    CLIENTCODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CLIENTNAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CUSTOMER_TYPE = table.Column<short>(type: "smallint", nullable: true),
                    DEEDINFO = table.Column<short>(type: "smallint", nullable: true),
                    DIMFIELD = table.Column<double>(type: "float", nullable: true),
                    DIMGROSS = table.Column<double>(type: "float", nullable: true),
                    ENDDATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    FUELDEPOTCOEFFICIENT1 = table.Column<double>(type: "float", nullable: true),
                    FUELDEPOTCOEFFICIENT2 = table.Column<double>(type: "float", nullable: true),
                    FUELDEPOTMETRE1 = table.Column<double>(type: "float", nullable: true),
                    FUELDEPOTMETRE2 = table.Column<double>(type: "float", nullable: true),
                    FUELFLOORCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    FUELFLOORMETRE = table.Column<double>(type: "float", nullable: true),
                    FUELGROUNDCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    FUELGROUNDMETRE = table.Column<double>(type: "float", nullable: true),
                    FUELHUNGCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    FUELHUNGMETRE = table.Column<double>(type: "float", nullable: true),
                    FUELPASSAGECOEFFICIENT1 = table.Column<double>(type: "float", nullable: true),
                    FUELPASSAGECOEFFICIENT2 = table.Column<double>(type: "float", nullable: true),
                    FUELPASSAGEMETRE1 = table.Column<double>(type: "float", nullable: true),
                    FUELPASSAGEMETRE2 = table.Column<double>(type: "float", nullable: true),
                    GASCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    GROUPCODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GROUPNAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HOTWATERMETERNO = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: true),
                    IDENTITYNR = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: true),
                    INDDIVNO = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    LOGREF = table.Column<int>(type: "int", nullable: false),
                    OFFICIALBEGDATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    OFFICIALENDDATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    PARLOGREF = table.Column<int>(type: "int", nullable: true),
                    PARCELREF = table.Column<int>(type: "int", nullable: true),
                    PARENTCLREF = table.Column<int>(type: "int", nullable: true),
                    PERSONCOUNT = table.Column<short>(type: "smallint", nullable: true),
                    PHASEREF = table.Column<int>(type: "int", nullable: true),
                    PROFITINGOWNER = table.Column<string>(type: "nvarchar(31)", maxLength: 31, nullable: true),
                    RESIDENCEGROUPREF = table.Column<int>(type: "int", nullable: true),
                    RESIDENCENO = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    RESIDENCETYPEREF = table.Column<int>(type: "int", nullable: true),
                    SHARENO = table.Column<int>(type: "int", nullable: true),
                    SPECODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TAXPAYER = table.Column<short>(type: "smallint", nullable: true),
                    TOTALBRUTCOEFFICIENTMETRE = table.Column<double>(type: "float", nullable: true),
                    TOTALFUELMETRE = table.Column<double>(type: "float", nullable: true),
                    TOTALNETMETRE = table.Column<double>(type: "float", nullable: true),
                    WATERMETERNO = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LG_XT001_211", x => x.Id);
                });
        }
    }
}
