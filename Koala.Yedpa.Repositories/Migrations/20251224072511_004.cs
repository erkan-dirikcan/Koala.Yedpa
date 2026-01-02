using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class _004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create BudgetRatio table
            migrationBuilder.CreateTable(
                name: "BudgetRatio",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Ratio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalBugget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BuggetRatioMounths = table.Column<int>(type: "int", nullable: false),
                    BuggetType = table.Column<int>(type: "int", nullable: false),
                    CreateUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetRatio", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRatio_BuggetType",
                table: "BudgetRatio",
                column: "BuggetType");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRatio_Year",
                table: "BudgetRatio",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRatio_Code_Year",
                table: "BudgetRatio",
                columns: new[] { "Code", "Year" },
                unique: true);

            // Add columns to LgXt001211
            migrationBuilder.AddColumn<int>(
                name: "PARENTCLREF",
                table: "LG_XT001_211",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SPECODE",
                table: "LG_XT001_211",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ACTIVE",
                table: "LG_XT001_211",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop LgXt001211 columns
            migrationBuilder.DropColumn(
                name: "PARENTCLREF",
                table: "LG_XT001_211");

            migrationBuilder.DropColumn(
                name: "SPECODE",
                table: "LG_XT001_211");

            migrationBuilder.DropColumn(
                name: "ACTIVE",
                table: "LG_XT001_211");

            // Drop BudgetRatio table
            migrationBuilder.DropTable(
                name: "BudgetRatio");
        }
    }
}
